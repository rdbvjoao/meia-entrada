using System.Globalization;
using MeiaEntrada.Web.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var root = builder.Environment.ContentRootPath;
while (!File.Exists(Path.Combine(root, "MeiaEntrada.slnx")) && Directory.GetParent(root) is { } parent) root = parent.FullName;
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connection)) connection = $"Data Source={Path.Combine(root, "var", "meia-entrada.db")};Foreign Keys=True;Default Timeout=30";
var cs = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connection) { ForeignKeys = true, DefaultTimeout = 30, Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Private };
cs.DataSource = Path.GetFullPath(cs.DataSource);
Directory.CreateDirectory(Path.GetDirectoryName(cs.DataSource)!);
builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite(cs.ToString(), s => s.CommandTimeout(30)).AddInterceptors(new SqliteConnectionInterceptor()));
builder.Services.AddScoped<SqliteWriteTransaction>();
builder.Services.AddScoped<MeiaEntrada.Web.Services.VendaService>();
builder.Services.AddScoped<MeiaEntrada.Web.Services.VendaConferenciaService>();
builder.Services.AddScoped<MeiaEntrada.Web.Data.Queries.VendaQueries>();
builder.Services.AddScoped<MeiaEntrada.Web.Services.IngressoService>();
builder.Services.AddScoped<MeiaEntrada.Web.Data.Queries.IngressoQueries>();
builder.Services.AddScoped<MeiaEntrada.Web.Services.EventoService>();
builder.Services.AddScoped<MeiaEntrada.Web.Data.Queries.EventoQueries>();
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDataProtection().SetApplicationName("MeiaEntrada").PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration["DataProtection:KeyPath"] ?? Path.Combine(root, "var", "keys")));
var app = builder.Build();
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("pt-BR").AddSupportedCultures("pt-BR").AddSupportedUICultures("pt-BR"));
using (var scope = app.Services.CreateScope())
using (var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext()) AppDbContextFactory.Prepare(db);
app.UseExceptionHandler("/erro/500");
app.UseStatusCodePagesWithReExecute("/erro/{0}");
app.UseStaticFiles();
app.UseRouting();
app.MapGet("/", () => Results.Redirect("/eventos/"));
app.MapControllers();
app.Run();
public partial class Program { }



