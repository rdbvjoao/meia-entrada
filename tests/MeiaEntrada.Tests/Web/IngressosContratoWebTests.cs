using System.Net;
using MeiaEntrada.Tests.Fixtures;
namespace MeiaEntrada.Tests.Web;
public class IngressosContratoWebTests
{
    [Fact] public async Task CrudConflitoEConfirmacao()
    {
        using var f=new SqliteFixture();var(e,i)=f.Oferta();using var app=new WebAppFactory(f);using var c=app.Client();
        var fields=new Dictionary<string,string>{["EventoId"]=e.ToString(),["Tipo"]="meia_entrada",["Preco"]="20,00",["QuantidadeDisponivel"]="5"};
        var novo=await AntiforgeryClient.Post(c,"/ingressos/novo/","/ingressos/novo/",fields);Assert.Equal(HttpStatusCode.Redirect,novo.StatusCode);
        var url=novo.Headers.Location!.ToString();Assert.Contains("20,00",await c.GetStringAsync(url));
        Assert.Equal(HttpStatusCode.Conflict,(await AntiforgeryClient.Post(c,"/ingressos/novo/","/ingressos/novo/",fields)).StatusCode);
        fields["Versao"]="1";fields["QuantidadeDisponivel"]="8";Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,url+"editar/",url+"editar/",fields)).StatusCode);
        var old=await AntiforgeryClient.Post(c,url+"editar/",url+"editar/",fields);Assert.Equal(HttpStatusCode.Conflict,old.StatusCode);Assert.Contains("Saldo atual",await old.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest,(await AntiforgeryClient.Post(c,url+"excluir/",url+"excluir/",new())).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,url+"excluir/",url+"excluir/",new(){["Confirmar"]="sim"})).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync("/ingressos/?page=0")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync("/ingressos/?evento="+e)).StatusCode);
    }
}
