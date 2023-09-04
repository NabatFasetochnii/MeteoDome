using System;
using System.IO;
using System.Windows.Forms;

namespace MeteoDome
{
    public static class Logger
    {
        public static  ListBox LogBox;

        public static void AddLogEntry(string entry)
        {
            if (LogBox.Items.Count >= 8192)
            {
                SaveLogs();
                ClearLogs();
            }

            try
            {
                LogBox.Invoke((MethodInvoker) delegate { LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}"); });
            }
            catch
            {
                LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
            }
        }

        public static void ClearLogs()
        {
            LogBox.Invoke((MethodInvoker) delegate
            {
                LogBox.Items.Clear();
                LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been cleaned");
            });
        }

        public static void SaveLogs()
        {
            LogBox.Invoke((MethodInvoker) delegate
            {
                var sw = new StreamWriter($"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt");
                foreach (string item in LogBox.Items) sw.WriteLine(item);
                sw.Close();
                LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved");
            });
        }
    }
}