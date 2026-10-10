using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Data.Queries;
public class EventoQueries(IDbContextFactory<AppDbContext> factory)
{
    public Evento Obter(long id) { using var db=factory.CreateDbContext(); return db.Eventos.AsNoTracking().SingleOrDefault(x=>x.Id==id) ?? throw new RegraException(404,"Evento não encontrado."); }
    public ListaViewModel<Evento> Listar(string? page) { using var db=factory.CreateDbContext(); return Paginas.Ler(db.Eventos.AsNoTracking().OrderBy(x=>x.Data).ThenBy(x=>x.Horario).ThenBy(x=>x.Id),page,"/eventos/"); }
    public List<Evento> Opcoes() { using var db=factory.CreateDbContext(); return db.Eventos.AsNoTracking().OrderBy(x=>x.Data).ThenBy(x=>x.Id).ToList(); }
}
