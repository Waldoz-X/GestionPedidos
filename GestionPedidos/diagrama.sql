CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [c_catalogo] (
    [IdCatalogo] int NOT NULL IDENTITY,
    [ClCatalogo] nvarchar(50) NOT NULL,
    [NbCatalogo] nvarchar(150) NOT NULL,
    [DsCatalogo] nvarchar(max) NULL,
    [IdCatalogoPadre] int NULL,
    [ClEstatusCatalogo] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [CatalogoPadreIdCatalogo] int NULL,
    CONSTRAINT [PK_c_catalogo] PRIMARY KEY ([IdCatalogo]),
    CONSTRAINT [FK_c_catalogo_c_catalogo_CatalogoPadreIdCatalogo] FOREIGN KEY ([CatalogoPadreIdCatalogo]) REFERENCES [c_catalogo] ([IdCatalogo])
);
GO


CREATE TABLE [PoliticasPrecios] (
    [IdPolitica] uniqueidentifier NOT NULL,
    [NbPolitica] nvarchar(max) NOT NULL,
    [ClTipoPolitica] nvarchar(max) NOT NULL,
    [NoPrioridad] int NOT NULL,
    [MnFactorDescuento] decimal(18,2) NOT NULL,
    [FeVigenteDesde] datetimeoffset NOT NULL,
    [FeVigenteHasta] datetimeoffset NULL,
    [ClEstatusPolitica] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    CONSTRAINT [PK_PoliticasPrecios] PRIMARY KEY ([IdPolitica])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [c_catalogo_elemento] (
    [IdCatalogoElemento] int NOT NULL IDENTITY,
    [IdCatalogo] int NOT NULL,
    [ClCatalogoElemento] nvarchar(50) NOT NULL,
    [NbCatalogoElemento] nvarchar(150) NOT NULL,
    [DsCatalogoElemento] nvarchar(max) NULL,
    [IdCatalogoElementoPadre] int NULL,
    [ClEstatusCatalogoElemento] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ElementoPadreIdCatalogoElemento] int NULL,
    CONSTRAINT [PK_c_catalogo_elemento] PRIMARY KEY ([IdCatalogoElemento]),
    CONSTRAINT [FK_c_catalogo_elemento_c_catalogo_IdCatalogo] FOREIGN KEY ([IdCatalogo]) REFERENCES [c_catalogo] ([IdCatalogo]) ON DELETE CASCADE,
    CONSTRAINT [FK_c_catalogo_elemento_c_catalogo_elemento_ElementoPadreIdCatalogoElemento] FOREIGN KEY ([ElementoPadreIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento])
);
GO


CREATE TABLE [Clientes] (
    [IdCliente] uniqueidentifier NOT NULL,
    [NbComercial] nvarchar(max) NOT NULL,
    [ClTipoCliente] nvarchar(max) NOT NULL,
    [ClMonedaIdCatalogoElemento] int NOT NULL,
    [DsCanalVenta] nvarchar(max) NULL,
    [MnLimiteCredito] decimal(18,2) NOT NULL,
    [ClEstatusCliente] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([IdCliente]),
    CONSTRAINT [FK_Clientes_c_catalogo_elemento_ClMonedaIdCatalogoElemento] FOREIGN KEY ([ClMonedaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [Productos] (
    [IdProducto] uniqueidentifier NOT NULL,
    [NbModelo] nvarchar(max) NOT NULL,
    [IdDivision] int NOT NULL,
    [IdSerieCatalogo] uniqueidentifier NOT NULL,
    [IdLineaColeccion] int NULL,
    [IdGamaCatalogo] int NULL,
    [ClHsCode] nvarchar(max) NULL,
    [IdEstado] int NOT NULL,
    [ClEstatusProducto] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [DivisionCatalogoIdCatalogoElemento] int NOT NULL,
    [LineaColeccionCatalogoIdCatalogoElemento] int NULL,
    [SerieCatalogoIdCatalogoElemento] int NOT NULL,
    [GamaCatalogoIdCatalogoElemento] int NULL,
    [EstadoIdCatalogoElemento] int NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_Productos_c_catalogo_elemento_DivisionCatalogoIdCatalogoElemento] FOREIGN KEY ([DivisionCatalogoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE,
    CONSTRAINT [FK_Productos_c_catalogo_elemento_EstadoIdCatalogoElemento] FOREIGN KEY ([EstadoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE,
    CONSTRAINT [FK_Productos_c_catalogo_elemento_GamaCatalogoIdCatalogoElemento] FOREIGN KEY ([GamaCatalogoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]),
    CONSTRAINT [FK_Productos_c_catalogo_elemento_LineaColeccionCatalogoIdCatalogoElemento] FOREIGN KEY ([LineaColeccionCatalogoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]),
    CONSTRAINT [FK_Productos_c_catalogo_elemento_SerieCatalogoIdCatalogoElemento] FOREIGN KEY ([SerieCatalogoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [DireccionesCliente] (
    [IdDireccion] uniqueidentifier NOT NULL,
    [IdCliente] uniqueidentifier NOT NULL,
    [IdPais] int NOT NULL,
    [IdEstadoCatalogo] int NULL,
    [NbAlias] nvarchar(max) NULL,
    [DsLinea1] nvarchar(max) NOT NULL,
    [DsLinea2] nvarchar(max) NULL,
    [NbCiudad] nvarchar(max) NOT NULL,
    [NbEstado] nvarchar(max) NULL,
    [ClCodigoPostal] nvarchar(max) NULL,
    [ClPais] nvarchar(max) NOT NULL,
    [ClEstatusDireccion] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ClienteIdCliente] uniqueidentifier NOT NULL,
    [PaisCatalogoIdCatalogoElemento] int NOT NULL,
    [EstadoCatalogoIdCatalogoElemento] int NULL,
    CONSTRAINT [PK_DireccionesCliente] PRIMARY KEY ([IdDireccion]),
    CONSTRAINT [FK_DireccionesCliente_Clientes_ClienteIdCliente] FOREIGN KEY ([ClienteIdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE CASCADE,
    CONSTRAINT [FK_DireccionesCliente_c_catalogo_elemento_EstadoCatalogoIdCatalogoElemento] FOREIGN KEY ([EstadoCatalogoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]),
    CONSTRAINT [FK_DireccionesCliente_c_catalogo_elemento_PaisCatalogoIdCatalogoElemento] FOREIGN KEY ([PaisCatalogoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [Estilos] (
    [IdEstilo] uniqueidentifier NOT NULL,
    [IdProducto] uniqueidentifier NOT NULL,
    [NbEstilo] nvarchar(max) NOT NULL,
    [DsEstilo] nvarchar(max) NULL,
    [UrlImagenReferencia] nvarchar(max) NULL,
    [ClEstatusEstilo] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_Estilos] PRIMARY KEY ([IdEstilo]),
    CONSTRAINT [FK_Estilos_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE
);
GO


CREATE TABLE [ProductosAccesorio] (
    [IdProducto] uniqueidentifier NOT NULL,
    [ClSubcategoria] nvarchar(max) NOT NULL,
    [DsMaterialPrincipal] nvarchar(max) NULL,
    [ClTipoMedidaIdCatalogoElemento] int NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosAccesorio] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosAccesorio_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductosAccesorio_c_catalogo_elemento_ClTipoMedidaIdCatalogoElemento] FOREIGN KEY ([ClTipoMedidaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento])
);
GO


CREATE TABLE [ProductosCono] (
    [IdProducto] uniqueidentifier NOT NULL,
    [NoAlturaCm] decimal(18,2) NOT NULL,
    [DsMaterial] nvarchar(max) NULL,
    [FgEsUnitalla] bit NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosCono] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosCono_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE
);
GO


CREATE TABLE [ProductosEspinillera] (
    [IdProducto] uniqueidentifier NOT NULL,
    [DsMaterial] nvarchar(max) NULL,
    [DsProteccion] nvarchar(max) NULL,
    [ClTipoMedidaIdCatalogoElemento] int NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosEspinillera] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosEspinillera_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductosEspinillera_c_catalogo_elemento_ClTipoMedidaIdCatalogoElemento] FOREIGN KEY ([ClTipoMedidaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [ProductosFitness] (
    [IdProducto] uniqueidentifier NOT NULL,
    [DsComposicion] nvarchar(max) NULL,
    [DsRelleno] nvarchar(max) NULL,
    [DsCierre] nvarchar(max) NULL,
    [DsProteccion] nvarchar(max) NULL,
    [NoPesoOz] decimal(18,2) NULL,
    [ClTipoMedidaIdCatalogoElemento] int NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosFitness] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosFitness_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductosFitness_c_catalogo_elemento_ClTipoMedidaIdCatalogoElemento] FOREIGN KEY ([ClTipoMedidaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [ProductosGuante] (
    [IdProducto] uniqueidentifier NOT NULL,
    [NbPalma] nvarchar(max) NOT NULL,
    [DsComposicion] nvarchar(max) NULL,
    [ClMsCode] nvarchar(max) NULL,
    [ClIndicePalma] nvarchar(max) NULL,
    [DsForro] nvarchar(max) NULL,
    [DsCierre] nvarchar(max) NULL,
    [DsHomologacion] nvarchar(max) NULL,
    [ClTipoMedidaIdCatalogoElemento] int NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosGuante] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosGuante_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductosGuante_c_catalogo_elemento_ClTipoMedidaIdCatalogoElemento] FOREIGN KEY ([ClTipoMedidaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [ProductosMochila] (
    [IdProducto] uniqueidentifier NOT NULL,
    [ClSubcategoria] nvarchar(max) NOT NULL,
    [DsMaterialPrincipal] nvarchar(max) NULL,
    [NoCapacidadLitros] decimal(18,2) NULL,
    [NoCompartimentos] int NULL,
    [DsDimensiones] nvarchar(max) NULL,
    [FgEsUnitalla] bit NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosMochila] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosMochila_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE
);
GO


CREATE TABLE [ProductosTextil] (
    [IdProducto] uniqueidentifier NOT NULL,
    [NbTejido] nvarchar(max) NOT NULL,
    [DsComposicion] nvarchar(max) NULL,
    [DsCorte] nvarchar(max) NULL,
    [NoGramajeGsm] int NULL,
    [ClGeneroIdCatalogoElemento] int NOT NULL,
    [ClTipoMedidaIdCatalogoElemento] int NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ProductosTextil] PRIMARY KEY ([IdProducto]),
    CONSTRAINT [FK_ProductosTextil_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductosTextil_c_catalogo_elemento_ClGeneroIdCatalogoElemento] FOREIGN KEY ([ClGeneroIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductosTextil_c_catalogo_elemento_ClTipoMedidaIdCatalogoElemento] FOREIGN KEY ([ClTipoMedidaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [VisibilidadesCatalogo] (
    [IdCliente] uniqueidentifier NOT NULL,
    [IdProducto] uniqueidentifier NOT NULL,
    [ClTipoAcceso] nvarchar(max) NOT NULL,
    [ClEstatusVisibilidad] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ClienteIdCliente] uniqueidentifier NOT NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_VisibilidadesCatalogo] PRIMARY KEY ([IdCliente], [IdProducto]),
    CONSTRAINT [FK_VisibilidadesCatalogo_Clientes_ClienteIdCliente] FOREIGN KEY ([ClienteIdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE CASCADE,
    CONSTRAINT [FK_VisibilidadesCatalogo_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE
);
GO


CREATE TABLE [Variantes] (
    [IdVariante] uniqueidentifier NOT NULL,
    [IdProducto] uniqueidentifier NOT NULL,
    [IdEstilo] uniqueidentifier NULL,
    [SegmentoIdCatalogoElemento] int NOT NULL,
    [ClCombinacion] nvarchar(max) NULL,
    [NbColor] nvarchar(max) NULL,
    [NbColorEn] nvarchar(max) NULL,
    [DsRangoCorrida] nvarchar(max) NULL,
    [UrlImagen] nvarchar(max) NULL,
    [ClItem] nvarchar(max) NULL,
    [ClCodigoBarras] nvarchar(max) NULL,
    [ClEstatusVariante] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ProductoIdProducto] uniqueidentifier NOT NULL,
    [EstiloIdEstilo] uniqueidentifier NULL,
    CONSTRAINT [PK_Variantes] PRIMARY KEY ([IdVariante]),
    CONSTRAINT [FK_Variantes_Estilos_EstiloIdEstilo] FOREIGN KEY ([EstiloIdEstilo]) REFERENCES [Estilos] ([IdEstilo]),
    CONSTRAINT [FK_Variantes_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
    CONSTRAINT [FK_Variantes_c_catalogo_elemento_SegmentoIdCatalogoElemento] FOREIGN KEY ([SegmentoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [Skus] (
    [IdSku] uniqueidentifier NOT NULL,
    [IdVariante] uniqueidentifier NOT NULL,
    [IdTalla] int NOT NULL,
    [ClEstatusSku] nvarchar(max) NOT NULL,
    [NoStockDisponible] int NOT NULL,
    [NoStockReservado] int NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [VarianteIdVariante] uniqueidentifier NOT NULL,
    [TallaIdCatalogoElemento] int NOT NULL,
    CONSTRAINT [PK_Skus] PRIMARY KEY ([IdSku]),
    CONSTRAINT [FK_Skus_Variantes_VarianteIdVariante] FOREIGN KEY ([VarianteIdVariante]) REFERENCES [Variantes] ([IdVariante]) ON DELETE CASCADE,
    CONSTRAINT [FK_Skus_c_catalogo_elemento_TallaIdCatalogoElemento] FOREIGN KEY ([TallaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [Precios] (
    [IdPrecio] uniqueidentifier NOT NULL,
    [IdSku] uniqueidentifier NOT NULL,
    [IdPolitica] uniqueidentifier NOT NULL,
    [IdCliente] uniqueidentifier NULL,
    [MnPrecioNeto] decimal(18,2) NOT NULL,
    [ClMonedaIdCatalogoElemento] int NOT NULL,
    [ClEstatusPrecio] nvarchar(max) NOT NULL,
    [FeVigenteDesde] datetimeoffset NOT NULL,
    [FeVigenteHasta] datetimeoffset NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [SkuIdSku] uniqueidentifier NOT NULL,
    [PoliticaIdPolitica] uniqueidentifier NOT NULL,
    [ClienteIdCliente] uniqueidentifier NULL,
    CONSTRAINT [PK_Precios] PRIMARY KEY ([IdPrecio]),
    CONSTRAINT [FK_Precios_Clientes_ClienteIdCliente] FOREIGN KEY ([ClienteIdCliente]) REFERENCES [Clientes] ([IdCliente]),
    CONSTRAINT [FK_Precios_PoliticasPrecios_PoliticaIdPolitica] FOREIGN KEY ([PoliticaIdPolitica]) REFERENCES [PoliticasPrecios] ([IdPolitica]) ON DELETE CASCADE,
    CONSTRAINT [FK_Precios_Skus_SkuIdSku] FOREIGN KEY ([SkuIdSku]) REFERENCES [Skus] ([IdSku]) ON DELETE CASCADE,
    CONSTRAINT [FK_Precios_c_catalogo_elemento_ClMonedaIdCatalogoElemento] FOREIGN KEY ([ClMonedaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [AsignacionesClienteEmpleado] (
    [IdEmpleado] uniqueidentifier NOT NULL,
    [IdCliente] uniqueidentifier NOT NULL,
    [ClTipoRelacion] nvarchar(max) NOT NULL,
    [ClEstatusAsignacion] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [EmpleadoIdEmpleado] uniqueidentifier NOT NULL,
    [ClienteIdCliente] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AsignacionesClienteEmpleado] PRIMARY KEY ([IdEmpleado], [IdCliente]),
    CONSTRAINT [FK_AsignacionesClienteEmpleado_Clientes_ClienteIdCliente] FOREIGN KEY ([ClienteIdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] uniqueidentifier NOT NULL,
    [IdCliente] uniqueidentifier NULL,
    [ClEstatusUsuario] nvarchar(max) NOT NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ClienteIdCliente] uniqueidentifier NULL,
    [EmpleadoIdEmpleado] uniqueidentifier NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Clientes_ClienteIdCliente] FOREIGN KEY ([ClienteIdCliente]) REFERENCES [Clientes] ([IdCliente])
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Empleados] (
    [IdEmpleado] uniqueidentifier NOT NULL,
    [IdUsuario] uniqueidentifier NULL,
    [IdArea] int NULL,
    [ClEmpleado] nvarchar(max) NOT NULL,
    [NbEmpleado] nvarchar(max) NOT NULL,
    [NbApellidos] nvarchar(max) NOT NULL,
    [DsArea] nvarchar(max) NULL,
    [ClEstatusEmpleado] nvarchar(max) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    CONSTRAINT [PK_Empleados] PRIMARY KEY ([IdEmpleado]),
    CONSTRAINT [FK_Empleados_AspNetUsers_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Empleados_c_catalogo_elemento_IdArea] FOREIGN KEY ([IdArea]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE NO ACTION
);
GO


CREATE TABLE [HistorialPrecios] (
    [Id] uniqueidentifier NOT NULL,
    [IdPrecio] uniqueidentifier NOT NULL,
    [PrecioAnterior] decimal(18,2) NOT NULL,
    [PrecioNuevo] decimal(18,2) NOT NULL,
    [IdUsuario] uniqueidentifier NOT NULL,
    [RegistradoEn] datetimeoffset NOT NULL,
    [PrecioIdPrecio] uniqueidentifier NOT NULL,
    [UsuarioId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_HistorialPrecios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HistorialPrecios_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_HistorialPrecios_Precios_PrecioIdPrecio] FOREIGN KEY ([PrecioIdPrecio]) REFERENCES [Precios] ([IdPrecio]) ON DELETE CASCADE
);
GO


CREATE TABLE [Pedidos] (
    [IdPedido] uniqueidentifier NOT NULL,
    [IdCliente] uniqueidentifier NOT NULL,
    [IdUsuarioCaptura] uniqueidentifier NOT NULL,
    [IdDireccionEnvio] uniqueidentifier NULL,
    [IdPolitica] uniqueidentifier NULL,
    [ClFolio] nvarchar(max) NOT NULL,
    [ClEstatusPedido] nvarchar(max) NOT NULL,
    [ClMonedaIdCatalogoElemento] int NOT NULL,
    [MnSubtotal] decimal(18,2) NOT NULL,
    [MnDescuentoComercial] decimal(18,2) NOT NULL,
    [MnDescuentoAdmin] decimal(18,2) NOT NULL,
    [MnTotal] decimal(18,2) NOT NULL,
    [FePedido] datetimeoffset NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [ClienteIdCliente] uniqueidentifier NOT NULL,
    [UsuarioCapturaId] uniqueidentifier NOT NULL,
    [DireccionEnvioIdDireccion] uniqueidentifier NULL,
    [PoliticaIdPolitica] uniqueidentifier NULL,
    CONSTRAINT [PK_Pedidos] PRIMARY KEY ([IdPedido]),
    CONSTRAINT [FK_Pedidos_AspNetUsers_UsuarioCapturaId] FOREIGN KEY ([UsuarioCapturaId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Pedidos_Clientes_ClienteIdCliente] FOREIGN KEY ([ClienteIdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE CASCADE,
    CONSTRAINT [FK_Pedidos_DireccionesCliente_DireccionEnvioIdDireccion] FOREIGN KEY ([DireccionEnvioIdDireccion]) REFERENCES [DireccionesCliente] ([IdDireccion]),
    CONSTRAINT [FK_Pedidos_PoliticasPrecios_PoliticaIdPolitica] FOREIGN KEY ([PoliticaIdPolitica]) REFERENCES [PoliticasPrecios] ([IdPolitica]),
    CONSTRAINT [FK_Pedidos_c_catalogo_elemento_ClMonedaIdCatalogoElemento] FOREIGN KEY ([ClMonedaIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [HistorialPedidos] (
    [Id] uniqueidentifier NOT NULL,
    [IdPedido] uniqueidentifier NOT NULL,
    [EstatusAnteriorIdCatalogoElemento] int NULL,
    [EstatusNuevoIdCatalogoElemento] int NOT NULL,
    [IdUsuario] uniqueidentifier NOT NULL,
    [Notas] nvarchar(max) NULL,
    [RegistradoEn] datetimeoffset NOT NULL,
    [PedidoIdPedido] uniqueidentifier NOT NULL,
    [UsuarioId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_HistorialPedidos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HistorialPedidos_AspNetUsers_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_HistorialPedidos_Pedidos_PedidoIdPedido] FOREIGN KEY ([PedidoIdPedido]) REFERENCES [Pedidos] ([IdPedido]) ON DELETE CASCADE,
    CONSTRAINT [FK_HistorialPedidos_c_catalogo_elemento_EstatusAnteriorIdCatalogoElemento] FOREIGN KEY ([EstatusAnteriorIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]),
    CONSTRAINT [FK_HistorialPedidos_c_catalogo_elemento_EstatusNuevoIdCatalogoElemento] FOREIGN KEY ([EstatusNuevoIdCatalogoElemento]) REFERENCES [c_catalogo_elemento] ([IdCatalogoElemento]) ON DELETE CASCADE
);
GO


CREATE TABLE [LineasPedido] (
    [IdLineaPedido] uniqueidentifier NOT NULL,
    [IdPedido] uniqueidentifier NOT NULL,
    [IdSku] uniqueidentifier NOT NULL,
    [NoCantidad] int NOT NULL,
    [MnPrecioUnitario] decimal(18,2) NOT NULL,
    [MnDescuentoLinea] decimal(18,2) NOT NULL,
    [MnSubtotal] decimal(18,2) NOT NULL,
    [ClOperadorCrea] nvarchar(max) NOT NULL,
    [ClOperadorModifica] nvarchar(max) NULL,
    [NbArtefactoCrea] nvarchar(max) NOT NULL,
    [NbArtefactoModifica] nvarchar(max) NULL,
    [FeCreacion] datetimeoffset NOT NULL,
    [FeModificacion] datetimeoffset NULL,
    [PedidoIdPedido] uniqueidentifier NOT NULL,
    [SkuIdSku] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_LineasPedido] PRIMARY KEY ([IdLineaPedido]),
    CONSTRAINT [FK_LineasPedido_Pedidos_PedidoIdPedido] FOREIGN KEY ([PedidoIdPedido]) REFERENCES [Pedidos] ([IdPedido]) ON DELETE CASCADE,
    CONSTRAINT [FK_LineasPedido_Skus_SkuIdSku] FOREIGN KEY ([SkuIdSku]) REFERENCES [Skus] ([IdSku]) ON DELETE CASCADE
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdCatalogo', N'CatalogoPadreIdCatalogo', N'ClCatalogo', N'ClEstatusCatalogo', N'ClOperadorCrea', N'ClOperadorModifica', N'DsCatalogo', N'FeCreacion', N'FeModificacion', N'IdCatalogoPadre', N'NbArtefactoCrea', N'NbArtefactoModifica', N'NbCatalogo') AND [object_id] = OBJECT_ID(N'[c_catalogo]'))
    SET IDENTITY_INSERT [c_catalogo] ON;
INSERT INTO [c_catalogo] ([IdCatalogo], [CatalogoPadreIdCatalogo], [ClCatalogo], [ClEstatusCatalogo], [ClOperadorCrea], [ClOperadorModifica], [DsCatalogo], [FeCreacion], [FeModificacion], [IdCatalogoPadre], [NbArtefactoCrea], [NbArtefactoModifica], [NbCatalogo])
VALUES (4000, NULL, N'SERIES_PREFIJOS', N'ACTIVO', N'Admin', NULL, NULL, '2026-05-14T23:28:51.8355578+00:00', NULL, NULL, N'Seed', NULL, N'Prefijos de Produccion');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdCatalogo', N'CatalogoPadreIdCatalogo', N'ClCatalogo', N'ClEstatusCatalogo', N'ClOperadorCrea', N'ClOperadorModifica', N'DsCatalogo', N'FeCreacion', N'FeModificacion', N'IdCatalogoPadre', N'NbArtefactoCrea', N'NbArtefactoModifica', N'NbCatalogo') AND [object_id] = OBJECT_ID(N'[c_catalogo]'))
    SET IDENTITY_INSERT [c_catalogo] OFF;
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdCatalogoElemento', N'ClCatalogoElemento', N'ClEstatusCatalogoElemento', N'ClOperadorCrea', N'ClOperadorModifica', N'DsCatalogoElemento', N'ElementoPadreIdCatalogoElemento', N'FeCreacion', N'FeModificacion', N'IdCatalogo', N'IdCatalogoElementoPadre', N'NbArtefactoCrea', N'NbArtefactoModifica', N'NbCatalogoElemento') AND [object_id] = OBJECT_ID(N'[c_catalogo_elemento]'))
    SET IDENTITY_INSERT [c_catalogo_elemento] ON;
INSERT INTO [c_catalogo_elemento] ([IdCatalogoElemento], [ClCatalogoElemento], [ClEstatusCatalogoElemento], [ClOperadorCrea], [ClOperadorModifica], [DsCatalogoElemento], [ElementoPadreIdCatalogoElemento], [FeCreacion], [FeModificacion], [IdCatalogo], [IdCatalogoElementoPadre], [NbArtefactoCrea], [NbArtefactoModifica], [NbCatalogoElemento])
VALUES (4016, N'PP', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8361625+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Guante'),
(4017, N'FN', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8362736+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Fitness'),
(4018, N'IH', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8362738+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Mochila, Bolsas, Zapateras'),
(4019, N'IK', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8362740+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Cono'),
(4020, N'IY', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8362742+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Espinillera'),
(4021, N'KT', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8362743+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Media'),
(4022, N'TX', N'ACTIVO', N'Admin', NULL, NULL, NULL, '2026-05-14T23:28:51.8362744+00:00', NULL, 4000, NULL, N'Seed', NULL, N'Textil');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdCatalogoElemento', N'ClCatalogoElemento', N'ClEstatusCatalogoElemento', N'ClOperadorCrea', N'ClOperadorModifica', N'DsCatalogoElemento', N'ElementoPadreIdCatalogoElemento', N'FeCreacion', N'FeModificacion', N'IdCatalogo', N'IdCatalogoElementoPadre', N'NbArtefactoCrea', N'NbArtefactoModifica', N'NbCatalogoElemento') AND [object_id] = OBJECT_ID(N'[c_catalogo_elemento]'))
    SET IDENTITY_INSERT [c_catalogo_elemento] OFF;
GO


CREATE INDEX [IX_AsignacionesClienteEmpleado_ClienteIdCliente] ON [AsignacionesClienteEmpleado] ([ClienteIdCliente]);
GO


CREATE INDEX [IX_AsignacionesClienteEmpleado_EmpleadoIdEmpleado] ON [AsignacionesClienteEmpleado] ([EmpleadoIdEmpleado]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE INDEX [IX_AspNetUsers_ClienteIdCliente] ON [AspNetUsers] ([ClienteIdCliente]);
GO


CREATE INDEX [IX_AspNetUsers_EmpleadoIdEmpleado] ON [AspNetUsers] ([EmpleadoIdEmpleado]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_c_catalogo_CatalogoPadreIdCatalogo] ON [c_catalogo] ([CatalogoPadreIdCatalogo]);
GO


CREATE INDEX [IX_c_catalogo_elemento_ElementoPadreIdCatalogoElemento] ON [c_catalogo_elemento] ([ElementoPadreIdCatalogoElemento]);
GO


CREATE INDEX [IX_c_catalogo_elemento_IdCatalogo] ON [c_catalogo_elemento] ([IdCatalogo]);
GO


CREATE INDEX [IX_Clientes_ClMonedaIdCatalogoElemento] ON [Clientes] ([ClMonedaIdCatalogoElemento]);
GO


CREATE INDEX [IX_DireccionesCliente_ClienteIdCliente] ON [DireccionesCliente] ([ClienteIdCliente]);
GO


CREATE INDEX [IX_DireccionesCliente_EstadoCatalogoIdCatalogoElemento] ON [DireccionesCliente] ([EstadoCatalogoIdCatalogoElemento]);
GO


CREATE INDEX [IX_DireccionesCliente_PaisCatalogoIdCatalogoElemento] ON [DireccionesCliente] ([PaisCatalogoIdCatalogoElemento]);
GO


CREATE INDEX [IX_Empleados_IdArea] ON [Empleados] ([IdArea]);
GO


CREATE UNIQUE INDEX [IX_Empleados_IdUsuario] ON [Empleados] ([IdUsuario]) WHERE [IdUsuario] IS NOT NULL;
GO


CREATE INDEX [IX_Estilos_ProductoIdProducto] ON [Estilos] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_HistorialPedidos_EstatusAnteriorIdCatalogoElemento] ON [HistorialPedidos] ([EstatusAnteriorIdCatalogoElemento]);
GO


CREATE INDEX [IX_HistorialPedidos_EstatusNuevoIdCatalogoElemento] ON [HistorialPedidos] ([EstatusNuevoIdCatalogoElemento]);
GO


CREATE INDEX [IX_HistorialPedidos_PedidoIdPedido] ON [HistorialPedidos] ([PedidoIdPedido]);
GO


CREATE INDEX [IX_HistorialPedidos_UsuarioId] ON [HistorialPedidos] ([UsuarioId]);
GO


CREATE INDEX [IX_HistorialPrecios_PrecioIdPrecio] ON [HistorialPrecios] ([PrecioIdPrecio]);
GO


CREATE INDEX [IX_HistorialPrecios_UsuarioId] ON [HistorialPrecios] ([UsuarioId]);
GO


CREATE INDEX [IX_LineasPedido_PedidoIdPedido] ON [LineasPedido] ([PedidoIdPedido]);
GO


CREATE INDEX [IX_LineasPedido_SkuIdSku] ON [LineasPedido] ([SkuIdSku]);
GO


CREATE INDEX [IX_Pedidos_ClienteIdCliente] ON [Pedidos] ([ClienteIdCliente]);
GO


CREATE INDEX [IX_Pedidos_ClMonedaIdCatalogoElemento] ON [Pedidos] ([ClMonedaIdCatalogoElemento]);
GO


CREATE INDEX [IX_Pedidos_DireccionEnvioIdDireccion] ON [Pedidos] ([DireccionEnvioIdDireccion]);
GO


CREATE INDEX [IX_Pedidos_PoliticaIdPolitica] ON [Pedidos] ([PoliticaIdPolitica]);
GO


CREATE INDEX [IX_Pedidos_UsuarioCapturaId] ON [Pedidos] ([UsuarioCapturaId]);
GO


CREATE INDEX [IX_Precios_ClienteIdCliente] ON [Precios] ([ClienteIdCliente]);
GO


CREATE INDEX [IX_Precios_ClMonedaIdCatalogoElemento] ON [Precios] ([ClMonedaIdCatalogoElemento]);
GO


CREATE INDEX [IX_Precios_PoliticaIdPolitica] ON [Precios] ([PoliticaIdPolitica]);
GO


CREATE INDEX [IX_Precios_SkuIdSku] ON [Precios] ([SkuIdSku]);
GO


CREATE INDEX [IX_Productos_DivisionCatalogoIdCatalogoElemento] ON [Productos] ([DivisionCatalogoIdCatalogoElemento]);
GO


CREATE INDEX [IX_Productos_EstadoIdCatalogoElemento] ON [Productos] ([EstadoIdCatalogoElemento]);
GO


CREATE INDEX [IX_Productos_GamaCatalogoIdCatalogoElemento] ON [Productos] ([GamaCatalogoIdCatalogoElemento]);
GO


CREATE INDEX [IX_Productos_LineaColeccionCatalogoIdCatalogoElemento] ON [Productos] ([LineaColeccionCatalogoIdCatalogoElemento]);
GO


CREATE INDEX [IX_Productos_SerieCatalogoIdCatalogoElemento] ON [Productos] ([SerieCatalogoIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosAccesorio_ClTipoMedidaIdCatalogoElemento] ON [ProductosAccesorio] ([ClTipoMedidaIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosAccesorio_ProductoIdProducto] ON [ProductosAccesorio] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_ProductosCono_ProductoIdProducto] ON [ProductosCono] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_ProductosEspinillera_ClTipoMedidaIdCatalogoElemento] ON [ProductosEspinillera] ([ClTipoMedidaIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosEspinillera_ProductoIdProducto] ON [ProductosEspinillera] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_ProductosFitness_ClTipoMedidaIdCatalogoElemento] ON [ProductosFitness] ([ClTipoMedidaIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosFitness_ProductoIdProducto] ON [ProductosFitness] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_ProductosGuante_ClTipoMedidaIdCatalogoElemento] ON [ProductosGuante] ([ClTipoMedidaIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosGuante_ProductoIdProducto] ON [ProductosGuante] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_ProductosMochila_ProductoIdProducto] ON [ProductosMochila] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_ProductosTextil_ClGeneroIdCatalogoElemento] ON [ProductosTextil] ([ClGeneroIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosTextil_ClTipoMedidaIdCatalogoElemento] ON [ProductosTextil] ([ClTipoMedidaIdCatalogoElemento]);
GO


CREATE INDEX [IX_ProductosTextil_ProductoIdProducto] ON [ProductosTextil] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_Skus_TallaIdCatalogoElemento] ON [Skus] ([TallaIdCatalogoElemento]);
GO


CREATE INDEX [IX_Skus_VarianteIdVariante] ON [Skus] ([VarianteIdVariante]);
GO


CREATE INDEX [IX_Variantes_EstiloIdEstilo] ON [Variantes] ([EstiloIdEstilo]);
GO


CREATE INDEX [IX_Variantes_ProductoIdProducto] ON [Variantes] ([ProductoIdProducto]);
GO


CREATE INDEX [IX_Variantes_SegmentoIdCatalogoElemento] ON [Variantes] ([SegmentoIdCatalogoElemento]);
GO


CREATE INDEX [IX_VisibilidadesCatalogo_ClienteIdCliente] ON [VisibilidadesCatalogo] ([ClienteIdCliente]);
GO


CREATE INDEX [IX_VisibilidadesCatalogo_ProductoIdProducto] ON [VisibilidadesCatalogo] ([ProductoIdProducto]);
GO


ALTER TABLE [AsignacionesClienteEmpleado] ADD CONSTRAINT [FK_AsignacionesClienteEmpleado_Empleados_EmpleadoIdEmpleado] FOREIGN KEY ([EmpleadoIdEmpleado]) REFERENCES [Empleados] ([IdEmpleado]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUserClaims] ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUserLogins] ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
GO


ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Empleados_EmpleadoIdEmpleado] FOREIGN KEY ([EmpleadoIdEmpleado]) REFERENCES [Empleados] ([IdEmpleado]);
GO


