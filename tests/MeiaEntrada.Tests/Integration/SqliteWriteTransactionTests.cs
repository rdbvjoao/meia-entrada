using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
namespace MeiaEntrada.Tests.Integration;
public class SqliteWriteTransactionTests
{
    [Fact] public void FalhaDesfazEscritaJaEnviada()
    {
        using var f = new SqliteFixture(); var (_, id) = f.Oferta();
        Assert.Throws<InvalidOperationException>(() => new SqliteWriteTransaction(f).Execute<int>(db => {
            db.Ingressos.Find(id)!.QuantidadeDisponivel = 0; db.SaveChanges(); throw new InvalidOperationException("falha de teste");
        }));
        using var final = f.CreateDbContext(); Assert.Equal(10, final.Ingressos.Find(id)!.QuantidadeDisponivel);
    }
}
