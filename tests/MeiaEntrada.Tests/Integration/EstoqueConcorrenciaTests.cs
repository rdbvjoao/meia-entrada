using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Services;
namespace MeiaEntrada.Tests.Integration;
public class EstoqueConcorrenciaTests
{
    [Fact] public async Task VinteCancelamentosRestituemUmaVez()
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada();var v=new VendasFixture(db);
        EstadoVenda original;
        using(var read=db.CreateDbContext())original=EstadoVenda.De(read.Vendas.Find(id)!);
        var results=await SqliteFixture.Concorrer(20,_=>v.Service.Cancelar(id));Assert.Single(results,x=>x);
        Assert.Equal(19,results.Count(x=>!x));
        using var final=db.CreateDbContext();Assert.Equal(10,final.Ingressos.Single().QuantidadeDisponivel);Assert.Equal(2,final.Ingressos.Single().Versao);Assert.NotNull(final.Vendas.Find(id)!.CanceladaEmUtc);
        Assert.Equal(original,EstadoVenda.De(final.Vendas.Find(id)!));
        Assert.Equal(MeiaEntrada.Web.Models.SituacaoVenda.Cancelada,final.Vendas.Find(id)!.Situacao);
        var instante=final.Vendas.Find(id)!.CanceladaEmUtc;
        Assert.False(v.Service.Cancelar(id));
        using var repetido=db.CreateDbContext();
        Assert.Equal(instante,repetido.Vendas.Find(id)!.CanceladaEmUtc);
        Assert.Equal(original,EstadoVenda.De(repetido.Vendas.Find(id)!));
        Assert.Equal((10L,2L),(repetido.Ingressos.Single().QuantidadeDisponivel,repetido.Ingressos.Single().Versao));
    }
    [Fact] public async Task VendaECancelamentoSomamCorretamente()
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada();long e,i;using(var read=db.CreateDbContext()){var ingresso=read.Ingressos.Single();e=ingresso.EventoId;i=ingresso.Id;}
        var v=new VendasFixture(db);var token=v.Conferir(e,i,"2").Conferencia;
        await SqliteFixture.Concorrer(2,n=>{if(n==0)v.Service.Cancelar(id);else v.Service.Registrar(token);return true;});
        using var final=db.CreateDbContext();Assert.Equal(8,final.Ingressos.Single().QuantidadeDisponivel);Assert.Equal(2,final.Vendas.Count());
        Assert.Equal(3,final.Ingressos.Single().Versao);
        Assert.Equal(MeiaEntrada.Web.Models.SituacaoVenda.Cancelada,final.Vendas.Find(id)!.Situacao);
        var nova=final.Vendas.Single(x=>x.Id!=id);
        Assert.Equal(MeiaEntrada.Web.Models.SituacaoVenda.Registrada,nova.Situacao);
        Assert.Equal((i,2L,4000L,8000L),(nova.IngressoId,nova.Quantidade,nova.PrecoUnitarioCentavos,nova.ValorTotalCentavos));
    }
    [Fact] public void SaldoRetornaMasVersaoAntigaContinuaInvalida()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();var v=new VendasFixture(db);var venda=v.Service.Registrar(v.Conferir(e,i).Conferencia);v.Service.Cancelar(venda);
        var s=new IngressoService(new MeiaEntrada.Web.Data.SqliteWriteTransaction(db));
        Assert.Equal(409,Assert.Throws<RegraException>(()=>s.Salvar(i,IngressosTests.Form(e,"inteira","40","10","1"))).Status);
    }
    [Theory][InlineData(20,10,1,10,0)][InlineData(2,5,4,1,1)]
    public async Task VendasSimultaneas(int workers,long saldo,long quantidade,int sucessos,long saldoFinal)
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta(saldo);var v=new VendasFixture(db);var token=v.Conferir(e,i,quantidade.ToString()).Conferencia;
        var results=await SqliteFixture.Concorrer(workers,_=>{try{v.Service.Registrar(token);return 200;}catch(RegraException ex){Assert.Contains("Saldo insuficiente",ex.Message);return ex.Status;}});
        Assert.Equal(sucessos,results.Count(x=>x==200));Assert.Equal(workers-sucessos,results.Count(x=>x==409));
        using var final=db.CreateDbContext();Assert.Equal(sucessos,final.Vendas.Count());Assert.Equal(saldoFinal,final.Ingressos.Find(i)!.QuantidadeDisponivel);
    }
}

