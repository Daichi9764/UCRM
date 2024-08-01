using System.Globalization;
using System.Text.RegularExpressions;
using DeepL;
using DeepL.Model;

namespace KNXBoostDesktop;

/// <summary>
/// Provides functionality to translate and format input strings. This class extends the abstract <see cref="Formatter"/> 
/// class and implements specific behavior for translating text using the DeepL API and formatting the translated text.
///
/// The <see cref="FormatterTranslate"/> class is designed to handle translation of text between different languages and
/// apply specific formatting to the translated strings. It uses the DeepL API to perform the translation and provides 
/// additional text processing like punctuation removal and capitalization of words.
/// </summary>
public class FormatterTranslate : Formatter
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Represents the destination language code used for translation. The value is retrieved from the application's settings,
    /// specifically from <see cref="SettingsWindow.TranslationDestinationLang"/>.
    /// </summary>
    private string _destLanguage = App.DisplayElements?.SettingsWindow?.TranslationDestinationLang ?? string.Empty;
        
    /// <summary>
    /// Represents the source language code used for translation. The value is retrieved from the application's settings,
    /// specifically from <see cref="SettingsWindow.TranslationSourceLang"/>.
    /// </summary>
    private readonly string _sourceLanguage = App.DisplayElements?.SettingsWindow?.TranslationSourceLang ?? string.Empty;

    /// <summary>
    /// Cache for storing previously translated strings
    /// </summary>
    private Dictionary<string, string> _translationCache = new();

       
        
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Translates the specified input string using the DeepL API and then formats the translated string. The formatting process
    /// involves replacing punctuation with spaces, converting to lowercase, replacing spaces with underscores, and capitalizing
    /// each word.
    ///
    /// <para>
    /// The method performs the following steps:
    /// <list type="bullet">
    ///     <item>Translates the input string based on predefined cases.</item>
    ///     <item>Removes punctuation and replaces it with spaces.</item>
    ///     <item>Converts the string to lowercase.</item>
    ///     <item>Replaces spaces with underscores and removes multiple underscores.</item>
    ///     <item>Capitalizes each word and joins them into a single string.</item>
    /// </list>
    /// </para>
    /// 
    /// <param name="input">The string to be translated and formatted.</param>
    /// <returns>A formatted string where punctuation is removed, spaces are replaced with underscores, and words are capitalized.</returns>
    /// <exception cref="Exception">Thrown when an error occurs during translation or formatting.</exception>
    /// </summary>
    public override string Format(string input)
    {
        try
        {
            switch (input.ToLower())
            {
                case "cmd" : input = "Commande";
                    break;
                case "ie" : input = "Indication d'état";
                    break;
                case "rdc" : input = "Rez-de-chaussée ";
                    break; 
                case "piece" : input = "salle";
                    break;
            }
                
            // Check if the translation is already in the cache
            if (_translationCache.TryGetValue(input, out var translated))
            {
                return translated;
            }
            else
            {

                // Translate the input string
                translated = Task.Run(() => GetTranslatedStringAsync(input)).Result;

                // Replace all punctuation with spaces
                translated = Regex.Replace(translated, @"[\p{P}]", " ");

                // Convert to lowercase
                translated = translated.ToLower();

                // Replace spaces with underscores
                translated = translated.Replace(" ", "_");

                // Split the words, capitalize each one, and join without underscores
                string[] words = translated.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < words.Length; i++)
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
                }

                translated = string.Join("", words);
                    
                // Store the translation in the cache
                _translationCache[input] = translated;

                return translated;
            }
        }
        catch (Exception ex)
        {
            App.ConsoleAndLogWriteLine($"Error in Format method in FormatterTranslate: {ex.Message}");
            return string.Empty;
        }
    }

        
    /// <summary>
    /// Translates the specified input string using the DeepL API. The translation is performed asynchronously and the result
    /// is returned as a string.
    ///
    /// <para>
    /// This method directly translates the input string without additional formatting. It uses the DeepL API for translation and
    /// handles exceptions related to API errors or configuration issues.
    /// </para>
    /// 
    /// <param name="input">The string to be translated.</param>
    /// <returns>The translated string, or an empty string if an error occurs.</returns>
    /// </summary>
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

        
    /// <summary>
    /// Asynchronously translates the specified input string using the DeepL API. The method adjusts the destination language
    /// based on predefined formats and optionally detects the source language automatically based on configuration settings.
    ///
    /// <para>
    /// This method performs the following steps:
    /// <list type="bullet">
    ///     <item>Adjusts the destination language format based on predefined settings.</item>
    ///     <item>Checks if the DeepL API key is configured.</item>
    ///     <item>Translates the input string using the DeepL API, optionally detecting the source language.</item>
    /// </list>
    /// </para>
    /// 
    /// <param name="input">The string to be translated.</param>
    /// <returns>The translated string as returned by the DeepL API, or an empty string if an error occurs.</returns>
    /// </summary>
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
                
            // Translate the text
            TextResult translatedText;
            if (App.DisplayElements?.SettingsWindow != null && App.DisplayElements.SettingsWindow.EnableAutomaticSourceLangDetection)
            {
                translatedText = await GroupAddressNameCorrector.Translator?.TranslateTextAsync(input, null, _destLanguage)!;
            }
            else
            {
                translatedText = await GroupAddressNameCorrector.Translator?.TranslateTextAsync(input, _sourceLanguage, _destLanguage)!;
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