using MeiaEntrada.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace MeiaEntrada.Tests.Fixtures;
public class WebAppFactory(SqliteFixture database) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("ConnectionStrings:DefaultConnection", database.ConnectionString);
        builder.UseSetting("DataProtection:KeyPath", Path.Combine(database.DirectoryPath, "keys"));
        builder.ConfigureServices(services => {
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.AddSingleton<IDbContextFactory<AppDbContext>>(database);
        });
    }
    public HttpClient Client() => CreateClient(new() { AllowAutoRedirect = false });
}
