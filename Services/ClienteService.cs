using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;
using GVC.MobileAPI.Services.Interfaces;

namespace GVC.MobileAPI.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(
        IClienteRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ClienteSyncDto>> ObterTodosAsync(
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        return _repository.ObterTodosAsync(
            empresaId,
            cancellationToken);
    }

    public Task<ClienteSyncDto?> ObterPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        if (clienteId <= 0)
        {
            throw new ArgumentException(
                "O identificador do cliente deve ser maior que zero.",
                nameof(clienteId));
        }

        return _repository.ObterPorIdAsync(
            clienteId,
            cancellationToken);
    }

    public Task<IReadOnlyList<ClienteSyncDto>> PesquisarAsync(
        string termo,
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            throw new ArgumentException(
                "Informe um termo para pesquisar.",
                nameof(termo));
        }

        return _repository.PesquisarAsync(
            termo.Trim(),
            empresaId,
            cancellationToken);
    }
}