using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using JavaSwitcher.Helper;
using JavaSwitcher.Models;
using JavaSwitcher.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;

namespace JavaSwitcher.Views;

public partial class NewOrEditJdk : Window
{
    public NewOrEditJdkViewModel ViewModel { get; }

    public NewOrEditJdk()
    {
        InitializeComponent();
        ViewModel = new NewOrEditJdkViewModel();
        DataContext = ViewModel;
    }

    public NewOrEditJdk(Jdk jdk) : this()
    {
        ViewModel.FromJdk(jdk);
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!JdkHelper.IsJdkPathValid(ViewModel.JavaPath))
        {
            var messageBox = new Window
            {
                Title = "Error",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = "不是正确JDK路径",
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                }
            };
            messageBox.ShowDialog(this);
            return;
        }
        Close(ViewModel.ToJdk());
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private async void btnBrowse_Click(object? sender, RoutedEventArgs e)
    {
        IStorageFolder? startFolder = null;

        // 如果 JavaPath 已经有值，使用它作为起始位置
        if (!string.IsNullOrEmpty(ViewModel.JavaPath) && Directory.Exists(ViewModel.JavaPath))
        {
            startFolder = await StorageProvider.TryGetFolderFromPathAsync(ViewModel.JavaPath);
        }
        else
        {
            // 否则使用默认的 JDK 路径
            var defaultPath = @"C:\Program Files\Java";
            if (Directory.Exists(defaultPath))
            {
                startFolder = await StorageProvider.TryGetFolderFromPathAsync(defaultPath);
            }
        }
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select JDK Directory",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder
        });
        if (folders.Count > 0)
        {
            ViewModel.JavaPath = folders[0].Path.LocalPath;
        }
    }
}