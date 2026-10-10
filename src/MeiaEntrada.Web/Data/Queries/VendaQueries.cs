using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Data.Queries;
public class VendaQueries(IDbContextFactory<AppDbContext> factory)
{
    public MeiaEntrada.Web.ViewModels.ListaViewModel<Venda> Listar(string? page)
    {
        using var db=factory.CreateDbContext();
        return Paginas.Ler(db.Vendas.AsNoTracking().OrderByDescending(x=>x.RegistradaEmUtc).ThenByDescending(x=>x.Id),page,"/vendas/");
    }
    public Venda Obter(long id){using var db=factory.CreateDbContext();return db.Vendas.AsNoTracking().SingleOrDefault(x=>x.Id==id)??throw new RegraException(404,"Venda não encontrada.");}
}
