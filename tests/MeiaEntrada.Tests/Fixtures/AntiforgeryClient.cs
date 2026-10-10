using System.Net;
using System.Text.RegularExpressions;
namespace MeiaEntrada.Tests.Fixtures;
public static class AntiforgeryClient
{
    public static string Campo(string html, string nome)
    {
        var tag = Regex.Match(html, "<input[^>]*name=\"" + Regex.Escape(nome) + "\"[^>]*>").Value;
        return WebUtility.HtmlDecode(Regex.Match(tag, "value=\"([^\"]*)\"").Groups[1].Value);
    }
    public static async Task<HttpResponseMessage> Post(HttpClient client, string get, string post, Dictionary<string,string> fields)
    {
        var html = await client.GetStringAsync(get);
        fields["__RequestVerificationToken"] = Campo(html, "__RequestVerificationToken");
        Assert.NotEmpty(fields["__RequestVerificationToken"]);
        return await client.PostAsync(post, new FormUrlEncodedContent(fields));
    }
}
