using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Services.Interfaces;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaSyncDto>> ObterTodasAsync(
        CancellationToken cancellationToken = default);

    Task<EmpresaSyncDto?> ObterPorIdAsync(
        int empresaId,
        CancellationToken cancellationToken = default);
}