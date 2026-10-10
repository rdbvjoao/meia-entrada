using MeiaEntrada.Web.Models;
namespace MeiaEntrada.Web.ViewModels.Vendas;
public record ConferenciaDados(long EventoId,long IngressoId,TipoIngresso Tipo,long PrecoUnitarioCentavos,long Quantidade);
public record VendaConferenciaViewModel(string Conferencia,string EventoNome,TipoIngresso Tipo,long Quantidade,long PrecoUnitarioCentavos,long ValorTotalCentavos);
