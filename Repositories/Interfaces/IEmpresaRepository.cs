using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Repositories.Interfaces;

public interface IEmpresaRepository
{
    Task<IReadOnlyList<EmpresaSyncDto>> ObterTodasAsync(
        CancellationToken cancellationToken = default);

    Task<EmpresaSyncDto?> ObterPorIdAsync(
        int empresaId,
        CancellationToken cancellationToken = default);
}