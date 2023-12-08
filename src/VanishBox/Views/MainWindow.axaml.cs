using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using VanishBox.ViewModels;

namespace VanishBox.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private readonly IDialogService _dialogService;
        public MainWindow()
        {
            InitializeComponent();
            _dialogService = new DialogService(this);
            this.WhenActivated(d =>
            {
                ViewModel!.ExitCommand.Subscribe(Close);
                ViewModel!.ShowKeyParamDialog.RegisterHandler(DoShowKeyParamDialogAsync);
                ViewModel!.ResetSettingsDialog.RegisterHandler(_dialogService.ShowConfirmDialogAsync);
                ViewModel!.SelectFilesDialog.RegisterHandler(_dialogService.SelectFilesAsync);
                ViewModel!.AboutProgramDialog.RegisterHandler(ShowAboutProgramDialogAsync);
                ViewModel!.SelectPathToKeyDialog.RegisterHandler(ShowSelectPathToKeyDialogAsync);
                ViewModel!.RequirePasswordDialog.RegisterHandler(ShowRequirePasswordDialogAsync);
                ViewModel!.ShowErrorDialog.RegisterHandler(_dialogService.ShowErrorDialogAsync);
            });
        }
       
        /// <summary>
        /// Окно ввода пароля и пути сохранения секретного ключа.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        private async Task DoShowKeyParamDialogAsync(InteractionContext<PasswordInputWindowViewModel, UserKeyViewModel?> interaction)
        {
            var dialog = new PasswordInputWindow();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<UserKeyViewModel?>(this);
            interaction.SetOutput(result);
        }

        /// <summary>
        /// Окно "О программе".
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        private async Task ShowAboutProgramDialogAsync(InteractionContext<AboutProgramViewModel, Unit> interaction)
        {
            var dialog = new AboutProgramWindow();
            dialog.DataContext = interaction.Input;
            await dialog.ShowDialog(this);
            interaction.SetOutput(new Unit());
        }

        /// <summary>
        /// Окно выбора пути к секретному ключу.
        /// </summary>
        private async Task ShowSelectPathToKeyDialogAsync(InteractionContext<Unit, string> interaction)
        {
            var dialog = new SelectPathToKeyWindow();
            dialog.DataContext = new SelectPathToKeyViewModel();

            var result = await dialog.ShowDialog<string>(this);
            interaction.SetOutput(result);
        }

        /// <summary>
        /// Окно ввода пароля.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        private async Task ShowRequirePasswordDialogAsync(InteractionContext<Unit, string> interaction)
        {
            var dialog = new RequirePasswordWindow();
            dialog.DataContext = new RequirePasswordViewModel();

            var result = await dialog.ShowDialog<string>(this);
            interaction.SetOutput(result);
        }
    }
}