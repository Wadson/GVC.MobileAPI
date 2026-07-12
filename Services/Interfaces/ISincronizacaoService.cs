using GVC.MobileAPI.Models;

namespace GVC.MobileAPI.Services.Interfaces;

public interface ISincronizacaoService
{
    Task<SyncPackageResult> GerarPacoteCompletoAsync(
        int? empresaId,
        CancellationToken cancellationToken = default);
}