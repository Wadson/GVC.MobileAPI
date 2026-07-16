using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;
using GVC.MobileAPI.Services.Interfaces;

namespace GVC.MobileAPI.Services;

public sealed class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _repository;

    public EmpresaService(
        IEmpresaRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<EmpresaSyncDto>> ObterTodasAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.ObterTodasAsync(
            cancellationToken);
    }

    public Task<EmpresaSyncDto?> ObterPorIdAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        if (empresaId <= 0)
        {
            throw new ArgumentException(
                "O código da empresa deve ser maior que zero.",
                nameof(empresaId));
        }

        return _repository.ObterPorIdAsync(
            empresaId,
            cancellationToken);
    }
}