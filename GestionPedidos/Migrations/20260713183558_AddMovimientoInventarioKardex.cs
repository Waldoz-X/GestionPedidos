using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimientoInventarioKardex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "et_movimiento_inventario",
                columns: table => new
                {
                    id_movimiento = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_sku = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    no_cantidad = table.Column<int>(type: "int", nullable: false),
                    cl_tipo_movimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ds_motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_movimiento_inventario", x => x.id_movimiento);
                    table.ForeignKey(
                        name: "fk_et_movimiento_inventario_skus_id_sku",
                        column: x => x.id_sku,
                        principalTable: "et_sku",
                        principalColumn: "id_sku",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_et_movimiento_inventario_id_sku",
                table: "et_movimiento_inventario",
                column: "id_sku");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "et_movimiento_inventario");
        }
    }
}
