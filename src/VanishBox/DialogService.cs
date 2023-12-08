using Avalonia.Controls;
using System.Threading.Tasks;
using ReactiveUI;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Models;
using MessageBoxAvaloniaEnums = MessageBox.Avalonia.Enums;

namespace VanishBox
{
    public class DialogService : IDialogService
    {
        private readonly Window _parentWindow;

        public DialogService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        /// <summary>
        /// Выбрать файл.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public async Task SelectFileAsync(InteractionContext<object, string[]?> interaction)
        {
            await SelectAnyFilesAsync(interaction, new FileDialogFilter() { Name = "All", Extensions = { "*" } }, false);
        }

        /// <summary>
        /// Выбрать файл.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public async Task SelectFileFilterAsync(InteractionContext<object, string[]?> interaction, FileDialogFilter filter)
        {
            await SelectAnyFilesAsync(interaction, filter, false);
        }

        /// <summary>
        /// Выбрать файлы.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="allowMultiple"></param>
        /// <returns></returns>
        public async Task SelectFilesAsync(InteractionContext<object, string[]?> interaction)
        {
            await SelectAnyFilesAsync(interaction, new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        }

        /// <summary>
        /// Выбрать файлы.
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="allowMultiple"></param>
        /// <returns></returns>
        public async Task SelectAnyFilesAsync(InteractionContext<object, string[]?> interaction,
            FileDialogFilter filter, bool allowMultiple = true )
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = allowMultiple ? "Выбрать файлы" : "Выбрать файл";
            dialog.AllowMultiple = allowMultiple;
            dialog.Filters.Add(filter);
            var result = await dialog.ShowAsync(_parentWindow);
            interaction.SetOutput(result);
        }

        /// <summary>
        /// Выбрать каталог.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public async Task SelectDirAsync(InteractionContext<object, string?> interaction)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync(_parentWindow);
            interaction.SetOutput(result);
        }

        /// <summary>
        /// Диалог подтверждения с кнопками «Да», «Нет».
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public async Task ShowConfirmDialogAsync(InteractionContext<string, bool> interaction)
        {

            var buttonNo = new ButtonDefinition { Name = "Нет", IsDefault = true };

            var messageBoxCustomWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(
                new MessageBoxCustomParams
                {
                    ContentTitle = "Ошибка",
                    ContentMessage = interaction.Input,
                    FontFamily = "Microsoft YaHei,Simsun",
                    Icon = MessageBoxAvaloniaEnums.Icon.Error,
                    WindowIcon = new WindowIcon("./Assets/app.ico"),
                    ButtonDefinitions = new[]
                    {
                        new ButtonDefinition { Name = "Да", IsDefault = false },
                        buttonNo
                    },
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
            var result = await messageBoxCustomWindow.ShowDialog(_parentWindow);
            
            bool action = result != buttonNo.Name;

            interaction.SetOutput(action);
        }

        /// <summary>
        /// Окно с текстом ошибки.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public async Task ShowErrorDialogAsync(InteractionContext<string, string?> interaction)
        {
            var messageBoxCustomWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxCustomWindow(
                new MessageBoxCustomParams
                {
                    ContentTitle = "Ошибка",
                    ContentMessage = interaction.Input,
                    FontFamily = "Microsoft YaHei,Simsun",
                    Icon = MessageBoxAvaloniaEnums.Icon.Error,
                    WindowIcon = new WindowIcon("./Assets/app.ico"),
                    ButtonDefinitions = new[]
                        { new ButtonDefinition { Name = "Ок", IsDefault = true }, },
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                });
            await messageBoxCustomWindow.ShowDialog(_parentWindow);

            interaction.SetOutput(null);
        }
    }
}
