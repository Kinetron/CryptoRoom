using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using VanishBox.ViewModels;

namespace VanishBox.Views;

public partial class AboutProgramWindow : ReactiveWindow<AboutProgramViewModel>
{
    public AboutProgramWindow()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel!.ExitCommand.Subscribe(Close);
        });
    }
}