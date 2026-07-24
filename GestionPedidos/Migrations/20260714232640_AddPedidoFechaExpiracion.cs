using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidoFechaExpiracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fe_expiracion",
                table: "et_pedido",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fe_expiracion",
                table: "et_pedido");
        }
    }
}
