using System.Globalization;
using System.Text.RegularExpressions;
namespace MeiaEntrada.Web.Services;
public static class ValoresMonetarios
{
    public static long Centavos(string? valor)
    {
        if (valor is null || !Regex.IsMatch(valor, @"^[0-9]+([.,][0-9]{1,2})?$") ||
            !decimal.TryParse(valor.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var n) ||
            n > long.MaxValue / 100m)
            throw new RegraException(400, "Informe preço não negativo, sem milhares e com até duas casas decimais.", "Preco");
        return checked((long)(n * 100m));
    }
    public static long Total(long preco, long quantidade)
    {
        if (preco < 0 || quantidade <= 0 || preco > long.MaxValue / quantidade)
            throw new RegraException(400, "Quantidade ou total fora do limite permitido.", "Quantidade");
        return checked(preco * quantidade);
    }
    public static string Brl(long centavos) => (centavos / 100m).ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
    public static string Entrada(long centavos) => (centavos / 100m).ToString("F2", CultureInfo.GetCultureInfo("pt-BR"));
    public static string Brasilia(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")).ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR"));
}
