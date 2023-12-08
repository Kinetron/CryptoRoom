using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace VanishBox
{
    public interface IDialogService
    {
        /// <summary>
        /// Выбрать каталог.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        Task SelectDirAsync(InteractionContext<object, string?> interaction);

        /// <summary>
        /// Выбрать файл.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        Task SelectFileAsync(InteractionContext<object, string[]?> interaction);

        /// <summary>
        /// Выбрать файл.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        Task SelectFileFilterAsync(InteractionContext<object, string[]?> interaction, FileDialogFilter filter);

        /// <summary>
        /// Выбрать файлы из каталога.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        Task SelectFilesAsync(InteractionContext<object, string[]?> interaction);

        /// <summary>
        /// Диалог подтверждения с кнопками «Да», «Нет».
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>

        Task ShowConfirmDialogAsync(InteractionContext<string, bool> interaction);

        /// <summary>
        /// Окно с текстом ошибки.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        Task ShowErrorDialogAsync(InteractionContext<string, string?> interaction);
    }
}
