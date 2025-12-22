using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JavaSwitcher.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JavaSwitcher.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
    }

    private void ClearButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is LogViewModel viewModel)
        {
            viewModel.ClearLogs();
        }
    }
}