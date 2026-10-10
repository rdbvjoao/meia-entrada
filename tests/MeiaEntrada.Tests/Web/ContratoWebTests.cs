using System.Net;
using MeiaEntrada.Tests.Fixtures;
namespace MeiaEntrada.Tests.Web;
public class ContratoWebTests
{
    [Fact]
    public async Task FalhaInesperadaProductionNaoExpoeDetalhesNemPersiste()
    {
        using var db = new SqliteFixture();
        using var app = new WebAppFactory(db);
        using var client = app.Client();
        var html = await client.GetStringAsync("/eventos/novo/");
        var fields = EventosContratoWebTests.Campos();
        fields["__RequestVerificationToken"] = AntiforgeryClient.Campo(html, "__RequestVerificationToken");
        db.Interceptors = [new FalhaSensivelInterceptor(db.DirectoryPath)];
        var response = await client.PostAsync("/eventos/novo/", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var error = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        foreach (var secret in new[] { db.DirectoryPath, "SELECT", "segredo-teste", "InvalidOperationException", "StackTrace", "FalhaSensivelInterceptor" })
            Assert.DoesNotContain(secret, error, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/eventos/", error);
        db.Interceptors = [];
        using var final = db.CreateDbContext();
        Assert.Empty(final.Eventos);
    }

    private sealed class FalhaSensivelInterceptor(string path) : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        public override int SavedChanges(Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesCompletedEventData eventData, int result)
            => throw new InvalidOperationException($"SELECT * FROM Eventos; {path}; segredo-teste");
    }
    [Fact] public async Task NavegacaoSegurancaEErros()
    {
        using var db=new SqliteFixture();var venda=db.VendaPreparada();using var app=new WebAppFactory(db);using var c=app.Client();
        var root=await c.GetAsync("/");Assert.Equal(HttpStatusCode.Redirect,root.StatusCode);Assert.Equal("/eventos/",root.Headers.Location!.ToString());
        foreach(var url in new[]{"/eventos/","/ingressos/","/vendas/nova/","/vendas/","/eventos/novo/","/ingressos/novo/"}){
            var html=await c.GetStringAsync(url);Assert.Contains("lang=\"pt-BR\"",html);Assert.DoesNotContain("cdn.",html);
            foreach(var nav in new[]{"/eventos/","/ingressos/","/vendas/","/vendas/nova/"})Assert.Contains(nav,html);
        }
        foreach(var url in new[]{"/eventos/novo/","/ingressos/novo/","/vendas/conferir/","/vendas/",$"/vendas/{venda}/cancelar/","/eventos/1/editar/","/eventos/1/excluir/","/ingressos/1/editar/","/ingressos/1/excluir/"}){
            var response=await c.PostAsync(url,new FormUrlEncodedContent(new Dictionary<string,string>()));Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        }
        foreach(var url in new[]{"/evento-inexistente/","/eventos/999/","/ingressos/999/","/vendas/999/","/vendas/999/cancelar/"}){
            var response=await c.GetAsync(url);Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
            var html=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("StackTrace",html);Assert.DoesNotContain(db.DirectoryPath,html);
        }
    }
    [Fact]public async Task CommitIncertoOrientaHistoricoSemRepetir()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();using var app=new WebAppFactory(db);using var c=app.Client();
        var summary=await AntiforgeryClient.Post(c,"/vendas/nova/?evento="+e,"/vendas/conferir/",new(){["EventoId"]=e.ToString(),["IngressoId"]=i.ToString(),["Quantidade"]="1"});
        var html=await summary.Content.ReadAsStringAsync();db.Interceptors=[new FalhaCommitInterceptor()];
        var response=await c.PostAsync("/vendas/",new FormUrlEncodedContent(new Dictionary<string,string>{["Conferencia"]=AntiforgeryClient.Campo(html,"Conferencia"),["Confirmar"]="sim",["__RequestVerificationToken"]=AntiforgeryClient.Campo(html,"__RequestVerificationToken")}));
        Assert.Equal(HttpStatusCode.InternalServerError,response.StatusCode);
        var error=WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());Assert.Contains("Consulte o histórico",error);Assert.DoesNotContain("Falha de teste",error);Assert.DoesNotContain("Sqlite",error);
        db.Interceptors=[];using var final=db.CreateDbContext();Assert.Empty(final.Vendas);Assert.Equal(10,final.Ingressos.Single().QuantidadeDisponivel);
    }
}
