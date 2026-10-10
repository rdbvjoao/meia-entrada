namespace MeiaEntrada.Web.Models;
public class Ingresso
{
    public long Id { get; set; }
    public long EventoId { get; set; }
    public Evento Evento { get; set; } = null!;
    public TipoIngresso Tipo { get; set; }
    public long PrecoCentavos { get; set; }
    public long QuantidadeDisponivel { get; set; }
    public long Versao { get; set; } = 1;
}
