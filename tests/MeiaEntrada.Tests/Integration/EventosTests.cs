using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Eventos;
namespace MeiaEntrada.Tests.Integration;
public class EventosTests
{
    [Fact] public void CrudEHistorico()
    {
        using var f=new SqliteFixture(); var s=new EventoService(new SqliteWriteTransaction(f));
        var form=new EventoFormViewModel { Nome=" Nome ",Descricao=" Texto ",Local=" Local ",Data="2000-01-01",Horario="00:00" };
        var id=s.Salvar(null,form); var outro=s.Salvar(null,form); Assert.NotEqual(id,outro);
        form.Nome="Alterado"; s.Salvar(id,form);
        using(var db=f.CreateDbContext()) Assert.Equal("Alterado",db.Eventos.Find(id)!.Nome);
        s.Excluir(id);
        var venda=f.VendaPreparada(cancelada:true);
        using var check=f.CreateDbContext(); var ingresso=check.Ingressos.Single(); var evento=check.Eventos.Find(ingresso.EventoId)!;
        s.Salvar(evento.Id,form); Assert.Throws<RegraException>(()=>s.Excluir(evento.Id)); Assert.Equal("Sessão original",check.Vendas.Find(venda)!.EventoNome);
    }
    [Theory][InlineData(" ","2026-12-01","19:00")][InlineData("nome","2026-02-30","19:00")][InlineData("nome","2026-12-01","25:00")]
    public void Rejeita(string nome,string data,string hora)
    {
        using var f=new SqliteFixture(); var s=new EventoService(new SqliteWriteTransaction(f));
        Assert.Throws<RegraException>(()=>s.Salvar(null,new(){Nome=nome,Descricao="D",Local="L",Data=data,Horario=hora}));
        using var db=f.CreateDbContext(); Assert.Empty(db.Eventos);
    }
}
