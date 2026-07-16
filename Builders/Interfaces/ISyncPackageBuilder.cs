using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Models;

namespace GVC.MobileAPI.Builders.Interfaces;

public interface ISyncPackageBuilder
{
    Task<SyncPackageResult> CriarPacoteAsync(
     IReadOnlyList<EmpresaSyncDto> empresas,
     IReadOnlyList<ProdutoSyncDto> produtos,
     IReadOnlyList<ClienteSyncDto> clientes,
     IReadOnlyList<ContaReceberSyncDto> contasReceber,
     SyncManifestDto manifest,
     IReadOnlyList<SyncImageFile> imagens,
     CancellationToken cancellationToken = default);
}