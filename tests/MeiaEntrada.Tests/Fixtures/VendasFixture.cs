using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Vendas;
using Microsoft.AspNetCore.DataProtection;
namespace MeiaEntrada.Tests.Fixtures;
public sealed class VendasFixture(SqliteFixture db)
{
    public VendaConferenciaService Conferencia { get; } = new(db,new EphemeralDataProtectionProvider());
    public VendaService Service => new(new SqliteWriteTransaction(db),Conferencia);
    public VendaConferenciaViewModel Conferir(long evento,long ingresso,string quantidade="1") => Conferencia.Conferir(new(){EventoId=evento.ToString(),IngressoId=ingresso.ToString(),Quantidade=quantidade});
}
