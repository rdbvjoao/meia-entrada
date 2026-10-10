using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;

namespace MeiaEntrada.Tests.Integration;

public class IngressosCoberturaTests
{
    [Fact]
    public void CadastroComEventoInexistenteNaoGravaEOverflowNaEdicaoNaoAltera()
    {
        using var f = new SqliteFixture(); var service = new IngressoService(new SqliteWriteTransaction(f));
        Assert.Equal(400, Assert.Throws<RegraException>(() => service.Salvar(null, IngressosTests.Form(99999))).Status);
        using (var vazio = f.CreateDbContext()) Assert.Empty(vazio.Ingressos);
        var (e, i) = f.Oferta();
        using (var edit = f.CreateDbContext()) { edit.Ingressos.Find(i)!.Versao = long.MaxValue; edit.SaveChanges(); }
        Assert.Equal(400, Assert.Throws<RegraException>(() => service.Salvar(i, IngressosTests.Form(e, "inteira", "99", "99", long.MaxValue.ToString()))).Status);
        using var check = f.CreateDbContext();
        Assert.Equal((4000L, 10L, long.MaxValue), (check.Ingressos.Find(i)!.PrecoCentavos, check.Ingressos.Find(i)!.QuantidadeDisponivel, check.Ingressos.Find(i)!.Versao));
    }

    [Theory]
    [InlineData("EventoId", "999999")]
    [InlineData("EventoId", "0")]
    [InlineData("EventoId", "1.5")]
    [InlineData("EventoId", "")]
    [InlineData("Tipo", "")]
    [InlineData("Preco", "")]
    [InlineData("QuantidadeDisponivel", "")]
    [InlineData("Versao", "")]
    [InlineData("Versao", "0")]
    [InlineData("Versao", "1.5")]
    [InlineData("Versao", "9223372036854775808")]
    public void EdicaoInvalidaPreservaTodosOsCampos(string campo, string valor)
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta();
        var form = IngressosTests.Form(e, "inteira", "50", "20");
        form.GetType().GetProperty(campo)!.SetValue(form, valor);
        var service = new IngressoService(new SqliteWriteTransaction(f));
        Assert.Equal(400, Assert.Throws<RegraException>(() => service.Salvar(i, form)).Status);
        using var check = f.CreateDbContext();
        var ingresso = Assert.Single(check.Ingressos);
        Assert.Equal((e, TipoIngresso.Inteira, 4000L, 10L, 1L),
            (ingresso.EventoId, ingresso.Tipo, ingresso.PrecoCentavos, ingresso.QuantidadeDisponivel, ingresso.Versao));
    }

    [Fact]
    public void SemVendasPodeMudarEventoETipoMasNaoDuplicarPar()
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta();
        var (outro, _) = f.Oferta();
        var service = new IngressoService(new SqliteWriteTransaction(f));
        Assert.Equal(409, Assert.Throws<RegraException>(() => service.Salvar(i, IngressosTests.Form(outro, "inteira"))).Status);
        using (var check = f.CreateDbContext()) Assert.Equal(1, check.Ingressos.Find(i)!.Versao);
        service.Salvar(i, IngressosTests.Form(outro, "meia_entrada", "0", "0"));
        using var final = f.CreateDbContext();
        var ingresso = final.Ingressos.Find(i)!;
        Assert.Equal((outro, TipoIngresso.MeiaEntrada, 0L, 0L, 2L),
            (ingresso.EventoId, ingresso.Tipo, ingresso.PrecoCentavos, ingresso.QuantidadeDisponivel, ingresso.Versao));
        Assert.NotNull(final.Eventos.Find(e));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ComVendaBloqueiaEventoTipoEExclusaoMasPermitePrecoESaldo(bool cancelada)
    {
        using var f = new SqliteFixture();
        var id = f.VendaPreparada(cancelada: cancelada);
        var (outro, _) = f.Oferta();
        EstadoVenda original;
        long e, i;
        using (var read = f.CreateDbContext())
        {
            original = EstadoVenda.De(read.Vendas.Find(id)!);
            i = original.IngressoId; e = read.Ingressos.Find(i)!.EventoId;
        }
        var service = new IngressoService(new SqliteWriteTransaction(f));
        Assert.Equal(409, Assert.Throws<RegraException>(() => service.Salvar(i, IngressosTests.Form(outro, "inteira"))).Status);
        Assert.Equal(409, Assert.Throws<RegraException>(() => service.Salvar(i, IngressosTests.Form(e, "meia_entrada"))).Status);
        Assert.Equal(409, Assert.Throws<RegraException>(() => service.Excluir(i)).Status);
        service.Salvar(i, IngressosTests.Form(e, "inteira", "50", "20"));
        using var check = f.CreateDbContext();
        var ingresso = check.Ingressos.Find(i)!;
        Assert.Equal((e, TipoIngresso.Inteira, 5000L, 20L, 2L),
            (ingresso.EventoId, ingresso.Tipo, ingresso.PrecoCentavos, ingresso.QuantidadeDisponivel, ingresso.Versao));
        Assert.Equal(original, EstadoVenda.De(check.Vendas.Find(id)!));
    }

    [Theory]
    [InlineData("venda", 9, 2)]
    [InlineData("cancelamento", 10, 3)]
    [InlineData("edicao", 20, 2)]
    public void VersaoAntigaNaoSobrescreveMovimentacao(string operacao, long saldo, long versao)
    {
        using var f = new SqliteFixture();
        var (e, i) = f.Oferta();
        var service = new IngressoService(new SqliteWriteTransaction(f));
        var vendas = new VendasFixture(f);
        if (operacao == "edicao") service.Salvar(i, IngressosTests.Form(e, "inteira", "40", "20"));
        else
        {
            var id = vendas.Service.Registrar(vendas.Conferir(e, i).Conferencia);
            if (operacao == "cancelamento") vendas.Service.Cancelar(id);
        }
        Assert.Equal(409, Assert.Throws<RegraException>(() => service.Salvar(i, IngressosTests.Form(e, "inteira", "99", "99"))).Status);
        using var check = f.CreateDbContext();
        Assert.Equal((saldo, versao, 4000L), (check.Ingressos.Find(i)!.QuantidadeDisponivel, check.Ingressos.Find(i)!.Versao, check.Ingressos.Find(i)!.PrecoCentavos));
    }
}
