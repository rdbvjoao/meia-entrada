using System.Security.Cryptography;
using System.Text.Json;
using MeiaEntrada.Web.Data;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.ViewModels.Vendas;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
namespace MeiaEntrada.Web.Services;
public class PrecoAlteradoException(VendaConferenciaViewModel conferencia):RegraException(409,"O preço mudou. Confira o novo total e confirme novamente.")
{ public VendaConferenciaViewModel Conferencia { get; }=conferencia; }
public class VendaConferenciaService(IDbContextFactory<AppDbContext> factory,IDataProtectionProvider protection)
{
    private readonly IDataProtector protector=protection.CreateProtector("MeiaEntrada.Venda.Conferencia.v1");
    public VendaConferenciaViewModel Conferir(VendaSelecaoViewModel form)
    {
        var evento=ValidacaoEntrada.Inteiro(form.EventoId,"EventoId",1);var ingresso=ValidacaoEntrada.Inteiro(form.IngressoId,"IngressoId",1);var quantidade=ValidacaoEntrada.Inteiro(form.Quantidade,"Quantidade",1);
        using var db=factory.CreateDbContext();
        var atual=db.Ingressos.AsNoTracking().Include(x=>x.Evento).SingleOrDefault(x=>x.Id==ingresso)??throw new RegraException(404,"Ingresso não encontrado.");
        if(atual.EventoId!=evento)throw new RegraException(400,"O ingresso não pertence ao evento.","IngressoId");
        Saldo(atual,quantidade);return Criar(atual,quantidade);
    }
    public VendaConferenciaViewModel Criar(Ingresso ingresso,long quantidade)
    {
        var total=ValoresMonetarios.Total(ingresso.PrecoCentavos,quantidade);
        var payload=new ConferenciaDados(ingresso.EventoId,ingresso.Id,ingresso.Tipo,ingresso.PrecoCentavos,quantidade);
        return new(protector.Protect(JsonSerializer.Serialize(payload)),ingresso.Evento.Nome,ingresso.Tipo,quantidade,ingresso.PrecoCentavos,total);
    }
    public ConferenciaDados Ler(string? token)
    {
        try {
            if(string.IsNullOrWhiteSpace(token))throw new CryptographicException();
            var data=JsonSerializer.Deserialize<ConferenciaDados>(protector.Unprotect(token))??throw new CryptographicException();
            if(data.EventoId<=0 || data.IngressoId<=0 || !Enum.IsDefined(data.Tipo))throw new CryptographicException();
            ValoresMonetarios.Total(data.PrecoUnitarioCentavos,data.Quantidade);return data;
        }
        catch(Exception ex) when(ex is CryptographicException or JsonException or RegraException) { throw new RegraException(400,"Conferência inválida. Selecione os ingressos e confira novamente."); }
    }
    public static void Saldo(Ingresso ingresso,long quantidade)
    {
        if(ingresso.QuantidadeDisponivel<quantidade)throw new RegraException(409,$"Saldo insuficiente. Disponíveis: {ingresso.QuantidadeDisponivel}. Corrija a quantidade.","Quantidade");
    }
}
