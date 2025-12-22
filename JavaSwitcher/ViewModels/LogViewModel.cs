using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JavaSwitcher.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        public ICommand ClearLogsCommand { get; }

        public LogViewModel()
        {
            ClearLogsCommand = ReactiveCommand.Create(ClearLogs);
        }

        public void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogMessages.Add($"[{timestamp}] {message}");
        }

        public void ClearLogs()
        {
            LogMessages.Clear();
        }
    }
}
