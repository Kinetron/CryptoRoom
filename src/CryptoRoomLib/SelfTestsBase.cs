namespace CryptoRoomLib
{
    /// <summary>
    /// Класс для реализации методов тестирования алгоритмов.
    /// </summary>
    public class SelfTestsBase
    {
        private List<Func<bool>> _tests;

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string LastError { get; set; }

        public SelfTestsBase()
        {
            LastError = string.Empty;
            _tests = new List<Func<bool>>();
        }

        /// <summary>
        /// Общий метод тестирования всего алгоритма.
        /// </summary>
        /// <returns></returns>
        public bool RunTests()
        {
            foreach (var test in _tests)
            {
                if (!test()) return false;
            }

            return true;
        }

        /// <summary>
        /// Добавление метода тестирования.
        /// </summary>
        public void AppendFunc(Func<bool> func)
        {
            _tests.Add(func);
        }
    }
}
