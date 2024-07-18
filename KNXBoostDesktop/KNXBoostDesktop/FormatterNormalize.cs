using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KNXBoostDesktop;

/// <summary>
/// Provides functionality to normalize input strings by removing punctuation, converting to lowercase, 
/// removing diacritics, and capitalizing words. This class extends the abstract <see cref="Formatter"/> 
/// class and implements the specific formatting behavior for normalizing strings.
///
/// The <see cref="FormatterNormalize"/> class is designed to transform strings into a normalized format suitable 
/// for various applications where consistent and clean string representation is required. It performs multiple
/// transformations including punctuation removal, diacritic stripping, and formatting adjustments.
/// </summary>
public class FormatterNormalize : Formatter
{
    /// <summary>
    /// Normalizes the specified input string by performing a series of transformations including replacing punctuation
    /// with spaces, converting to lowercase, removing diacritics (accents), and capitalizing words. The final result 
    /// is returned as a string with underscores replaced by single underscores and multiple underscores reduced to one.
    ///
    /// <para>
    /// The method performs the following steps:
    /// <list type="bullet">
    ///     <item>Replaces punctuation and special characters with spaces.</item>
    ///     <item>Replaces underscores with spaces.</item>
    ///     <item>Converts the string to lowercase.</item>
    ///     <item>Removes diacritics by normalizing to FormD and filtering out non-spacing marks.</item>
    ///     <item>Normalizes the result to FormC and removes any remaining punctuation.</item>
    ///     <item>Replaces spaces with underscores, reduces multiple underscores to a single underscore, and capitalizes words.</item>
    /// </list>
    /// </para>
    /// 
    /// <param name="input">The string to be normalized.</param>
    /// <returns>A normalized string where punctuation is removed, diacritics are stripped, and words are capitalized.</returns>
    /// <exception cref="ApplicationException">Thrown when an error occurs during normalization.</exception>
    /// </summary>
    public override string Format(string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replace all punctuation and special characters with spaces
            input = Regex.Replace(input, @"[\p{P}\p{S}]", " ");
            
            // Replace underscores with spaces
            input = input.Replace('_', ' '); 
            //input = input.Replace('$',' ');
            
            // Convert to lowercase
            input = input.ToLower();

            // Remove diacritics (accents)
            var stFormD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(); 
            // Iterate over each character in the normalized string
            foreach (var c in stFormD) 
            { 
                var uc = CharUnicodeInfo.GetUnicodeCategory(c); 
                // If the character is not a non-spacing mark, append it to the result
                if (uc != UnicodeCategory.NonSpacingMark) 
                { 
                    // Special case for German sharp s (ß) -> replace with "ss"
                    if (c == 'ß') sb.Append("ss");
                    else sb.Append(c);
                }
            }
            
            // Normalize result to FormC (canonical composition) and return
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            // Build the string without punctuation
            var result = new StringBuilder(); 
            foreach (var c in noDiacritics) 
            { 
                if (char.IsLetterOrDigit(c) || c == ' ' || !char.IsPunctuation(c)) 
                { 
                    result.Append(c);
                }
            }

            // Replace spaces with underscores
            var withUnderscores = result.ToString().Replace(" ", "_");

            // Replace multiple underscores with a single underscore
            var singleUnderscore = Regex.Replace(withUnderscores, @"_+", "_");

            // Split the words, capitalize each one, and join without underscore
            string[] words = singleUnderscore.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < words.Length; i++)
            {
                words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
            }

            return string.Join("", words);
        }
        catch (Exception ex)
        { 
            throw new ApplicationException($"An error occurred while normalizing '{input}': {ex.Message}", ex);
        }
    }

    
    /// <summary>
    /// This method is not implemented in <see cref="FormatterNormalize"/>. It logs a message indicating that
    /// the translation functionality is not supported for this formatter.
    ///
    /// <para>
    /// Since <see cref="FormatterNormalize"/> focuses on normalizing strings and does not support translation, this method
    /// is provided as a placeholder and returns an empty string.
    /// </para>
    /// 
    /// <param name="input">The string to be translated (not used in this implementation).</param>
    /// <returns>An empty string indicating that translation is not supported.</returns>
    /// </summary>
    public override string Translate(string input)
    {
        App.ConsoleAndLogWrite("Translate method is not implemented in FormatterNormalize");
        return string.Empty;
    }
}




