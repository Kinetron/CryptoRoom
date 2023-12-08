using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace VanishBox.ViewModels
{
    public class AboutProgramViewModel : ViewModelBase
    {
        /// <summary>
        /// Закрыть окно.
        /// </summary>
        public ReactiveCommand<Unit, object> ExitCommand { get; }

        /// <summary>
        /// Текст в окне.
        /// </summary>
        public string Info => "Система защиты информации \"Vanish Box\". \r\n" +
                              "  Vanish Box - это коробка в которую фокусник прячет кролика.\r\n\r\n" +
                              "Автор: Боб, специально для Алисы.\r\n\r\n" +
                              "MIT License\r\n" +
                              "@2023";
        public AboutProgramViewModel()
        {
            ExitCommand = ReactiveCommand.Create(() => new object());
        }
    }
}
