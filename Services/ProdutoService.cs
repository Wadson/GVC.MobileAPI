using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;
using GVC.MobileAPI.Services.Interfaces;

namespace GVC.MobileAPI.Services;

public sealed class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public Task<IReadOnlyList<ProdutoDto>> ObterTodosAsync(
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        return _produtoRepository.ObterTodosAsync(
            empresaId,
            cancellationToken);
    }

    public Task<ProdutoDto?> ObterPorIdAsync(
        int produtoId,
        CancellationToken cancellationToken = default)
    {
        if (produtoId <= 0)
        {
            throw new ArgumentException(
                "O identificador do produto deve ser maior que zero.",
                nameof(produtoId));
        }

        return _produtoRepository.ObterPorIdAsync(
            produtoId,
            cancellationToken);
    }

    public Task<IReadOnlyList<ProdutoDto>> PesquisarAsync(
        string termo,
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            throw new ArgumentException(
                "Informe um termo para pesquisar o produto.",
                nameof(termo));
        }

        termo = termo.Trim();

        return _produtoRepository.PesquisarAsync(
            termo,
            empresaId,
            cancellationToken);
    }
}