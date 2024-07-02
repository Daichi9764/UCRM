using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KNXBoostDesktop;

public class FormatterNormalize : Formatter
{
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
            string stFormD = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(); 
            // Iterate over each character in the normalized string
            foreach (char c in stFormD) 
            { 
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c); 
                // If the character is not a non-spacing mark, append it to the result
                if (uc != UnicodeCategory.NonSpacingMark) 
                { 
                    // Special case for German sharp s (ß) -> replace with "ss"
                    if (c == 'ß') sb.Append("ss");
                    else sb.Append(c);
                }
            }
            
            // Normalize result to FormC (canonical composition) and return
            string noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            // Build the string without punctuation
            StringBuilder result = new StringBuilder(); 
            foreach (char c in noDiacritics) 
            { 
                if (char.IsLetterOrDigit(c) || c == ' ' || !char.IsPunctuation(c)) 
                { 
                    result.Append(c);
                }
            }

            // Replace spaces with underscores
            string withUnderscores = result.ToString().Replace(" ", "_");

            // Replace multiple underscores with a single underscore
            string singleUnderscore = Regex.Replace(withUnderscores, @"_+", "_");

            // Split the words, capitalize each one, and join without underscore
            string[] words = singleUnderscore.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
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
}




