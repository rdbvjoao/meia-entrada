using MeiaEntrada.Web.Models;
namespace MeiaEntrada.Web.ViewModels.Ingressos;
public class IngressoFormViewModel
{
    public string? EventoId { get; set; }
    public string? Tipo { get; set; }
    public string? Preco { get; set; }
    public string? QuantidadeDisponivel { get; set; }
    public string? Versao { get; set; }
    public long? SaldoAtual { get; set; }
    public List<Evento> Eventos { get; set; } = [];
}
