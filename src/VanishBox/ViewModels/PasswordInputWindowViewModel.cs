using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CryptoRoomLib.KeyGenerator;

namespace VanishBox.ViewModels
{
    public class PasswordInputWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Данные создаваемого ключа.
        /// </summary>
        public UserKeyViewModel? UserKeyData { get; set; }

        /// <summary>
        /// Создать ключ.
        /// </summary>
        public ReactiveCommand<Unit, UserKeyViewModel?> CreateKeyCommand { get; }

        /// <summary>
        /// Закрыть окно и выйти.
        /// </summary>
        public ReactiveCommand<Unit, UserKeyViewModel?> ExitCommand { get; }

        /// <summary>
        /// Выбрать путь сохранения ключа.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectPathCommand { get; }

        public Interaction<object, string?> SelectPathDialog { get; }

        public Interaction<string, string?> ShowErrorDialog { get; }
        
        private string _pathToSaveDir;

        /// <summary>
        /// Путь сохранения ключа.
        /// </summary>
        public string PathToSaveDir
        {
            get => _pathToSaveDir;
            set => this.RaiseAndSetIfChanged(ref _pathToSaveDir, value);
        }
        
        private string _password;
        /// <summary>
        /// Пароль для секретного ключа.
        /// </summary>
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        
        private string _passwordConfirm;

        /// <summary>
        /// Подтверждение пароля для секретного ключа.
        /// </summary>
        public string PasswordConfirm
        {
            get => _passwordConfirm;
            set => this.RaiseAndSetIfChanged(ref _passwordConfirm, value);
        }

        public PasswordInputWindowViewModel()
        {
            ShowErrorDialog = new Interaction<string, string?>();
            ExitCommand = ReactiveCommand.Create(() => UserKeyData);
            SelectPathDialog = new Interaction<object, string?>();

            CreateKeyCommand = ReactiveCommand.CreateFromTask<UserKeyViewModel?>(async () =>
            {
                return await CreateKey();
            });
            
            SelectPathCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectPath();
            });
        }

        /// <summary>
        /// Создать ключ.
        /// </summary>
        /// <returns></returns>
        public async Task<UserKeyViewModel?> CreateKey()
        {
            var answer = CheckFields();

            if (answer != null)
            {
                await ShowErrorDialog.Handle(answer);
                return null;
            }

            UserKeyData = new UserKeyViewModel
            {
                Password = Password,
                Path = PathToSaveDir
            };

            return UserKeyData;
        }

        /// <summary>
        /// Выбрать путь сохранения ключа.
        /// </summary>
        /// <returns></returns>
        public async Task SelectPath()
        {
            var result = await SelectPathDialog.Handle(new object());
            if (result == null) return;

            PathToSaveDir = result;
        }

        /// <summary>
        /// Проверяет заполенены ли поля.
        /// </summary>
        /// <returns></returns>
        private string CheckFields()
        {
            if (string.IsNullOrEmpty(Password)) return "Введите пароль.";
            if (string.IsNullOrEmpty(PasswordConfirm)) return "Введите подтверждение пароля.";

            if (Password != PasswordConfirm) return "Пароль и подтверждение не совпадают.";

            if (string.IsNullOrEmpty(PathToSaveDir)) return "Не выбран каталог сохранения ключа.";

            if (Password.Length < SecretKeyMaker.PasswordMinLength)
            {
                return $"Пароль не может быть менее {SecretKeyMaker.PasswordMinLength}-ми символов";
            }

            if (Password.Length > SecretKeyMaker.PasswordMaxLength)
            {
                return $"Пароль не может быть более {SecretKeyMaker.PasswordMaxLength}-ми символов. Сейчас {Password.Length}.";
            }

            return null;
        }
    }
}
