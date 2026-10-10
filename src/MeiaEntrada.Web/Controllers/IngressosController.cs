using MeiaEntrada.Web.Data.Queries;
using MeiaEntrada.Web.Models;
using MeiaEntrada.Web.Services;
using MeiaEntrada.Web.ViewModels.Ingressos;
using Microsoft.AspNetCore.Mvc;
namespace MeiaEntrada.Web.Controllers;
[Route("ingressos")]
public class IngressosController(IngressoService service,IngressoQueries queries,EventoQueries eventos):DominioController
{
    [HttpGet("")] public IActionResult Index(string? page,string? evento)=>Executar(()=>View(queries.Listar(page,evento)));
    [HttpGet("novo")] public IActionResult Novo(string? evento)=>Executar(()=>{
        if(!string.IsNullOrEmpty(evento)){if(!long.TryParse(evento,out var id)||id<1)throw new RegraException(404,"Evento não encontrado.");eventos.Obter(id);}
        return View(new IngressoFormViewModel{EventoId=evento,Eventos=eventos.Opcoes()});
    });
    [HttpPost("novo")] public IActionResult Novo(IngressoFormViewModel form)=>Salvar(null,form,"Novo");
    [HttpGet("{id:long:min(1)}")] public IActionResult Detalhes(long id)=>Executar(()=>View(queries.Obter(id)));
    [HttpGet("{id:long:min(1)}/editar")] public IActionResult Editar(long id)=>Executar(()=>{var i=queries.Obter(id);return View(new IngressoFormViewModel{EventoId=i.EventoId.ToString(),Tipo=i.Tipo.Valor(),Preco=ValoresMonetarios.Entrada(i.PrecoCentavos),QuantidadeDisponivel=i.QuantidadeDisponivel.ToString(),Versao=i.Versao.ToString(),Eventos=eventos.Opcoes()});});
    [HttpPost("{id:long:min(1)}/editar")] public IActionResult Editar([FromRoute] long id,IngressoFormViewModel form)=>Salvar(id,form,"Editar");
    [HttpGet("{id:long:min(1)}/excluir")] public IActionResult Excluir(long id)=>Executar(()=>View(queries.Obter(id)));
    [HttpPost("{id:long:min(1)}/excluir")] public IActionResult Excluir([FromRoute] long id,string? confirmar)=>Executar(()=>{ValidacaoEntrada.Confirmacao(confirmar);service.Excluir(id);TempData["Sucesso"]="Ingresso excluído.";return Redirect("/ingressos/");});
    private IActionResult Salvar(long? id,IngressoFormViewModel form,string view)
    {
        try{var saved=service.Salvar(id,form);TempData["Sucesso"]="Ingresso salvo.";return Redirect($"/ingressos/{saved}/");}
        catch(RegraException ex){
            form.Eventos=eventos.Opcoes();
            if(ex.Status==409 && id.HasValue){var atual=queries.Obter(id.Value);form.SaldoAtual=atual.QuantidadeDisponivel;form.Versao=atual.Versao.ToString();ModelState.Remove("Versao");}
            return FormErro(ex,view,form);
        }
    }
}
