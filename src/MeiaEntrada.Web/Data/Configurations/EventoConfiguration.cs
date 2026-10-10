using MeiaEntrada.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace MeiaEntrada.Web.Data.Configurations;
public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> b)
    {
        b.ToTable("Eventos", t => {
            t.HasCheckConstraint("CK_Evento_Nome", "length(trim(Nome)) > 0");
            t.HasCheckConstraint("CK_Evento_Descricao", "length(trim(Descricao)) > 0");
            t.HasCheckConstraint("CK_Evento_Local", "length(trim(Local)) > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).IsRequired();
        b.Property(x => x.Descricao).IsRequired();
        b.Property(x => x.Local).IsRequired();
    }
}
