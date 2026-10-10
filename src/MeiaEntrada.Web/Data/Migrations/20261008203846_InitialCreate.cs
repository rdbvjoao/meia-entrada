using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeiaEntrada.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Local = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Horario = table.Column<TimeOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.Id);
                    table.CheckConstraint("CK_Evento_Descricao", "length(trim(Descricao)) > 0");
                    table.CheckConstraint("CK_Evento_Local", "length(trim(Local)) > 0");
                    table.CheckConstraint("CK_Evento_Nome", "length(trim(Nome)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Ingressos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventoId = table.Column<long>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    PrecoCentavos = table.Column<long>(type: "INTEGER", nullable: false),
                    QuantidadeDisponivel = table.Column<long>(type: "INTEGER", nullable: false),
                    Versao = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingressos", x => x.Id);
                    table.CheckConstraint("CK_Ingresso_Preco", "typeof(PrecoCentavos) = 'integer' AND PrecoCentavos >= 0");
                    table.CheckConstraint("CK_Ingresso_Saldo", "typeof(QuantidadeDisponivel) = 'integer' AND QuantidadeDisponivel >= 0");
                    table.CheckConstraint("CK_Ingresso_Tipo", "Tipo IN ('inteira','meia_entrada')");
                    table.CheckConstraint("CK_Ingresso_Versao", "typeof(Versao) = 'integer' AND Versao >= 1");
                    table.ForeignKey(
                        name: "FK_Ingressos_Eventos_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IngressoId = table.Column<long>(type: "INTEGER", nullable: false),
                    EventoNome = table.Column<string>(type: "TEXT", nullable: false),
                    IngressoTipo = table.Column<string>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<long>(type: "INTEGER", nullable: false),
                    PrecoUnitarioCentavos = table.Column<long>(type: "INTEGER", nullable: false),
                    ValorTotalCentavos = table.Column<long>(type: "INTEGER", nullable: false),
                    RegistradaEmUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Situacao = table.Column<string>(type: "TEXT", nullable: false),
                    CanceladaEmUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                    table.CheckConstraint("CK_Venda_Nome", "length(trim(EventoNome)) > 0");
                    table.CheckConstraint("CK_Venda_Preco", "typeof(PrecoUnitarioCentavos) = 'integer' AND PrecoUnitarioCentavos >= 0");
                    table.CheckConstraint("CK_Venda_Quantidade", "typeof(Quantidade) = 'integer' AND Quantidade > 0");
                    table.CheckConstraint("CK_Venda_Situacao", "(Situacao = 'registrada' AND CanceladaEmUtc IS NULL) OR (Situacao = 'cancelada' AND CanceladaEmUtc IS NOT NULL)");
                    table.CheckConstraint("CK_Venda_Tipo", "IngressoTipo IN ('inteira','meia_entrada')");
                    table.CheckConstraint("CK_Venda_Total", "typeof(ValorTotalCentavos) = 'integer' AND ValorTotalCentavos >= 0 AND PrecoUnitarioCentavos <= 9223372036854775807 / Quantidade AND typeof(PrecoUnitarioCentavos * Quantidade) = 'integer' AND ValorTotalCentavos = PrecoUnitarioCentavos * Quantidade");
                    table.ForeignKey(
                        name: "FK_Vendas_Ingressos_IngressoId",
                        column: x => x.IngressoId,
                        principalTable: "Ingressos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_EventoId",
                table: "Ingressos",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_EventoId_Tipo",
                table: "Ingressos",
                columns: new[] { "EventoId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_IngressoId",
                table: "Vendas",
                column: "IngressoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_RegistradaEmUtc_Id",
                table: "Vendas",
                columns: new[] { "RegistradaEmUtc", "Id" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Ingressos");

            migrationBuilder.DropTable(
                name: "Eventos");
        }
    }
}
