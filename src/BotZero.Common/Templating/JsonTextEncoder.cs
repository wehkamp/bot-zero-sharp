using HandlebarsDotNet;
using System.Text;

namespace BotZero.Common.Templating;

public class JsonTextEncoder : ITextEncoder
{
    public void Encode(StringBuilder text, TextWriter target)
    {
        Encode(text.ToString(), target);
    }

    public void Encode(string text, TextWriter target)
    {
        if (text == null || text == "") return;
        text = System.Web.HttpUtility.JavaScriptStringEncode(text);
        target.Write(text);
    }

    public void Encode<T>(T text, TextWriter target) where T : IEnumerator<char>
    {
        var str = text?.ToString();
        if (str == null) return;

        Encode(str, target);
    }
}