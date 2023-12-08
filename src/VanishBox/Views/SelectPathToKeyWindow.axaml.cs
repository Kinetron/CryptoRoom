using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using VanishBox.ViewModels;

namespace VanishBox.Views;

public partial class SelectPathToKeyWindow : ReactiveWindow<SelectPathToKeyViewModel>
{
    private readonly IDialogService _dialogService;
    public SelectPathToKeyWindow()
    {
        InitializeComponent();
        _dialogService = new DialogService(this);
        this.WhenActivated(d =>
        {
            ViewModel!.ExitCommand.Subscribe(Close);
             ViewModel!.SelectPathDialog.RegisterHandler((context =>
             {
                 return _dialogService.SelectFileFilterAsync(context, 
                    new FileDialogFilter() { Name = "Файл ключа", Extensions = { "grk" } });
             }));
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