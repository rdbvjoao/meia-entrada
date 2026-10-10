namespace MeiaEntrada.Web.Models;
public enum TipoIngresso { Inteira, MeiaEntrada }
public static class TipoIngressoTexto
{
    public static string Valor(this TipoIngresso tipo) => tipo switch { TipoIngresso.Inteira => "inteira", TipoIngresso.MeiaEntrada => "meia_entrada", _ => throw new ArgumentOutOfRangeException(nameof(tipo)) };
    public static string Rotulo(this TipoIngresso tipo) => tipo == TipoIngresso.Inteira ? "Inteira" : "Meia-entrada";
    public static TipoIngresso Ler(string valor) => valor switch { "inteira" => TipoIngresso.Inteira, "meia_entrada" => TipoIngresso.MeiaEntrada, _ => throw new ArgumentException("Tipo inválido.") };
}
