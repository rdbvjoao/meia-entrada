using System.Diagnostics;
using System.Net;
using MeiaEntrada.Tests.Fixtures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Tests.Integration;
public class ContencaoTests
{
    [Fact]public async Task EscritorOcupadoRetorna503SemMutacao()
    {
        using var db=new SqliteFixture();db.Oferta();using var app=new WebAppFactory(db);using var c=app.Client();c.Timeout=TimeSpan.FromSeconds(60);
        var html=await c.GetStringAsync("/eventos/novo/");var fields=MeiaEntrada.Tests.Web.EventosContratoWebTests.Campos();fields["__RequestVerificationToken"]=AntiforgeryClient.Campo(html,"__RequestVerificationToken");
        using var blocker=db.CreateDbContext();blocker.Database.OpenConnection();
        using var transaction=((SqliteConnection)blocker.Database.GetDbConnection()).BeginTransaction(deferred:false);
        var watch=Stopwatch.StartNew();
        try {
            var response=await Task.Run(()=>c.PostAsync("/eventos/novo/",new FormUrlEncodedContent(fields))).WaitAsync(TimeSpan.FromSeconds(50));
            Assert.Equal(HttpStatusCode.ServiceUnavailable,response.StatusCode);Assert.Contains("Sistema ocupado; tente novamente",WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));Assert.InRange(watch.Elapsed.TotalSeconds,30,48);
        } finally { transaction.Rollback(); }
        using (var final=db.CreateDbContext()) { Assert.Single(final.Eventos);Assert.Equal(10,final.Ingressos.Single().QuantidadeDisponivel);Assert.Equal(1,final.Ingressos.Single().Versao);Assert.Empty(final.Vendas); }
        var retry = await AntiforgeryClient.Post(c,"/eventos/novo/","/eventos/novo/",MeiaEntrada.Tests.Web.EventosContratoWebTests.Campos());
        Assert.Equal(HttpStatusCode.Redirect,retry.StatusCode);
        using var recovered=db.CreateDbContext();Assert.Equal(2,recovered.Eventos.Count());
    }
}
