using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using VanishBox.ViewModels;

namespace VanishBox.Views;

public partial class RequirePasswordWindow : ReactiveWindow<RequirePasswordViewModel>
{
    private readonly IDialogService _dialogService;
    public RequirePasswordWindow()
    {
        InitializeComponent();
        _dialogService = new DialogService(this);
        this.WhenActivated(d =>
        {
            ViewModel!.ExitCommand.Subscribe(Close);
            ViewModel!.OkCommand.Subscribe(CloseIfNotEmpty);
            ViewModel!.ShowErrorDialog.RegisterHandler(_dialogService.ShowErrorDialogAsync);
        });
    }

    /// <summary>
    /// Если пользователь не заполнил поля - окно не закрываем.
    /// </summary>
    /// <param name="dialogResult"></param>
    public void CloseIfNotEmpty(object dialogResult)
    {
        if (dialogResult != null)
        {
            Close(dialogResult);
        };
    }
}