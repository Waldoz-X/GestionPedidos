using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPedidos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "c_catalogo",
                columns: table => new
                {
                    id_catalogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cl_catalogo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nb_catalogo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ds_catalogo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    id_catalogo_padre = table.Column<int>(type: "int", nullable: true),
                    cl_estatus_catalogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_c_catalogo", x => x.id_catalogo);
                    table.ForeignKey(
                        name: "fk_c_catalogo_c_catalogo_id_catalogo_padre",
                        column: x => x.id_catalogo_padre,
                        principalTable: "c_catalogo",
                        principalColumn: "id_catalogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_politica_precio",
                columns: table => new
                {
                    id_politica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nb_politica = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_tipo_politica = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    no_prioridad = table.Column<int>(type: "int", nullable: false),
                    mn_factor_descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fe_vigente_desde = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_vigente_hasta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    cl_estatus_politica = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_politica_precio", x => x.id_politica);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "c_catalogo_elemento",
                columns: table => new
                {
                    id_catalogo_elemento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_catalogo = table.Column<int>(type: "int", nullable: false),
                    cl_catalogo_elemento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    nb_catalogo_elemento = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ds_catalogo_elemento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    id_catalogo_elemento_padre = table.Column<int>(type: "int", nullable: true),
                    cl_estatus_catalogo_elemento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_c_catalogo_elemento", x => x.id_catalogo_elemento);
                    table.ForeignKey(
                        name: "fk_c_catalogo_elemento_c_catalogo_elemento_id_catalogo_elemento_padre",
                        column: x => x.id_catalogo_elemento_padre,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_c_catalogo_elemento_c_catalogo_id_catalogo",
                        column: x => x.id_catalogo,
                        principalTable: "c_catalogo",
                        principalColumn: "id_catalogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_cliente",
                columns: table => new
                {
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nb_comercial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_tipo_cliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_elem_moneda = table.Column<int>(type: "int", nullable: false),
                    ds_canal_venta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mn_limite_credito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cl_estatus_cliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_cliente", x => x.id_cliente);
                    table.ForeignKey(
                        name: "fk_et_cliente_c_catalogo_elemento_id_elem_moneda",
                        column: x => x.id_elem_moneda,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cl_producto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_producto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    id_elem_categoria = table.Column<int>(type: "int", nullable: false),
                    id_elem_linea_coleccion = table.Column<int>(type: "int", nullable: true),
                    cl_hs_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_estatus_producto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_c_catalogo_elemento_id_elem_categoria",
                        column: x => x.id_elem_categoria,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_producto_c_catalogo_elemento_id_elem_linea_coleccion",
                        column: x => x.id_elem_linea_coleccion,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_direccion_cliente",
                columns: table => new
                {
                    id_direccion = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_pais = table.Column<int>(type: "int", nullable: false),
                    id_estado_catalogo = table.Column<int>(type: "int", nullable: true),
                    nb_alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_linea1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ds_linea2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_ciudad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_codigo_postal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_pais = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_estatus_direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    cliente_id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pais_catalogo_id_catalogo_elemento = table.Column<int>(type: "int", nullable: false),
                    estado_catalogo_id_catalogo_elemento = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_direccion_cliente", x => x.id_direccion);
                    table.ForeignKey(
                        name: "fk_et_direccion_cliente_c_catalogo_elemento_estado_catalogo_id_catalogo_elemento",
                        column: x => x.estado_catalogo_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_direccion_cliente_c_catalogo_elemento_pais_catalogo_id_catalogo_elemento",
                        column: x => x.pais_catalogo_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_direccion_cliente_et_cliente_cliente_id_cliente",
                        column: x => x.cliente_id_cliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_accesorio",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cl_subcategoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ds_material_principal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_accesorio", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_accesorio_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_cono",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    no_altura_cm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ds_material = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fg_es_unitalla = table.Column<bool>(type: "bit", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_cono", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_cono_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_espinillera",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ds_material = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_proteccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_espinillera", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_espinillera_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_fitness",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ds_composicion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_relleno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_cierre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_proteccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    no_peso_oz = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_fitness", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_fitness_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_guante",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nb_palma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_composicion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_ms_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_indice_palma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_forro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_cierre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_homologacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_guante", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_guante_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_mochila",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cl_subcategoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ds_material_principal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    no_capacidad_litros = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    no_compartimentos = table.Column<int>(type: "int", nullable: true),
                    ds_dimensiones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fg_es_unitalla = table.Column<bool>(type: "bit", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_mochila", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_mochila_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_producto_textil",
                columns: table => new
                {
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nb_tejido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ds_composicion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ds_corte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    no_gramaje_gsm = table.Column<int>(type: "int", nullable: true),
                    cl_genero_id_catalogo_elemento = table.Column<int>(type: "int", nullable: false),
                    producto_id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_producto_textil", x => x.id_producto);
                    table.ForeignKey(
                        name: "fk_et_producto_textil_c_catalogo_elemento_cl_genero_id_catalogo_elemento",
                        column: x => x.cl_genero_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_producto_textil_et_producto_producto_id_producto",
                        column: x => x.producto_id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_variante",
                columns: table => new
                {
                    id_variante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_elem_combinacion = table.Column<int>(type: "int", nullable: true),
                    url_imagen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_estatus_variante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_variante", x => x.id_variante);
                    table.ForeignKey(
                        name: "fk_et_variante_c_catalogo_elemento_id_elem_combinacion",
                        column: x => x.id_elem_combinacion,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_variante_et_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_visibilidad_catalogo",
                columns: table => new
                {
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_producto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cl_tipo_acceso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_estatus_visibilidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_visibilidad_catalogo", x => new { x.id_cliente, x.id_producto });
                    table.ForeignKey(
                        name: "fk_et_visibilidad_catalogo_et_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_visibilidad_catalogo_et_producto_id_producto",
                        column: x => x.id_producto,
                        principalTable: "et_producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_sku",
                columns: table => new
                {
                    id_sku = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_variante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_elem_talla = table.Column<int>(type: "int", nullable: true),
                    cl_item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_codigo_barras = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_estatus_sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    no_stock_disponible = table.Column<int>(type: "int", nullable: false),
                    no_stock_reservado = table.Column<int>(type: "int", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_sku", x => x.id_sku);
                    table.ForeignKey(
                        name: "fk_et_sku_c_catalogo_elemento_id_elem_talla",
                        column: x => x.id_elem_talla,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_sku_variantes_id_variante",
                        column: x => x.id_variante,
                        principalTable: "et_variante",
                        principalColumn: "id_variante",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_precio",
                columns: table => new
                {
                    id_precio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_sku = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_politica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    mn_precio_neto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cl_moneda_id_catalogo_elemento = table.Column<int>(type: "int", nullable: false),
                    cl_estatus_precio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fe_vigente_desde = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_vigente_hasta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    sku_id_sku = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    politica_id_politica = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cliente_id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_precio", x => x.id_precio);
                    table.ForeignKey(
                        name: "fk_et_precio_c_catalogo_elemento_cl_moneda_id_catalogo_elemento",
                        column: x => x.cl_moneda_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_precio_et_cliente_cliente_id_cliente",
                        column: x => x.cliente_id_cliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_precio_et_politica_precio_politica_id_politica",
                        column: x => x.politica_id_politica,
                        principalTable: "et_politica_precio",
                        principalColumn: "id_politica",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_precio_skus_sku_id_sku",
                        column: x => x.sku_id_sku,
                        principalTable: "et_sku",
                        principalColumn: "id_sku",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClEstatusUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeCreacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FeModificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClienteIdCliente = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmpleadoIdEmpleado = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_et_cliente_ClienteIdCliente",
                        column: x => x.ClienteIdCliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_empleado",
                columns: table => new
                {
                    id_empleado = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_elem_area = table.Column<int>(type: "int", nullable: true),
                    nu_empleado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cl_empleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_empleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_estatus_empleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_empleado", x => x.id_empleado);
                    table.ForeignKey(
                        name: "fk_et_empleado_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_empleado_c_catalogo_elemento_id_elem_area",
                        column: x => x.id_elem_area,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_pedido",
                columns: table => new
                {
                    id_pedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_usuario_captura = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_direccion_envio = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    id_politica = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cl_folio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_estatus_pedido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_moneda_id_catalogo_elemento = table.Column<int>(type: "int", nullable: false),
                    mn_subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    mn_descuento_comercial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    mn_descuento_admin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    mn_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fe_pedido = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    cliente_id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_captura_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    direccion_envio_id_direccion = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    politica_id_politica = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_pedido", x => x.id_pedido);
                    table.ForeignKey(
                        name: "fk_et_pedido_asp_net_users_usuario_captura_id",
                        column: x => x.usuario_captura_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_pedido_c_catalogo_elemento_cl_moneda_id_catalogo_elemento",
                        column: x => x.cl_moneda_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_pedido_et_cliente_cliente_id_cliente",
                        column: x => x.cliente_id_cliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_pedido_et_direccion_cliente_direccion_envio_id_direccion",
                        column: x => x.direccion_envio_id_direccion,
                        principalTable: "et_direccion_cliente",
                        principalColumn: "id_direccion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_pedido_politicas_precios_politica_id_politica",
                        column: x => x.politica_id_politica,
                        principalTable: "et_politica_precio",
                        principalColumn: "id_politica",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "h_historial_precio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_precio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    precio_anterior = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    precio_nuevo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    registrado_en = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    precio_id_precio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_h_historial_precio", x => x.id);
                    table.ForeignKey(
                        name: "fk_h_historial_precio_asp_net_users_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_h_historial_precio_precios_precio_id_precio",
                        column: x => x.precio_id_precio,
                        principalTable: "et_precio",
                        principalColumn: "id_precio",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_asignacion_cliente_empleado",
                columns: table => new
                {
                    id_empleado = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    cl_tipo_relacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_estatus_asignacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_asignacion_cliente_empleado", x => new { x.id_empleado, x.id_cliente });
                    table.ForeignKey(
                        name: "fk_et_asignacion_cliente_empleado_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "et_cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_asignacion_cliente_empleado_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalTable: "et_empleado",
                        principalColumn: "id_empleado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "et_linea_pedido",
                columns: table => new
                {
                    id_linea_pedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_pedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_sku = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    no_cantidad = table.Column<int>(type: "int", nullable: false),
                    mn_precio_unitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    mn_descuento_linea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    mn_subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cl_operador_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cl_operador_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nb_artefacto_crea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nb_artefacto_modifica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fe_creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    fe_modificacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    pedido_id_pedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sku_id_sku = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_et_linea_pedido", x => x.id_linea_pedido);
                    table.ForeignKey(
                        name: "fk_et_linea_pedido_pedidos_pedido_id_pedido",
                        column: x => x.pedido_id_pedido,
                        principalTable: "et_pedido",
                        principalColumn: "id_pedido",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_et_linea_pedido_skus_sku_id_sku",
                        column: x => x.sku_id_sku,
                        principalTable: "et_sku",
                        principalColumn: "id_sku",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "h_historial_pedido",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id_pedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    estatus_anterior_id_catalogo_elemento = table.Column<int>(type: "int", nullable: true),
                    estatus_nuevo_id_catalogo_elemento = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    registrado_en = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_h_historial_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_h_historial_pedido_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_h_historial_pedido_c_catalogo_elemento_estatus_anterior_id_catalogo_elemento",
                        column: x => x.estatus_anterior_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_h_historial_pedido_c_catalogo_elemento_estatus_nuevo_id_catalogo_elemento",
                        column: x => x.estatus_nuevo_id_catalogo_elemento,
                        principalTable: "c_catalogo_elemento",
                        principalColumn: "id_catalogo_elemento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_h_historial_pedido_pedidos_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "et_pedido",
                        principalColumn: "id_pedido",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClienteIdCliente",
                table: "AspNetUsers",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EmpleadoIdEmpleado",
                table: "AspNetUsers",
                column: "EmpleadoIdEmpleado");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_c_catalogo_id_catalogo_padre",
                table: "c_catalogo",
                column: "id_catalogo_padre");

            migrationBuilder.CreateIndex(
                name: "ix_c_catalogo_elemento_id_catalogo",
                table: "c_catalogo_elemento",
                column: "id_catalogo");

            migrationBuilder.CreateIndex(
                name: "ix_c_catalogo_elemento_id_catalogo_elemento_padre",
                table: "c_catalogo_elemento",
                column: "id_catalogo_elemento_padre");

            migrationBuilder.CreateIndex(
                name: "ix_et_asignacion_cliente_empleado_id_cliente",
                table: "et_asignacion_cliente_empleado",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_cliente_id_elem_moneda",
                table: "et_cliente",
                column: "id_elem_moneda");

            migrationBuilder.CreateIndex(
                name: "ix_et_direccion_cliente_cliente_id_cliente",
                table: "et_direccion_cliente",
                column: "cliente_id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_direccion_cliente_estado_catalogo_id_catalogo_elemento",
                table: "et_direccion_cliente",
                column: "estado_catalogo_id_catalogo_elemento");

            migrationBuilder.CreateIndex(
                name: "ix_et_direccion_cliente_pais_catalogo_id_catalogo_elemento",
                table: "et_direccion_cliente",
                column: "pais_catalogo_id_catalogo_elemento");

            migrationBuilder.CreateIndex(
                name: "ix_et_empleado_id_elem_area",
                table: "et_empleado",
                column: "id_elem_area");

            migrationBuilder.CreateIndex(
                name: "ix_et_empleado_id_usuario",
                table: "et_empleado",
                column: "id_usuario",
                unique: true,
                filter: "[id_usuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_pedido_id_pedido",
                table: "et_linea_pedido",
                column: "pedido_id_pedido");

            migrationBuilder.CreateIndex(
                name: "ix_et_linea_pedido_sku_id_sku",
                table: "et_linea_pedido",
                column: "sku_id_sku");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_cl_moneda_id_catalogo_elemento",
                table: "et_pedido",
                column: "cl_moneda_id_catalogo_elemento");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_cliente_id_cliente",
                table: "et_pedido",
                column: "cliente_id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_direccion_envio_id_direccion",
                table: "et_pedido",
                column: "direccion_envio_id_direccion");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_politica_id_politica",
                table: "et_pedido",
                column: "politica_id_politica");

            migrationBuilder.CreateIndex(
                name: "ix_et_pedido_usuario_captura_id",
                table: "et_pedido",
                column: "usuario_captura_id");

            migrationBuilder.CreateIndex(
                name: "ix_et_precio_cl_moneda_id_catalogo_elemento",
                table: "et_precio",
                column: "cl_moneda_id_catalogo_elemento");

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

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_id_elem_categoria",
                table: "et_producto",
                column: "id_elem_categoria");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_id_elem_linea_coleccion",
                table: "et_producto",
                column: "id_elem_linea_coleccion");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_accesorio_producto_id_producto",
                table: "et_producto_accesorio",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_cono_producto_id_producto",
                table: "et_producto_cono",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_espinillera_producto_id_producto",
                table: "et_producto_espinillera",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_fitness_producto_id_producto",
                table: "et_producto_fitness",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_guante_producto_id_producto",
                table: "et_producto_guante",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_mochila_producto_id_producto",
                table: "et_producto_mochila",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_textil_cl_genero_id_catalogo_elemento",
                table: "et_producto_textil",
                column: "cl_genero_id_catalogo_elemento");

            migrationBuilder.CreateIndex(
                name: "ix_et_producto_textil_producto_id_producto",
                table: "et_producto_textil",
                column: "producto_id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_sku_id_elem_talla",
                table: "et_sku",
                column: "id_elem_talla");

            migrationBuilder.CreateIndex(
                name: "ix_et_sku_id_variante",
                table: "et_sku",
                column: "id_variante");

            migrationBuilder.CreateIndex(
                name: "ix_et_variante_id_elem_combinacion",
                table: "et_variante",
                column: "id_elem_combinacion");

            migrationBuilder.CreateIndex(
                name: "ix_et_variante_id_producto",
                table: "et_variante",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_et_visibilidad_catalogo_id_producto",
                table: "et_visibilidad_catalogo",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "ix_h_historial_pedido_estatus_anterior_id_catalogo_elemento",
                table: "h_historial_pedido",
                column: "estatus_anterior_id_catalogo_elemento");

            migrationBuilder.CreateIndex(
                name: "ix_h_historial_pedido_estatus_nuevo_id_catalogo_elemento",
                table: "h_historial_pedido",
                column: "estatus_nuevo_id_catalogo_elemento");

            migrationBuilder.CreateIndex(
                name: "ix_h_historial_pedido_id_pedido",
                table: "h_historial_pedido",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "ix_h_historial_pedido_id_usuario",
                table: "h_historial_pedido",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_h_historial_precio_precio_id_precio",
                table: "h_historial_precio",
                column: "precio_id_precio");

            migrationBuilder.CreateIndex(
                name: "ix_h_historial_precio_usuario_id",
                table: "h_historial_precio",
                column: "usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_et_empleado_EmpleadoIdEmpleado",
                table: "AspNetUsers",
                column: "EmpleadoIdEmpleado",
                principalTable: "et_empleado",
                principalColumn: "id_empleado",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_et_empleado_asp_net_users_id_usuario",
                table: "et_empleado");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "et_asignacion_cliente_empleado");

            migrationBuilder.DropTable(
                name: "et_linea_pedido");

            migrationBuilder.DropTable(
                name: "et_producto_accesorio");

            migrationBuilder.DropTable(
                name: "et_producto_cono");

            migrationBuilder.DropTable(
                name: "et_producto_espinillera");

            migrationBuilder.DropTable(
                name: "et_producto_fitness");

            migrationBuilder.DropTable(
                name: "et_producto_guante");

            migrationBuilder.DropTable(
                name: "et_producto_mochila");

            migrationBuilder.DropTable(
                name: "et_producto_textil");

            migrationBuilder.DropTable(
                name: "et_visibilidad_catalogo");

            migrationBuilder.DropTable(
                name: "h_historial_pedido");

            migrationBuilder.DropTable(
                name: "h_historial_precio");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "et_pedido");

            migrationBuilder.DropTable(
                name: "et_precio");

            migrationBuilder.DropTable(
                name: "et_direccion_cliente");

            migrationBuilder.DropTable(
                name: "et_politica_precio");

            migrationBuilder.DropTable(
                name: "et_sku");

            migrationBuilder.DropTable(
                name: "et_variante");

            migrationBuilder.DropTable(
                name: "et_producto");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "et_cliente");

            migrationBuilder.DropTable(
                name: "et_empleado");

            migrationBuilder.DropTable(
                name: "c_catalogo_elemento");

            migrationBuilder.DropTable(
                name: "c_catalogo");
        }
    }
}
