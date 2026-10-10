using MeiaEntrada.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Tests.Integration;
public class MigrationsTests
{
    [Fact] public void CriaReaplicaPreservaEConfigura()
    {
        using var f = new SqliteFixture(); f.VendaPreparada();
        using var db = f.CreateDbContext(); db.Database.Migrate();
        Assert.Single(db.Vendas); Assert.Single(db.Ingressos); Assert.Single(db.Eventos);
        db.Database.OpenConnection();
        foreach (var (pragma, esperado) in new[] { ("journal_mode","wal"),("foreign_keys","1"),("synchronous","2") })
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand(); cmd.CommandText = $"PRAGMA {pragma}";
            Assert.Equal(esperado, cmd.ExecuteScalar()!.ToString());
        }
        Assert.NotEmpty(db.Database.GetAppliedMigrations());
    }
    [Theory]
    [InlineData("UPDATE Ingressos SET QuantidadeDisponivel=-1")]
    [InlineData("UPDATE Ingressos SET QuantidadeDisponivel=1.5")]
    [InlineData("UPDATE Ingressos SET PrecoCentavos=-1")]
    [InlineData("UPDATE Ingressos SET Tipo='outro'")]
    [InlineData("UPDATE Ingressos SET EventoId=9999")]
    [InlineData("UPDATE Ingressos SET Versao=0")]
    [InlineData("UPDATE Vendas SET ValorTotalCentavos=1")]
    [InlineData("UPDATE Vendas SET Quantidade=0")]
    [InlineData("UPDATE Vendas SET Situacao='cancelada'")]
    [InlineData("UPDATE Vendas SET PrecoUnitarioCentavos=9223372036854775807, ValorTotalCentavos=9223372036854775807")]
    [InlineData("DELETE FROM Ingressos")]
    [InlineData("DELETE FROM Eventos")]
    public void ConstraintsRejeitamPersistenciaInvalida(string sql)
    {
        using var f = new SqliteFixture(); f.VendaPreparada();
        using var db = f.CreateDbContext();
        Assert.ThrowsAny<Exception>(() => db.Database.ExecuteSqlRaw(sql));
    }
}
