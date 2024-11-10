namespace InSourceTransIdCheckAndFix
{
    public interface IDevTextManager
    {
        /// <summary>
        /// 
        /// Checks if a given text has an existing translation key.
        /// </summary>
        /// <param name="text">The developer text to be translated.</param>
        /// <returns>The translation key if one exists, otherwise null.</returns>
        string? GetKeyForText(string text);

        /// <summary>
        /// Verifies if the given key matches the provided developer text.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="text">The developer text to check.</param>
        /// <returns>True if the key matches the text, otherwise false.</returns>
        bool VerifyKeyMatchesText(string key, string text);

        /// <summary>
        /// 
        /// Adds a new developer text that requires translation.
        /// </summary>
        /// <param name="text">The developer text to be translated.</param>
        void AddNewTranslationRequest(string text);

        /// <summary>
        /// Retrieves a list of developer texts that still require translation.
        /// </summary>
        /// <returns>A list of pending developer texts.</returns>
        List<string> GetPendingTranslations();

        bool IsPendingTranslationDevText(string devText);

        /// <summary>
        /// Updates an existing translation with a given key and developer text.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="text">The developer text that should be associated with the key.</param>
        void UpdateTranslation(string key, string text);
    }
}
