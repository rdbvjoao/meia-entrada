using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.ViewModels.Eventos;
namespace MeiaEntrada.Web.Services;
public class EventoService(SqliteWriteTransaction write)
{
    public long Salvar(long? id,EventoFormViewModel form)
    {
        var nome=ValidacaoEntrada.Texto(form.Nome,"Nome"); var descricao=ValidacaoEntrada.Texto(form.Descricao,"Descricao"); var local=ValidacaoEntrada.Texto(form.Local,"Local");
        var data=ValidacaoEntrada.Data(form.Data); var hora=ValidacaoEntrada.Horario(form.Horario);
        return write.Execute(db=>{
            var entity=id.HasValue ? db.Eventos.Find(id.Value) ?? throw new RegraException(404,"Evento não encontrado.") : new Evento();
            entity.Nome=nome; entity.Descricao=descricao; entity.Local=local; entity.Data=data; entity.Horario=hora;
            if(!id.HasValue) db.Add(entity); db.SaveChanges(); return entity.Id;
        });
    }
    public void Excluir(long id) => write.Execute(db=>{
        var entity=db.Eventos.Find(id) ?? throw new RegraException(404,"Evento não encontrado.");
        if(db.Ingressos.Any(x=>x.EventoId==id)) throw new RegraException(409,"Este evento tem ingressos ou vendas vinculados e não pode ser excluído.");
        db.Remove(entity); return true;
    });
}
