using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels;
namespace MeiaEntrada.Web.Data.Queries;
public static class Paginas
{
    public static ListaViewModel<T> Ler<T>(IQueryable<T> query, string? page, string caminho, long? evento = null)
    {
        var pagina = 1;
        if (page is not null && (!int.TryParse(page, out pagina) || pagina < 1 || pagina > int.MaxValue / 25))
            throw new RegraException(404, "Página não encontrada.");
        var itens = query.Skip((pagina - 1) * 25).Take(26).ToList();
        if (pagina > 1 && itens.Count == 0) throw new RegraException(404, "Página não encontrada.");
        return new(itens.Take(25).ToList(), new(pagina, itens.Count > 25, caminho, evento));
    }
}
