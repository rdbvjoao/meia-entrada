using MeiaEntrada.Tests.Fixtures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MeiaEntrada.Tests.Integration;

public class MigrationsCoberturaTests
{
    [Fact]
    public void IndicesChavesEConfiguracaoPersistemEmConexoesIndependentes()
    {
        using var f = new SqliteFixture();
        var id = f.VendaPreparada();
        for (var n = 0; n < 2; n++)
        {
            using var db = f.CreateDbContext();
            db.Database.Migrate();
            db.Database.OpenConnection();
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            foreach (var (sql, esperado) in new[] {
                ("PRAGMA foreign_keys", "1"), ("PRAGMA synchronous", "2"), ("PRAGMA journal_mode", "wal"),
                ("SELECT count(*) FROM pragma_index_list('Ingressos') WHERE name='IX_Ingressos_EventoId_Tipo' AND \"unique\"=1", "1"),
                ("SELECT count(*) FROM pragma_index_list('Ingressos') WHERE name='IX_Ingressos_EventoId'", "1"),
                ("SELECT count(*) FROM pragma_index_list('Vendas') WHERE name='IX_Vendas_IngressoId'", "1"),
                ("SELECT count(*) FROM pragma_index_xinfo('IX_Vendas_RegistradaEmUtc_Id') WHERE name IN ('RegistradaEmUtc','Id') AND desc=1", "2"),
                ("SELECT count(*) FROM pragma_foreign_key_list('Ingressos') WHERE \"table\"='Eventos' AND on_delete='RESTRICT'", "1"),
                ("SELECT count(*) FROM pragma_foreign_key_list('Vendas') WHERE \"table\"='Ingressos' AND on_delete='RESTRICT'", "1") })
            {
                cmd.CommandText = sql;
                Assert.Equal(esperado, cmd.ExecuteScalar()!.ToString());
            }
            Assert.Equal(id, Assert.Single(db.Vendas).Id);
            Assert.Equal(12000, db.Vendas.Single().ValorTotalCentavos);
        }
    }

    [Theory]
    [InlineData("UPDATE Eventos SET Nome=' '")]
    [InlineData("UPDATE Eventos SET Descricao=''")]
    [InlineData("UPDATE Eventos SET Local=NULL")]
    [InlineData("UPDATE Ingressos SET PrecoCentavos=1.5")]
    [InlineData("UPDATE Ingressos SET Versao=1.5")]
    [InlineData("UPDATE Vendas SET Quantidade=1.5")]
    [InlineData("UPDATE Vendas SET PrecoUnitarioCentavos=1.5")]
    [InlineData("UPDATE Vendas SET ValorTotalCentavos=1.5")]
    [InlineData("UPDATE Vendas SET IngressoId=99999")]
    [InlineData("UPDATE Vendas SET IngressoTipo='outro'")]
    [InlineData("UPDATE Vendas SET Situacao='outra'")]
    [InlineData("UPDATE Vendas SET CanceladaEmUtc='2026-01-01'")]
    [InlineData("INSERT INTO Ingressos(EventoId,Tipo,PrecoCentavos,QuantidadeDisponivel,Versao) SELECT EventoId,Tipo,PrecoCentavos,QuantidadeDisponivel,Versao FROM Ingressos")]
    public void ConstraintsRejeitamValoresInvalidosSemAlterarDados(string sql)
    {
        using var f = new SqliteFixture();
        var id = f.VendaPreparada();
        EstadoVenda original;
        using (var read = f.CreateDbContext()) original = EstadoVenda.De(read.Vendas.Find(id)!);
        using (var db = f.CreateDbContext())
        {
            var error = Assert.Throws<SqliteException>(() => db.Database.ExecuteSqlRaw(sql));
            Assert.Equal(19, error.SqliteErrorCode);
        }
        using var check = f.CreateDbContext();
        Assert.Equal(original, EstadoVenda.De(check.Vendas.Find(id)!));
        Assert.Single(check.Ingressos);
        Assert.Equal(1, check.Ingressos.Single().Versao);
        Assert.Equal("Sessão original", check.Eventos.Single().Nome);
        Assert.Null(check.Vendas.Single().CanceladaEmUtc);
    }
}
