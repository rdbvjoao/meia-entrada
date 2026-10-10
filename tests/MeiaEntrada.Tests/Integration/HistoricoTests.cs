using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data.Queries;
namespace MeiaEntrada.Tests.Integration;
public class HistoricoTests
{
    [Fact] public void PaginaOrdenaEPreserva()
    {
        using var db=new SqliteFixture();for(var n=0;n<26;n++)db.VendaPreparada(cancelada:n%2==0);
        Dictionary<long,EstadoVenda> originais;
        using(var edit=db.CreateDbContext()){
            foreach(var venda in edit.Vendas)venda.RegistradaEmUtc=new DateTime(2026,1,1,12,0,0,DateTimeKind.Utc);
            originais=edit.Vendas.ToDictionary(x=>x.Id,EstadoVenda.De);
            foreach(var evento in edit.Eventos)evento.Nome="Nome atual";
            foreach(var ingresso in edit.Ingressos)ingresso.PrecoCentavos=9000;
            edit.SaveChanges();
        }
        var q=new VendaQueries(db);var pagina=q.Listar(null);Assert.Equal(25,pagina.Itens.Count);Assert.True(pagina.Paginacao.TemProxima);
        Assert.Equal(26,pagina.Itens[0].Id);Assert.Single(q.Listar("2").Itens);
        Assert.All(pagina.Itens,v=>{Assert.Equal("Sessão original",v.EventoNome);Assert.Equal(4000,v.PrecoUnitarioCentavos);Assert.Equal(12000,v.ValorTotalCentavos);});
        Assert.All(pagina.Itens.Concat(q.Listar("2").Itens),v=>Assert.Equal(originais[v.Id],EstadoVenda.De(v)));
    }
}
