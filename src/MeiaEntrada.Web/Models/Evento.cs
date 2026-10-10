namespace MeiaEntrada.Web.Models;
public class Evento
{
    public long Id { get; set; }
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Local { get; set; } = "";
    public DateOnly Data { get; set; }
    public TimeOnly Horario { get; set; }
}
