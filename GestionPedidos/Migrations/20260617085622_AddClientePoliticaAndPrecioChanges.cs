using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class AddClientePoliticaAndPrecioChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_et_cliente_cliente_id_cliente",
                table: "et_precio");

            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_et_politica_precio_politica_id_politica",
                table: "et_precio");

            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_skus_sku_id_sku",
                table: "et_precio");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_cliente_id_cliente",
                table: "et_precio");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_politica_id_politica",
                table: "et_precio");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_sku_id_sku",
                table: "et_precio");

            migrationBuilder.DropColumn(
                name: "cliente_id_cliente",
                table: "et_precio");

            migrationBuilder.DropColumn(
                name: "politica_id_politica",
                table: "et_precio");

            migrationBuilder.DropColumn(
                name: "sku_id_sku",
                table: "et_precio");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_politica",
                table: "et_precio",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "cl_region",
                table: "et_cliente",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "et_cliente_politica",
                columns: table => new
                {
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_politica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    es_principal = table.Column<bool>(type: "bit", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_cliente_politica", x => new { x.id_cliente, x.id_politica });
                    table.ForeignKey(
                        name: "fk_et_cliente_politica_et_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_cliente_politica_politicas_precios_id_politica",
                        column: x => x.id_politica,
                        principalTable: "et_politica_precio",
                        principalColumn: "id_politica",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_id_cliente",
                table: "et_precio",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_id_politica",
                table: "et_precio",
                column: "id_politica");

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_id_sku",
                table: "et_precio",
                column: "id_sku");

            migrationBuilder.CreateIndex(
                name: "ix_et_cliente_politica_id_politica",
                table: "et_cliente_politica",
                column: "id_politica");

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_et_cliente_id_cliente",
                table: "et_precio",
                column: "id_cliente",
                principalTable: "et_cliente",
                principalColumn: "id_cliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_et_politica_precio_id_politica",
                table: "et_precio",
                column: "id_politica",
                principalTable: "et_politica_precio",
                principalColumn: "id_politica",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_skus_id_sku",
                table: "et_precio",
                column: "id_sku",
                principalTable: "et_sku",
                principalColumn: "id_sku",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_et_cliente_id_cliente",
                table: "et_precio");

            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_et_politica_precio_id_politica",
                table: "et_precio");

            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_skus_id_sku",
                table: "et_precio");

            migrationBuilder.DropTable(
                name: "et_cliente_politica");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_id_cliente",
                table: "et_precio");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_id_politica",
                table: "et_precio");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_id_sku",
                table: "et_precio");

            migrationBuilder.DropColumn(
                name: "cl_region",
                table: "et_cliente");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_politica",
                table: "et_precio",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cliente_id_cliente",
                table: "et_precio",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "politica_id_politica",
                table: "et_precio",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "sku_id_sku",
                table: "et_precio",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_cliente_id_cliente",
                table: "et_precio",
                column: "cliente_id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_politica_id_politica",
                table: "et_precio",
                column: "politica_id_politica");

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_sku_id_sku",
                table: "et_precio",
                column: "sku_id_sku");

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_et_cliente_cliente_id_cliente",
                table: "et_precio",
                column: "cliente_id_cliente",
                principalTable: "et_cliente",
                principalColumn: "id_cliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_et_politica_precio_politica_id_politica",
                table: "et_precio",
                column: "politica_id_politica",
                principalTable: "et_politica_precio",
                principalColumn: "id_politica",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_skus_sku_id_sku",
                table: "et_precio",
                column: "sku_id_sku",
                principalTable: "et_sku",
                principalColumn: "id_sku",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
