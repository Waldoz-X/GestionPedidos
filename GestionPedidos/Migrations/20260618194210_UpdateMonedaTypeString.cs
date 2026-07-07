using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMonedaTypeString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_precio_c_catalogo_elemento_cl_moneda_id_catalogo_elemento",
                table: "et_precio");

            migrationBuilder.DropIndex(
                name: "ix_et_precio_cl_moneda_id_catalogo_elemento",
                table: "et_precio");

            migrationBuilder.DropColumn(
                name: "cl_moneda_id_catalogo_elemento",
                table: "et_precio");

            migrationBuilder.AddColumn<string>(
                name: "cl_moneda",
                table: "et_precio",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cl_moneda",
                table: "et_precio");

            migrationBuilder.AddColumn<int>(
                name: "cl_moneda_id_catalogo_elemento",
                table: "et_precio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_cl_moneda_id_catalogo_elemento",
                table: "et_precio",
                column: "cl_moneda_id_catalogo_elemento");

            migrationBuilder.AddForeignKey(
                name: "fk_et_precio_c_catalogo_elemento_cl_moneda_id_catalogo_elemento",
                table: "et_precio",
                column: "cl_moneda_id_catalogo_elemento",
                principalTable: "c_catalogo_elemento",
                principalColumn: "id_catalogo_elemento",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
