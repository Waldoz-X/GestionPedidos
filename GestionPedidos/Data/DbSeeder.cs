using GestionPedidos.Models;
using GestionPedidos.Models.Catalogo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionPedidos.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<etUsuario>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedCatalogosAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Admin", "User", "Manager", "EMPLEADO", "CLIENTE" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<etUsuario> userManager)
    {
        var adminEmail = "admin@gp.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new etUsuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                ClEstatusUsuario = "ACTIVO",
                FeCreacion = DateTimeOffset.UtcNow
            };
            
            // Recreamos con la contraseña requerida
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedCatalogosAsync(AppDbContext context)
    {
        // Solo sembrar si no hay datos
        if (await context.CCatalogos.AnyAsync()) return;

        var operador = "SISTEMA";
        var fecha = DateTimeOffset.UtcNow;

        // 1. Crear Catálogos Padre
        var catAreas = CrearCatalogo("AREAS_DEPARTAMENTOS", "Áreas o Departamentos", "Departamentos de la empresa", operador, fecha);
        var catDivisiones = CrearCatalogo("DIVISIONES", "Divisiones de Producto", "Líneas principales de producto", operador, fecha);
        var catMonedas = CrearCatalogo("MONEDAS", "Monedas", "Divisas internacionales", operador, fecha);
        var catPaises = CrearCatalogo("PAISES", "Países", "Países donde se opera", operador, fecha);
        var catTallas = CrearCatalogo("TALLAS", "Tallas", "Tallas de productos", operador, fecha);
        var catCombinaciones = CrearCatalogo("COMBINACIONES", "Combinaciones de Color", "Catálogo maestro de combinaciones de colores para variantes de producto", operador, fecha);

        context.CCatalogos.AddRange(catAreas, catDivisiones, catMonedas, catPaises, catTallas, catCombinaciones);
        await context.SaveChangesAsync();

        // 2. Elementos: AREAS_DEPARTAMENTOS
        context.CCatalogoElementos.AddRange(
            CrearElemento(catAreas.IdCatalogo, "COMERCIALIZADORA", "Comercializadora", null, operador, fecha),
            CrearElemento(catAreas.IdCatalogo, "EXPORTACION", "Exportación", null, operador, fecha)
        );

        // 3. Elementos: DIVISIONES
        context.CCatalogoElementos.AddRange(
            CrearElemento(catDivisiones.IdCatalogo, "PP", "GUANTE", null, operador, fecha),
            CrearElemento(catDivisiones.IdCatalogo, "FN", "FITNESS", null, operador, fecha),
            CrearElemento(catDivisiones.IdCatalogo, "IH", "MOCHILA", null, operador, fecha),
            CrearElemento(catDivisiones.IdCatalogo, "IK", "CONO", null, operador, fecha),
            CrearElemento(catDivisiones.IdCatalogo, "IY", "ESPINILLERA", null, operador, fecha),
            CrearElemento(catDivisiones.IdCatalogo, "KT", "MEDIA", null, operador, fecha),
            CrearElemento(catDivisiones.IdCatalogo, "TX", "TEXTIL", null, operador, fecha)
        );

        // 4. Elementos: MONEDAS
        context.CCatalogoElementos.AddRange(
            CrearElemento(catMonedas.IdCatalogo, "MXN", "Peso Mexicano", null, operador, fecha),
            CrearElemento(catMonedas.IdCatalogo, "USD", "Dólar Estadounidense", null, operador, fecha),
            CrearElemento(catMonedas.IdCatalogo, "EUR", "Euro", null, operador, fecha),
            CrearElemento(catMonedas.IdCatalogo, "CAD", "Dólar Canadiense", null, operador, fecha),
            CrearElemento(catMonedas.IdCatalogo, "GBP", "Libra Esterlina", null, operador, fecha)
        );

        // 5. Elementos: PAISES Y ESTADOS
        var elMexico = CrearElemento(catPaises.IdCatalogo, "MX", "México", null, operador, fecha);
        context.CCatalogoElementos.Add(elMexico);
        await context.SaveChangesAsync(); // Guardamos para obtener su ID

        // Crear Estados hijos de México
        context.CCatalogoElementos.AddRange(
            CrearElemento(catPaises.IdCatalogo, "AGS", "Aguascalientes", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "JAL", "Jalisco", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "CDMX", "Ciudad de México", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "NL", "Nuevo León", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "EDOMEX", "Estado de México", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "GTO", "Guanajuato", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "QRO", "Querétaro", elMexico.IdCatalogoElemento, operador, fecha),
            CrearElemento(catPaises.IdCatalogo, "YUC", "Yucatán", elMexico.IdCatalogoElemento, operador, fecha)
        );

        // 6. Elementos: TALLAS (Padres e hijos)
        var elTextilInfantil = CrearElemento(catTallas.IdCatalogo, "TEXTIL_INFANTIL", "Textil Infantil", null, operador, fecha);
        var elTextilAdulto = CrearElemento(catTallas.IdCatalogo, "TEXTIL_ADULTO", "Textil Adulto", null, operador, fecha);
        var elGuanteInfantil = CrearElemento(catTallas.IdCatalogo, "GUANTE_INFANTIL", "Guante Infantil", null, operador, fecha);
        var elGuanteAdulto = CrearElemento(catTallas.IdCatalogo, "GUANTE_ADULTO", "Guante Adulto", null, operador, fecha);
        
        context.CCatalogoElementos.AddRange(elTextilInfantil, elTextilAdulto, elGuanteInfantil, elGuanteAdulto);
        await context.SaveChangesAsync();

        // Tallas infantiles textil
        context.CCatalogoElementos.AddRange(
            CrearElemento(catTallas.IdCatalogo, "YL", "Youth Large", elTextilInfantil.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "YM", "Youth Medium", elTextilInfantil.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "YS", "Youth Small", elTextilInfantil.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "YXS", "Youth Extra Small", elTextilInfantil.IdCatalogoElemento, operador, fecha)
        );

        // Tallas de adulto textil
        context.CCatalogoElementos.AddRange(
            CrearElemento(catTallas.IdCatalogo, "AXXL", "Adult Extra Extra Large", elTextilAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "AXL", "Adult Extra Large", elTextilAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "AL", "Adult Large", elTextilAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "AM", "Adult Medium", elTextilAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "AS", "Adult Small", elTextilAdulto.IdCatalogoElemento, operador, fecha)
        );

        // Tallas infantiles guante
        context.CCatalogoElementos.AddRange(
            CrearElemento(catTallas.IdCatalogo, "3", "Talla 3 - Infantil", elGuanteInfantil.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "4", "Talla 4 - Infantil", elGuanteInfantil.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "5", "Talla 5 - Infantil", elGuanteInfantil.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "6", "Talla 6 - Infantil", elGuanteInfantil.IdCatalogoElemento, operador, fecha)
        );

        // Tallas de adulto guante
        context.CCatalogoElementos.AddRange(
            CrearElemento(catTallas.IdCatalogo, "7", "Talla 7 - Adulto", elGuanteAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "8", "Talla 8 - Adulto", elGuanteAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "9", "Talla 9 - Adulto", elGuanteAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "10", "Talla 10 - Adulto", elGuanteAdulto.IdCatalogoElemento, operador, fecha),
            CrearElemento(catTallas.IdCatalogo, "11", "Talla 11 - Adulto", elGuanteAdulto.IdCatalogoElemento, operador, fecha)
        );

        // 7. OTROS CATÁLOGOS NECESARIOS (Gamas)
        var catGamas = CrearCatalogo("GAMAS", "Gamas de Producto", "Gamas", operador, fecha);
        
        context.CCatalogos.AddRange(catGamas);
        await context.SaveChangesAsync();

        context.CCatalogoElementos.AddRange(
            CrearElemento(catGamas.IdCatalogo, "SEMI PROFESIONAL", "SEMI PROFESIONAL", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "REPLICA", "REPLICA", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "PROFESIONAL", "PROFESIONAL", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "SEMI SPINE", "SEMI SPINE", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "BASICO", "BASICO", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "AS", "AS", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "SPINE TURF", "SPINE TURF", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "TURF", "TURF", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "ALPHA", "ALPHA", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "MASTER", "MASTER", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "SUPERIOR", "SUPERIOR", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "PRIME", "PRIME", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "TRAINING", "TRAINING", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "ALPHA PREMIER", "ALPHA PREMIER", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "TRAINING SPINE", "TRAINING SPINE", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "PREMIUM", "PREMIUM", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "PRIME SPINE", "PRIME SPINE", null, operador, fecha),
            CrearElemento(catGamas.IdCatalogo, "GRAVITI", "GRAVITI", null, operador, fecha)
        );

        await context.SaveChangesAsync();

        // 8. Elementos: COMBINACIONES DE COLOR
        await SeedCombinacionesAsync(context, catCombinaciones.IdCatalogo, operador, fecha);
    }

    private static async Task SeedCombinacionesAsync(AppDbContext context, int idCatCombinaciones, string operador, DateTimeOffset fecha)
    {
        // Mapa completo de combinaciones: Clave → Nombre del color
        var combinaciones = new (string clave, string nombre)[]
        {
            ("1", "NEGRO / AMARILLO NEON"), ("2", "BLANCO / NEGRO"), ("3", "NARANJA FLT / TURQUEZA"),
            ("4", "BLANCO / AMARILLO NEON"), ("5", "AMARILLO NEON/ROJO"), ("6", "NEGRO / ROJO"),
            ("7", "AMARILLO NEON / AZUL TURQUESA"), ("8", "NEGRO / AMARILLO / AZUL"), ("9", "AZUL / NARANJA FTL"),
            ("10", "NARANJA / NEGRO / BLANCO"), ("11", "NEGRO / AMARILLO NEON"), ("12", "BLANCO CON EL 1 DORADO"),
            ("13", "NGO / MORADO / AMA NEON"), ("16", "NARANJA/ AZUL"), ("17", "COBALTO"),
            ("25", "BLANCO/VINO/NEGRO"), ("27", "BLANCO/AZUL"), ("55", "ESTAMPADO SNAKE"),
            ("65", "AZUL TURQUESA/AZUL MARINO"), ("101", "AZUL CIELO"), ("102", "NARANJA"),
            ("103", "VERDE"), ("104", "ROJO"), ("105", "AMARILLO"), ("106", "ROSA"),
            ("107", "GRIS OXFORD"), ("108", "AZUL REY"), ("109", "NEGRO"), ("110", "BLANCO"),
            ("111", "AZUL CIELO/ BLANCO"), ("113", "VERDE / GRIS"), ("115", "BLANCO/NEGRO/ROJO"),
            ("117", "NEGRO/BLANCO"), ("118", "NEGRO /AMARILLO"), ("119", "NEGRO / ROJO"),
            ("120", "NEGRO / AZUL"), ("121", "NEGRO / NARANJA"), ("122", "NEGRO/VERDE"),
            ("123", "AZUL/ BLANCO"), ("124", "NEGRO/ORO"), ("125", "NEGRO/ GRIS"),
            ("126", "NEGRO/ROSA"), ("127", "GRIS OXFORD/ NEGRO"), ("128", "AZUL REY/ NEGRO"),
            ("129", "AZUL CIELO/NEGRO"), ("130", "BLANCO/ NEGRO"), ("131", "BLANCO / ROJO"),
            ("132", "BLANCO/AZUL CIELO"), ("133", "NARANJA/ NEGRO"), ("134", "ROJO/NEGRO"),
            ("136", "ROSA/NEGRO"), ("138", "ORO"), ("139", "TURQUESA"), ("140", "ROJO/ BLANCO"),
            ("141", "BLANCO/ROSA"), ("142", "BEIGE"), ("143", "VARIOS"),
            ("145", "AZUL CIELO/AZUL MARINO"), ("147", "AZUL REY/ AMARILLO"),
            ("148", "BLANCO/ AZUL REY"), ("149", "BLANCO/ AZUL MARINO"), ("150", "AMARILLO / NEGRO"),
            ("152", "BLANCO/ NARANJA"), ("153", "VERDE/ NEGRO"), ("154", "ROJO/ PLATA"),
            ("155", "NARANJA/ AZUL REY"), ("156", "AZUL REY/ NARANJA"), ("158", "AZUL/ AMARILLO"),
            ("161", "AZUL/ ORO"), ("163", "AMARILLO NEON/ NEGRO"), ("164", "ROSA/ MORADO"),
            ("166", "AZUL/ PLATA"), ("167", "GRIS/ NARANJA"), ("168", "AZUL/BLANCO/NEGRO"),
            ("169", "BLANCO / VERDE"), ("170", "NEGRO/ PLATA"), ("171", "PLATA"),
            ("172", "ROJO/ GRIS"), ("173", "AMARILLO/ PLATA"), ("175", "BLANCO/ORO"),
            ("176", "BLANCO/ PLATA"), ("177", "VERDE/AZUL"), ("178", "VERDE NEON/NEGRO"),
            ("179", "GRIS OXFORD / GRIS"), ("182", "AZUL TURQUESA/NEGRO"), ("184", "NEGRO/ MORADO"),
            ("185", "NEGRO / AZUL CIELO"), ("187", "AMARILLO NEON"), ("188", "PLATA/NEGRO"),
            ("189", "MORADO/ NEGRO"), ("190", "AQUA/ NEGRO"), ("191", "NARANJA/ BLANCO"),
            ("192", "VERDE/BLANCO"), ("193", "BLANCO/AZUL"), ("194", "BLANCO/GRIS"),
            ("195", "ROSA/BLANCO"), ("196", "GRIS/ BLANCO"), ("197", "VERDE AQUA"),
            ("198", "AZUL/ NEGRO"), ("199", "AZUL MARINO"), ("200", "AMARILLO / BLANCO"),
            ("201", "NEGRO / AMARILLO NEON"), ("206", "AZUL MARINO/ROJO"),
            ("207", "BLANCO/ VERDE AQUA"), ("209", "NEGRO/ AZUL AQUA"), ("210", "BLANCO/ MORADO"),
            ("211", "NEGRO/ AZUL MARINO"), ("212", "ROJO RUBI"), ("213", "PLATA / AMARILLO"),
            ("214", "NEGRO/ ARENA"), ("215", "AZUL AQUA / AMARILLO NEON"),
            ("218", "AZUL/ AMARILLO/ROJO"), ("219", "NEGRO / ROJO / AMARILLO"),
            ("220", "BLANCO/ROJO/AMARILLO"), ("221", "NEGRO/ MORADO/ AMARILLO"),
            ("224", "OXFORD/AMARILLO NEON"), ("225", "MORADO/AMARILLO"), ("226", "GRIS"),
            ("227", "AMARILLO/OXFORD"), ("228", "VERDE AQUA/ AMARILLO"), ("233", "ROJO/AZUL REY"),
            ("236", "NARANJA/ AMARILLO NEON"), ("237", "AMARILLO/AZUL"),
            ("240", "BLANCO/ AZUL/ NEGRO"), ("247", "AMARILLO NEON/ NEGRO"),
            ("248", "NEGRO / FIUSHA"), ("251", "AZUL/NARANJA"), ("254", "LILA-NEGRO-BLANCO"),
            ("255", "GRIS/OXFORD"), ("257", "NEGRO / AMA. NEON/ TURQUESA"),
            ("258", "AZUL REY / AMARILLO NEÓN"), ("259", "NARANJA / GRIS"),
            ("260", "NEGRO / NARANJA / AZUL"), ("261", "NEGRO / AZUL REY / AMARILLO"),
            ("263", "NEGRO/VERDE NEON/ROSA"), ("264", "NEGRO / TURQUEZA / ROJO"),
            ("267", "MORADO"), ("269", "ROJO / AZUL / NEGRO"),
            ("270", "AZUL MARINO/VERDE/AMARILLO"), ("271", "MORADO / ROJO / AMARILLO"),
            ("276", "MULTICOLOR/BLANCO"), ("277", "MULTICOLOR/NEGRO"),
            ("279", "MAGENTA/NEON/AZUL REY"), ("282", "AMA.NEON/VERDE MILITAR/NJA"),
            ("283", "NARANJA / MORADO/ AMA NEON."), ("286", "SALMON"),
            ("287", "AZUL REY / AQUA"), ("288", "BLANCO / ROJO / NEGRO"),
            ("289", "BLANCO / NARANJA / NEGRO"), ("290", "BLANCO / GRIS / NEGRO"),
            ("294", "AMA NEON/ TURQUESA"), ("295", "AMARILLO NEON/NARANJA"),
            ("296", "MORADO/NARANJA FLT."), ("297", "AZUL REY/ ROJO"),
            ("299", "ROJO / VERDE"), ("300", "NEGRO/PURPLE/NEON YELLOW"),
            ("301", "TURQUESA/VERDE"), ("303", "VERDE / AMARILLO NEON"),
            ("305", "OXFORD/NARANJA/AMA. NEON"), ("306", "TURQUESA/ NEON"),
            ("307", "BLANCO/ROJO/AZUL MARINO"), ("308", "MAGENTA/AZUL MARINO"),
            ("310", "NARANJA/MAGENTA"), ("312", "NEON/ FIUSHA"), ("313", "MULTICOLOR"),
            ("314", "AMARILLO NEON/ OXFORD"), ("315", "VERDE/BLANCO/ROJO"),
            ("316", "MAGENTA / ROSA"), ("318", "MAGENTA / AMARILLO NEON"),
            ("319", "AZUL/ TURQUESA / AMA. NEON"), ("320", "AZUL REY/ ROJO/ AMARILLO"),
            ("321", "AZUL/ROJO/BLANCO"), ("322", "GRIS/ROJO/BLANCO"),
            ("323", "NEGRO/ AQUA / PLATA"), ("326", "AZUL/ AMARILLO NEON"),
            ("328", "BLANCO / VERDE / NEGRO"), ("330", "AZUL MARINO/ROJO/BLANCO"),
            ("331", "AMARILLO NEON/VERDE"), ("332", "AZUL/ MENTA/ MAGENTA"),
            ("333", "VERDE NEON"), ("335", "BLANCO/VERDE/NEGRO"), ("336", "NEGRO/OXFORD"),
            ("338", "BLANCO/VERDE/ROJO NEON"), ("339", "AMARILLO NEON/NEGRO/ROJO"),
            ("340", "BLANCO / AZUL / AMA. NEON"), ("341", "ROSA/NEGRO/AMARILLO NEON"),
            ("342", "AMARILLO NEON /NEGRO/ OXFORD"), ("343", "NEGRO/BLANCO/ROJO"),
            ("344", "NEGRO/MAGENTA"), ("345", "MORADO/NEGRO/NARANJA"),
            ("346", "TURQUEZA/AZUL MARINO/ROJO"), ("347", "NARANJA / AZUL REY"),
            ("348", "AMARILLO NEON/ROSA"), ("349", "AZUL REY/FIUSHA/AMA. NEON"),
            ("350", "BLANCO/NEGRO/VERDE NEON"), ("351", "OXFORD/AMA.NEON/NARANJA FLT."),
            ("352", "BLANCO/NEGRO/AZUL"), ("353", "ROJO/AZUL/AMARILLO NEON"),
            ("354", "OXFORD/VERDE NEON/ROSA"), ("355", "MORADO / LILA"),
            ("356", "AZUL MARINO/NARANJA/VERDE NEON"), ("357", "CORAL/ROJO CEREZA/AQUA"),
            ("358", "VERDE/VIOLETA"), ("359", "MORADO/ VERDE"),
            ("361", "AMARILLO NEON/AZUL"), ("362", "NARANJA METALICO/MORADO"),
            ("363", "BLANCO / AZUL / ROJO"), ("364", "MORADO/NARANJA/NEGRO"),
            ("365", "TURQUESA/ROJO/AMARILLO NEON"), ("366", "NEGRO/NARANJA FLT."),
            ("367", "TURQUESA/AMARILLO NEON"), ("368", "AZUL MARINO/VERDE NEON"),
            ("369", "BLANCO/OXFORD/AMARILLO NEON"), ("370", "NEGRO/VERDE NEON"),
            ("371", "NEGRO/AMARILLO"), ("372", "NEGRO/BLANCO/NARANJA"),
            ("373", "VERDE NEON/NEGRO/MORADO"), ("374", "NARANJA/NEGRO/AMARILLO NEON"),
            ("375", "AMARILLO NEON/TURQUESA"), ("376", "BLANCO / GRIS / AZUL"),
            ("377", "BLANCO/GRIS/ROJO"), ("378", "NEGRO/ NEON/ PLATA"),
            ("379", "AMARILLO NEON/PLATA/NEGRO"), ("380", "MORADO/ ROSA/ NEGRO"),
            ("381", "AZUL/PLATA/ NEGRO"), ("382", "BLANCO/ROJO/NEGRO"),
            ("383", "BLANCO/ NARAJA/ AZUL"), ("384", "BLANCO/ NEGRO/ NEON"),
            ("385", "NEGRO/BLANCO/ROJO"), ("387", "NEGRO/ PLATA/ NEON"),
            ("388", "ROJO/PLATA/NEGRO"), ("389", "BLANCO/ AZUL / PLATA"),
            ("390", "AMARILLO NEON/ MORADO/NEGRO"), ("391", "PLATA/ ROJO/NEGRO"),
            ("392", "MORADO/NARANJA FLT/NEGRO"), ("393", "TURQUESA/ AZUL/ NEGRO"),
            ("394", "OXFORD /NARANJA NEON"), ("395", "AMARILLO NEON/AZUL/OXFORD"),
            ("396", "FIUSHA/AZUL/NEGRO"), ("397", "VERDE/NARANJA FLT."),
            ("398", "OXFORD/VERDE NEON"), ("399", "BLANCO/AMA NEON/VERDE NEON"),
            ("400", "AZUL/BLANCO/GRIS"), ("401", "AMARILLO NEON/MORADO/ROSA"),
            ("402", "BLANCO/AMARILLO NEON"), ("403", "MORADO/ NEON"),
            ("404", "AZUL TURQUESA/ ROJO"), ("405", "VERDE OSCURO"),
            ("406", "MORADO/ NEON/ FIUSHA"), ("407", "AZUL OSCURO/ AZUL TURQUESA/NJA"),
            ("408", "BLANCO/ ROJO/ NEON"), ("409", "BLANCO/ AZUL MARINO/ AMARILLO"),
            ("410", "BLANCO/ VERDE/ AMARILLO NEON"), ("411", "BLANCO/NEGRO/AMARILLO NEON"),
            ("412", "NARANJA/ AQUA"), ("413", "AZUL AQUA/NARANJA"), ("414", "ROJO/OXFORD"),
            ("415", "NEGRO/MENTA"), ("416", "MORADO / FIUSHA"),
            ("417", "AMARILLO NEON/ VERDE NEON"), ("419", "AZUL/ NARANJA FLT"),
            ("420", "NARANJA FLT/ PLATA"), ("421", "TORNASOL/ AMARILLO NEON"),
            ("426", "TRANSPARENTE/ NEGRO"), ("427", "TRANSPARENTE/ AMARILLO NEON"),
            ("428", "TRANSPARENTE/ROSA"), ("429", "TRANSPARENTE/ VERDE"),
            ("431", "NEGRO/ ROSA/ AZUL"), ("432", "AMA. NEON/NEGRO"),
            ("433", "NARANJA/TURQUEZA"), ("434", "AZUL MARINO/ AMARILLO NEON"),
            ("435", "ROSA FLT./NEON"), ("436", "AMARILLO / AZUL TURQUEZA"),
            ("438", "MORADO/ PLATA/ NEGRO"), ("439", "NARANJA FLT/ PLATA/ NEGRO"),
            ("440", "NEGRO/TURQUESA"), ("441", "AZUL/ROJO/AMARILLO NEON"),
            ("442", "BLANCO/OXFORD/AMA. NEON"), ("443", "NEGRO/OXFORD/AMA. NEON"),
            ("444", "VERDE/ AQUA"), ("445", "MORADO/VERDE FLT./ROJO CEREZA"),
            ("446", "AMA.NEON/ MORADO/ROJO CEREZA"), ("447", "NEGRO/ ROJO/ BLANCO"),
            ("448", "ROJO/ AZUL/ BLANCO"), ("449", "BLANCO/ NEGRO/ NARANJA"),
            ("450", "AZUL METALICO"), ("451", "NARANJA/AZUL MARINO"),
            ("452", "NEGRO/ NARANJA FLT."), ("453", "AMA.NEON/ NARANJA FLT."),
            ("454", "ROSA FLT./AMARILLO NEON"), ("456", "NEGRO/ ROJO"),
            ("457", "BLANCO/ VERDE NEON/ NEGRO"), ("458", "BLANCO/ AZUL/ NEGRO"),
            ("460", "ESTAMPADO"), ("461", "LISO"), ("462", "COMPUESTO"),
            ("463", "AMARILLO NEON/ PLATA"), ("464", "VERDE/ BEIGE"), ("465", "GRIS JASPE"),
            ("466", "VERDE PISTACHE"), ("467", "AZUL PITUFO"), ("468", "GUINDA"),
            ("469", "ROSA JASPE"), ("470", "CORAL/ NEON"), ("471", "CORAL JASPE"),
            ("472", "VERDE MANZANA"), ("473", "ROSA MEXICANO"), ("474", "MORADO/ AZUL"),
            ("475", "PURPURA"), ("476", "VIOLETA"), ("477", "AZUL JASPE"),
            ("478", "AZUL PASTEL"), ("479", "FIUSHA"), ("480", "VERDE JASPE"),
            ("481", "AMARILLO NEON/VERDE/NJA FLT"), ("482", "NARANJA FLT./NEGRO/AMA.NEON"),
            ("483", "AZUL AQUA"), ("484", "VERDE LIMON"), ("485", "AZUL / MORADO"),
            ("486", "PISTACHE GRIS"), ("487", "NARANJA/PINK"), ("488", "LILA/AZUL"),
            ("489", "NEGRO/BLANCO"), ("490", "AZUL MARINO/ NEGRO"), ("491", "LILA"),
            ("492", "ROSA RAYAS"), ("493", "OXFORD / PLATA / AMARILLO NEON"),
            ("494", "MORADO / AZUL CIELO"), ("495", "AMARILLO CANARIO"),
            ("496", "MORADO / AMARILLO NEON"), ("497", "NARANJA / VERDE NEON"),
            ("498", "BLANCO / NEGRO"), ("499", "BLANCO / OXFORD / PLATA"),
            ("500", "GRIS/ AMARILLO NEON"), ("501", "NEGRO/CORAL"),
            ("502", "NEGRO / GRIS / AMARILLO NEON"), ("503", "NEGRO/ BLANCO/ AMARILLO NEON"),
            ("504", "AZUL/ NEGRO / ROJO"), ("505", "CORAL"),
            ("506", "AMARILLO NEON/ MORADO"), ("507", "VERDE NEON/AMARILLO NEON"),
            ("508", "NEGRO / GRIS"), ("509", "AZUL / ROJO"), ("510", "VERDE /NARANJA"),
            ("511", "AZUL / FIUSHA"), ("512", "NEGRO / AZUL"), ("513", "GRIS / ROJO / NEGRO"),
            ("514", "VERDE MILITAR"), ("515", "ROJO/NEGRO/AMARILLO NEON"),
            ("516", "AMA NEON / VERDE MZA / NJA FLT"), ("517", "ROSA/ AMARILLO NEON/ NEGRO"),
            ("518", "NEON/NEGRO"), ("519", "VERDE CAMUFLAJE"), ("520", "AZUL"),
            ("521", "GRIS/NEGRO"), ("522", "GRIS/FIUSHA"), ("523", "GRIS/VERDE"),
            ("526", "AZUL REY/VERDE NEON"), ("528", "ROJO/AMARILLO"),
            ("530", "BLANCO/OXFORD NACARADO"), ("531", "ROJO/GUINDA"), ("532", "VINO"),
            ("533", "GRIS/AZUL"), ("534", "OXFORD/NARANJA"), ("535", "BLANCO/VERDE NEON"),
            ("536", "GUINDA/NEGRO"), ("537", "BLANCO"), ("538", "PLATA/OXFORD"),
            ("539", "AZUL MARINO/BLANCO"), ("540", "AZUL MARINO/BLANCO/ORO"),
            ("541", "NEGRO/AMARILLO NEON"), ("542", "NEGRO/AZUL"),
            ("543", "AZUL CELESTE/AZUL MARINO"), ("544", "VERDE JADE"),
            ("545", "BLANCO/PERLA"), ("546", "ESTAMPADO MILITAR NEGRO"),
            ("547", "ESTAMPADO MILITAR MORADO"), ("548", "ESTAMPADO LIRIOS"),
            ("549", "ESTAMPADO SNAKE"), ("550", "ESTAMPADO HOLIDAY"),
            ("551", "ESTAMPADO TROPICAL"), ("552", "ESTAMPADO MANDALA"),
            ("553", "ESTAMPADO CALEIDOSCOPIO"), ("554", "ESTAMPADO ATARDECER"),
            ("555", "ESTAMPADO PAISLEY"), ("556", "ESTAMPADO BIGARO"),
            ("557", "ESTAMPADO PARADISE"), ("558", "ESTAMPADO ACUARELA TROPICAL"),
            ("559", "ESTAMPADO PARAISO"), ("560", "ESTAMPADO DAHILA"), ("561", "AZUL"),
            ("564", "TURQUESA/OXFORD"), ("565", "VINO/ROJO"), ("566", "GRIS CLARO JASPE"),
            ("567", "AZUL REY JASPE"), ("568", "CORAL ROSA"), ("569", "NEON AZUL"),
            ("570", "OCRES"), ("571", "MORADO/ORO"), ("572", "AZUL/OXFORD"),
            ("573", "BLANCO/VERDE"), ("574", "OXFORD"), ("575", "OXFORD/VERDE LIMON"),
            ("576", "ROJO/NEGRO/BLANCO"), ("577", "AZUL/VERDE"),
            ("578", "NEGRO/AMARILLO/ROJO"), ("579", "NARANJA NEON"),
            ("580", "BLANCO/PURPURA"), ("581", "NARANJA/ROJO"), ("582", "NEGRO/GRIS/ROSA"),
            ("583", "ROSA/FIUSHA"), ("584", "NEGRO/GRIS/ROJO"), ("585", "VERDE/BLANCO/ROJO"),
            ("586", "AZUL AQUA/VERDE MUSGO"), ("587", "NARANJA/AMARILLO/NEGRO"),
            ("588", "NARANJA NEON/BLANCO"), ("589", "ROJO/AZUL MARINO"),
            ("590", "AZUL JASPE/NARANJA"), ("591", "FIUSHA/NEGRO"), ("592", "OXFORD/AZUL"),
            ("593", "ORO/NARANJA"), ("594", "NEGRO/AZUL/GRIS"), ("595", "BLANCO/VERDE/ROJO"),
            ("596", "NÁCAR/ORO"), ("597", "ORO/NÁCAR"), ("598", "VERDE/OXFORD"),
            ("599", "AMARILLO/ VERDE"), ("600", "AMARILLO/VERDE MUSGO"),
            ("601", "AZUL / ROSA"), ("602", "BEIGE"), ("603", "NEGRO/MULTICOLOR"),
            ("604", "TURQUESA/ROJO/MARINO"), ("605", "CAFÉ"), ("606", "AZUL/CAFÉ"),
            ("607", "CAFÉ/NEGRO"), ("608", "CAFÉ/VERDE"), ("609", "CHERRY/NEGRO"),
            ("610", "VERDE/CAFÉ"), ("611", "CAFÉ/CHERRY"), ("612", "CHERRY/CAFÉ"),
            ("613", "NEGRO/CAFÉ"), ("614", "NEGRO/CHERRY"), ("615", "VERDE/NEGRO"),
            ("616", "VERDE/CHERRY"), ("617", "CAFÉ/VERDE/NARANJA"),
            ("618", "CHERRY/CAFÉ/MIEL"), ("619", "CAFÉ"), ("620", "NEGRO/CAFÉ"),
            ("621", "CHERRY"), ("622", "NEGRO/ROJO"), ("623", "VERDE/CHERRY/CAFÉ"),
            ("624", "CAFÉ/MIEL/ROJO"), ("625", "ROJO/CAFÉ/MIEL"), ("626", "MIEL/ROJO/CAFÉ"),
            ("627", "MIEL/ROJO/AZUL"), ("628", "ROJO/CAFÉ/NEGRO"), ("629", "AZUL BRILLANTE"),
            ("630", "VERDE OLIVO"), ("631", "AZUL MARINO/ AZUL REY"), ("632", "CORAL /NEGRO"),
            ("633", "GRIS OXFORD/NARANJA"), ("634", "MULTICOLOR TRIANGULOS"),
            ("635", "NEGRO/VERDE/AZUL/ROSA"), ("636", "JADE / NARANJA"),
            ("637", "BLANCO / ORO / NEGRO"), ("638", "NEGRO / AMARILLO / PLATA"),
            ("639", "BLANCO / NARANJA / AZUL"), ("641", "ROSA / VERDE / AMARILLO"),
            ("642", "AMARILLO / ROSA / NARANJA"), ("643", "MAGENTA / ROJO"),
            ("644", "AZUL / GRIS"), ("645", "BLANCO / ROSA / AZUL"),
            ("646", "ORO / NEGRO"), ("647", "BLANCO / ROJO / AZUL"),
            ("649", "NEGRO / ROJO / ORO"), ("650", "AZUL / NARANJA / NEGRO"),
            ("651", "BLANCO/ AZUL/ NARANJA"), ("652", "AZUL/ BLANCO/ ORO"),
            ("653", "GRIS / ROJO"), ("654", "GRIS / VERDE / NARANJA"),
            ("655", "BLANCO / PLATA / ORO"), ("656", "NEGRO / ORO / PLATA"),
            ("667", "NEGRO / AQUA"), ("668", "OXFORD / BLANCO"), ("669", "VERDE / AMARILLO"),
            ("670", "NEGRO / AMARILLO"), ("671", "BLANCO/AMARILLO"),
            ("674", "AZUL TURQUEZA/BLANCO"), ("683", "AZUL REY/BLANCO"),
            ("684", "VERDE AQUA/NEGRO"), ("685", "NEGRO/VERDE AQUA"),
            ("686", "ROSA/GUINDA"), ("688", "AZUL MARINO/ORO"), ("689", "AMARILLO/ROJO"),
            ("690", "VERDE AQUA/OXFORD"), ("691", "NARANJA/AZUL"), ("695", "INDIGO"),
            ("696", "AMARILLO/ NARANJA"), ("796", "AZUL CELESTE/ROSA"),
            ("904", "ROJO/AZUL/AMARILLO"), ("905", "AZUL/VERDE/ROSA"),
            ("906", "AMARILLO/AZUL/BLANCO"), ("907", "NEGRO/ AZUL REY"),
            ("908", "MENTA"), ("989", "NEGRO/AZUL/ROSA")
        };

        // Insertar en lotes de 50 para no saturar EF
        var batch = new List<CCatalogoElemento>();
        foreach (var (clave, nombre) in combinaciones)
        {
            batch.Add(CrearElemento(idCatCombinaciones, clave, nombre, null, operador, fecha));
            if (batch.Count >= 50)
            {
                context.CCatalogoElementos.AddRange(batch);
                await context.SaveChangesAsync();
                batch.Clear();
            }
        }
        if (batch.Count > 0)
        {
            context.CCatalogoElementos.AddRange(batch);
            await context.SaveChangesAsync();
        }
    }

    private static CCatalogo CrearCatalogo(string clave, string nombre, string desc, string operador, DateTimeOffset fecha)
    {
        return new CCatalogo
        {
            ClCatalogo = clave,
            NbCatalogo = nombre,
            DsCatalogo = desc,
            ClEstatusCatalogo = "ACTIVO",
            ClOperadorCrea = operador,
            NbArtefactoCrea = "DbSeeder",
            FeCreacion = fecha
        };
    }

    private static CCatalogoElemento CrearElemento(int idCat, string clave, string nombre, int? idPadre, string operador, DateTimeOffset fecha)
    {
        return new CCatalogoElemento
        {
            IdCatalogo = idCat,
            ClCatalogoElemento = clave,
            NbCatalogoElemento = nombre,
            DsCatalogoElemento = nombre,
            IdCatalogoElementoPadre = idPadre,
            ClEstatusCatalogoElemento = "ACTIVO",
            ClOperadorCrea = operador,
            NbArtefactoCrea = "DbSeeder",
            FeCreacion = fecha
        };
    }
}
