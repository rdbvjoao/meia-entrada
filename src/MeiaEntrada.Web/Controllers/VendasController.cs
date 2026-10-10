using MeiaEntrada.Web.Data.Queries;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Vendas;
using Microsoft.AspNetCore.Mvc;
namespace MeiaEntrada.Web.Controllers;
[Route("vendas")]
public class VendasController(VendaService service,VendaConferenciaService conferencia,VendaQueries queries,EventoQueries eventos,IngressoQueries ingressos):DominioController
{
    [HttpGet("")] public IActionResult Index(string? page)=>Executar(()=>View(queries.Listar(page)));
    [HttpGet("{id:long:min(1)}/cancelar")] public IActionResult Cancelar(long id)=>Executar(()=>View(new VendaCancelamentoViewModel(queries.Obter(id))));
    [HttpPost("{id:long:min(1)}/cancelar")] public IActionResult Cancelar([FromRoute] long id,string? confirmar)=>Executar(()=>{
        ValidacaoEntrada.Confirmacao(confirmar);var cancelada=service.Cancelar(id);
        TempData["Sucesso"]=cancelada?"Venda cancelada. Quantidade restituída.":"Venda já cancelada";
        return Redirect($"/vendas/{id}/");
    });
    [HttpGet("nova")] public IActionResult Nova(string? evento)=>Executar(()=>{
        if(evento is not null){if(!long.TryParse(evento,out var id)||id<1)throw new RegraException(404,"Evento não encontrado.");eventos.Obter(id);}
        return View(Preparar(new(){EventoId=evento}));
    });
    [HttpPost("conferir")] public IActionResult Conferir(VendaSelecaoViewModel form)
    {
        try{return View(conferencia.Conferir(form));}catch(RegraException ex){return FormErro(ex,"Nova",Preparar(form));}
    }
    [HttpPost("")] public IActionResult Registrar(string? conferencia,string? confirmar)
    {
        try { ValidacaoEntrada.Confirmacao(confirmar);var id=service.Registrar(conferencia);TempData["Sucesso"]="Venda registrada com sucesso.";return Redirect($"/vendas/{id}/"); }
        catch(PrecoAlteradoException ex){Response.StatusCode=409;ModelState.AddModelError("",ex.Message);return View("Conferir",ex.Conferencia);}
        catch(RegraException ex){return Erro(ex);}
    }
    [HttpGet("{id:long:min(1)}")] public IActionResult Detalhes(long id)=>Executar(()=>View(new VendaDetalheViewModel(queries.Obter(id))));
    private VendaSelecaoViewModel Preparar(VendaSelecaoViewModel form)
    {
        form.Eventos=eventos.Opcoes();
        if(long.TryParse(form.EventoId,out var id) && id>0)form.Ingressos=ingressos.Oferta(id);
        return form;
    }
}
