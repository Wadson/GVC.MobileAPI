using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Services.Interfaces;

public interface IContaReceberService
{
    Task<IReadOnlyList<ContaReceberSyncDto>> ObterTodasAsync(
        int? empresaId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContaReceberSyncDto>> ObterPorClienteAsync(
        int clienteId,
        int? empresaId,
        CancellationToken cancellationToken = default);
}