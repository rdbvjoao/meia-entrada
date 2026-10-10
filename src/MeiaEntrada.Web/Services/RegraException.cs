namespace MeiaEntrada.Web.Services;
public class RegraException(int status, string mensagem, string campo = "") : Exception(mensagem)
{
    public int Status { get; } = status;
    public string Campo { get; } = campo;
}
