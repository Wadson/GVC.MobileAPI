using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Services.Interfaces;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteSyncDto>> ObterTodosAsync(
        int? empresaId,
        CancellationToken cancellationToken = default);

    Task<ClienteSyncDto?> ObterPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClienteSyncDto>> PesquisarAsync(
        string termo,
        int? empresaId,
        CancellationToken cancellationToken = default);
}