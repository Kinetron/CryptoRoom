using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using Splat;
using VanishBox.Appsettings;
using VanishBox.ViewModels;
using VanishBox.Views;

namespace VanishBox
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
           
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(new Settings(), new CipherService()),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}