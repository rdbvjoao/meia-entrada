using MeiaEntrada.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Ingresso> Ingressos => Set<Ingresso>();
    public DbSet<Venda> Vendas => Set<Venda>();
    protected override void OnModelCreating(ModelBuilder builder) => builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
