using System.Net;
using MeiaEntrada.Tests.Fixtures;
namespace MeiaEntrada.Tests.Web;
public class VendasContratoWebTests
{
    [Fact] public async Task ConferirConfirmarSemConfiarNoCliente()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();using var app=new WebAppFactory(db);using var c=app.Client();
        var summary=await AntiforgeryClient.Post(c,"/vendas/nova/?evento="+e,"/vendas/conferir/",new(){["EventoId"]=e.ToString(),["IngressoId"]=i.ToString(),["Quantidade"]="3"});
        Assert.Equal(HttpStatusCode.OK,summary.StatusCode);var html=await summary.Content.ReadAsStringAsync();Assert.Contains("120,00",html);
        var fields=new Dictionary<string,string>{["Conferencia"]=AntiforgeryClient.Campo(html,"Conferencia"),["Confirmar"]="sim",["ValorTotalCentavos"]="1",["__RequestVerificationToken"]=AntiforgeryClient.Campo(html,"__RequestVerificationToken")};
        var result=await c.PostAsync("/vendas/",new FormUrlEncodedContent(fields));Assert.Equal(HttpStatusCode.Redirect,result.StatusCode);
        Assert.Contains("120,00",await c.GetStringAsync(result.Headers.Location));
        fields["Conferencia"]="fake";Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsync("/vendas/",new FormUrlEncodedContent(fields))).StatusCode);
    }
}
