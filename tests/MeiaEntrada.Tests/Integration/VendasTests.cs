using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Services;
namespace MeiaEntrada.Tests.Integration;
public class VendasTests
{
    [Fact] public void RegistraCopiaHistoricaEBaixa()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();var v=new VendasFixture(db);var resumo=v.Conferir(e,i,"3");
        using(var check=db.CreateDbContext()){Assert.Empty(check.Vendas);Assert.Equal(10,check.Ingressos.Find(i)!.QuantidadeDisponivel);}
        var id=v.Service.Registrar(resumo.Conferencia);using var final=db.CreateDbContext();var venda=final.Vendas.Find(id)!;
        Assert.Equal(12000,venda.ValorTotalCentavos);Assert.Equal("Sessão original",venda.EventoNome);Assert.Equal(7,final.Ingressos.Find(i)!.QuantidadeDisponivel);Assert.Equal(2,final.Ingressos.Find(i)!.Versao);
    }
    [Fact] public void PrecoAlteradoExigeReconfirmar()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();var v=new VendasFixture(db);var old=v.Conferir(e,i);
        using(var edit=db.CreateDbContext()){edit.Ingressos.Find(i)!.PrecoCentavos=5000;edit.SaveChanges();}
        var error=Assert.Throws<PrecoAlteradoException>(()=>v.Service.Registrar(old.Conferencia));Assert.Equal(5000,error.Conferencia.ValorTotalCentavos);
        using(var check=db.CreateDbContext())Assert.Empty(check.Vendas);
        v.Service.Registrar(error.Conferencia.Conferencia);
    }
    [Theory][InlineData("0")][InlineData("-1")][InlineData("1.5")][InlineData("")][InlineData("11")]
    public void RejeitaQuantidade(string q){using var db=new SqliteFixture();var(e,i)=db.Oferta();var v=new VendasFixture(db);Assert.Throws<RegraException>(()=>v.Conferir(e,i,q));using var final=db.CreateDbContext();Assert.Empty(final.Vendas);Assert.Equal(10,final.Ingressos.Find(i)!.QuantidadeDisponivel);}
    [Fact] public void GratisExatoETokenInvalido()
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta(2,0);var v=new VendasFixture(db);
        Assert.Throws<RegraException>(()=>v.Service.Registrar("adulterada"));
        v.Service.Registrar(v.Conferir(e,i,"2").Conferencia);Assert.Throws<RegraException>(()=>v.Conferir(e,i));
        using var final=db.CreateDbContext();Assert.Equal(0,final.Vendas.Single().ValorTotalCentavos);Assert.Equal(0,final.Ingressos.Find(i)!.QuantidadeDisponivel);
    }
}
