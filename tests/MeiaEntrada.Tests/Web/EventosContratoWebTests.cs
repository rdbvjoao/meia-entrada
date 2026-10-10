using System.Net;
using MeiaEntrada.Tests.Fixtures;
namespace MeiaEntrada.Tests.Web;
public class EventosContratoWebTests
{
    public static Dictionary<string,string> Campos(string nome = "Evento novo") => new() { ["Nome"]=nome, ["Descricao"]="Descrição válida", ["Local"]="Sala A", ["Data"]="2026-12-01", ["Horario"]="19:00" };
    [Fact] public async Task CrudConfirmacaoEValidacao()
    {
        using var db = new SqliteFixture(); using var app = new WebAppFactory(db); using var c = app.Client();
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/eventos/")).StatusCode);
        var fields=Campos(); fields["Nome"]=" "; var invalid=await AntiforgeryClient.Post(c,"/eventos/novo/","/eventos/novo/",fields);
        Assert.Equal(HttpStatusCode.BadRequest,invalid.StatusCode); Assert.Contains("Sala A",await invalid.Content.ReadAsStringAsync());
        var response=await AntiforgeryClient.Post(c,"/eventos/novo/","/eventos/novo/",Campos("<script>alert(1)</script>"));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode); var url=response.Headers.Location!.ToString();
        var html=await c.GetStringAsync(url); Assert.DoesNotContain("<script>alert",html); Assert.Contains("&lt;script&gt;",html);
        var edit=await AntiforgeryClient.Post(c,url+"editar/",url+"editar/",Campos("Editado")); Assert.Equal(HttpStatusCode.Redirect,edit.StatusCode);
        Assert.Contains("Editado",await c.GetStringAsync(url));
        await c.GetAsync(url+"excluir/"); using(var check=db.CreateDbContext()) Assert.Single(check.Eventos);
        var delete=await AntiforgeryClient.Post(c,url+"excluir/",url+"excluir/",new(){["Confirmar"]="sim"}); Assert.Equal(HttpStatusCode.Redirect,delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync(url)).StatusCode);
    }
    [Fact] public async Task ContratosEListagem()
    {
        using var db = new SqliteFixture(); var (id,_)=db.Oferta(); using var app=new WebAppFactory(db); using var c=app.Client();
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsync("/eventos/novo/",new FormUrlEncodedContent(Campos()))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await AntiforgeryClient.Post(c,$"/eventos/{id}/excluir/",$"/eventos/{id}/excluir/",new(){["Confirmar"]="sim"})).StatusCode);
        foreach(var page in new[]{"0","abc","999"}) Assert.Equal(HttpStatusCode.NotFound,(await c.GetAsync("/eventos/?page="+page)).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed,(await c.PutAsync($"/eventos/{id}/",null)).StatusCode);
    }
}
