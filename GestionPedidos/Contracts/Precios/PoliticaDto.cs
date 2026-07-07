namespace GestionPedidos.Contracts.Precios;

public record PoliticaDto(
    Guid IdPolitica,
    string NbPolitica,
    string ClTipoPolitica,
    int NoPrioridad,
    decimal MnFactorDescuento,
    DateTimeOffset FeVigenteDesde,
    DateTimeOffset? FeVigenteHasta,
    string ClEstatusPolitica,
    int ConteoClientes
);

public record PoliticaUpsertDto(
    string NbPolitica,
    string ClTipoPolitica,
    int NoPrioridad,
    decimal MnFactorDescuento,
    DateTimeOffset FeVigenteDesde,
    DateTimeOffset? FeVigenteHasta
);

public record ClientePoliticaDto(
    Guid IdCliente,
    string NbComercial,
    Guid IdPolitica,
    bool EsPrincipal
);

public record ClientePoliticaUpsertDto(
    Guid IdCliente,
    Guid IdPolitica,
    bool EsPrincipal
);
