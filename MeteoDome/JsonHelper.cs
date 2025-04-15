using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MeteoDome;

public static class JsonHelper
{
    public static void SaveJsonToFile(string path, JObject root)
    {
        try
        {
            var tempPath = path + ".tmp";
            var backupPath = path + ".bak";

            if (File.Exists(path))
                File.Copy(path, backupPath, true);

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(fs);
            writer.Write(root.ToString(Formatting.Indented));

            // File.Replace(tempPath, path, backupPath);

            // Logger.AddLogEntry($"[JsonHelper] ✅ JSON сохранён в {path} ({root.ToString(Formatting.None).Length} символов)");
        }
        catch (Exception ex)
        {
            Logger.AddLogEntry($"[JsonHelper] ❌ Ошибка при записи JSON в {path}: {ex.Message}");
        }
    }

    public static bool TryRestoreJsonFromBackup(string path)
    {
        string backupPath = path + ".bak";

        try
        {
            bool isCorrupt = !File.Exists(path) || new FileInfo(path).Length == 0;

            if (!File.Exists(backupPath))
            {
                Logger.AddLogEntry($"[JsonHelper] ❌ Резервная копия отсутствует: {backupPath}");
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

    public static JObject LoadJsonSafely(string path)
    {
        try
        {
            string jsonText = File.Exists(path) ? File.ReadAllText(path) : null;

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                Logger.AddLogEntry($"[JsonHelper] ⚠️ Файл {path} пустой. Пытаюсь восстановить...");
                if (!TryRestoreJsonFromBackup(path)) return null;
                jsonText = File.ReadAllText(path);
            }

            try
            {
                return JObject.Parse(jsonText);
            }
            catch (JsonReaderException ex)
            {
                Logger.AddLogEntry($"[JsonHelper] ❌ Ошибка парсинга JSON: {ex.Message}");
                if (TryRestoreJsonFromBackup(path))
                {
                    string backupJson = File.ReadAllText(path);
                    return JObject.Parse(backupJson);
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.AddLogEntry($"[JsonHelper] ❌ Не удалось загрузить JSON из {path}: {ex.Message}");
            return null;
        }
    }
}