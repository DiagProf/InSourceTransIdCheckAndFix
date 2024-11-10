using System.Reflection;

namespace DemoConsoleApplicationWithText
{
    public partial class Trs
    {
        private readonly string _translationKey;
        private readonly string _developerText;

        

        // Constructor for translations with a key and developer text.
        private Trs(string translationKey, string developerText)
        {
            this._translationKey = translationKey;
            this._developerText = developerText;
        }

        // Constructor for translations without a key (e.g., 'Off' or 'ToDo' method).
        private Trs(string developerText)
        {
            _translationKey = string.Empty;
            this._developerText = developerText;
        }

        /// <summary>
        ///     Translation needed. Creates a Trs instance with developer text that needs translation.
        /// </summary>
        /// <param name="text">The developer text to be translated.</param>
        /// <returns>A Trs instance representing the text needing translation.</returns>
        public static Trs ToDo(string text)
        {
            return new Trs(text);
        }

        /// <summary>
        ///     Translation available. Creates a Trs instance with a translation key and developer text.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="text">The developer text.</param>
        /// <returns>A Trs instance representing the text with translation available.</returns>
        public static Trs On(string key, string text)
        {
            return new Trs(key, text);
        }

        /// <summary>
        ///     No translation needed. Creates a Trs instance with text that does not require translation.
        /// </summary>
        /// <param name="text">The text that does not require translation.</param>
        /// <returns>A Trs instance representing the text without translation.</returns>
        public static Trs Off(string text)
        {
            return new Trs(text);
        }
    }
}
