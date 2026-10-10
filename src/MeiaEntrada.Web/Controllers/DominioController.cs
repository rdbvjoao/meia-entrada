using MeiaEntrada.Web.Services;
using Microsoft.AspNetCore.Mvc;
namespace MeiaEntrada.Web.Controllers;
public abstract class DominioController : Controller
{
    protected IActionResult Executar(Func<IActionResult> action)
    {
        try { return action(); }
        catch (RegraException ex) { return Erro(ex); }
    }
    protected IActionResult Erro(RegraException ex)
    {
        Response.StatusCode=ex.Status; ViewBag.Status=ex.Status; ViewBag.Mensagem=ex.Message;
        return View("~/Views/Shared/Erro.cshtml");
    }
    protected IActionResult FormErro(RegraException ex,string view,object model)
    {
        if(ex.Status is 404 or 500 or 503) return Erro(ex);
        Response.StatusCode=ex.Status; ModelState.AddModelError(ex.Campo,ex.Message); return View(view,model);
    }
}
