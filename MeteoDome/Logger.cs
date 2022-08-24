using System;
using System.IO;
using System.Windows.Forms;

namespace MeteoDome
{
    public class Logger
    {
        private readonly ListBox _logBox;

        public Logger(ListBox listBox)
        {
            _logBox = listBox;
        }

        public void AddLogEntry(string entry)
        {
            if (_logBox.Items.Count >= 100)
            {
                SaveLogs();
                _logBox.Items.Clear();
                _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved and cleaned");
            }

            _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
        }

        private void SaveLogs()
        {
            var sw = new StreamWriter($"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt");
            foreach (string item in _logBox.Items) sw.WriteLine(item);
            sw.Close();
        }
    }
}