using System.Text.RegularExpressions;
using GestionPedidos.Models;
using GestionPedidos.Models.Catalogo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GestionPedidos.Data;

public class AppDbContext : IdentityDbContext<etUsuario, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Catálogos Genéricos ──
    public DbSet<CCatalogo> CCatalogos => Set<CCatalogo>();
    public DbSet<CCatalogoElemento> CCatalogoElementos => Set<CCatalogoElemento>();

    // ── Catálogo de Productos (Jerarquía: Producto → Variante → SKU) ──
    public DbSet<etProducto> Productos => Set<etProducto>();
    public DbSet<etProductoGuante> ProductosGuante => Set<etProductoGuante>();
    public DbSet<etProductoFitness> ProductosFitness => Set<etProductoFitness>();
    public DbSet<etProductoMochila> ProductosMochila => Set<etProductoMochila>();
    public DbSet<etProductoCono> ProductosCono => Set<etProductoCono>();
    public DbSet<etProductoEspinillera> ProductosEspinillera => Set<etProductoEspinillera>();
    public DbSet<etProductoAccesorio> ProductosAccesorio => Set<etProductoAccesorio>();
    public DbSet<etProductoTextil> ProductosTextil => Set<etProductoTextil>();
    public DbSet<etVariante> Variantes => Set<etVariante>();
    public DbSet<etSku> Skus => Set<etSku>();

    // ── Clientes ──
    public DbSet<etCliente> Clientes => Set<etCliente>();
    public DbSet<etAsignacionClienteEmpleado> AsignacionesClienteEmpleado => Set<etAsignacionClienteEmpleado>();
    public DbSet<etDireccionCliente> DireccionesCliente => Set<etDireccionCliente>();

    // ── Precios ──
    public DbSet<etPoliticaPrecio> PoliticasPrecios => Set<etPoliticaPrecio>();
    public DbSet<etPrecio> Precios => Set<etPrecio>();
    public DbSet<etVisibilidadCatalogo> VisibilidadesCatalogo => Set<etVisibilidadCatalogo>();

    // ── Pedidos ──
    public DbSet<etPedido> Pedidos => Set<etPedido>();
    public DbSet<etLineaPedido> LineasPedido => Set<etLineaPedido>();

    // ── Historial / Auditoría ──
    public DbSet<etHistorialPrecio> HistorialPrecios => Set<etHistorialPrecio>();
    public DbSet<etHistorialPedido> HistorialPedidos => Set<etHistorialPedido>();

    // ── Identity ──
    public DbSet<etEmpleado> Empleados => Set<etEmpleado>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ────────────────────────────────────────────────────────────
        // 1. CLAVES PRIMARIAS
        // ────────────────────────────────────────────────────────────
        ConfigureKeys(b);

        // ────────────────────────────────────────────────────────────
        // 2. CATÁLOGOS GENÉRICOS (c_catalogo, c_catalogo_elemento)
        // ────────────────────────────────────────────────────────────
        ConfigureCatalogosGenericos(b);

        // ────────────────────────────────────────────────────────────
        // 3. RELACIONES EXPLÍCITAS
        // ────────────────────────────────────────────────────────────
        ConfigureRelationships(b);

        // ────────────────────────────────────────────────────────────
        // 4. CONVENCIÓN GLOBAL: Restrict para evitar ciclos de CASCADE
        // ────────────────────────────────────────────────────────────
        foreach (var relationship in b.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // ────────────────────────────────────────────────────────────
        // 5. NOMENCLATURA snake_case EN BD
        //    Tablas: et_producto, c_catalogo, h_historial_precio, etc.
        //    Columnas: id_producto, nb_modelo, cl_hs_code, etc.
        //    Las tablas de Identity (AspNet*) NO se renombran.
        // ────────────────────────────────────────────────────────────
        ApplySnakeCaseNamingConvention(b);
    }

    // ================================================================
    //  CLAVES PRIMARIAS
    // ================================================================
    private static void ConfigureKeys(ModelBuilder b)
    {
        // --- Entidades con PK simple (convención Id + nombre sin prefijo) ---
        b.Entity<etProducto>().HasKey(p => p.IdProducto);
        b.Entity<etVariante>().HasKey(v => v.IdVariante);
        b.Entity<etSku>().HasKey(s => s.IdSku);
        b.Entity<etCliente>().HasKey(c => c.IdCliente);
        b.Entity<etDireccionCliente>().HasKey(d => d.IdDireccion);
        b.Entity<etPoliticaPrecio>().HasKey(p => p.IdPolitica);
        b.Entity<etPrecio>().HasKey(p => p.IdPrecio);
        b.Entity<etPedido>().HasKey(p => p.IdPedido);
        b.Entity<etLineaPedido>().HasKey(l => l.IdLineaPedido);
        b.Entity<etEmpleado>().HasKey(e => e.IdEmpleado);
        b.Entity<etHistorialPrecio>().HasKey(h => h.Id);
        b.Entity<etHistorialPedido>().HasKey(h => h.Id);

        // --- Entidades con PK compuesta ---
        b.Entity<etAsignacionClienteEmpleado>().HasKey(a => new { a.IdEmpleado, a.IdCliente });
        b.Entity<etVisibilidadCatalogo>().HasKey(v => new { v.IdCliente, v.IdProducto });

        // --- Especializaciones CTI (PK = FK a etProducto) ---
        b.Entity<etProductoGuante>().HasKey(p => p.IdProducto);
        b.Entity<etProductoFitness>().HasKey(p => p.IdProducto);
        b.Entity<etProductoMochila>().HasKey(p => p.IdProducto);
        b.Entity<etProductoCono>().HasKey(p => p.IdProducto);
        b.Entity<etProductoEspinillera>().HasKey(p => p.IdProducto);
        b.Entity<etProductoAccesorio>().HasKey(p => p.IdProducto);
        b.Entity<etProductoTextil>().HasKey(p => p.IdProducto);
    }

    // ================================================================
    //  CATÁLOGOS GENÉRICOS
    // ================================================================
    private static void ConfigureCatalogosGenericos(ModelBuilder b)
    {
        b.Entity<CCatalogo>(e =>
        {
            e.HasKey(x => x.IdCatalogo);
            e.Property(x => x.IdCatalogo).ValueGeneratedOnAdd();
            e.Property(x => x.ClCatalogo).HasMaxLength(50).IsRequired();
            e.Property(x => x.NbCatalogo).HasMaxLength(150).IsRequired();

            // Relación autorreferenciada (jerarquía padre-hijo entre catálogos)
            e.HasOne(x => x.CatalogoPadre)
                .WithMany(x => x.CatalogosHijos)
                .HasForeignKey(x => x.IdCatalogoPadre)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CCatalogoElemento>(e =>
        {
            e.HasKey(x => x.IdCatalogoElemento);
            e.Property(x => x.IdCatalogoElemento).ValueGeneratedOnAdd();
            e.Property(x => x.ClCatalogoElemento).HasMaxLength(50).IsRequired();
            e.Property(x => x.NbCatalogoElemento).HasMaxLength(150).IsRequired();

            e.HasOne(x => x.Catalogo)
                .WithMany(c => c.Elementos)
                .HasForeignKey(x => x.IdCatalogo)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación autorreferenciada (jerarquía padre-hijo entre elementos)
            e.HasOne(x => x.ElementoPadre)
                .WithMany(x => x.ElementosHijos)
                .HasForeignKey(x => x.IdCatalogoElementoPadre)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ================================================================
    //  RELACIONES EXPLÍCITAS
    // ================================================================
    private static void ConfigureRelationships(ModelBuilder b)
    {
        // ── etEmpleado ──
        b.Entity<etEmpleado>(e =>
        {
            e.HasOne(emp => emp.Usuario)
                .WithOne()
                .HasForeignKey<etEmpleado>(emp => emp.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<CCatalogoElemento>(emp => emp.Area)
                .WithMany()
                .HasForeignKey(emp => emp.IdElemArea)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── etProducto → Catálogos dinámicos ──
        b.Entity<etProducto>(e =>
        {            e.HasOne<CCatalogoElemento>(p => p.Division)
                .WithMany()
                .HasForeignKey(p => p.IdElemDivision)
                .OnDelete(DeleteBehavior.Restrict);



            e.HasOne<CCatalogoElemento>(p => p.LineaColeccion)
                .WithMany()
                .HasForeignKey(p => p.IdElemLineaColeccion)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne<CCatalogoElemento>(p => p.Gama)
                .WithMany()
                .HasForeignKey(p => p.IdElemGama)
                .OnDelete(DeleteBehavior.Restrict);


        });

        // ── etSku → Talla (catálogo) ──
        b.Entity<etSku>(e =>
        {
            e.HasOne<CCatalogoElemento>(s => s.Talla)
                .WithMany()
                .HasForeignKey(s => s.IdElemTalla)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── etVariante → Combinación de Color (catálogo) ──
        b.Entity<etVariante>(e =>
        {
            e.HasOne<CCatalogoElemento>(v => v.Combinacion)
                .WithMany()
                .HasForeignKey(v => v.IdElemCombinacion)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── etCliente ──
        b.Entity<etCliente>(e =>
        {
            e.HasOne<CCatalogoElemento>(c => c.ClMoneda)
                .WithMany()
                .HasForeignKey(c => c.IdElemMoneda)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── etVisibilidadCatalogo ──
        b.Entity<etVisibilidadCatalogo>(e =>
        {
            e.HasOne(v => v.Cliente)
                .WithMany(c => c.Visibilidades)
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Producto)
                .WithMany()
                .HasForeignKey(v => v.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── etAsignacionClienteEmpleado ──
        b.Entity<etAsignacionClienteEmpleado>(e =>
        {
            e.HasOne(a => a.Empleado)
                .WithMany(emp => emp.AsignacionesCliente)
                .HasForeignKey(a => a.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Cliente)
                .WithMany(c => c.AsignacionesEmpleado)
                .HasForeignKey(a => a.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── etHistorialPedido ──
        b.Entity<etHistorialPedido>(e =>
        {
            e.HasOne(h => h.Pedido)
                .WithMany(p => p.Historial)
                .HasForeignKey(h => h.IdPedido)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(h => h.Usuario)
                .WithMany(u => u.HistorialPedidos)
                .HasForeignKey(h => h.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // ================================================================
    //  CONVENCIÓN GLOBAL: snake_case
    //  Convierte nombres de tabla y columna PascalCase → snake_case.
    //  Respeta ToTable() explícitos ya configurados (c_catalogo, etc.).
    //  Excluye tablas de ASP.NET Identity (AspNet*).
    // ================================================================
    private static void ApplySnakeCaseNamingConvention(ModelBuilder b)
    {
        // ── Mapeo explícito de tablas con prefijo según nomenclatura ──
        // Prefijo et_ = Entidades
        // Prefijo c_  = Catálogos
        // Prefijo h_  = Historial
        var tableMap = new Dictionary<Type, string>
        {
            // Catálogos genéricos (prefijo c_)
            [typeof(CCatalogo)]           = "c_catalogo",
            [typeof(CCatalogoElemento)]   = "c_catalogo_elemento",

            // Catálogo de productos (prefijo et_)
            [typeof(etProducto)]          = "et_producto",
            [typeof(etProductoGuante)]    = "et_producto_guante",
            [typeof(etProductoFitness)]   = "et_producto_fitness",
            [typeof(etProductoMochila)]   = "et_producto_mochila",
            [typeof(etProductoCono)]      = "et_producto_cono",
            [typeof(etProductoEspinillera)] = "et_producto_espinillera",
            [typeof(etProductoAccesorio)] = "et_producto_accesorio",
            [typeof(etProductoTextil)]    = "et_producto_textil",
            [typeof(etVariante)]          = "et_variante",
            [typeof(etSku)]               = "et_sku",

            // Clientes (prefijo et_)
            [typeof(etCliente)]           = "et_cliente",
            [typeof(etDireccionCliente)]  = "et_direccion_cliente",
            [typeof(etAsignacionClienteEmpleado)] = "et_asignacion_cliente_empleado",

            // Precios (prefijo et_)
            [typeof(etPoliticaPrecio)]    = "et_politica_precio",
            [typeof(etPrecio)]            = "et_precio",
            [typeof(etVisibilidadCatalogo)] = "et_visibilidad_catalogo",

            // Pedidos (prefijo et_)
            [typeof(etPedido)]            = "et_pedido",
            [typeof(etLineaPedido)]       = "et_linea_pedido",

            // Historial (prefijo h_)
            [typeof(etHistorialPrecio)]   = "h_historial_precio",
            [typeof(etHistorialPedido)]   = "h_historial_pedido",

            // Identity (prefijo et_)
            [typeof(etEmpleado)]          = "et_empleado",
            // etUsuario → NO se mapea aquí, ASP.NET Identity controla su tabla (AspNetUsers)
        };

        foreach (var entityType in b.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            // ── Tablas: mapear con nombre explícito o saltar Identity ──
            if (tableMap.TryGetValue(clrType, out var tableName))
            {
                entityType.SetTableName(tableName);
            }
            else if (entityType.GetTableName()?.StartsWith("AspNet") == true)
            {
                // Las tablas de ASP.NET Identity se dejan intactas
                continue;
            }

            // ── Columnas: PascalCase → snake_case ──
            foreach (var property in entityType.GetProperties())
            {
                var storeColumnName = property.GetColumnName(
                    StoreObjectIdentifier.Table(
                        entityType.GetTableName()!,
                        entityType.GetSchema()));

                // Solo renombrar si no tiene un nombre explícito diferente ya configurado
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // ── Índices: snake_case ──
            foreach (var index in entityType.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (indexName != null)
                {
                    index.SetDatabaseName(ToSnakeCase(indexName));
                }
            }

            // ── Foreign Keys: snake_case ──
            foreach (var fk in entityType.GetForeignKeys())
            {
                var constraintName = fk.GetConstraintName();
                if (constraintName != null)
                {
                    fk.SetConstraintName(ToSnakeCase(constraintName));
                }
            }
        }
    }

    /// <summary>
    /// Convierte un nombre PascalCase a snake_case.
    /// Ejemplo: "IdElemDivision" → "id_elem_division"
    /// Ejemplo: "NbPalma" → "nb_palma"
    ///          "IX_Productos_DivisionCatalogoIdCatalogoElemento" → "ix_productos_division_catalogo_id_catalogo_elemento"
    /// </summary>
    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        // Inserta _ antes de cada transición minúscula→mayúscula o mayúscula→mayúscula+minúscula
        var result = Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2");
        result = Regex.Replace(result, @"([A-Z]+)([A-Z][a-z])", "$1_$2");

        return result.ToLowerInvariant();
    }
}
