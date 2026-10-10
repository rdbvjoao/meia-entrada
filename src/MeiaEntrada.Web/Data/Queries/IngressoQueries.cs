using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Data.Queries;
public class IngressoQueries(IDbContextFactory<AppDbContext> factory)
{
    public Ingresso Obter(long id){using var db=factory.CreateDbContext();return db.Ingressos.AsNoTracking().Include(x=>x.Evento).SingleOrDefault(x=>x.Id==id)??throw new RegraException(404,"Ingresso não encontrado.");}
    public ListaViewModel<Ingresso> Listar(string? page,string? evento)
    {
        using var db=factory.CreateDbContext();long? id=null;
        if(evento is not null){try{id=ValidacaoEntrada.Inteiro(evento,"EventoId",1);}catch(RegraException){throw new RegraException(404,"Evento não encontrado.");}
            if(!db.Eventos.Any(x=>x.Id==id))throw new RegraException(404,"Evento não encontrado.");}
        var query=db.Ingressos.AsNoTracking().Include(x=>x.Evento).Where(x=>id==null || x.EventoId==id);
        return Paginas.Ler(query.OrderBy(x=>x.EventoId).ThenBy(x=>x.Tipo).ThenBy(x=>x.Id),page,"/ingressos/",id);
    }
    public List<Ingresso> Oferta(long evento){using var db=factory.CreateDbContext();return db.Ingressos.AsNoTracking().Include(x=>x.Evento).Where(x=>x.EventoId==evento).OrderBy(x=>x.Tipo).ToList();}
}
