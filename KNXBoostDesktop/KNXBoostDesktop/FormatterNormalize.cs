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

                // Remplacer toute la ponctuation et les signes spéciaux par des espaces
                input = Regex.Replace(input, @"[\p{P}\p{S}]", " ");

                input = input.Replace('_', ' ');
                //input = input.Replace('$',' ');

                // Mettre en minuscules
                input = input.ToLower();

                // Enlever les accents
                string stFormD = input.Normalize(NormalizationForm.FormD);
                StringBuilder sb = new StringBuilder();
                foreach (char c in stFormD)
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (uc != UnicodeCategory.NonSpacingMark)
                    {
                        if (c == 'ß') sb.Append("ss");
                        else sb.Append(c);
                    }
                }

                string noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

                // Construire la chaîne sans ponctuation
                StringBuilder result = new StringBuilder();
                foreach (char c in noDiacritics)
                {
                    if (char.IsLetterOrDigit(c) || c == ' ' || !char.IsPunctuation(c))
                    {
                        result.Append(c);
                    }
                }

                // Remplacer les espaces par des underscores
                string withUnderscores = result.ToString().Replace(" ", "_");

                // Remplacer les doubles underscores par un seul
                string singleUnderscore = Regex.Replace(withUnderscores, @"_+", "_");

                // Séparer les mots et capitaliser chacun d'eux, puis les joindre sans underscore
                string[] words = singleUnderscore.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
                }

                return string.Join("", words);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Il y a eu une erreur lors de la normalisation de {input}.", ex);
            }
        }
    }




