using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using JavaSwitcher.Helper;
using JavaSwitcher.Models;
using JavaSwitcher.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JavaSwitcher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private bool disposedValue;
        private Jdk? _selectedJdk;
        public Jdk? SelectedJdk
        {
            get => _selectedJdk;
            set => this.RaiseAndSetIfChanged(ref _selectedJdk, value);
        }

        private string _currentJdkVersion = "";

        public string CurrentJdkVersion
        {
            get => _currentJdkVersion;
            set => this.RaiseAndSetIfChanged(ref _currentJdkVersion, value);
        }

        private string _currentJavaHome = "";

        public string CurrentJavaHome
        {
            get => _currentJavaHome;
            set => this.RaiseAndSetIfChanged(ref _currentJavaHome, value);
        }

        public ObservableCollection<Jdk> Jdks { get; } = new ObservableCollection<Jdk>();

        public ICommand SaveCommand { get; }
        public ICommand AddJdkCommand { get; }
        public ICommand OpenSetEnvironmentVariablesCommand { get; }
        public LogViewModel LogViewModel { get; }
        public ICommand SetCurrentJdkCommand { get; }
        public ICommand EditJdkCommand { get; }
        public ICommand DeleteJdkCommand { get; }

        public MainWindowViewModel()
        {
            SaveCommand = ReactiveCommand.Create(SaveClick);
            AddJdkCommand = ReactiveCommand.Create(AddJdkClick);
            OpenSetEnvironmentVariablesCommand = ReactiveCommand.Create(OpenSetEnvironmentVariablesClick);
            LogViewModel = new LogViewModel();
            SetCurrentJdkCommand = ReactiveCommand.Create<Jdk>(SetCurrentJdk);
            EditJdkCommand = ReactiveCommand.CreateFromTask<Jdk>(EditJdk);
            DeleteJdkCommand = ReactiveCommand.Create<Jdk>(DeleteJdk);

            if (AppConfigHelper.Appsetting.Jdks.Any())
                Jdks = new ObservableCollection<Jdk>(AppConfigHelper.Appsetting.Jdks);
            else
                Jdks = new ObservableCollection<Jdk>(JdkHelper.FindJdks());

            CurrentJdkVersion = JdkHelper.GetCurrentJdk();
            CurrentJavaHome = JdkHelper.GetCurrentJavaVersion();
        }

        public async Task SaveClick()
        {
            Save();
        }

        public async Task AddJdkClick()
        {
            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            NewOrEditJdk dialog = new NewOrEditJdk();
            var result = await dialog.ShowDialog<Jdk>(mainWindow);
            if (result != null)
            {
                Jdks.Add(result);
                LogViewModel.AddLog($"添加 JDK: {result.Name} -> {result.JavaPath}");
                Save();
            }
            else
            {
                LogViewModel.AddLog("取消添加 JDK");
            }
        }

        public void OpenSetEnvironmentVariablesClick()
        {
            try
            {
                Process.Start("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables");// 直接打开“环境变量”设置界面
            }
            catch (Exception ex)
            {
                LogViewModel.AddLog($"无法正常打开系统环境变量设置界面，异常信息为：\n{ex}");
            }
        }

        private void SetCurrentJdk(Jdk jdk)
        {
            if (jdk != null)
            {

                LogViewModel.AddLog($"设置当前 JDK: {jdk.Name}");
                JdkHelper.SetAllJdk(jdk.JavaPath);
                JdkHelper.RefreshEnvironmentVariables();
                CurrentJdkVersion = JdkHelper.GetCurrentJdk();
                CurrentJavaHome = JdkHelper.GetCurrentJavaVersion();
            }
        }

        private async Task EditJdk(Jdk jdk)
        {
            if (jdk != null)
            {
                var queryJdk = AppConfigHelper.Appsetting.Jdks.FirstOrDefault(w => w.Name == jdk.Name && w.JavaPath == jdk.JavaPath);
                var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                var dialog = new NewOrEditJdk(jdk);
                var result = await dialog.ShowDialog<Jdk?>(mainWindow);

                if (result != null)
                {
                    jdk.Name = result.Name;
                    jdk.JavaPath = result.JavaPath;

                    LogViewModel.AddLog($"编辑 JDK: {result.Name}");

                    queryJdk.Name = result.Name;
                    queryJdk.JavaPath = result.JavaPath;
                    Save();
                }
                else
                {
                    LogViewModel.AddLog("取消编辑 JDK");
                }
            }
        }

        private void DeleteJdk(Jdk jdk)
        {
            if (jdk != null)
            {
                LogViewModel.AddLog($"删除 JDK: {jdk.Name}");
                Jdks.Remove(jdk);
                Save();
            }
        }

        /// <summary>
        /// 保存到配置文件
        /// </summary>
        private void Save()
        {
            AppConfigHelper.Appsetting.Jdks = Jdks.ToList();
            AppConfigHelper.SaveSetting();
            LogViewModel.AddLog("保存成功");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
