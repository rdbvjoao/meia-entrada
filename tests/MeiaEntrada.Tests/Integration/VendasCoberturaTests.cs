using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;

namespace MeiaEntrada.Tests.Integration;

public class VendasCoberturaTests
{
    [Theory]
    [InlineData("evento", 400)]
    [InlineData("ingresso", 404)]
    [InlineData("incompativel", 400)]
    public void SelecaoInvalidaNaoReservaNemGrava(string caso, int status)
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta();
        var (outro, _) = f.Oferta();
        var vendas = new VendasFixture(f);
        Assert.Equal(status, Assert.Throws<RegraException>(() => vendas.Conferir(
            caso == "evento" ? 99999 : caso == "incompativel" ? outro : e,
            caso == "ingresso" ? 99999 : i)).Status);
        using var check = f.CreateDbContext();
        Assert.Empty(check.Vendas);
        Assert.All(check.Ingressos, ingresso => Assert.Equal((10L, 1L), (ingresso.QuantidadeDisponivel, ingresso.Versao)));
    }

    [Theory]
    [InlineData("total")]
    [InlineData("versao")]
    public void OverflowNaoProduzVendaNemBaixa(string limite)
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta(2, limite == "total" ? long.MaxValue : 4000);
        if (limite == "versao")
        {
            using var edit = f.CreateDbContext();
            edit.Ingressos.Find(i)!.Versao = long.MaxValue;
            edit.SaveChanges();
        }
        var vendas = new VendasFixture(f);
        Assert.Equal(400, Assert.Throws<RegraException>(() => vendas.Service.Registrar(vendas.Conferir(e, i, "2").Conferencia)).Status);
        using var check = f.CreateDbContext();
        Assert.Empty(check.Vendas);
        Assert.Equal(2, check.Ingressos.Find(i)!.QuantidadeDisponivel);
        Assert.Equal(limite == "versao" ? long.MaxValue : 1, check.Ingressos.Find(i)!.Versao);
    }

    [Theory]
    [InlineData("evento")]
    [InlineData("tipo")]
    [InlineData("saldo")]
    [InlineData("exclusao")]
    public void ConfirmacaoRevalidaEstadoDepoisDaConferencia(string mudanca)
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta();
        var (outro, _) = f.Oferta();
        var vendas = new VendasFixture(f);
        var token = vendas.Conferir(e, i, "3").Conferencia;
        using (var edit = f.CreateDbContext())
        {
            var ingresso = edit.Ingressos.Find(i)!;
            if (mudanca == "evento") { ingresso.EventoId = outro; ingresso.Tipo = TipoIngresso.MeiaEntrada; }
            if (mudanca == "tipo") ingresso.Tipo = TipoIngresso.MeiaEntrada;
            if (mudanca == "saldo") ingresso.QuantidadeDisponivel = 2;
            if (mudanca == "exclusao") edit.Remove(ingresso);
            edit.SaveChanges();
        }
        Assert.Equal(mudanca == "exclusao" ? 404 : 409, Assert.Throws<RegraException>(() => vendas.Service.Registrar(token)).Status);
        using var check = f.CreateDbContext();
        Assert.Empty(check.Vendas);
        if (mudanca != "exclusao") Assert.Equal(mudanca == "saldo" ? 2 : 10, check.Ingressos.Find(i)!.QuantidadeDisponivel);
    }
}
