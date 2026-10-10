using MeiaEntrada.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MeiaEntrada.Web.Data.Configurations;
public class IngressoConfiguration : IEntityTypeConfiguration<Ingresso>
{
    public void Configure(EntityTypeBuilder<Ingresso> b)
    {
        b.ToTable("Ingressos", t => {
            t.HasCheckConstraint("CK_Ingresso_Tipo", "Tipo IN ('inteira','meia_entrada')");
            t.HasCheckConstraint("CK_Ingresso_Preco", "typeof(PrecoCentavos) = 'integer' AND PrecoCentavos >= 0");
            t.HasCheckConstraint("CK_Ingresso_Saldo", "typeof(QuantidadeDisponivel) = 'integer' AND QuantidadeDisponivel >= 0");
            t.HasCheckConstraint("CK_Ingresso_Versao", "typeof(Versao) = 'integer' AND Versao >= 1");
        });
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Evento).WithMany().HasForeignKey(x => x.EventoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Tipo).HasConversion(x => x.Valor(), x => TipoIngressoTexto.Ler(x));
        b.Property(x => x.Versao).IsConcurrencyToken();
        b.HasIndex(x => new { x.EventoId, x.Tipo }).IsUnique();
        b.HasIndex(x => x.EventoId);
    }
}
