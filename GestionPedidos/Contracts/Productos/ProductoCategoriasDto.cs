namespace GestionPedidos.Contracts.Productos;

// ============================================================================
//  1. TEXTIL
// ============================================================================
public record ProductoTextilCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string NbTejido,
    string? DsComposicion,
    string? DsCorte,
    int? NoGramajeGsm,
    int IdElemGenero, // FK a CCatalogoElemento (Genero)
    List<VarianteNestedCreateDto>? Variantes
);

public record ProductoTextilUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string NbTejido,
    string? DsComposicion,
    string? DsCorte,
    int? NoGramajeGsm,
    int IdElemGenero,
    List<VarianteNestedUpdateDto>? Variantes
);

public record ProductoTextilDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string NbTejido,
    string? DsComposicion,
    string? DsCorte,
    int? NoGramajeGsm,
    int IdElemGenero,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record ProductoTextilBulkDto(
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string NbTejido,
    string? DsComposicion,
    string? DsCorte,
    int? NoGramajeGsm,
    string ClGenero, // Clave del elemento en catálogo (ej: "MASCULINO", "FEMENINO")
    List<VarianteBulkDto>? Variantes
);

// ============================================================================
//  2. CONO
// ============================================================================
public record ProductoConoCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    decimal NoAlturaCm,
    string? DsMaterial,
    bool FgEsUnitalla,
    List<VarianteNestedCreateDto>? Variantes
);

public record ProductoConoUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    decimal NoAlturaCm,
    string? DsMaterial,
    bool FgEsUnitalla,
    List<VarianteNestedUpdateDto>? Variantes
);

public record ProductoConoDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    decimal NoAlturaCm,
    string? DsMaterial,
    bool FgEsUnitalla,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record ProductoConoBulkDto(
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    decimal NoAlturaCm,
    string? DsMaterial,
    bool FgEsUnitalla,
    List<VarianteBulkDto>? Variantes
);

// ============================================================================
//  3. ESPINILLERA
// ============================================================================
public record ProductoEspinilleraCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsMaterial,
    string? DsProteccion,
    List<VarianteNestedCreateDto>? Variantes
);

public record ProductoEspinilleraUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsMaterial,
    string? DsProteccion,
    List<VarianteNestedUpdateDto>? Variantes
);

public record ProductoEspinilleraDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsMaterial,
    string? DsProteccion,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record ProductoEspinilleraBulkDto(
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsMaterial,
    string? DsProteccion,
    List<VarianteBulkDto>? Variantes
);

// ============================================================================
//  4. ACCESORIO
// ============================================================================
public record ProductoAccesorioCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    List<VarianteNestedCreateDto>? Variantes
);

public record ProductoAccesorioUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    List<VarianteNestedUpdateDto>? Variantes
);

public record ProductoAccesorioDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record ProductoAccesorioBulkDto(
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    List<VarianteBulkDto>? Variantes
);

// ============================================================================
//  5. MOCHILA
// ============================================================================
public record ProductoMochilaCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    decimal? NoCapacidadLitros,
    int? NoCompartimentos,
    string? DsDimensiones,
    bool FgEsUnitalla,
    List<VarianteNestedCreateDto>? Variantes
);

public record ProductoMochilaUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    decimal? NoCapacidadLitros,
    int? NoCompartimentos,
    string? DsDimensiones,
    bool FgEsUnitalla,
    List<VarianteNestedUpdateDto>? Variantes
);

public record ProductoMochilaDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    decimal? NoCapacidadLitros,
    int? NoCompartimentos,
    string? DsDimensiones,
    bool FgEsUnitalla,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record ProductoMochilaBulkDto(
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string ClSubcategoria,
    string? DsMaterialPrincipal,
    decimal? NoCapacidadLitros,
    int? NoCompartimentos,
    string? DsDimensiones,
    bool FgEsUnitalla,
    List<VarianteBulkDto>? Variantes
);

// ============================================================================
//  6. FITNESS
// ============================================================================
public record ProductoFitnessCreateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsComposicion,
    string? DsRelleno,
    string? DsCierre,
    string? DsProteccion,
    decimal? NoPesoOz,
    List<VarianteNestedCreateDto>? Variantes
);

public record ProductoFitnessUpdateDto(
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsComposicion,
    string? DsRelleno,
    string? DsCierre,
    string? DsProteccion,
    decimal? NoPesoOz,
    List<VarianteNestedUpdateDto>? Variantes
);

public record ProductoFitnessDto(
    Guid IdProducto,
    string? ClProducto,
    string NbProducto,
    int IdElemCategoria,
    int? IdElemLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsComposicion,
    string? DsRelleno,
    string? DsCierre,
    string? DsProteccion,
    decimal? NoPesoOz,
    DateTimeOffset FeCreacion,
    DateTimeOffset? FeModificacion,
    List<GestionPedidos.Contracts.Variantes.VarianteDto>? Variantes = null
);

public record ProductoFitnessBulkDto(
    string? ClProducto,
    string NbProducto,
    string ClCategoria,
    string? ClLineaColeccion,
    string? ClHsCode,
    string ClEstatusProducto,
    string? DsComposicion,
    string? DsRelleno,
    string? DsCierre,
    string? DsProteccion,
    decimal? NoPesoOz,
    List<VarianteBulkDto>? Variantes
);
