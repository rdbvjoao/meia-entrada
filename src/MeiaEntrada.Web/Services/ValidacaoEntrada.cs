using System.Globalization;
using System.Text.RegularExpressions;
using MeiaEntrada.Web.Models;
namespace MeiaEntrada.Web.Services;
public static class ValidacaoEntrada
{
    public static string Texto(string? valor, string campo) => string.IsNullOrWhiteSpace(valor)
        ? throw new RegraException(400, "Preencha este campo.", campo) : valor.Trim();
    public static long Inteiro(string? valor, string campo, long minimo = 0)
    {
        if (valor is null || !Regex.IsMatch(valor, @"^[0-9]+$") ||
            !long.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n < minimo)
            throw new RegraException(400, $"Informe um número inteiro maior ou igual a {minimo}.", campo);
        return n;
    }
    public static DateOnly Data(string? valor)
    {
        if (!DateOnly.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            throw new RegraException(400, "Informe uma data válida.", "Data");
        return data;
    }
    public static TimeOnly Horario(string? valor)
    {
        if (!TimeOnly.TryParseExact(valor, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
            throw new RegraException(400, "Informe um horário válido.", "Horario");
        return hora;
    }
    public static TipoIngresso Tipo(string? valor) => valor switch {
        "inteira" => TipoIngresso.Inteira, "meia_entrada" => TipoIngresso.MeiaEntrada,
        _ => throw new RegraException(400, "Escolha Inteira ou Meia-entrada.", "Tipo") };
    public static void Confirmacao(string? valor)
    {
        if (valor != "sim") throw new RegraException(400, "Confirme a operação.");
    }
}
