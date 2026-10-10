using MeiaEntrada.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MeiaEntrada.Web.Data.Configurations;
public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> b)
    {
        b.ToTable("Vendas", t => {
            t.HasCheckConstraint("CK_Venda_Nome", "length(trim(EventoNome)) > 0");
            t.HasCheckConstraint("CK_Venda_Tipo", "IngressoTipo IN ('inteira','meia_entrada')");
            t.HasCheckConstraint("CK_Venda_Quantidade", "typeof(Quantidade) = 'integer' AND Quantidade > 0");
            t.HasCheckConstraint("CK_Venda_Preco", "typeof(PrecoUnitarioCentavos) = 'integer' AND PrecoUnitarioCentavos >= 0");
            t.HasCheckConstraint("CK_Venda_Total", "typeof(ValorTotalCentavos) = 'integer' AND ValorTotalCentavos >= 0 AND PrecoUnitarioCentavos <= 9223372036854775807 / Quantidade AND typeof(PrecoUnitarioCentavos * Quantidade) = 'integer' AND ValorTotalCentavos = PrecoUnitarioCentavos * Quantidade");
            t.HasCheckConstraint("CK_Venda_Situacao", "(Situacao = 'registrada' AND CanceladaEmUtc IS NULL) OR (Situacao = 'cancelada' AND CanceladaEmUtc IS NOT NULL)");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.EventoNome).IsRequired();
        b.HasOne(x => x.Ingresso).WithMany().HasForeignKey(x => x.IngressoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.IngressoTipo).HasConversion(x => x.Valor(), x => TipoIngressoTexto.Ler(x));
        b.Property(x => x.Situacao).HasConversion(x => x == SituacaoVenda.Registrada ? "registrada" : "cancelada", x => x == "registrada" ? SituacaoVenda.Registrada : SituacaoVenda.Cancelada);
        b.Property(x => x.RegistradaEmUtc).HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        b.Property(x => x.CanceladaEmUtc).HasConversion(x => x, x => x.HasValue ? DateTime.SpecifyKind(x.Value, DateTimeKind.Utc) : (DateTime?)null);
        b.HasIndex(x => new { x.RegistradaEmUtc, x.Id }).IsDescending(true, true);
        b.HasIndex(x => x.IngressoId);
    }
}
