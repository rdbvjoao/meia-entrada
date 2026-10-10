using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Eventos;

namespace MeiaEntrada.Tests.Integration;

public class EventosCoberturaTests
{
    public static IEnumerable<object[]> CamposInvalidos()
    {
        foreach (var campo in new[] { "Nome", "Descricao", "Local", "Data", "Horario" })
        foreach (var valor in new string?[] { null, "", " \t\r\n " })
        foreach (var editar in new[] { false, true })
            yield return new object[] { campo, valor!, editar };
    }

    [Theory]
    [MemberData(nameof(CamposInvalidos))]
    public void ObrigatoriosInvalidosNaoCriamNemAlteram(string campo, string? valor, bool editar)
    {
        using var f = new SqliteFixture();
        var service = new EventoService(new SqliteWriteTransaction(f));
        var form = new EventoFormViewModel { Nome = "Original", Descricao = "Descrição", Local = "Sala", Data = "2000-01-01", Horario = "00:00" };
        var id = service.Salvar(null, form);
        typeof(EventoFormViewModel).GetProperty(campo)!.SetValue(form, valor);
        Assert.Equal(400, Assert.Throws<RegraException>(() => service.Salvar(editar ? id : null, form)).Status);
        using var check = f.CreateDbContext();
        var evento = Assert.Single(check.Eventos);
        Assert.Equal(id, evento.Id);
        Assert.Equal(("Original", "Descrição", "Sala", new DateOnly(2000, 1, 1), new TimeOnly(0, 0)),
            (evento.Nome, evento.Descricao, evento.Local, evento.Data, evento.Horario));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VinculosRegistradosECanceladosPreservamHistorico(bool cancelada)
    {
        using var f = new SqliteFixture();
        var vendaId = f.VendaPreparada(cancelada: cancelada);
        EstadoVenda original;
        long eventoId;
        using (var read = f.CreateDbContext())
        {
            original = EstadoVenda.De(read.Vendas.Find(vendaId)!);
            eventoId = read.Ingressos.Single().EventoId;
        }
        var service = new EventoService(new SqliteWriteTransaction(f));
        service.Salvar(eventoId, new() { Nome = "Novo", Descricao = "Nova", Local = "Outro", Data = "1999-01-01", Horario = "23:59" });
        Assert.Equal(409, Assert.Throws<RegraException>(() => service.Excluir(eventoId)).Status);
        using var check = f.CreateDbContext();
        Assert.Equal(original, EstadoVenda.De(check.Vendas.Find(vendaId)!));
        Assert.Equal(eventoId, check.Eventos.Single().Id);
        Assert.Equal("Novo", check.Eventos.Single().Nome);
    }
}
