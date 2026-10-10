using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Services;
public class VendaService(SqliteWriteTransaction write,VendaConferenciaService conferencia)
{
    public bool Cancelar(long id) => write.Execute(db=>{
        var venda=db.Vendas.Include(x=>x.Ingresso).SingleOrDefault(x=>x.Id==id)??throw new RegraException(404,"Venda não encontrada.");
        if(venda.Situacao==SituacaoVenda.Cancelada)return false;
        var ingresso=venda.Ingresso;
        ingresso.QuantidadeDisponivel=checked(ingresso.QuantidadeDisponivel+venda.Quantidade);
        ingresso.Versao=checked(ingresso.Versao+1);
        db.SaveChanges();
        venda.Situacao=SituacaoVenda.Cancelada;venda.CanceladaEmUtc=DateTime.UtcNow;
        db.SaveChanges();return true;
    });
    public long Registrar(string? token)
    {
        var conferido=conferencia.Ler(token);
        return write.Execute(db=>{
            var ingresso=db.Ingressos.Include(x=>x.Evento).SingleOrDefault(x=>x.Id==conferido.IngressoId)??throw new RegraException(404,"Ingresso não encontrado. Selecione novamente.");
            if(ingresso.EventoId!=conferido.EventoId || ingresso.Tipo!=conferido.Tipo)throw new RegraException(409,"O evento ou tipo mudou. Refazer a seleção de ingressos é necessário.");
            VendaConferenciaService.Saldo(ingresso,conferido.Quantidade);
            if(ingresso.PrecoCentavos!=conferido.PrecoUnitarioCentavos)throw new PrecoAlteradoException(conferencia.Criar(ingresso,conferido.Quantidade));
            var venda=new Venda{IngressoId=ingresso.Id,EventoNome=ingresso.Evento.Nome,IngressoTipo=ingresso.Tipo,Quantidade=conferido.Quantidade,PrecoUnitarioCentavos=ingresso.PrecoCentavos,ValorTotalCentavos=ValoresMonetarios.Total(ingresso.PrecoCentavos,conferido.Quantidade),RegistradaEmUtc=DateTime.UtcNow,Situacao=SituacaoVenda.Registrada};
            ingresso.QuantidadeDisponivel=checked(ingresso.QuantidadeDisponivel-conferido.Quantidade);ingresso.Versao=checked(ingresso.Versao+1);
            db.SaveChanges();
            db.Add(venda);db.SaveChanges();return venda.Id;
        });
    }
}
