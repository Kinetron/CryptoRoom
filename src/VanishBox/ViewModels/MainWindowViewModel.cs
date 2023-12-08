using System;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VanishBox.Appsettings;
using CryptoRoomLib.KeyGenerator;

namespace VanishBox.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Настройки программы.
        /// </summary>
        private ISettings _settings;

        /// <summary>
        /// Логика шифрования.
        /// </summary>
        private ICipherService _cipherService;

        private string _infoText;
        /// <summary>
        /// Текст выводимый в окне.
        /// </summary>
        public string InfoText
        {
            get => _infoText;
            set => this.RaiseAndSetIfChanged(ref _infoText, value);
        }

        private bool _showProgressControls;
        /// <summary>
        /// Отображать компоненты информирующее о прогрессе выполняемой операции.
        /// </summary>
        public bool ShowProgressControls
        {
            get => _showProgressControls;
            set => this.RaiseAndSetIfChanged(ref _showProgressControls, value);
        }

        private string _progressText;
        /// <summary>
        /// Текст с информацией о текущей операции.
        /// </summary>
        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }

        private int _progressValue;
        /// <summary>
        /// Значение прогресса 0-100%.
        /// </summary>
        public int ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        private bool _operationsButtonEnable;
        /// <summary>
        /// Активны кнопки управления «Зашифровать» и «Расшифровать».
        /// </summary>
        public bool OperationsButtonEnable
        {
            get => _operationsButtonEnable;
            set => this.RaiseAndSetIfChanged(ref _operationsButtonEnable, value);
        }

        /// <summary>
        /// Выбрать файлы которые требуется зашифровать/расшифровать.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectFilesCommand { get; }

        /// <summary>
        /// Сбросить настройки программы.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetSettingsCommand { get; }

        /// <summary>
        /// Сгенерировать ключ.
        /// </summary>
        public ReactiveCommand<Unit, Unit> GeneratedKeyCommand { get; }

        /// <summary>
        /// О программе.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AboutProgramCommand { get; }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        public ReactiveCommand<Unit, object> ExitCommand { get; }

        /// <summary>
        /// Зашифровать.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CryptCommand { get; }

        /// <summary>
        /// Расшифровать.
        /// </summary>
        public ReactiveCommand<Unit, Unit> DecryptCommand { get; }

        /// <summary>
        /// Ввод пароля и пути сохранения секретного ключа.
        /// </summary>
        public Interaction<PasswordInputWindowViewModel, UserKeyViewModel?> ShowKeyParamDialog { get; }

        /// <summary>
        /// Сбросить настройки программы.
        /// </summary>
        public Interaction<string, bool> ResetSettingsDialog { get; }

        /// <summary>
        /// Выбрать файлы которые требуется зашифровать/расшифровать.
        /// </summary>
        public Interaction<object, string[]?> SelectFilesDialog { get; }

        /// <summary>
        /// Окно "О программе".
        /// </summary>
        public Interaction<AboutProgramViewModel, Unit> AboutProgramDialog { get; }

        /// <summary>
        /// Окно выбора пути к секретному ключу.
        /// </summary>
        public Interaction<Unit, string> SelectPathToKeyDialog { get; }

        /// <summary>
        /// Окно ввода пароля.
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public Interaction<Unit, string> RequirePasswordDialog { get; }

        /// <summary>
        /// Отображает диалог с сообщением об ошибке.
        /// </summary>
        public Interaction<string, string?> ShowErrorDialog { get; }

        private string[] _selectedFiles { get; set; }

        /// <summary>
        /// Пароль к ключу.
        /// </summary>
        private string _passwordToKey;
        
        public MainWindowViewModel(ISettings settings, ICipherService cipherService)
        {
            _settings = settings;
            _cipherService = cipherService;

            SelectFilesDialog = new Interaction<object, string[]?>();
            ResetSettingsDialog = new Interaction<string, bool>();
            ShowKeyParamDialog = new Interaction<PasswordInputWindowViewModel, UserKeyViewModel?>();
            AboutProgramDialog = new Interaction<AboutProgramViewModel, Unit>();
            SelectPathToKeyDialog = new Interaction<Unit, string>();
            RequirePasswordDialog = new Interaction<Unit, string>();
            ShowErrorDialog = new Interaction<string, string?>();

            ExitCommand = ReactiveCommand.Create(() => new object());
            CryptCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await CryptFiles();
            });

            DecryptCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await DecryptFiles();
            });

            SelectFilesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectFiles();
            });

            ResetSettingsCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await ResetSettings();
            });

            GeneratedKeyCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await GeneratedKey();
            });

            AboutProgramCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await AboutProgramDialog.Handle(new AboutProgramViewModel());
            });

            SetUserInfoText();
        }

        /// <summary>
        /// Выбрать файлы которые требуется зашифровать/расшифровать.
        /// </summary>
        /// <returns></returns>
        public async Task SelectFiles()
        {
            _selectedFiles = await SelectFilesDialog.Handle(new object());
            
            if (_selectedFiles.Length > 0)
            {
                OperationsButtonEnable = true;
            }

            foreach (var item in _selectedFiles)
            {
                AppendInfoText(item);
            }
        }

        /// <summary>
        /// Сбросить настройки программы.
        /// </summary>
        /// <returns></returns>
        public async Task ResetSettings()
        {
            var result = await ResetSettingsDialog.Handle("Вы действительно хотите сбросить настройки?");
            if (result == false) return;

            _settings.Clear();
            _settings.Save();

            AppendInfoText("Настройки сброшены.");
        }

        /// <summary>
        /// Сгенерировать ключ.
        /// </summary>
        /// <returns></returns>
        public async Task GeneratedKey()
        {
            var keyParam = new PasswordInputWindowViewModel();
            var result = await ShowKeyParamDialog.Handle(keyParam);
            if (result == null) return;

            string fileName = $"Key{DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss")}.{SecretKeyMaker.KeyExtension}";
            string path = Path.Combine(result.Path, fileName);

            SecretKeyMaker maker = new SecretKeyMaker();
            if (!maker.CreateKeyFileNoReq(result.Password, path))
            {
                AppendInfoText(maker.LastError);
                return;
            }

            AppendInfoText(happyMessageKeyGen(result.Path, fileName));
        }

        /// <summary>
        /// Информационное сообщение об успешности генерации ключа.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string happyMessageKeyGen(string path, string fileName)
        {
            string delimiter = new string('*', 90) + "\r\n";

            return delimiter +
                   "Уважаемый пользователь!\r\n" +
                   $"Программа создала секретный ключ {fileName} и сохранила его в файл по пути: \r\n" +
                   $"{path}\r\n" +
                   "Храните в тайне пароль. Не передавайте секретный ключ другим людям.\r\n" +
                   delimiter;
        }

        /// <summary>
        /// Задает справочный текст в информационном окне.
        /// </summary>
        private void SetUserInfoText()
        {
            string infoText = "Для начала работы в меню \"Файл\" выберите \"Открыть\", выделите файлы требующие обработки.";
            AppendInfoText(infoText);
        }

        /// <summary>
        /// Переводит компоненты управления в первоначальное состояние. Очищает список файлов, блокирует кнопки.
        /// </summary>
        private void ResetControls()
        {
            ShowProgressControls = false;
            OperationsButtonEnable = false;
            _selectedFiles = new string[]{};
        }

        /// <summary>
        /// Отображает элементы информирующее о текущем состоянии операции (progress bar, textblock).
        /// </summary>
        private void EnableProgressControls()
        {
            ShowProgressControls = true;
            OperationsButtonEnable = false;
        }

        /// <summary>
        /// Выводит текст в информационное окно.
        /// </summary>
        /// <param name="text"></param>
        private void AppendInfoText(string text)
        {
            InfoText += $"{text}\r\n";
        }

        /// <summary>
        /// Шифрует  все файлы.
        /// </summary>
        /// <returns></returns>
        private async Task CryptFiles()
        {
            if (!await GetUserParam()) return;

            EnableProgressControls();

            await Task.Run(() =>
            {
                var complete = _cipherService.RunOperation(_selectedFiles, true, (text) =>
                {
                    AppendInfoText(text);
                }, (progress) =>
                {
                    ProgressValue = progress;
                }, (textProgress) =>
                {
                    ProgressText = textProgress;
                });


                if (!complete)
                {
                    AppendInfoText(_cipherService.LastError);
                }

                AppendInfoText("*** Шифрование завершено ***");
                ResetControls();
            });
        }

        /// <summary>
        /// Расшифровать все файлы.
        /// </summary>
        /// <returns></returns>
        private async Task DecryptFiles()
        {
            if(!await GetUserParam()) return;

            EnableProgressControls();

            await Task.Run(() =>
            {
                var complete = _cipherService.RunOperation(_selectedFiles, false, (text) =>
                {
                    AppendInfoText(text);
                }, (progress) =>
                {
                    ProgressValue = progress;
                }, (textProgress) =>
                {
                    ProgressText = textProgress;
                });


                if (!complete)
                {
                    AppendInfoText(_cipherService.LastError);
                }

                AppendInfoText("*** Расшифровка завершена ***");
                ResetControls();
            });
        }

        /// <summary>
        /// Считывает путь к ключу, проверяет пароль.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GetUserParam()
        {
            var pathToKey = await GetPathToKey();
            if (pathToKey == null) return false;

            var result = await GetPasswordToKey();
            if (result == null) return false;

            if (!_cipherService.CheckPassword(_passwordToKey, pathToKey))
            {
                await ShowErrorDialog.Handle(_cipherService.LastError);
                _passwordToKey = string.Empty;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Возвращает путь к секретному ключу. Если путь не задан – отображает диалоговое окно. Сохраняет полученный путь.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetPathToKey()
        {
            if (string.IsNullOrEmpty(_settings.AppSettings.PathSecretKey))
            {
                var path = await SelectPathToKeyDialog.Handle(new Unit());
                if (path != null)
                {
                    _settings.AppSettings.PathSecretKey = path;
                    _settings.Save();
                }
                return path;
            }

            return _settings.AppSettings.PathSecretKey;
        }

        /// <summary>
        /// Возвращает пароль. Если пароль не вводился первый раз в программу – отображает диалоговое окно.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetPasswordToKey()
        {
            if(!string.IsNullOrEmpty(_passwordToKey)) return _passwordToKey;

            var result = await RequirePasswordDialog.Handle(new Unit());
            if (result == null) return null;

            _passwordToKey = result;
            return _passwordToKey;
        }

    }
}