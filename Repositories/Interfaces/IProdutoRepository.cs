using GVC.MobileAPI.DTOs;

namespace GVC.MobileAPI.Repositories.Interfaces;

public interface IProdutoRepository
{
    Task<IReadOnlyList<ProdutoDto>> ObterTodosAsync(
        int? empresaId,
        CancellationToken cancellationToken = default);

    Task<ProdutoDto?> ObterPorIdAsync(
        int produtoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoDto>> PesquisarAsync(
        string termo,
        int? empresaId,
        CancellationToken cancellationToken = default);
}