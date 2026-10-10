using MeiaEntrada.Web.Data.Queries;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Eventos;
using Microsoft.AspNetCore.Mvc;
namespace MeiaEntrada.Web.Controllers;
[Route("eventos")]
public class EventosController(EventoService service,EventoQueries queries):DominioController
{
    [HttpGet("")] public IActionResult Index(string? page)=>Executar(()=>View(queries.Listar(page)));
    [HttpGet("novo")] public IActionResult Novo()=>View(new EventoFormViewModel());
    [HttpPost("novo")] public IActionResult Novo(EventoFormViewModel form)=>Salvar(null,form,"Novo");
    [HttpGet("{id:long:min(1)}")] public IActionResult Detalhes(long id)=>Executar(()=>View(new EventoDetalheViewModel(queries.Obter(id))));
    [HttpGet("{id:long:min(1)}/editar")] public IActionResult Editar(long id)=>Executar(()=>{var e=queries.Obter(id);return View(new EventoFormViewModel{Nome=e.Nome,Descricao=e.Descricao,Local=e.Local,Data=e.Data.ToString("yyyy-MM-dd"),Horario=e.Horario.ToString("HH:mm")});});
    [HttpPost("{id:long:min(1)}/editar")] public IActionResult Editar([FromRoute] long id,EventoFormViewModel form)=>Salvar(id,form,"Editar");
    [HttpGet("{id:long:min(1)}/excluir")] public IActionResult Excluir(long id)=>Executar(()=>View(new EventoDetalheViewModel(queries.Obter(id))));
    [HttpPost("{id:long:min(1)}/excluir")] public IActionResult Excluir([FromRoute] long id,string? confirmar)=>Executar(()=>{ValidacaoEntrada.Confirmacao(confirmar);service.Excluir(id);TempData["Sucesso"]="Evento excluído.";return Redirect("/eventos/");});
    private IActionResult Salvar(long? id,EventoFormViewModel form,string view)
    {
        try { var saved=service.Salvar(id,form);TempData["Sucesso"]="Evento salvo.";return Redirect($"/eventos/{saved}/"); }
        catch(RegraException ex){return FormErro(ex,view,form);}
    }
}
