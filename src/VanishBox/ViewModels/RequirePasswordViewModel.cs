using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VanishBox.ViewModels
{
    public class RequirePasswordViewModel : ViewModelBase
    {
        private string _password;

        /// <summary>
        /// Путь ключа.
        /// </summary>
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        public ReactiveCommand<Unit, object> ExitCommand { get; }

        /// <summary>
        /// Закрыть окно и вернуть параметры.
        /// </summary>
        public ReactiveCommand<Unit, object> OkCommand { get; }
        public Interaction<string, string?> ShowErrorDialog { get; }

        public RequirePasswordViewModel()
        {
            ExitCommand = ReactiveCommand.Create(() => (object)Password);
            ShowErrorDialog = new Interaction<string, string?>();

            OkCommand = ReactiveCommand.CreateFromTask<object>(async () =>
            {
                if (Password == null)
                {
                    await ShowErrorDialog.Handle("Введите пароль.");
                    return null;
                }

                return Password;
            });

        }
    }
}
