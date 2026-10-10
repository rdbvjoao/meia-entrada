using System.Net;
using System.Text.RegularExpressions;
using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Tests.Integration;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;

namespace MeiaEntrada.Tests.Web;

public class CadastrosContratoWebCoberturaTests
{
    [Fact]
    public async Task IngressosIgnoramCamposExtrasNaCriacaoEEdicao()
    {
        using var f = new SqliteFixture(); var (e, original) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var fields = new Dictionary<string, string> { ["EventoId"] = e.ToString(), ["Tipo"] = "meia_entrada", ["Preco"] = "20,00", ["QuantidadeDisponivel"] = "5", ["Id"] = original.ToString(), ["Versao"] = "999", ["PrecoCentavos"] = "1", ["Evento.Nome"] = "Adulterado" };
        var response = await AntiforgeryClient.Post(c, "/ingressos/novo/", "/ingressos/novo/", fields);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var url = response.Headers.Location!.ToString();
        using (var check = f.CreateDbContext())
        {
            Assert.Equal(2, check.Ingressos.Count());
            var novo = check.Ingressos.Single(x => x.Id != original);
            Assert.Equal((2000L, 5L, 1L), (novo.PrecoCentavos, novo.QuantidadeDisponivel, novo.Versao));
            Assert.Equal("Sessão original", check.Eventos.Find(e)!.Nome);
        }
        fields["Versao"] = "1"; fields["QuantidadeDisponivel"] = "8";
        Assert.Equal(HttpStatusCode.Redirect, (await AntiforgeryClient.Post(c, url + "editar/", url + "editar/", fields)).StatusCode);
        using var final = f.CreateDbContext();
        Assert.Equal((4000L, 10L, 1L), (final.Ingressos.Find(original)!.PrecoCentavos, final.Ingressos.Find(original)!.QuantidadeDisponivel, final.Ingressos.Find(original)!.Versao));
        var editado = final.Ingressos.Single(x => x.Id != original);
        Assert.Equal((2000L, 8L, 2L), (editado.PrecoCentavos, editado.QuantidadeDisponivel, editado.Versao));
    }

    [Theory]
    [InlineData("EventoId", "99999")]
    [InlineData("Tipo", "outro")]
    [InlineData("Preco", "1.001")]
    [InlineData("QuantidadeDisponivel", "1.5")]
    public async Task IngressosInvalidosPreservamEntradasESemMutacao(string campo, string valor)
    {
        using var f = new SqliteFixture(); var (e, _) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var fields = new Dictionary<string, string> { ["EventoId"] = e.ToString(), ["Tipo"] = "meia_entrada", ["Preco"] = "20,00", ["QuantidadeDisponivel"] = "5" };
        fields[campo] = valor;
        var response = await AntiforgeryClient.Post(c, "/ingressos/novo/", "/ingressos/novo/", fields);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(fields["Preco"], AntiforgeryClient.Campo(html, "Preco"));
        Assert.Equal(fields["QuantidadeDisponivel"], AntiforgeryClient.Campo(html, "QuantidadeDisponivel"));
        using var check = f.CreateDbContext(); Assert.Single(check.Ingressos); Assert.Equal(1, check.Ingressos.Single().Versao);
    }

    [Theory]
    [InlineData("eventos")]
    [InlineData("ingressos")]
    public async Task TodosOsPostsDeCadastroExigemAntiforgery(string recurso)
    {
        using var f = new SqliteFixture(); var (e, i) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var id = recurso == "eventos" ? e : i;
        foreach (var url in new[] { $"/{recurso}/novo/", $"/{recurso}/{id}/editar/", $"/{recurso}/{id}/excluir/" })
        foreach (var token in new[] { "", "adulterado" })
        {
            var fields = EventosContratoWebTests.Campos();
            fields["Confirmar"] = "sim"; fields["EventoId"] = e.ToString(); fields["Tipo"] = "meia_entrada";
            fields["Preco"] = "20"; fields["QuantidadeDisponivel"] = "20"; fields["Versao"] = "1";
            fields["__RequestVerificationToken"] = token;
            Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsync(url, new FormUrlEncodedContent(fields))).StatusCode);
        }
        using var check = f.CreateDbContext(); Assert.Single(check.Eventos); Assert.Single(check.Ingressos);
        Assert.Equal((4000L, 10L, 1L), (check.Ingressos.Single().PrecoCentavos, check.Ingressos.Single().QuantidadeDisponivel, check.Ingressos.Single().Versao));
    }

    private static long[] Ids(string html, string recurso) => Regex.Matches(html,
        "href=\"/" + recurso + "/([0-9]+)/\"").Select(x => long.Parse(x.Groups[1].Value)).ToArray();

    [Fact]
    public async Task EventosOrdenamDataHorarioIdEPaginam26Registros()
    {
        using var f = new SqliteFixture();
        var esperado = new List<Evento>();
        using (var db = f.CreateDbContext())
        {
            for (var n = 0; n < 26; n++)
            {
                var evento = new Evento { Nome = $"Evento {n}", Descricao = "D", Local = "L", Data = new DateOnly(2026, 1, 1).AddDays(n % 3), Horario = new TimeOnly(n % 2, 0) };
                db.Add(evento); esperado.Add(evento);
            }
            db.SaveChanges();
        }
        using var app = new WebAppFactory(f); using var c = app.Client();
        var primeira = await c.GetStringAsync("/eventos/");
        var segunda = await c.GetStringAsync("/eventos/?page=2");
        Assert.Equal(25, Ids(primeira, "eventos").Length);
        Assert.Single(Ids(segunda, "eventos"));
        Assert.Equal(esperado.OrderBy(x => x.Data).ThenBy(x => x.Horario).ThenBy(x => x.Id).Select(x => x.Id),
            Ids(primeira, "eventos").Concat(Ids(segunda, "eventos")));
        Assert.Contains("page=2", primeira);
        foreach (var page in new[] { "-1", "abc", "3", "2147483647" })
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/eventos/?page=" + page)).StatusCode);
    }

    [Fact]
    public async Task IngressosOrdenamEventoTipoEPaginamComFiltroEPreSelecao()
    {
        using var f = new SqliteFixture();
        var esperado = new List<Ingresso>();
        for (var n = 0; n < 13; n++)
        {
            var (e, _) = f.Oferta();
            using var db = f.CreateDbContext();
            db.Add(new Ingresso { EventoId = e, Tipo = TipoIngresso.MeiaEntrada, PrecoCentavos = 2000, QuantidadeDisponivel = 5 });
            db.SaveChanges();
        }
        using (var read = f.CreateDbContext()) esperado = read.Ingressos.ToList();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var primeira = await c.GetStringAsync("/ingressos/");
        var segunda = await c.GetStringAsync("/ingressos/?page=2");
        Assert.Equal(25, Ids(primeira, "ingressos").Length);
        Assert.Single(Ids(segunda, "ingressos"));
        Assert.Equal(esperado.OrderBy(x => x.EventoId).ThenBy(x => x.Tipo).ThenBy(x => x.Id).Select(x => x.Id),
            Ids(primeira, "ingressos").Concat(Ids(segunda, "ingressos")));
        var evento = esperado[0].EventoId;
        var filtrada = await c.GetStringAsync("/ingressos/?evento=" + evento);
        Assert.Equal(esperado.Where(x => x.EventoId == evento).OrderBy(x => x.Tipo).Select(x => x.Id), Ids(filtrada, "ingressos"));
        var novo = await c.GetStringAsync("/ingressos/novo/?evento=" + evento);
        Assert.Matches("<option(?=[^>]*value=\"" + evento + "\")(?=[^>]*selected=\"selected\")[^>]*>", novo);
        foreach (var url in new[] { "/ingressos/?page=3", "/ingressos/?page=abc", "/ingressos/?evento=99999", "/ingressos/novo/?evento=99999", "/ingressos/?evento=" + evento + "&page=2" })
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task EstadosVaziosOferecemCadastroESemEventoNaoHaOferta()
    {
        using var f = new SqliteFixture(); using var app = new WebAppFactory(f); using var c = app.Client();
        Assert.Contains("/eventos/novo/", await c.GetStringAsync("/eventos/"));
        Assert.Contains("/ingressos/novo/", await c.GetStringAsync("/ingressos/"));
        Assert.Contains("Cadastre um evento", await c.GetStringAsync("/ingressos/novo/"));
        Assert.Contains("Nenhum ingresso", await c.GetStringAsync("/vendas/nova/"));
        using var check = f.CreateDbContext(); Assert.Empty(check.Eventos); Assert.Empty(check.Ingressos);
    }

    [Theory]
    [InlineData("Nome")]
    [InlineData("Descricao")]
    [InlineData("Local")]
    [InlineData("Data")]
    [InlineData("Horario")]
    public async Task EventoInvalidoPreservaCamposValidosEIgnoraCamposExtras(string campo)
    {
        using var f = new SqliteFixture(); using var app = new WebAppFactory(f); using var c = app.Client();
        var fields = EventosContratoWebTests.Campos("Nome preservado");
        fields[campo] = " "; fields["Id"] = "99999";
        var response = await AntiforgeryClient.Post(c, "/eventos/novo/", "/eventos/novo/", fields);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        foreach (var item in fields.Where(x => x.Key != campo && x.Key != "Id" && x.Key != "__RequestVerificationToken")) Assert.Contains(item.Value, html);
        using (var check = f.CreateDbContext()) Assert.Empty(check.Eventos);
        fields = EventosContratoWebTests.Campos(); fields["Id"] = "99999";
        Assert.Equal(HttpStatusCode.Redirect, (await AntiforgeryClient.Post(c, "/eventos/novo/", "/eventos/novo/", fields)).StatusCode);
        using var final = f.CreateDbContext(); Assert.NotEqual(99999, Assert.Single(final.Eventos).Id);
    }

    [Fact]
    public async Task IngressoConflitoMostraTentativaSaldoEVersaoAtualEExigeNovoEnvio()
    {
        using var f = new SqliteFixture(); var (e, i) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var url = $"/ingressos/{i}/editar/";
        var aberto = await c.GetStringAsync(url);
        var vendas = new VendasFixture(f);
        vendas.Service.Registrar(vendas.Conferir(e, i).Conferencia);
        var fields = new Dictionary<string, string> { ["EventoId"] = e.ToString(), ["Tipo"] = "inteira", ["Preco"] = "40", ["QuantidadeDisponivel"] = "20", ["Versao"] = AntiforgeryClient.Campo(aberto, "Versao"), ["__RequestVerificationToken"] = AntiforgeryClient.Campo(aberto, "__RequestVerificationToken") };
        var response = await c.PostAsync(url, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<strong>9</strong>", html);
        Assert.Equal("20", AntiforgeryClient.Campo(html, "QuantidadeDisponivel"));
        Assert.Equal("2", AntiforgeryClient.Campo(html, "Versao"));
        using (var check = f.CreateDbContext()) Assert.Equal((9L, 2L), (check.Ingressos.Find(i)!.QuantidadeDisponivel, check.Ingressos.Find(i)!.Versao));
        fields["Versao"] = AntiforgeryClient.Campo(html, "Versao");
        fields["__RequestVerificationToken"] = AntiforgeryClient.Campo(html, "__RequestVerificationToken");
        Assert.Equal(HttpStatusCode.Redirect, (await c.PostAsync(url, new FormUrlEncodedContent(fields))).StatusCode);
        using var final = f.CreateDbContext(); Assert.Equal((20L, 3L), (final.Ingressos.Find(i)!.QuantidadeDisponivel, final.Ingressos.Find(i)!.Versao)); Assert.Single(final.Vendas);
    }

    [Theory]
    [InlineData("eventos")]
    [InlineData("ingressos")]
    public async Task ContratosRejeitamMetodosEConfirmacaoAusenteSemMutacao(string recurso)
    {
        using var f = new SqliteFixture(); var (e, i) = f.Oferta();
        using var app = new WebAppFactory(f); using var c = app.Client();
        var id = recurso == "eventos" ? e : i;
        var url = $"/{recurso}/{id}/";
        foreach (var sufixo in new[] { "", "editar/", "excluir/" })
        {
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(url + sufixo)).StatusCode);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, (await c.PutAsync(url + sufixo, null)).StatusCode);
        }
        Assert.Equal(HttpStatusCode.BadRequest, (await AntiforgeryClient.Post(c, url + "excluir/", url + "excluir/", new())).StatusCode);
        foreach (var sufixo in new[] { "", "editar/", "excluir/" })
            Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync($"/{recurso}/99999/" + sufixo)).StatusCode);
        using var check = f.CreateDbContext(); Assert.Single(check.Eventos); Assert.Single(check.Ingressos); Assert.Equal(1, check.Ingressos.Single().Versao);
    }
}
