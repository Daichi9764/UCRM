using System.Globalization;
using System.Text.RegularExpressions;
using DeepL;
using DeepL.Model;
using System.Threading.Tasks;

namespace KNXBoostDesktop
{
    public class FormatterTranslate : Formatter
    {
        // Destination language for translation
        private string _destLanguage = App.DisplayElements?.SettingsWindow.TranslationDestinationLang ?? string.Empty;
        private string _sourceLanguage = App.DisplayElements?.SettingsWindow.TranslationSourceLang ?? string.Empty;

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

        public override string Translate(string input)
        {
            try
            {
                // Translate the input string 
                return Task.Run(() => GetTranslatedStringAsync(input)).Result;
            }
            catch (Exception ex)
            {
                App.ConsoleAndLogWriteLine($"Error in Translate method in FormatterTranslate: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> GetTranslatedStringAsync(string input)
        {
            try
            {
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

                if (string.IsNullOrEmpty(GroupAddressNameCorrector.AuthKey))
                {
                    throw new ArgumentNullException("DeepL API key is not configured.");
                }

                // Translate the text
                TextResult translatedText;
                if (App.DisplayElements != null && App.DisplayElements.SettingsWindow.EnableAutomaticSourceLangDetection)
                {
                    translatedText = await GroupAddressNameCorrector.Translator.TranslateTextAsync(input, null, _destLanguage);
                }
                else
                {
                    translatedText = await GroupAddressNameCorrector.Translator.TranslateTextAsync(input, _sourceLanguage, _destLanguage);
                }

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
