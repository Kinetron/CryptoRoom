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
    public class SelectPathToKeyViewModel : ViewModelBase
    {
        private string _pathToSaveDir;

        /// <summary>
        /// Путь ключа.
        /// </summary>
        public string PathToSaveDir
        {
            get => _pathToSaveDir;
            set => this.RaiseAndSetIfChanged(ref _pathToSaveDir, value);
        }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        public ReactiveCommand<Unit, object> ExitCommand { get; }

        /// <summary>
        /// Закрыть окно и вернуть параметры.
        /// </summary>
        public ReactiveCommand<Unit, object> OkCommand { get; }

        /// <summary>
        /// Выбрать путь хранения ключа.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectPathCommand { get; }

        public Interaction<object, string[]?> SelectPathDialog { get; }
        public Interaction<string, string?> ShowErrorDialog { get; }

        public SelectPathToKeyViewModel()
        {
            ExitCommand = ReactiveCommand.Create(()=>(object)PathToSaveDir);
            SelectPathDialog = new Interaction<object, string[]?>();
            ShowErrorDialog = new Interaction<string, string?>();
            SelectPathCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await SelectPath();
            });

            OkCommand = ReactiveCommand.CreateFromTask<object>(async () =>
            {
                var answer = CheckInput();

                if (answer != null)
                {
                    await ShowErrorDialog.Handle(answer);
                    return null;
                }

                return PathToSaveDir;
            });
        }

        /// <summary>
        /// Выбрать путь хранения ключа.
        /// </summary>
        /// <returns></returns>
        public async Task SelectPath()
        {
            var result = await SelectPathDialog.Handle(new Unit());
            if (result == null || result.Length == 0) return;

            PathToSaveDir = result[0];
        }

        public string CheckInput()
        {
            if(string.IsNullOrEmpty(PathToSaveDir)) return "Не выбран ключ.";

            return null;
        }
    }
}
