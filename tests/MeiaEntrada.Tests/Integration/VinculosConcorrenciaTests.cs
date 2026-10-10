using MeiaEntrada.Tests.Fixtures;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Services;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Tests.Integration;
public class VinculosConcorrenciaTests
{
    [Theory][InlineData("excluirIngresso")][InlineData("excluirEvento")][InlineData("mudarTipo")][InlineData("mudarEvento")][InlineData("preco")][InlineData("saldo")][InlineData("nome")]
    public async Task DisputaPreservaOrigemEHistorico(string operacao)
    {
        using var db=new SqliteFixture();var(e,i)=db.Oferta();var v=new VendasFixture(db);var token=v.Conferir(e,i).Conferencia;
        var ingresso=new IngressoService(new SqliteWriteTransaction(db));var evento=new EventoService(new SqliteWriteTransaction(db));
        var outro=evento.Salvar(null,new(){Nome="Destino",Descricao="D",Local="L",Data="2026-01-01",Horario="10:00"});
        var resultados=await SqliteFixture.Concorrer(2,n=>{
            try{
                if(n==0)v.Service.Registrar(token);
                else switch(operacao){
                    case "excluirIngresso":ingresso.Excluir(i);break;
                    case "excluirEvento":evento.Excluir(e);break;
                    case "mudarTipo":ingresso.Salvar(i,IngressosTests.Form(e,"meia_entrada","40","10"));break;
                    case "mudarEvento":ingresso.Salvar(i,IngressosTests.Form(outro,"inteira","40","10"));break;
                    case "preco":ingresso.Salvar(i,IngressosTests.Form(e,"inteira","50","10"));break;
                    case "saldo":ingresso.Salvar(i,IngressosTests.Form(e,"inteira","40","20"));break;
                    case "nome":evento.Salvar(e,new(){Nome="Novo nome",Descricao="D",Local="L",Data="2026-01-01",Horario="10:00"});break;
                }
                return 200;
            }catch(RegraException ex){Assert.Contains(ex.Status,new[]{404,409});return ex.Status;}
        });
        using var final=db.CreateDbContext();
        var atual=final.Ingressos.Find(i);
        if(resultados[0]==200){
            var venda=Assert.Single(final.Vendas.Include(x=>x.Ingresso).ThenInclude(x=>x.Evento));
            Assert.Equal((i,e,MeiaEntrada.Web.Models.TipoIngresso.Inteira,1L,4000L,4000L),
                (venda.IngressoId,venda.Ingresso.EventoId,venda.IngressoTipo,venda.Quantidade,venda.PrecoUnitarioCentavos,venda.ValorTotalCentavos));
            Assert.Equal(MeiaEntrada.Web.Models.SituacaoVenda.Registrada,venda.Situacao);
            Assert.Null(venda.CanceladaEmUtc);Assert.Equal(DateTimeKind.Utc,venda.RegistradaEmUtc.Kind);
            Assert.Contains(venda.EventoNome,operacao=="nome"?new[]{"Sessão original","Novo nome"}:new[]{"Sessão original"});
            Assert.Equal(operacao=="saldo" && resultados[1]==200?19:9,atual!.QuantidadeDisponivel);
            Assert.Equal(operacao=="saldo" && resultados[1]==200?3:2,atual.Versao);
            Assert.Equal(4000,atual.PrecoCentavos);
            Assert.Equal(operacao=="nome" || operacao=="saldo" && resultados[1]==200?200:409,resultados[1]);
            if(operacao=="nome")Assert.Equal("Novo nome",final.Eventos.Find(e)!.Nome);
        }else{
            Assert.Empty(final.Vendas);Assert.Equal(200,resultados[1]);
            Assert.Contains(operacao,new[]{"excluirIngresso","mudarTipo","mudarEvento","preco"});
            Assert.Equal(operacao=="excluirIngresso"?404:409,resultados[0]);
            if(operacao=="excluirIngresso")Assert.Null(atual);
            else{
                Assert.Equal((10L,2L),(atual!.QuantidadeDisponivel,atual.Versao));
                Assert.Equal(operacao=="mudarEvento"?outro:e,atual.EventoId);
                Assert.Equal(operacao=="mudarTipo"?MeiaEntrada.Web.Models.TipoIngresso.MeiaEntrada:MeiaEntrada.Web.Models.TipoIngresso.Inteira,atual.Tipo);
                Assert.Equal(operacao=="preco"?5000:4000,atual.PrecoCentavos);
            }
        }
        Assert.NotNull(final.Eventos.Find(e));
    }
}
