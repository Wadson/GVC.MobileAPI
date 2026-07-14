using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Repositories.Interfaces;

public interface IContaReceberRepository
{
    Task<IReadOnlyList<ContaReceberSyncDto>> ObterTodasAsync(
        int? empresaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContaReceberSyncDto>> ObterPorClienteAsync(
        int clienteId,
        int? empresaId,
        CancellationToken cancellationToken = default);
}