using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MeiaEntrada.Tests.Integration;

public class CancelamentosCoberturaTests
{
    [Fact]
    public void VendaGratuitaCancelaERepeticaoNaoEnviaEscrita()
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta(10, 0);
        var vendas = new VendasFixture(f);
        var id = vendas.Service.Registrar(vendas.Conferir(e, i, "3").Conferencia);
        EstadoVenda original;
        using (var read = f.CreateDbContext()) original = EstadoVenda.De(read.Vendas.Find(id)!);
        Assert.Equal(0, original.Total);
        Assert.True(vendas.Service.Cancelar(id));
        DateTime? instante;
        using (var read = f.CreateDbContext()) instante = read.Vendas.Find(id)!.CanceladaEmUtc;
        Assert.NotNull(instante);
        f.Interceptors = [new ProibirMutacoesInterceptor()];
        Assert.False(vendas.Service.Cancelar(id));
        Assert.False(vendas.Service.Cancelar(id));
        using var check = f.CreateDbContext();
        var venda = check.Vendas.Find(id)!;
        Assert.Equal(original, EstadoVenda.De(venda));
        Assert.Equal(instante, venda.CanceladaEmUtc);
        Assert.Equal(SituacaoVenda.Cancelada, venda.Situacao);
        Assert.Equal((10L, 3L), (check.Ingressos.Find(i)!.QuantidadeDisponivel, check.Ingressos.Find(i)!.Versao));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OverflowDeSaldoOuVersaoPreservaEstadoIntegral(bool versao)
    {
        using var f = new SqliteFixture();
        var id = f.VendaPreparada(saldo: versao ? 7 : long.MaxValue);
        EstadoVenda original;
        using (var edit = f.CreateDbContext())
        {
            if (versao) edit.Ingressos.Single().Versao = long.MaxValue;
            edit.SaveChanges();
            original = EstadoVenda.De(edit.Vendas.Find(id)!);
        }
        var vendas = new VendasFixture(f);
        Assert.Equal(400, Assert.Throws<RegraException>(() => vendas.Service.Cancelar(id)).Status);
        using var check = f.CreateDbContext();
        var venda = check.Vendas.Find(id)!;
        Assert.Equal(original, EstadoVenda.De(venda));
        Assert.Null(venda.CanceladaEmUtc);
        Assert.Equal(SituacaoVenda.Registrada, venda.Situacao);
        Assert.Equal(versao ? 7 : long.MaxValue, check.Ingressos.Single().QuantidadeDisponivel);
        Assert.Equal(versao ? long.MaxValue : 1, check.Ingressos.Single().Versao);
    }

    private sealed class ProibirMutacoesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Assert.False(eventData.Context!.ChangeTracker.HasChanges());
            return result;
        }
    }
}
