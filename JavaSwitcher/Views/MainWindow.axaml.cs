using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace JavaSwitcher.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnHyperlinkButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton button)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    bool success = await topLevel.Launcher.LaunchUriAsync(button.NavigateUri);
                }
            }
        }
    }
}