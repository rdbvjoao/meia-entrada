using System.Net;
using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Tests.Integration;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;

namespace MeiaEntrada.Tests.Web;

public class IdDaRotaContratoWebCoberturaTests
{
    [Fact]
    public async Task EventoEditaIdDaRotaMesmoComIdExtraNoFormulario()
    {
        using var f = new SqliteFixture(); var (alvo, _) = f.Oferta(); var (outro, _) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var fields = EventosContratoWebTests.Campos("Nome alterado"); fields["Id"] = outro.ToString();
        var url = $"/eventos/{alvo}/editar/";
        var response = await AntiforgeryClient.Post(c, url, url, fields);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal($"/eventos/{alvo}/", response.Headers.Location!.ToString());
        using var check = f.CreateDbContext();
        Assert.Equal("Nome alterado", check.Eventos.Find(alvo)!.Nome);
        Assert.Equal("Sessão original", check.Eventos.Find(outro)!.Nome);
    }

    [Theory]
    [InlineData("eventos")]
    [InlineData("ingressos")]
    public async Task ExclusaoUsaIdDaRotaEPreservaOutroRegistro(string recurso)
    {
        using var f = new SqliteFixture(); var (e1, i1) = f.Oferta(); var (e2, i2) = f.Oferta();
        if (recurso == "eventos")
        {
            var service = new IngressoService(new SqliteWriteTransaction(f));
            service.Excluir(i1); service.Excluir(i2);
        }
        var alvo = recurso == "eventos" ? e1 : i1;
        var outro = recurso == "eventos" ? e2 : i2;
        using var app = new WebAppFactory(f); using var c = app.Client();
        var url = $"/{recurso}/{alvo}/excluir/";
        Assert.Equal(HttpStatusCode.Redirect, (await AntiforgeryClient.Post(c, url, url, new() { ["Confirmar"] = "sim", ["Id"] = outro.ToString() })).StatusCode);
        using var check = f.CreateDbContext();
        if (recurso == "eventos") { Assert.Null(check.Eventos.Find(alvo)); Assert.NotNull(check.Eventos.Find(outro)); }
        else { Assert.Null(check.Ingressos.Find(alvo)); Assert.NotNull(check.Ingressos.Find(outro)); }
    }

    [Fact]
    public async Task CancelamentoUsaIdDaRotaSemRestituirOutraVenda()
    {
        using var f = new SqliteFixture(); var alvo = f.VendaPreparada(); var outro = f.VendaPreparada();
        EstadoVenda originalAlvo, originalOutro;
        using (var read = f.CreateDbContext()) { originalAlvo = EstadoVenda.De(read.Vendas.Find(alvo)!); originalOutro = EstadoVenda.De(read.Vendas.Find(outro)!); }
        using var app = new WebAppFactory(f); using var c = app.Client();
        var url = $"/vendas/{alvo}/cancelar/";
        var response = await AntiforgeryClient.Post(c, url, url, new() { ["Confirmar"] = "sim", ["Id"] = outro.ToString() });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); Assert.Equal($"/vendas/{alvo}/", response.Headers.Location!.ToString());
        using var check = f.CreateDbContext();
        Assert.Equal(originalAlvo, EstadoVenda.De(check.Vendas.Find(alvo)!)); Assert.Equal(originalOutro, EstadoVenda.De(check.Vendas.Find(outro)!));
        Assert.Equal(SituacaoVenda.Cancelada, check.Vendas.Find(alvo)!.Situacao); Assert.NotNull(check.Vendas.Find(alvo)!.CanceladaEmUtc);
        Assert.Equal(SituacaoVenda.Registrada, check.Vendas.Find(outro)!.Situacao); Assert.Null(check.Vendas.Find(outro)!.CanceladaEmUtc);
        Assert.Equal((10L, 2L), (check.Ingressos.Find(originalAlvo.IngressoId)!.QuantidadeDisponivel, check.Ingressos.Find(originalAlvo.IngressoId)!.Versao));
        Assert.Equal((7L, 1L), (check.Ingressos.Find(originalOutro.IngressoId)!.QuantidadeDisponivel, check.Ingressos.Find(originalOutro.IngressoId)!.Versao));
    }
}
