using MeiaEntrada.Web.Services;
namespace MeiaEntrada.Tests.Unit;
public class ValidacaoEntradaTests
{
    [Theory]
    [InlineData(null)][InlineData("0")][InlineData("-1")][InlineData("1.0")][InlineData("+1")][InlineData("1e2")][InlineData("9223372036854775808")]
    public void IdDeveSerInteiroPositivo(string? valor) => Assert.Throws<RegraException>(() => ValidacaoEntrada.Inteiro(valor,"Id",1));

    [Fact]
    public void LimitesInteirosEPrecoSemTetoComercialAdicional()
    {
        Assert.Equal(long.MaxValue,ValidacaoEntrada.Inteiro(long.MaxValue.ToString(),"Id",1));
        Assert.Equal(0,ValidacaoEntrada.Inteiro("0","Saldo"));
        Assert.Equal(long.MaxValue,ValoresMonetarios.Total(long.MaxValue,1));
        Assert.Equal(long.MaxValue-1,ValoresMonetarios.Total(long.MaxValue/2,2));
        Assert.Throws<RegraException>(()=>ValoresMonetarios.Total(long.MaxValue/2+1,2));
    }

    [Theory]
    [InlineData("40,00",4000)][InlineData("40.01",4001)][InlineData("0",0)][InlineData("92233720368547758.07",long.MaxValue)]
    public void ConverteSemArredondar(string valor,long esperado) => Assert.Equal(esperado, ValoresMonetarios.Centavos(valor));
    [Theory]
    [InlineData("-1")][InlineData("1.001")][InlineData("NaN")][InlineData("Infinity")][InlineData("1e2")][InlineData("1.000,00")][InlineData("92233720368547758.08")][InlineData("")]
    public void RejeitaPreco(string valor) => Assert.Throws<RegraException>(() => ValoresMonetarios.Centavos(valor));
    [Theory]
    [InlineData("1.5")][InlineData("-1")][InlineData("")][InlineData("9223372036854775808")]
    public void RejeitaQuantidade(string valor) => Assert.Throws<RegraException>(() => ValidacaoEntrada.Inteiro(valor,"Quantidade"));
    [Fact] public void TextoDataHorario()
    {
        Assert.Throws<RegraException>(() => ValidacaoEntrada.Texto(" \t ", "Nome"));
        Assert.Equal("nome", ValidacaoEntrada.Texto(" nome ", "Nome"));
        Assert.Throws<RegraException>(() => ValidacaoEntrada.Data("2026-02-30"));
        Assert.Throws<RegraException>(() => ValidacaoEntrada.Horario("25:00"));
        Assert.Equal(new DateOnly(2000,1,1), ValidacaoEntrada.Data("2000-01-01"));
    }
}
