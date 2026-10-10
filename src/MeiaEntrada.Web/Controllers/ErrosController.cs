using Microsoft.AspNetCore.Mvc;
namespace MeiaEntrada.Web.Controllers;
public class ErrosController : Controller
{
    [Route("/erro/{status:int}"), IgnoreAntiforgeryToken]
    public IActionResult Mostrar(int status)
    {
        Response.StatusCode = status;
        ViewBag.Status = status;
        ViewBag.Mensagem = status switch {
            400 => "Revise os dados e envie novamente pelo formulário.",
            404 => "Registro ou página não encontrado.",
            405 => "Esta operação não está disponível.",
            409 => "Os dados foram alterados. Revise e tente novamente.",
            503 => "Sistema ocupado; tente novamente",
            _ => "Não foi possível concluir a operação. Consulte o histórico antes de registrar outra venda." };
        return View("~/Views/Shared/Erro.cshtml");
    }
}
