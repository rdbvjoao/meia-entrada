using System.Net;
using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Tests.Integration;
public class PersistenciaReinicioTests
{
    [Fact] public async Task ReiniciaHostPreservandoDadosEConferencia()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();string token;
        EstadoVenda original; DateTime? canceladaEm; string[] keys;
        using(var app=new WebAppFactory(db))using(var c=app.Client()){
            var response=await AntiforgeryClient.Post(c,"/vendas/nova/?evento="+e,"/vendas/conferir/",new(){["EventoId"]=e.ToString(),["IngressoId"]=i.ToString(),["Quantidade"]="3"});
            token=AntiforgeryClient.Campo(await response.Content.ReadAsStringAsync(),"Conferencia");
        }
        using(var app=new WebAppFactory(db))using(var c=app.Client()){
            var response=await AntiforgeryClient.Post(c,"/eventos/novo/","/vendas/",new(){["Conferencia"]=token,["Confirmar"]="sim"});Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
            var url=response.Headers.Location!.ToString();Assert.Contains("120,00",await c.GetStringAsync(url));
            using(var snapshot=db.CreateDbContext()) original=EstadoVenda.De(snapshot.Vendas.Single());
            Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,url+"cancelar/",url+"cancelar/",new(){["Confirmar"]="sim"})).StatusCode);
            using(var snapshot=db.CreateDbContext()) canceladaEm=snapshot.Vendas.Single().CanceladaEmUtc;
            Assert.NotNull(canceladaEm);
            var edit=MeiaEntrada.Tests.Web.EventosContratoWebTests.Campos("Nome atualizado");
            Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,$"/eventos/{e}/editar/",$"/eventos/{e}/editar/",edit)).StatusCode);
            Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,$"/ingressos/{i}/editar/",$"/ingressos/{i}/editar/",new(){["EventoId"]=e.ToString(),["Tipo"]="inteira",["Preco"]="50,00",["QuantidadeDisponivel"]="10",["Versao"]="3"})).StatusCode);
            var ticket=await AntiforgeryClient.Post(c,"/ingressos/novo/","/ingressos/novo/",new(){["EventoId"]=e.ToString(),["Tipo"]="meia_entrada",["Preco"]="20,00",["QuantidadeDisponivel"]="5"});
            Assert.Equal(HttpStatusCode.Redirect,ticket.StatusCode);
            var ticketUrl=ticket.Headers.Location!.ToString();
            Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,ticketUrl+"excluir/",ticketUrl+"excluir/",new(){["Confirmar"]="sim"})).StatusCode);
            var created=await AntiforgeryClient.Post(c,"/eventos/novo/","/eventos/novo/",MeiaEntrada.Tests.Web.EventosContratoWebTests.Campos("Temporário"));
            Assert.Equal(HttpStatusCode.Redirect,created.StatusCode);
            var temporary=created.Headers.Location!.ToString();
            Assert.Equal(HttpStatusCode.Redirect,(await AntiforgeryClient.Post(c,temporary+"excluir/",temporary+"excluir/",new(){["Confirmar"]="sim"})).StatusCode);
        }
        keys=Directory.GetFiles(Path.Combine(db.DirectoryPath,"keys"),"*.xml").Select(File.ReadAllText).ToArray();
        Assert.NotEmpty(keys);
        using(var app=new WebAppFactory(db))using(var c=app.Client())Assert.Contains("Cancelada",await c.GetStringAsync("/vendas/"));
        using var final=db.CreateDbContext();final.Database.Migrate();Assert.Equal(10,final.Ingressos.Single().QuantidadeDisponivel);Assert.Single(final.Vendas);Assert.Equal(12000,final.Vendas.Single().ValorTotalCentavos);
        Assert.Equal(original,EstadoVenda.De(final.Vendas.Single()));
        Assert.Equal(canceladaEm,final.Vendas.Single().CanceladaEmUtc);
        Assert.Equal(MeiaEntrada.Web.Models.SituacaoVenda.Cancelada,final.Vendas.Single().Situacao);
        Assert.Equal("Nome atualizado",final.Eventos.Single().Nome);
        Assert.Equal("Descrição válida",final.Eventos.Single().Descricao);
        Assert.Equal("Sala A",final.Eventos.Single().Local);
        Assert.Equal(new DateOnly(2026,12,1),final.Eventos.Single().Data);
        Assert.Equal(new TimeOnly(19,0),final.Eventos.Single().Horario);
        Assert.Equal(4,final.Ingressos.Single().Versao);
        Assert.Equal(5000,final.Ingressos.Single().PrecoCentavos);
        Assert.Equal(e,final.Ingressos.Single().EventoId);
        Assert.Equal(keys,Directory.GetFiles(Path.Combine(db.DirectoryPath,"keys"),"*.xml").Select(File.ReadAllText).ToArray());
        Assert.ThrowsAny<Exception>(()=>final.Database.ExecuteSqlRaw("UPDATE Ingressos SET QuantidadeDisponivel=-1"));
        Assert.ThrowsAny<Exception>(()=>final.Database.ExecuteSqlRaw("DELETE FROM Ingressos"));
    }
}
