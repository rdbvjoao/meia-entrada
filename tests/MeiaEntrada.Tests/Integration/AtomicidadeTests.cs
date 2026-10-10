using MeiaEntrada.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace MeiaEntrada.Tests.Integration;
public class AtomicidadeTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public void CancelamentoFalhaSemRestituicaoParcial(bool commit)
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada();var v=new VendasFixture(db);
        db.Interceptors=commit?[new FalhaCommitInterceptor()]:[new FalhaPersistenciaInterceptor()];
        Assert.ThrowsAny<Exception>(()=>v.Service.Cancelar(id));db.Interceptors=[];
        using var final=db.CreateDbContext();Assert.Null(final.Vendas.Find(id)!.CanceladaEmUtc);Assert.Equal(MeiaEntrada.Web.Models.SituacaoVenda.Registrada,final.Vendas.Find(id)!.Situacao);Assert.Equal(7,final.Ingressos.Single().QuantidadeDisponivel);Assert.Equal(1,final.Ingressos.Single().Versao);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public void VendaFalhaSemBaixaParcial(bool commit)
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();var v=new VendasFixture(db);var token=v.Conferir(e,i).Conferencia;
        db.Interceptors=commit?[new FalhaCommitInterceptor()]:[new FalhaPersistenciaInterceptor()];
        Assert.ThrowsAny<Exception>(()=>v.Service.Registrar(token));db.Interceptors=[];
        using var final=db.CreateDbContext();Assert.Empty(final.Vendas);Assert.Equal(10,final.Ingressos.Find(i)!.QuantidadeDisponivel);Assert.Equal(1,final.Ingressos.Find(i)!.Versao);
    }
}
