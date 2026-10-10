using System.Net;
using System.Text.RegularExpressions;
using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Models;

namespace MeiaEntrada.Tests.Web;

public class HistoricoCancelamentosContratoWebCoberturaTests
{
    [Fact]
    public async Task HistoricoEDetalheExibemCamposOriginaisBRLEBrasilia()
    {
        using var f = new SqliteFixture(); var id = f.VendaPreparada(quantidade: 2, cancelada: true);
        using (var edit = f.CreateDbContext())
        {
            var v = edit.Vendas.Find(id)!;
            v.RegistradaEmUtc = new DateTime(2026, 1, 2, 1, 30, 0, DateTimeKind.Utc);
            v.CanceladaEmUtc = new DateTime(2026, 1, 2, 15, 45, 0, DateTimeKind.Utc);
            edit.Eventos.Single().Nome = "Nome atual"; edit.Ingressos.Single().PrecoCentavos = 9999;
            edit.SaveChanges();
        }
        using var app = new WebAppFactory(f); using var c = app.Client();
        foreach (var url in new[] { "/vendas/", $"/vendas/{id}/" })
        {
            var html = WebUtility.HtmlDecode(await c.GetStringAsync(url));
            foreach (var texto in new[] { "Sessão original", "Inteira", "80,00", "01/01/2026 22:30", "Cancelada" }) Assert.Contains(texto, html);
            Assert.DoesNotContain("Nome atual", html); Assert.DoesNotContain("99,99", html);
            Assert.Matches(@">\s*2\s*</(?:td|dd)>", html);
        }
        var detalhe = WebUtility.HtmlDecode(await c.GetStringAsync($"/vendas/{id}/"));
        Assert.Contains("40,00", detalhe); Assert.Contains("02/01/2026 12:45", detalhe);
        Assert.DoesNotContain($"href=\"/vendas/{id}/cancelar/\"", detalhe);
    }

    [Fact]
    public async Task HistoricoHttpPermiteAcessarTodosOs26Registros()
    {
        using var f = new SqliteFixture();
        var ids = Enumerable.Range(0, 26).Select(n => f.VendaPreparada(cancelada: n % 2 == 0)).ToArray();
        using (var edit = f.CreateDbContext())
        {
            foreach (var venda in edit.Vendas) venda.RegistradaEmUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            edit.SaveChanges();
        }
        using var app = new WebAppFactory(f); using var c = app.Client();
        static long[] Links(string html) => Regex.Matches(html, "href=\"/vendas/([0-9]+)/\"").Select(x => long.Parse(x.Groups[1].Value)).ToArray();
        var primeira = await c.GetStringAsync("/vendas/"); var segunda = await c.GetStringAsync("/vendas/?page=2");
        Assert.Equal(25, Links(primeira).Length); Assert.Single(Links(segunda));
        Assert.Equal(ids.Reverse(), Links(primeira).Concat(Links(segunda)));
        Assert.Contains("page=2", primeira);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/vendas/?page=3")).StatusCode);
    }

    [Fact]
    public async Task CancelamentoDesistenciaInexistenciaERepeticaoPreservamTodosOsDados()
    {
        using var f = new SqliteFixture(); var id = f.VendaPreparada();
        EstadoVenda original;
        using (var read = f.CreateDbContext()) original = EstadoVenda.De(read.Vendas.Find(id)!);
        using var app = new WebAppFactory(f); using var c = app.Client();
        var url = $"/vendas/{id}/cancelar/";
        var html = await c.GetStringAsync(url);
        Assert.Contains($"/vendas/{id}/", html);
        await c.GetStringAsync($"/vendas/{id}/"); // Segue Voltar, sem POST.
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/vendas/99999/cancelar/")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await AntiforgeryClient.Post(c, url, "/vendas/99999/cancelar/", new() { ["Confirmar"] = "sim" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await AntiforgeryClient.Post(c, url, url, new())).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string> { ["Confirmar"] = "sim", ["__RequestVerificationToken"] = "adulterado" }))).StatusCode);
        using (var check = f.CreateDbContext())
        {
            Assert.Equal(original, EstadoVenda.De(check.Vendas.Find(id)!)); Assert.Null(check.Vendas.Find(id)!.CanceladaEmUtc);
            Assert.Equal(SituacaoVenda.Registrada, check.Vendas.Find(id)!.Situacao);
            Assert.Equal((7L, 1L), (check.Ingressos.Single().QuantidadeDisponivel, check.Ingressos.Single().Versao));
        }
        var response = await AntiforgeryClient.Post(c, url, url, new() { ["Confirmar"] = "sim", ["Quantidade"] = "999", ["Situacao"] = "registrada" });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode); Assert.Equal($"/vendas/{id}/", response.Headers.Location!.ToString());
        DateTime? instante;
        using (var read = f.CreateDbContext()) instante = read.Vendas.Find(id)!.CanceladaEmUtc;
        Assert.NotNull(instante);
        response = await AntiforgeryClient.Post(c, url, url, new() { ["Confirmar"] = "sim" });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Venda já cancelada", WebUtility.HtmlDecode(await c.GetStringAsync(response.Headers.Location)));
        using var final = f.CreateDbContext(); var venda = final.Vendas.Find(id)!;
        Assert.Equal(original, EstadoVenda.De(venda)); Assert.Equal(instante, venda.CanceladaEmUtc); Assert.Equal(SituacaoVenda.Cancelada, venda.Situacao);
        Assert.Equal((10L, 2L), (final.Ingressos.Single().QuantidadeDisponivel, final.Ingressos.Single().Versao));
    }

    [Fact]
    public async Task OperacoesProibidasNaoAlteramNenhumCampoHistorico()
    {
        using var f = new SqliteFixture(); var id = f.VendaPreparada(cancelada: true);
        EstadoVenda original; DateTime? instante;
        using (var read = f.CreateDbContext()) { original = EstadoVenda.De(read.Vendas.Find(id)!); instante = read.Vendas.Find(id)!.CanceladaEmUtc; }
        using var app = new WebAppFactory(f); using var c = app.Client();
        foreach (var op in new[] { "editar", "excluir", "reativar" })
        foreach (var metodo in new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete })
        {
            using var request = new HttpRequestMessage(metodo, $"/vendas/{id}/{op}/");
            Assert.Contains((await c.SendAsync(request)).StatusCode, new[] { HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed });
        }
        using var check = f.CreateDbContext(); var venda = Assert.Single(check.Vendas);
        Assert.Equal(original, EstadoVenda.De(venda)); Assert.Equal(instante, venda.CanceladaEmUtc); Assert.Equal(SituacaoVenda.Cancelada, venda.Situacao);
        Assert.Equal((7L, 1L), (check.Ingressos.Single().QuantidadeDisponivel, check.Ingressos.Single().Versao));
    }
}
