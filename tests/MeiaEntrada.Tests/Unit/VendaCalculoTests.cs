using MeiaEntrada.Web.Services;
namespace MeiaEntrada.Tests.Unit;
public class VendaCalculoTests
{
    [Fact] public void PrecisaoELimites(){Assert.Equal(12000,ValoresMonetarios.Total(4000,3));Assert.Equal(0,ValoresMonetarios.Total(0,long.MaxValue));Assert.Throws<RegraException>(()=>ValoresMonetarios.Total(long.MaxValue,2));Assert.Throws<RegraException>(()=>ValoresMonetarios.Total(1,0));}
}
