using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVisibilidadCatalogoHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_et_visibilidad_catalogo",
                table: "et_visibilidad_catalogo");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_producto",
                table: "et_visibilidad_catalogo",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "id_visibilidad",
                table: "et_visibilidad_catalogo",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "id_sku",
                table: "et_visibilidad_catalogo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id_variante",
                table: "et_visibilidad_catalogo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_et_visibilidad_catalogo",
                table: "et_visibilidad_catalogo",
                column: "id_visibilidad");

            migrationBuilder.CreateIndex(
                name: "ix_et_visibilidad_catalogo_id_cliente",
                table: "et_visibilidad_catalogo",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_visibilidad_catalogo_id_sku",
                table: "et_visibilidad_catalogo",
                column: "id_sku");

            migrationBuilder.CreateIndex(
                name: "ix_et_visibilidad_catalogo_id_variante",
                table: "et_visibilidad_catalogo",
                column: "id_variante");

            migrationBuilder.AddForeignKey(
                name: "fk_et_visibilidad_catalogo_et_sku_id_sku",
                table: "et_visibilidad_catalogo",
                column: "id_sku",
                principalTable: "et_sku",
                principalColumn: "id_sku",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_visibilidad_catalogo_et_variante_id_variante",
                table: "et_visibilidad_catalogo",
                column: "id_variante",
                principalTable: "et_variante",
                principalColumn: "id_variante",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_visibilidad_catalogo_et_sku_id_sku",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropForeignKey(
                name: "fk_et_visibilidad_catalogo_et_variante_id_variante",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_et_visibilidad_catalogo",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropIndex(
                name: "ix_et_visibilidad_catalogo_id_cliente",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropIndex(
                name: "ix_et_visibilidad_catalogo_id_sku",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropIndex(
                name: "ix_et_visibilidad_catalogo_id_variante",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropColumn(
                name: "id_visibilidad",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropColumn(
                name: "id_sku",
                table: "et_visibilidad_catalogo");

            migrationBuilder.DropColumn(
                name: "id_variante",
                table: "et_visibilidad_catalogo");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_producto",
                table: "et_visibilidad_catalogo",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_et_visibilidad_catalogo",
                table: "et_visibilidad_catalogo",
                columns: new[] { "id_cliente", "id_producto" });
        }
    }
}
