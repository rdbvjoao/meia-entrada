using System.Net;
using MeiaEntrada.Tests.Fixtures;
namespace MeiaEntrada.Tests.Web;
public class HistoricoContratoWebTests
{
    [Fact]public async Task VazioHistoricoEOperacoesProibidas()
    {
        using var db=new SqliteFixture();using var app=new WebAppFactory(db);using var c=app.Client();
        Assert.Contains("Nenhuma venda",await c.GetStringAsync("/vendas/"));var id=db.VendaPreparada(cancelada:true);
        var html=await c.GetStringAsync("/vendas/");Assert.Contains("120,00",html);Assert.Contains("Cancelada",html);
        foreach(var page in new[]{"0","abc","2"})Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync("/vendas/?page="+page)).StatusCode);
        foreach(var op in new[]{"editar","excluir","reativar"})Assert.Contains((await c.PostAsync($"/vendas/{id}/{op}/",new StringContent(""))).StatusCode,new[]{HttpStatusCode.NotFound,HttpStatusCode.MethodNotAllowed});
        using var final=db.CreateDbContext();Assert.Single(final.Vendas);
    }
}
