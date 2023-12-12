using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CryptoRoomLib.Enums;
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
	            ISettings settings = new Settings();
	            string algoritm = settings.AppSettings.СipherAlgoritm;
				//Добавить проверку если пользователь введет не правильный идентификатор.

				СipherAlgoritmsEnum alg = Enum.Parse<СipherAlgoritmsEnum>(algoritm);

				desktop.MainWindow = new MainWindow
                {
	               DataContext = new MainWindowViewModel(settings, new CipherService(alg)),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}