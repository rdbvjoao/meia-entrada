using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.ViewModels.Ingressos;
namespace MeiaEntrada.Web.Services;
public class IngressoService(SqliteWriteTransaction write)
{
    public long Salvar(long? id,IngressoFormViewModel form)
    {
        var evento=ValidacaoEntrada.Inteiro(form.EventoId,"EventoId",1);var tipo=ValidacaoEntrada.Tipo(form.Tipo);
        var preco=ValoresMonetarios.Centavos(form.Preco);var saldo=ValidacaoEntrada.Inteiro(form.QuantidadeDisponivel,"QuantidadeDisponivel");
        var versao=id.HasValue?ValidacaoEntrada.Inteiro(form.Versao,"Versao",1):1;
        return write.Execute(db=>{
            var entity=id.HasValue?db.Ingressos.Find(id.Value)??throw new RegraException(404,"Ingresso não encontrado."):new Ingresso();
            if(id.HasValue && versao!=entity.Versao) throw new RegraException(409,"O ingresso mudou desde a abertura do formulário. Revise o saldo atual e confirme novamente.","QuantidadeDisponivel");
            if(!db.Eventos.Any(x=>x.Id==evento)) throw new RegraException(400,"Escolha um evento existente.","EventoId");
            if(id.HasValue && (entity.EventoId!=evento || entity.Tipo!=tipo) && db.Vendas.Any(x=>x.IngressoId==id)) throw new RegraException(409,"Evento e tipo não podem mudar após uma venda, inclusive cancelada.");
            if(db.Ingressos.Any(x=>x.EventoId==evento && x.Tipo==tipo && x.Id!=entity.Id)) throw new RegraException(409,"Este tipo já está cadastrado para o evento.","Tipo");
            entity.EventoId=evento;entity.Tipo=tipo;entity.PrecoCentavos=preco;entity.QuantidadeDisponivel=saldo;
            if(id.HasValue)entity.Versao=checked(entity.Versao+1);else db.Add(entity);
            db.SaveChanges();return entity.Id;
        });
    }
    public void Excluir(long id)=>write.Execute(db=>{
        var entity=db.Ingressos.Find(id)??throw new RegraException(404,"Ingresso não encontrado.");
        if(db.Vendas.Any(x=>x.IngressoId==id))throw new RegraException(409,"Este ingresso tem vendas vinculadas, inclusive canceladas, e não pode ser excluído.");
        db.Remove(entity);return true;
    });
}
