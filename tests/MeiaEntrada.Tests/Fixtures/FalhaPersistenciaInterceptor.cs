using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace MeiaEntrada.Tests.Fixtures;
public class FalhaPersistenciaInterceptor : SaveChangesInterceptor
{
    private int saves;
    public override int SavedChanges(SaveChangesCompletedEventData eventData,int result)
    {
        if(Interlocked.Increment(ref saves)==1) throw new InvalidOperationException("Falha de teste após primeira escrita.");
        return result;
    }
}
public class FalhaCommitInterceptor : DbTransactionInterceptor
{
    public override InterceptionResult TransactionCommitting(DbTransaction transaction,TransactionEventData eventData,InterceptionResult result)=>throw new InvalidOperationException("Falha de teste antes do commit.");
}
