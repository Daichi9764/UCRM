using System.Threading.Tasks;
using DeepL;

namespace KNXBoostDesktop;

public class FormatterTranslate : Formatter
{
    public string sourceLanguage  = "fr";
    public string destLanguage  = "en-Gb";




    /*public override string Format(string input)
    {


        Task<string> task = GetTranslatedStringAsync(input);
        task.Wait(); // Wait for the async task to complete

        // Get the result after waiting
        string result = task.Result;

        return result;
    }*/
    
    public override string Format(string input)
    {
        return Task.Run(() => GetTranslatedStringAsync(input)).Result;
    }
    
    

    public static async Task<string> GetTranslatedStringAsync(string input)
    {
        var authKey = "b2945c4a-90c9-40d6-ad6a-fa88854b7e42:fx";
        var translator = new Translator(authKey);
        return (await translator.TranslateTextAsync(input, "fr", "en-Gb")).Text;
    }

}

