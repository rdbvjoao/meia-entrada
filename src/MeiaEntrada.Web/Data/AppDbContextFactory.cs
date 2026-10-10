using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace MeiaEntrada.Web.Data;
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(root, "MeiaEntrada.slnx")) && Directory.GetParent(root) is { } parent) root = parent.FullName;
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? $"Data Source={Path.Combine(root, "var", "meia-entrada.db")};Foreign Keys=True;Default Timeout=30";
        return new AppDbContext(Options(connection).Options);
    }
    public static DbContextOptionsBuilder<AppDbContext> Options(string connection)
    {
        var cs = new SqliteConnectionStringBuilder(connection) { ForeignKeys = true, DefaultTimeout = 30, Cache = SqliteCacheMode.Private };
        var path = Path.GetFullPath(cs.DataSource);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        cs.DataSource = path;
        return new DbContextOptionsBuilder<AppDbContext>().UseSqlite(cs.ToString(), o => o.CommandTimeout(30)).AddInterceptors(new SqliteConnectionInterceptor());
    }
    public static void Prepare(AppDbContext db)
    {
        db.Database.OpenConnection();
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        if (!string.Equals(command.ExecuteScalar()?.ToString(), "wal", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("O banco precisa operar em WAL.");
        db.Database.CloseConnection();
    }
}
