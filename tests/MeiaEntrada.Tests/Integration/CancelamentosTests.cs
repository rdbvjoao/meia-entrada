using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;
namespace MeiaEntrada.Tests.Integration;
public class CancelamentosTests
{
    [Fact]public void CancelaUmaVezMantendoHistorico()
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada();var v=new VendasFixture(db);
        Assert.True(v.Service.Cancelar(id));DateTime? instante;long versao;
        using(var check=db.CreateDbContext()){var venda=check.Vendas.Find(id)!;instante=venda.CanceladaEmUtc;versao=check.Ingressos.Single().Versao;Assert.Equal(SituacaoVenda.Cancelada,venda.Situacao);Assert.Equal(10,check.Ingressos.Single().QuantidadeDisponivel);}
        Assert.False(v.Service.Cancelar(id));using var final=db.CreateDbContext();Assert.Equal(instante,final.Vendas.Find(id)!.CanceladaEmUtc);Assert.Equal(versao,final.Ingressos.Single().Versao);Assert.Equal(12000,final.Vendas.Find(id)!.ValorTotalCentavos);Assert.Equal(10,final.Ingressos.Single().QuantidadeDisponivel);
    }
    [Fact]public void AjusteManualENaoExiste()
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada(saldo:20);var v=new VendasFixture(db);
        Assert.Equal(404,Assert.Throws<RegraException>(()=>v.Service.Cancelar(999)).Status);v.Service.Cancelar(id);
        using var final=db.CreateDbContext();Assert.Equal(23,final.Ingressos.Single().QuantidadeDisponivel);
    }
    [Fact]public void OverflowNaoAlteraSituacao()
    {
        using var db=new SqliteFixture();var id=db.VendaPreparada(saldo:long.MaxValue);var v=new VendasFixture(db);Assert.Throws<RegraException>(()=>v.Service.Cancelar(id));
        using var final=db.CreateDbContext();Assert.Equal(SituacaoVenda.Registrada,final.Vendas.Find(id)!.Situacao);Assert.Null(final.Vendas.Find(id)!.CanceladaEmUtc);
    }
}
