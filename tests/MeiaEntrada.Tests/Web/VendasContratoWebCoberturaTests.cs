using System.Net;
using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Tests.Integration;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Services;

namespace MeiaEntrada.Tests.Web;

public class VendasContratoWebCoberturaTests
{
    [Fact]
    public async Task PrecoAlteradoExigeNovoResumoTokenEConfirmacaoExplicita()
    {
        using var f = new SqliteFixture(); var (e, i) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var response = await AntiforgeryClient.Post(c, "/vendas/nova/?evento=" + e, "/vendas/conferir/",
            new() { ["EventoId"] = e.ToString(), ["IngressoId"] = i.ToString(), ["Quantidade"] = "3" });
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Contains("120,00", html);
        var tokenAntigo = AntiforgeryClient.Campo(html, "Conferencia");
        var ingressos = new IngressoService(new SqliteWriteTransaction(f));
        ingressos.Salvar(i, IngressosTests.Form(e, "inteira", "50", "10"));
        var fields = new Dictionary<string, string> { ["Conferencia"] = tokenAntigo, ["Confirmar"] = "sim", ["__RequestVerificationToken"] = AntiforgeryClient.Campo(html, "__RequestVerificationToken"), ["PrecoUnitarioCentavos"] = "1", ["ValorTotalCentavos"] = "1" };
        response = await c.PostAsync("/vendas/", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        html = await response.Content.ReadAsStringAsync();
        Assert.Contains("150,00", html); Assert.Contains("50,00", html);
        Assert.Contains("confirme novamente", WebUtility.HtmlDecode(html));
        var tokenNovo = AntiforgeryClient.Campo(html, "Conferencia");
        Assert.NotEmpty(tokenNovo); Assert.NotEqual(tokenAntigo, tokenNovo);
        using (var check = f.CreateDbContext()) { Assert.Empty(check.Vendas); Assert.Equal((10L, 2L), (check.Ingressos.Find(i)!.QuantidadeDisponivel, check.Ingressos.Find(i)!.Versao)); }
        fields["Conferencia"] = tokenNovo; fields["__RequestVerificationToken"] = AntiforgeryClient.Campo(html, "__RequestVerificationToken");
        fields.Remove("Confirmar");
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsync("/vendas/", new FormUrlEncodedContent(fields))).StatusCode);
        using (var check = f.CreateDbContext()) Assert.Empty(check.Vendas);
        fields["Confirmar"] = "sim";
        response = await c.PostAsync("/vendas/", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("150,00", await c.GetStringAsync(response.Headers.Location));
        using var final = f.CreateDbContext(); var venda = Assert.Single(final.Vendas);
        Assert.Equal((3L, 5000L, 15000L), (venda.Quantidade, venda.PrecoUnitarioCentavos, venda.ValorTotalCentavos));
        Assert.Equal((7L, 3L), (final.Ingressos.Find(i)!.QuantidadeDisponivel, final.Ingressos.Find(i)!.Versao));
    }

    [Fact]
    public async Task NovaConferenciaAindaRevalidaSaldoAoConfirmar()
    {
        using var f = new SqliteFixture(); var (e, i) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var response = await AntiforgeryClient.Post(c, "/vendas/nova/?evento=" + e, "/vendas/conferir/", new() { ["EventoId"] = e.ToString(), ["IngressoId"] = i.ToString(), ["Quantidade"] = "3" });
        var html = await response.Content.ReadAsStringAsync();
        new IngressoService(new SqliteWriteTransaction(f)).Salvar(i, IngressosTests.Form(e, "inteira", "40", "2"));
        response = await c.PostAsync("/vendas/", new FormUrlEncodedContent(new Dictionary<string, string> { ["Conferencia"] = AntiforgeryClient.Campo(html, "Conferencia"), ["Confirmar"] = "sim", ["__RequestVerificationToken"] = AntiforgeryClient.Campo(html, "__RequestVerificationToken") }));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Saldo insuficiente", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
        using var check = f.CreateDbContext(); Assert.Empty(check.Vendas); Assert.Equal((2L, 2L), (check.Ingressos.Find(i)!.QuantidadeDisponivel, check.Ingressos.Find(i)!.Versao));
    }

    [Fact]
    public async Task RotasSelecaoConfirmacaoEDetalheRejeitamRequisicoesInvalidas()
    {
        using var f = new SqliteFixture(); var (e, i) = f.Oferta(0);
        using var app = new WebAppFactory(f); using var c = app.Client();
        var html = await c.GetStringAsync("/vendas/nova/?evento=" + e);
        Assert.Contains("Ingressos esgotados", html); Assert.Contains("disabled", html);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/vendas/nova/?evento=99999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/vendas/99999/")).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await c.GetAsync("/vendas/conferir/")).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await c.PutAsync("/vendas/", null)).StatusCode);
        foreach (var url in new[] { "/vendas/conferir/", "/vendas/" })
            Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>()))).StatusCode);
        var inexistente = await AntiforgeryClient.Post(c, "/vendas/nova/?evento=" + e, "/vendas/conferir/", new() { ["EventoId"] = e.ToString(), ["IngressoId"] = "99999", ["Quantidade"] = "1" });
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        var saldo = await AntiforgeryClient.Post(c, "/vendas/nova/?evento=" + e, "/vendas/conferir/", new() { ["EventoId"] = e.ToString(), ["IngressoId"] = i.ToString(), ["Quantidade"] = "1" });
        Assert.Equal(HttpStatusCode.Conflict, saldo.StatusCode);
        using var check = f.CreateDbContext(); Assert.Empty(check.Vendas); Assert.Equal(0, check.Ingressos.Find(i)!.QuantidadeDisponivel);
    }
}
