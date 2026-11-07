using System;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace MeteoDome
{
    public static class StatusUpdater
    {
        private static readonly string StatusPath =
            @"C:\Users\Администратор\RiderProjects\telescope-backend\status\status.json";
        private const string GlobalMutexName = "Global\\RoboPhotStatusFileLock";
        private const int MutexWaitTimeoutMs = 2000;

        /// <summary>
        /// Обновляет значение по вложенному пути, например ["dome", "south_shutter", "position"]
        /// </summary>
        public static void UpdateNestedField(string[] path, JToken value)
        {
            using var mutex = new Mutex(false, GlobalMutexName);
            var hasMutex = false;
            try
            {
                hasMutex = mutex.WaitOne(MutexWaitTimeoutMs);
                if (!hasMutex)
                {
                    Logger.AddLogEntry($"[StatusUpdater] Mutex timeout ({MutexWaitTimeoutMs} ms). Пропущено обновление JSON.");
                    return;
                }

                var root = JsonHelper.LoadJsonSafely(StatusPath) ?? new JObject();
                EnsureBaseFields(root);

                // Спускаемся по дереву до нужного узла
                JToken current = root;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (current[path[i]] == null)
                        current[path[i]] = new JObject();

                    current = current[path[i]];
                }

                // Обновляем целевое поле
                string lastKey = path[path.Length - 1];
                current[lastKey] = value;

                // Обновляем время последнего изменения
                root["last_update"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                // Сохраняем обратно в файл
                JsonHelper.SaveJsonToFile(StatusPath, root);
            }
            finally
            {
                if (hasMutex)
                {
                    try { mutex.ReleaseMutex(); } catch { /* ignore */ }
                }
            }
        }


        private static void EnsureBaseFields(JObject root)
        {
            if (root["power"] == null)
                root["power"] = new JObject
                {
                    ["source"] = "unknown",
                    ["state"] = "unknown",
                    ["emergency_active"] = false,
                    ["emergency_latched"] = false
                };

            if (root["emergency"] == null)
                root["emergency"] = new JObject
                {
                    ["active"] = false,
                    ["latched"] = false,
                    ["reason"] = null,
                    ["phase"] = "idle",
                    ["last_error"] = null
                };
        }


        /// <summary>
        /// Обновляет поле верхнего уровня, например "mount"
        /// </summary>
        public static void UpdateRootField(string field, JToken value)
        {
            UpdateNestedField(new[] { field }, value);
        }
    }
}