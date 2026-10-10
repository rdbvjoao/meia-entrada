using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Ingressos;
namespace MeiaEntrada.Tests.Integration;
public class IngressosTests
{
    public static IngressoFormViewModel Form(long evento,string tipo="meia_entrada",string preco="0",string saldo="0",string versao="1") => new(){EventoId=evento.ToString(),Tipo=tipo,Preco=preco,QuantidadeDisponivel=saldo,Versao=versao};
    [Fact] public void CrudUnicidadeVersaoEVinculos()
    {
        using var f=new SqliteFixture();var(e,i)=f.Oferta();var s=new IngressoService(new SqliteWriteTransaction(f));
        var id=s.Salvar(null,Form(e));Assert.Throws<RegraException>(()=>s.Salvar(null,Form(e)));
        s.Salvar(id,Form(e,saldo:"12"));Assert.Equal(409,Assert.Throws<RegraException>(()=>s.Salvar(id,Form(e))).Status);
        s.Excluir(id);using(var db=f.CreateDbContext()) Assert.Null(db.Ingressos.Find(id));
        f.VendaPreparada(cancelada:true);using var final=f.CreateDbContext();var vendido=final.Vendas.Single().IngressoId;var ingresso=final.Ingressos.Find(vendido)!;
        Assert.Equal(409,Assert.Throws<RegraException>(()=>s.Salvar(vendido,Form(ingresso.EventoId))).Status);
        Assert.Equal(409,Assert.Throws<RegraException>(()=>s.Excluir(vendido)).Status);
    }
    [Theory][InlineData("inteira","-1","1")][InlineData("outro","1","1")][InlineData("inteira","1.001","1")][InlineData("inteira","1","1.5")][InlineData("inteira","1","-1")]
    public void InvalidoSemMutacao(string tipo,string preco,string saldo)
    {
        using var f=new SqliteFixture();var(e,_)=f.Oferta();var s=new IngressoService(new SqliteWriteTransaction(f));
        Assert.Throws<RegraException>(()=>s.Salvar(null,Form(e,tipo,preco,saldo)));
        using var db=f.CreateDbContext();Assert.Single(db.Ingressos);
    }
}
