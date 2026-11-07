using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeteoDome;

public static class JsonHelper
{
    private const int ReadAttempts = 3;
    private const int ReadRetryDelayMs = 50;

    public static void SaveJsonToFile(string path, JObject root)
    {
        string? tempPath = null;
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var backupPath = path + ".bak";
            tempPath = path + "." + System.Diagnostics.Process.GetCurrentProcess().Id + "." + Guid.NewGuid().ToString("N") + ".tmp";

            using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fs, new UTF8Encoding(false)))
            {
                writer.Write(root.ToString(Formatting.Indented));
                writer.Flush();
                fs.Flush(true);
            }

            if (File.Exists(path))
            {
                try
                {
                    File.Replace(tempPath, path, backupPath, true);
                    tempPath = null;
                }
                catch (PlatformNotSupportedException)
                {
                    ReplaceByMove(path, tempPath, backupPath);
                    tempPath = null;
                }
                catch (UnauthorizedAccessException)
                {
                    ReplaceByMove(path, tempPath, backupPath);
                    tempPath = null;
                }
            }
            else
            {
                File.Move(tempPath, path);
                tempPath = null;
            }
        }
        catch (Exception ex)
        {
            Logger.AddLogEntry($"[JsonHelper] ❌ Ошибка при атомарной записи JSON в {path}: {ex}");
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(tempPath))
            {
                try
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch
                {
                    // ignore cleanup errors
                }
            }
        }
    }

    private static void ReplaceByMove(string path, string tempPath, string backupPath)
    {
        if (File.Exists(path))
            File.Copy(path, backupPath, true);

        var oldPath = path + ".old." + Guid.NewGuid().ToString("N");
        try
        {
            if (File.Exists(path)) File.Move(path, oldPath);
            File.Move(tempPath, path);
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }
        catch
        {
            try
            {
                if (!File.Exists(path) && File.Exists(oldPath)) File.Move(oldPath, path);
            }
            catch
            {
                // ignore rollback errors; original exception is rethrown
            }

            throw;
        }
    }

    public static bool TryRestoreJsonFromBackup(string path)
    {
        string backupPath = path + ".bak";

        try
        {
            bool isCorrupt = !File.Exists(path) || new FileInfo(path).Length == 0 || !IsValidJsonFile(path);

            if (!File.Exists(backupPath))
            {
                Logger.AddLogEntry($"[JsonHelper] ❌ Резервная копия отсутствует: {backupPath}");
                return false;
            }

            if (!IsValidJsonFile(backupPath))
            {
                Logger.AddLogEntry($"[JsonHelper] ❌ Резервная копия повреждена: {backupPath}");
                return false;
            }

            if (!isCorrupt)
            {
                Logger.AddLogEntry($"[JsonHelper] Основной файл {path} не повреждён, восстановление не требуется.");
                return false;
            }

            File.Copy(backupPath, path, true);
            Logger.AddLogEntry($"[JsonHelper] 🔄 Восстановлено из резервной копии: {backupPath}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.AddLogEntry($"[JsonHelper] ❌ Ошибка восстановления {path} из {backupPath}: {ex.Message}");
            return false;
        }
    }

    private static bool IsValidJsonFile(string path)
    {
        try
        {
            if (!File.Exists(path) || new FileInfo(path).Length == 0) return false;
            JObject.Parse(ReadAllTextShared(path));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static JObject? LoadJsonSafely(string path)
    {
        for (int attempt = 1; attempt <= ReadAttempts; attempt++)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Logger.AddLogEntry($"[JsonHelper] ⚠️ Файл {path} отсутствует. Использую пустой JSON.");
                    return new JObject();
                }

                string jsonText = ReadAllTextShared(path);
                if (string.IsNullOrWhiteSpace(jsonText))
                    throw new JsonReaderException("JSON file is empty");

                return JObject.Parse(jsonText);
            }
            catch (Exception ex) when (attempt < ReadAttempts)
            {
                Logger.AddLogEntry($"[JsonHelper] ⚠️ Ошибка чтения JSON {path}, попытка {attempt}/{ReadAttempts}: {ex.Message}");
                Thread.Sleep(ReadRetryDelayMs);
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry($"[JsonHelper] ❌ Не удалось прочитать JSON {path}: {ex.Message}");
            }
        }

        if (TryRestoreJsonFromBackup(path))
        {
            try
            {
                return JObject.Parse(ReadAllTextShared(path));
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry($"[JsonHelper] ❌ Не удалось прочитать JSON после восстановления {path}: {ex.Message}");
            }
        }

        return null;
    }

    private static string ReadAllTextShared(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(fs, Encoding.UTF8, true);
        return reader.ReadToEnd();
    }
}
