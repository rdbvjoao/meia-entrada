using MeiaEntrada.Web.Models;
namespace MeiaEntrada.Web.ViewModels.Vendas;
public class VendaSelecaoViewModel
{
    public string? EventoId { get; set; }
    public string? IngressoId { get; set; }
    public string? Quantidade { get; set; }
    public List<Evento> Eventos { get; set; } = [];
    public List<Ingresso> Ingressos { get; set; } = [];
}
