using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class FixPedidoForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_linea_pedido_pedidos_pedido_id_pedido",
                table: "et_linea_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_linea_pedido_skus_sku_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_asp_net_users_usuario_captura_id",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_et_cliente_cliente_id_cliente",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_et_direccion_cliente_direccion_envio_id_direccion",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_politicas_precios_politica_id_politica",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_estatus_anterior_id_catalogo_elemento",
                table: "h_historial_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_estatus_nuevo_id_catalogo_elemento",
                table: "h_historial_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_pedido_cliente_id_cliente",
                table: "et_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_pedido_usuario_captura_id",
                table: "et_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_linea_pedido_pedido_id_pedido",
                table: "et_linea_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_linea_pedido_sku_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.DropColumn(
                name: "cliente_id_cliente",
                table: "et_pedido");

            migrationBuilder.DropColumn(
                name: "usuario_captura_id",
                table: "et_pedido");

            migrationBuilder.DropColumn(
                name: "pedido_id_pedido",
                table: "et_linea_pedido");

            migrationBuilder.DropColumn(
                name: "sku_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.RenameColumn(
                name: "estatus_nuevo_id_catalogo_elemento",
                table: "h_historial_pedido",
                newName: "id_elem_estatus_nuevo");

            migrationBuilder.RenameColumn(
                name: "estatus_anterior_id_catalogo_elemento",
                table: "h_historial_pedido",
                newName: "id_elem_estatus_anterior");

            migrationBuilder.RenameIndex(
                name: "ix_h_historial_pedido_estatus_nuevo_id_catalogo_elemento",
                table: "h_historial_pedido",
                newName: "ix_h_historial_pedido_id_elem_estatus_nuevo");

            migrationBuilder.RenameIndex(
                name: "ix_h_historial_pedido_estatus_anterior_id_catalogo_elemento",
                table: "h_historial_pedido",
                newName: "ix_h_historial_pedido_id_elem_estatus_anterior");

            migrationBuilder.RenameColumn(
                name: "politica_id_politica",
                table: "et_pedido",
                newName: "et_usuario_id");

            migrationBuilder.RenameColumn(
                name: "direccion_envio_id_direccion",
                table: "et_pedido",
                newName: "et_direccion_cliente_id_direccion");

            migrationBuilder.RenameIndex(
                name: "ix_et_pedido_politica_id_politica",
                table: "et_pedido",
                newName: "ix_et_pedido_et_usuario_id");

            migrationBuilder.RenameIndex(
                name: "ix_et_pedido_direccion_envio_id_direccion",
                table: "et_pedido",
                newName: "ix_et_pedido_et_direccion_cliente_id_direccion");

            migrationBuilder.AddColumn<Guid>(
                name: "et_sku_id_sku",
                table: "et_linea_pedido",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_id_cliente",
                table: "et_pedido",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_id_direccion_envio",
                table: "et_pedido",
                column: "id_direccion_envio");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_id_politica",
                table: "et_pedido",
                column: "id_politica");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_id_usuario_captura",
                table: "et_pedido",
                column: "id_usuario_captura");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_et_sku_id_sku",
                table: "et_linea_pedido",
                column: "et_sku_id_sku");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_id_pedido",
                table: "et_linea_pedido",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_id_sku",
                table: "et_linea_pedido",
                column: "id_sku");

            migrationBuilder.AddForeignKey(
                name: "fk_et_linea_pedido_pedidos_id_pedido",
                table: "et_linea_pedido",
                column: "id_pedido",
                principalTable: "et_pedido",
                principalColumn: "id_pedido",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_linea_pedido_skus_et_sku_id_sku",
                table: "et_linea_pedido",
                column: "et_sku_id_sku",
                principalTable: "et_sku",
                principalColumn: "id_sku",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_linea_pedido_skus_id_sku",
                table: "et_linea_pedido",
                column: "id_sku",
                principalTable: "et_sku",
                principalColumn: "id_sku",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_asp_net_users_et_usuario_id",
                table: "et_pedido",
                column: "et_usuario_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_asp_net_users_id_usuario_captura",
                table: "et_pedido",
                column: "id_usuario_captura",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_et_cliente_id_cliente",
                table: "et_pedido",
                column: "id_cliente",
                principalTable: "et_cliente",
                principalColumn: "id_cliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_et_direccion_cliente_et_direccion_cliente_id_direccion",
                table: "et_pedido",
                column: "et_direccion_cliente_id_direccion",
                principalTable: "et_direccion_cliente",
                principalColumn: "id_direccion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_et_direccion_cliente_id_direccion_envio",
                table: "et_pedido",
                column: "id_direccion_envio",
                principalTable: "et_direccion_cliente",
                principalColumn: "id_direccion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_politicas_precios_id_politica",
                table: "et_pedido",
                column: "id_politica",
                principalTable: "et_politica_precio",
                principalColumn: "id_politica",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_id_elem_estatus_anterior",
                table: "h_historial_pedido",
                column: "id_elem_estatus_anterior",
                principalTable: "c_catalogo_elemento",
                principalColumn: "id_catalogo_elemento",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_id_elem_estatus_nuevo",
                table: "h_historial_pedido",
                column: "id_elem_estatus_nuevo",
                principalTable: "c_catalogo_elemento",
                principalColumn: "id_catalogo_elemento",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_linea_pedido_pedidos_id_pedido",
                table: "et_linea_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_linea_pedido_skus_et_sku_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_linea_pedido_skus_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_asp_net_users_et_usuario_id",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_asp_net_users_id_usuario_captura",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_et_cliente_id_cliente",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_et_direccion_cliente_et_direccion_cliente_id_direccion",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_et_direccion_cliente_id_direccion_envio",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_et_pedido_politicas_precios_id_politica",
                table: "et_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_id_elem_estatus_anterior",
                table: "h_historial_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_id_elem_estatus_nuevo",
                table: "h_historial_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_pedido_id_cliente",
                table: "et_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_pedido_id_direccion_envio",
                table: "et_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_pedido_id_politica",
                table: "et_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_pedido_id_usuario_captura",
                table: "et_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_linea_pedido_et_sku_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_linea_pedido_id_pedido",
                table: "et_linea_pedido");

            migrationBuilder.DropIndex(
                name: "ix_et_linea_pedido_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.DropColumn(
                name: "et_sku_id_sku",
                table: "et_linea_pedido");

            migrationBuilder.RenameColumn(
                name: "id_elem_estatus_nuevo",
                table: "h_historial_pedido",
                newName: "estatus_nuevo_id_catalogo_elemento");

            migrationBuilder.RenameColumn(
                name: "id_elem_estatus_anterior",
                table: "h_historial_pedido",
                newName: "estatus_anterior_id_catalogo_elemento");

            migrationBuilder.RenameIndex(
                name: "ix_h_historial_pedido_id_elem_estatus_nuevo",
                table: "h_historial_pedido",
                newName: "ix_h_historial_pedido_estatus_nuevo_id_catalogo_elemento");

            migrationBuilder.RenameIndex(
                name: "ix_h_historial_pedido_id_elem_estatus_anterior",
                table: "h_historial_pedido",
                newName: "ix_h_historial_pedido_estatus_anterior_id_catalogo_elemento");

            migrationBuilder.RenameColumn(
                name: "et_usuario_id",
                table: "et_pedido",
                newName: "politica_id_politica");

            migrationBuilder.RenameColumn(
                name: "et_direccion_cliente_id_direccion",
                table: "et_pedido",
                newName: "direccion_envio_id_direccion");

            migrationBuilder.RenameIndex(
                name: "ix_et_pedido_et_usuario_id",
                table: "et_pedido",
                newName: "ix_et_pedido_politica_id_politica");

            migrationBuilder.RenameIndex(
                name: "ix_et_pedido_et_direccion_cliente_id_direccion",
                table: "et_pedido",
                newName: "ix_et_pedido_direccion_envio_id_direccion");

            migrationBuilder.AddColumn<Guid>(
                name: "cliente_id_cliente",
                table: "et_pedido",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "usuario_captura_id",
                table: "et_pedido",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "pedido_id_pedido",
                table: "et_linea_pedido",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "sku_id_sku",
                table: "et_linea_pedido",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_cliente_id_cliente",
                table: "et_pedido",
                column: "cliente_id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_usuario_captura_id",
                table: "et_pedido",
                column: "usuario_captura_id");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_pedido_id_pedido",
                table: "et_linea_pedido",
                column: "pedido_id_pedido");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_sku_id_sku",
                table: "et_linea_pedido",
                column: "sku_id_sku");

            migrationBuilder.AddForeignKey(
                name: "fk_et_linea_pedido_pedidos_pedido_id_pedido",
                table: "et_linea_pedido",
                column: "pedido_id_pedido",
                principalTable: "et_pedido",
                principalColumn: "id_pedido",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_linea_pedido_skus_sku_id_sku",
                table: "et_linea_pedido",
                column: "sku_id_sku",
                principalTable: "et_sku",
                principalColumn: "id_sku",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_asp_net_users_usuario_captura_id",
                table: "et_pedido",
                column: "usuario_captura_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_et_cliente_cliente_id_cliente",
                table: "et_pedido",
                column: "cliente_id_cliente",
                principalTable: "et_cliente",
                principalColumn: "id_cliente",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_et_direccion_cliente_direccion_envio_id_direccion",
                table: "et_pedido",
                column: "direccion_envio_id_direccion",
                principalTable: "et_direccion_cliente",
                principalColumn: "id_direccion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_et_pedido_politicas_precios_politica_id_politica",
                table: "et_pedido",
                column: "politica_id_politica",
                principalTable: "et_politica_precio",
                principalColumn: "id_politica",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_estatus_anterior_id_catalogo_elemento",
                table: "h_historial_pedido",
                column: "estatus_anterior_id_catalogo_elemento",
                principalTable: "c_catalogo_elemento",
                principalColumn: "id_catalogo_elemento",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_h_historial_pedido_c_catalogo_elemento_estatus_nuevo_id_catalogo_elemento",
                table: "h_historial_pedido",
                column: "estatus_nuevo_id_catalogo_elemento",
                principalTable: "c_catalogo_elemento",
                principalColumn: "id_catalogo_elemento",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
