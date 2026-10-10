namespace MeiaEntrada.Web.ViewModels;
public record Paginacao(int Pagina, bool TemProxima, string Caminho, long? Evento = null)
{
    public string Url(int pagina) => $"{Caminho}?page={pagina}" + (Evento.HasValue ? $"&evento={Evento}" : "");
}
public record ListaViewModel<T>(IReadOnlyList<T> Itens, Paginacao Paginacao);
