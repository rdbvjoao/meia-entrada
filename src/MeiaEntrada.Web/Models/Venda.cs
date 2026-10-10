namespace MeiaEntrada.Web.Models;
public class Venda
{
    public long Id { get; set; }
    public long IngressoId { get; set; }
    public Ingresso Ingresso { get; set; } = null!;
    public string EventoNome { get; set; } = "";
    public TipoIngresso IngressoTipo { get; set; }
    public long Quantidade { get; set; }
    public long PrecoUnitarioCentavos { get; set; }
    public long ValorTotalCentavos { get; set; }
    public DateTime RegistradaEmUtc { get; set; }
    public SituacaoVenda Situacao { get; set; }
    public DateTime? CanceladaEmUtc { get; set; }
}
