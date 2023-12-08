using System;
using Avalonia.ReactiveUI;
using VanishBox.ViewModels;
using ReactiveUI;

namespace VanishBox.Views;

public partial class PasswordInputWindow : ReactiveWindow<PasswordInputWindowViewModel>
{
    private readonly IDialogService _dialogService;
    public PasswordInputWindow()
    {
        InitializeComponent();
        _dialogService = new DialogService(this);
        this.WhenActivated(d =>
        {
            ViewModel!.CreateKeyCommand.Subscribe(CloseIfNotEmpty);
            ViewModel!.ExitCommand.Subscribe(Close);
            ViewModel!.SelectPathDialog.RegisterHandler(_dialogService.SelectDirAsync);
            ViewModel!.ShowErrorDialog.RegisterHandler(_dialogService.ShowErrorDialogAsync);
        });
    }
    
    /// <summary>
    /// Если пользователь не заполнил поля-окно не закрываем.
    /// </summary>
    /// <param name="dialogResult"></param>
    public void CloseIfNotEmpty(UserKeyViewModel dialogResult)
    {
        if (dialogResult != null)
        {
            Close(dialogResult);
        };
    }
}