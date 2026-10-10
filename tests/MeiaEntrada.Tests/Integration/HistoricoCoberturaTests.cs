using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Data.Queries;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;

namespace MeiaEntrada.Tests.Integration;

public class HistoricoCoberturaTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TodosOsCamposOriginaisSobrevivemAEdicoesEBloqueios(bool cancelar)
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta();
        var vendas = new VendasFixture(f);
        var id = vendas.Service.Registrar(vendas.Conferir(e, i, "2").Conferencia);
        var queries = new VendaQueries(f);
        var original = EstadoVenda.De(queries.Obter(id));
        Assert.Equal(8000, original.Total);
        var write = new SqliteWriteTransaction(f);
        var eventos = new EventoService(write);
        var ingressos = new IngressoService(write);
        eventos.Salvar(e, new() { Nome = "Nome novo", Descricao = "D", Local = "L", Data = "2026-01-01", Horario = "10:00" });
        ingressos.Salvar(i, IngressosTests.Form(e, "inteira", "50", "20", "2"));
        if (cancelar) vendas.Service.Cancelar(id);
        Assert.Equal(409, Assert.Throws<RegraException>(() => eventos.Excluir(e)).Status);
        Assert.Equal(409, Assert.Throws<RegraException>(() => ingressos.Excluir(i)).Status);
        var detalhe = queries.Obter(id);
        Assert.Equal(original, EstadoVenda.De(detalhe));
        Assert.Equal(original, EstadoVenda.De(Assert.Single(queries.Listar(null).Itens)));
        Assert.Equal(cancelar ? SituacaoVenda.Cancelada : SituacaoVenda.Registrada, detalhe.Situacao);
        Assert.Equal(DateTimeKind.Utc, detalhe.RegistradaEmUtc.Kind);
        if (cancelar) Assert.Equal(DateTimeKind.Utc, detalhe.CanceladaEmUtc!.Value.Kind);
        using var check = f.CreateDbContext();
        Assert.Equal(e, check.Ingressos.Find(original.IngressoId)!.EventoId);
        Assert.NotNull(check.Eventos.Find(e));
    }

    [Fact]
    public void HistoricoOrdenaPorInstanteAntesDoIdESemPerderCamposEntrePaginas()
    {
        using var f = new SqliteFixture();
        var esperado = new List<EstadoVenda>();
        for (var n = 0; n < 26; n++)
        {
            var id = f.VendaPreparada(quantidade: n + 1, cancelada: n % 2 == 0);
            using var edit = f.CreateDbContext();
            var venda = edit.Vendas.Find(id)!;
            venda.RegistradaEmUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddMinutes(n % 3);
            edit.SaveChanges();
            esperado.Add(EstadoVenda.De(venda));
        }
        var queries = new VendaQueries(f);
        var primeira = queries.Listar(null);
        var segunda = queries.Listar("2");
        Assert.Equal(25, primeira.Itens.Count);
        Assert.Single(segunda.Itens);
        Assert.Equal(esperado.OrderByDescending(x => x.RegistradaEmUtc).ThenByDescending(x => x.Id),
            primeira.Itens.Concat(segunda.Itens).Select(EstadoVenda.De));
    }
}
