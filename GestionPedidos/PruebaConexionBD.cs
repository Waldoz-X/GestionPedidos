using GestionPedidos.Data;
using GestionPedidos.Models.Catalogo;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Pruebas;

/// <summary>
/// Script de prueba para verificar que la BD está lista para CRUD
/// Ejecutar: var context = new AppDbContext(options);
/// </summary>
public class PruebaConexionBD
{
    public static async Task ProbarConexionBD(AppDbContext context)
    {
        Console.WriteLine("\n=== PRUEBA DE CONEXIÓN A BD ===\n");

        try
        {
            // 1. VERIFICAR CONEXIÓN
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"✅ Conexión a BD: {(canConnect ? "EXITOSA" : "FALLIDA")}");

            // 2. VERIFICAR CATÁLOGOS
            var catalogosCount = await context.CCatalogos.CountAsync();
            Console.WriteLine($"✅ Catálogos en BD: {catalogosCount}");

            // 3. CREAR PRODUCTO DE PRUEBA
            var producto = new etProducto
            {
                IdProducto = Guid.NewGuid(),
                ClProducto = "TEST001",
                NbProducto = "PRODUCTO DE PRUEBA",
                IdElemCategoria = 1,
                ClEstatusProducto = "ACTIVO",
                ClOperadorCrea = "Admin",
                NbArtefactoCrea = "Sistema",
                FeCreacion = DateTimeOffset.UtcNow
            };

            context.Add(producto);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ CREAR: Producto insertado: {producto.NbProducto} (ID: {producto.IdProducto})");

            // 4. LEER PRODUCTO
            var productoLeido = await context.Set<etProducto>()
                .FirstOrDefaultAsync(e => e.ClProducto == "TEST001");
            Console.WriteLine($"✅ LEER: Producto recuperado: {productoLeido?.NbProducto}");

            // 5. ACTUALIZAR PRODUCTO
            if (productoLeido != null)
            {
                productoLeido.NbProducto = "PRODUCTO ACTUALIZADO";
                productoLeido.ClOperadorModifica = "Admin";
                productoLeido.FeModificacion = DateTimeOffset.UtcNow;
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ ACTUALIZAR: Producto actualizado a: {productoLeido.NbProducto}");
            }

            // 6. ELIMINAR PRODUCTO
            if (productoLeido != null)
            {
                context.Remove(productoLeido);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ ELIMINAR: Producto eliminado");
            }

            Console.WriteLine("\n=== ✅ BD LISTA PARA PRODUCCIÓN ===\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
}

