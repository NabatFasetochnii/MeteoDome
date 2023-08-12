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
            if (_logBox.Items.Count >= 8192)
            {
                SaveLogs();
                ClearLogs();
            }

            try
            {
                _logBox.Invoke((MethodInvoker) delegate { _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}"); });
            }
            catch
            {
                _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
            }
        }

        public void ClearLogs()
        {
            _logBox.Invoke((MethodInvoker) delegate
            {
                _logBox.Items.Clear();
                _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been cleaned");
            });
        }

        public void SaveLogs()
        {
            _logBox.Invoke((MethodInvoker) delegate
            {
                var sw = new StreamWriter($"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt");
                foreach (string item in _logBox.Items) sw.WriteLine(item);
                sw.Close();
                _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved");
            });
        }
    }
}