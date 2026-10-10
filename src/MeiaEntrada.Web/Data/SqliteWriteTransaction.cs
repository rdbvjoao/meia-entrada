using MeiaEntrada.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Data;
public class SqliteWriteTransaction(IDbContextFactory<AppDbContext> factory)
{
    public T Execute<T>(Func<AppDbContext, T> action)
    {
        using var db = factory.CreateDbContext();
        SqliteTransaction? transaction = null;
        var committing = false;
        try
        {
            db.Database.OpenConnection();
            transaction = ((SqliteConnection)db.Database.GetDbConnection()).BeginTransaction(deferred: false);
            db.Database.UseTransaction(transaction);
            var result = action(db);
            db.SaveChanges();
            committing = true;
            // Commit through EF so diagnostic interceptors can verify the transaction boundary.
            db.Database.CurrentTransaction!.Commit();
            return result;
        }
        catch (Exception ex)
        {
            var rolledBack = transaction is null;
            if (transaction is not null)
                try { transaction.Rollback(); rolledBack = true; } catch { rolledBack = false; }
            if (committing) throw new RegraException(500, "Não foi possível confirmar o resultado. Consulte o histórico antes de registrar outra venda.");
            if (ex is SqliteException { SqliteErrorCode: 5 or 6 } && rolledBack)
                throw new RegraException(503, "Sistema ocupado; tente novamente");
            if (ex is DbUpdateException { InnerException: SqliteException { SqliteErrorCode: 5 or 6 } } && rolledBack)
                throw new RegraException(503, "Sistema ocupado; tente novamente");
            if (ex is OverflowException) throw new RegraException(400, "O valor excede o limite permitido. Nenhum dado foi alterado.");
            throw;
        }
        finally { transaction?.Dispose(); }
    }
}
