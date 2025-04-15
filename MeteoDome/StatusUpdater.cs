using Newtonsoft.Json.Linq;
using System.Threading;

namespace MeteoDome
{
    public static class StatusUpdater
    {
        private static readonly string StatusPath =
            @"C:\Users\Администратор\RiderProjects\telescope-backend\status\status.json";
        private const string GlobalMutexName = "Global\\RoboPhotStatusFileLock";

        /// <summary>
        /// Обновляет значение по вложенному пути, например ["dome", "south_shutter", "position"]
        /// </summary>
        public static void UpdateNestedField(string[] path, JToken value)
        {
            using var mutex = new Mutex(false, GlobalMutexName);
            try
            {
                mutex.WaitOne();

                var root = JsonHelper.LoadJsonSafely(StatusPath);
                if (root == null)
                {
                    Logger.AddLogEntry("[StatusUpdater] Пропущено обновление JSON — не удалось загрузить.");
                    return;
                }

                JToken current = root;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (current[path[i]] == null)
                        current[path[i]] = new JObject();

                    current = current[path[i]];
                }

                string lastKey = path[path.Length - 1];
                current[lastKey] = value;

                JsonHelper.SaveJsonToFile(StatusPath, root);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
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