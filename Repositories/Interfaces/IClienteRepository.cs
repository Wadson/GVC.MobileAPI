using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Repositories.Interfaces;

public interface IClienteRepository
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