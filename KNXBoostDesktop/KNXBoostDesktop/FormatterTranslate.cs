using System.Globalization;
using System.Text.RegularExpressions;
using DeepL;

namespace KNXBoostDesktop
{
    public class FormatterTranslate : Formatter
    {
        // Destination language for translation
        private string _destLanguage = App.DisplayElements?.SettingsWindow.TranslationDestinationLang ?? string.Empty;

        public override string Format(string input)
        {
            try
            {
                switch (input)
                {
                    case "Cmd" : input = "Commande";
                        break;
                    case "Ie" : input = "Indication d'état";
                        break;
                }
                
                // Translate the input string
                var translated = Task.Run(() => GetTranslatedStringAsync(input)).Result;

                // Replace all punctuation with spaces
                translated = Regex.Replace(translated, @"[\p{P}]", " ");

                // Convert to lowercase
                translated = translated.ToLower();

                // Replace spaces with underscores
                translated = translated.Replace(" ", "_");

                // Split the words, capitalize each one, and join without underscores
                string[] words = translated.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
                }

                return string.Join("", words);
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"Error in Format method in FormatterTranslate: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> GetTranslatedStringAsync(string input)
        {
            try
            {
                // Retrieve the DeepL API authentication key
                var authKey = App.DisplayElements?.SettingsWindow.DecryptStringFromBytes(App.DisplayElements.SettingsWindow.DeeplKey);

                // Adjust the destination language format
                switch (_destLanguage)
                {
                    case "PT":
                        _destLanguage = "pt-PT";
                        break;
                    case "EN":
                        _destLanguage = "en-GB";
                        break;
                }

                if (string.IsNullOrEmpty(authKey))
                {
                    throw new ArgumentNullException($"DeepL API key is not configured.");
                }

                // Initialize the DeepL Translator
                var translator = new Translator(authKey);

                // Translate the text
                var translatedText = await translator.TranslateTextAsync(input,null,_destLanguage);
                return translatedText.Text;
            }
            catch (ArgumentNullException ex)
            {
                App.ConsoleAndLogWriteLine($"Error: {ex.Message}");
                return string.Empty;
            }
            catch (DeepLException ex)
            {
                App.ConsoleAndLogWriteLine($"DeepL API error: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"An unexpected error occurred in GetTranslatedStringAsync(): {ex.Message}");
                return string.Empty;
            }
        }
    }
}
