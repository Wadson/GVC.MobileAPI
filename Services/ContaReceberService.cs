using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;
using GVC.MobileAPI.Services.Interfaces;

namespace GVC.MobileAPI.Services;

public sealed class ContaReceberService : IContaReceberService
{
    private readonly IContaReceberRepository _repository;

    public ContaReceberService(
        IContaReceberRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ContaReceberSyncDto>> ObterTodasAsync(
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        return _repository.ObterTodasAsync(
            empresaId,
            cancellationToken);
    }

    public Task<IReadOnlyList<ContaReceberSyncDto>>
        ObterPorClienteAsync(
            int clienteId,
            int? empresaId,
            CancellationToken cancellationToken = default)
    {
        if (clienteId <= 0)
        {
            throw new ArgumentException(
                "O identificador do cliente deve ser maior que zero.",
                nameof(clienteId));
        }

        return _repository.ObterPorClienteAsync(
            clienteId,
            empresaId,
            cancellationToken);
    }
}