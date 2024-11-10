using System.Reflection;

namespace DemoConsoleApplicationWithText
{
    public partial class Trs
    {
        private static string _currentLanguage = "Developer";

        private static readonly Lazy<List<string>> _availableLanguages = new(() =>
        {
            var languages = typeof(RtTranslationRecord)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != nameof(RtTranslationRecord.TextId))
                .Select(p => p.Name)
                .ToList();

            // Add 'Developer' as a pseudo language.
            languages.Add("Developer");
            return languages;
        });

        private string _cachedTranslation = string.Empty;
        private string _lastLanguage = string.Empty;

        /// <summary>
        ///     Gets or sets the current language for translations.
        ///     Changing the language will invalidate cached translations.
        /// </summary>
        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if ( !string.Equals(_currentLanguage, value, StringComparison.OrdinalIgnoreCase) )
                {
                    _currentLanguage = value;
                    // No need for a languageChanged flag; lastLanguage in each instance will differ.
                }
            }
        }

        /// <summary>
        ///     Gets the list of available languages based on the properties of RtTranslationRecord,
        ///     including the pseudo language 'Developer'.
        /// </summary>
        public static IReadOnlyList<string> AvailableLanguages => _availableLanguages.Value;

        /// <summary>
        ///     Controls whether the DisplayName includes the translation key.
        /// </summary>
        public static bool IncludeDbIdInDisplayName { get; set; } = false;

        /// <summary>
        ///     Gets the display name, which is the translated text or falls back according to the hierarchy.
        /// </summary>
        public string DisplayName
        {
            get
            {
                // If translationKey is empty, return developer text (no translation needed).
                if ( string.IsNullOrEmpty(_translationKey) )
                {
                    return _developerText;
                }

                // Check if we have a cached translation and the language hasn't changed.
                if ( _lastLanguage == CurrentLanguage )
                {
                    return FormatDisplayName(_cachedTranslation);
                }

                var translation = _developerText;

                if ( CurrentLanguage.Equals("Developer", StringComparison.OrdinalIgnoreCase) )
                {
                    // If 'Developer' is selected, use the developer text.
                    translation = _developerText;
                }
                else
                {
                    // Find the translation record by key using FindById for faster access.
                    var record = LiteDatabaseManager.Instance.Collection.FindOne(x => x.TextId.Equals(_translationKey));

                    if ( record != null )
                    {
                        // Get the property corresponding to the current language.
                        var languageProperty = typeof(RtTranslationRecord).GetProperty(
                            CurrentLanguage,
                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                        if ( languageProperty != null )
                        {
                            translation = languageProperty.GetValue(record) as string;
                        }

                        // If translation is null or empty, fall back to English.
                        if ( string.IsNullOrEmpty(translation) &&
                             !string.Equals(CurrentLanguage, "English", StringComparison.OrdinalIgnoreCase) )
                        {
                            translation = record.English;
                        }
                    }

                    // If still null or empty, fall back to developer text.
                    if ( string.IsNullOrEmpty(translation) )
                    {
                        translation = _developerText;
                    }
                }

                // Cache the translation and the language it corresponds to.
                _cachedTranslation = translation;
                _lastLanguage = CurrentLanguage;

                return FormatDisplayName(translation);
            }
        }

        /// <summary>
        ///     Returns the developer text. Trs behaves like a string when used directly.
        /// </summary>
        /// <returns>The developer text.</returns>
        public override string ToString()
        {
            return _developerText;
        }

        /// <summary>
        ///     Formats the display name based on the IncludeDbIdInDisplayName setting.
        /// </summary>
        /// <param name="translation">The translated text.</param>
        /// <returns>The formatted display name.</returns>
        private string FormatDisplayName(string translation)
        {
            if ( IncludeDbIdInDisplayName && !string.IsNullOrEmpty(_translationKey) )
            {
                return $"{_translationKey}: {translation}";
            }

            return translation;
        }

        /// <summary>
        ///     Implicit conversion from Trs to string.
        /// </summary>
        /// <param name="trs">The Trs instance.</param>
        public static implicit operator string(Trs trs)
        {
            return trs.ToString();
        }
    }
}
