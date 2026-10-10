using System.Net;
using MeiaEntrada.Tests.Fixtures;
namespace MeiaEntrada.Tests.Web;
public class CancelamentosContratoWebTests
{
    [Fact]public async Task ConfirmacaoRepeticaoEAntiforgery()
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada();using var app=new WebAppFactory(db);using var c=app.Client();var url=$"/vendas/{id}/cancelar/";
        Assert.Equal(HttpStatusCode.OK,(await c.GetAsync(url)).StatusCode);
        using(var check=db.CreateDbContext())Assert.Equal(7,check.Ingressos.Single().QuantidadeDisponivel);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsync(url,new FormUrlEncodedContent(new Dictionary<string,string>{["Confirmar"]="sim"}))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await AntiforgeryClient.Post(c,url,url,new())).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,url,url,new(){["Confirmar"]="sim"})).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,url,url,new(){["Confirmar"]="sim"})).StatusCode);
        var html=await c.GetStringAsync($"/vendas/{id}/");Assert.Contains("Venda j",html);Assert.Contains("cancelada",html);
        using var final=db.CreateDbContext();Assert.Equal(10,final.Ingressos.Single().QuantidadeDisponivel);
    }
}
