using MeiaEntrada.Web.Models;

namespace MeiaEntrada.Tests.Fixtures;

// Compara todos os campos históricos, sem depender de entidades rastreadas pelo EF.
public sealed record EstadoVenda(long Id, long IngressoId, string EventoNome,
    TipoIngresso Tipo, long Quantidade, long Preco, long Total, DateTime RegistradaEmUtc)
{
    public static EstadoVenda De(Venda venda) => new(venda.Id, venda.IngressoId,
        venda.EventoNome, venda.IngressoTipo, venda.Quantidade,
        venda.PrecoUnitarioCentavos, venda.ValorTotalCentavos, venda.RegistradaEmUtc);
}
