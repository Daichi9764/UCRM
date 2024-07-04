using System.Globalization;
using System.Text.RegularExpressions;
using DeepL;

namespace KNXBoostDesktop;

public class FormatterTranslate : Formatter
{
    private string _destLanguage = App.DisplayElements?.SettingsWindow?.TranslationLang ?? string.Empty; 
    
    public override string Format(string input)
    {
        var translated = Task.Run(() => GetTranslatedStringAsync(input)).Result;
        
        // Remplacer toute la ponctuation et les signes spéciaux par des espaces
        translated = Regex.Replace(translated, @"[\p{P}]", " ");
        
        // Mettre en minuscules
        translated = translated.ToLower();
        
        // Remplacer les espaces par des underscores
        translated = translated.Replace(" ", "_");
        
        // Séparer les mots et capitaliser chacun d'eux, puis les joindre sans underscore
        string[] words = translated.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);
        }

        return string.Join("", words);
        
    }


    private async Task<string> GetTranslatedStringAsync(string input)
    {
        var authKey = App.DisplayElements?.SettingsWindow?.DecryptStringFromBytes(App.DisplayElements.SettingsWindow.DeeplKey);
        switch (_destLanguage)
        {
            case "PT" : _destLanguage = "pt-Pt";
                break;
            case "EN" : _destLanguage = "en-Gb";
                break;
        }
        var translator = new Translator(authKey ?? string.Empty); //call une exception ici
        return (await translator.TranslateTextAsync(input, "fr", _destLanguage)).Text;
    }

}