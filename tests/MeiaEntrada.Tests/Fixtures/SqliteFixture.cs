using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace MeiaEntrada.Tests.Fixtures;
public sealed class SqliteFixture : IDisposable, IDbContextFactory<AppDbContext>
{
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "meia-entrada-tests", Guid.NewGuid().ToString("N"));
    public string ConnectionString => $"Data Source={Path.Combine(DirectoryPath, "test.db")};Foreign Keys=True;Default Timeout=30;Pooling=False";
    public IInterceptor[] Interceptors { get; set; } = [];
    public SqliteFixture()
    {
        using var db = CreateDbContext();
        db.Database.Migrate();
        AppDbContextFactory.Prepare(db);
    }
    public AppDbContext CreateDbContext() => new(AppDbContextFactory.Options(ConnectionString).AddInterceptors(Interceptors).Options);
    public (long Evento, long Ingresso) Oferta(long saldo = 10, long preco = 4000)
    {
        using var db = CreateDbContext();
        var evento = new Evento { Nome = "Sessão original", Descricao = "Descrição", Local = "Sala A", Data = new(2026,12,1), Horario = new(19,0) };
        var ingresso = new Ingresso { Evento = evento, Tipo = TipoIngresso.Inteira, PrecoCentavos = preco, QuantidadeDisponivel = saldo };
        db.Add(ingresso); db.SaveChanges(); return (evento.Id, ingresso.Id);
    }
    public long VendaPreparada(long quantidade = 3, bool cancelada = false, long saldo = 7)
    {
        var (_, ingresso) = Oferta(saldo);
        using var db = CreateDbContext();
        var venda = new Venda { IngressoId = ingresso, EventoNome = "Sessão original", IngressoTipo = TipoIngresso.Inteira, Quantidade = quantidade, PrecoUnitarioCentavos = 4000, ValorTotalCentavos = 4000 * quantidade, RegistradaEmUtc = DateTime.UtcNow, Situacao = cancelada ? SituacaoVenda.Cancelada : SituacaoVenda.Registrada, CanceladaEmUtc = cancelada ? DateTime.UtcNow : null };
        db.Add(venda); db.SaveChanges(); return venda.Id;
    }
    public void Dispose() { if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true); }
    public static async Task<T[]> Concorrer<T>(int count, Func<int,T> action)
    {
        using var ready = new CountdownEvent(count);
        using var start = new ManualResetEventSlim();
        var jobs = Enumerable.Range(0,count).Select(i => Task.Factory.StartNew(() => { ready.Signal(); start.Wait(TimeSpan.FromSeconds(20)); return action(i); }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray();
        Assert.True(ready.Wait(TimeSpan.FromSeconds(20)));
        start.Set();
        return await Task.WhenAll(jobs).WaitAsync(TimeSpan.FromSeconds(90));
    }
}
