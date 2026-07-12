using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Models;

namespace GVC.MobileAPI.Builders.Interfaces;

public interface ISyncPackageBuilder
{
    Task<SyncPackageResult> CriarPacoteAsync(
        IReadOnlyList<ProdutoSyncDto> produtos,
        SyncManifestDto manifest,
        IReadOnlyList<SyncImageFile> imagens,
        CancellationToken cancellationToken = default);
}