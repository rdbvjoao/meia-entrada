using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace MeiaEntrada.Web.Data;
public class SqliteConnectionInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA synchronous=FULL;";
        command.ExecuteNonQuery();
    }
    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        ConnectionOpened(connection, eventData);
        return Task.CompletedTask;
    }
}
