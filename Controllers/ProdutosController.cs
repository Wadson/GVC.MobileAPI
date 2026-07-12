using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Responses;
using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/produtos")]
public sealed class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    /// <summary>
    /// Retorna todos os produtos disponíveis para sincronização.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<ProdutoDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProdutoDto>>>>
        ObterTodos(
            [FromQuery] int? empresaId,
            CancellationToken cancellationToken)
    {
        var produtos = await _produtoService.ObterTodosAsync(
            empresaId,
            cancellationToken);

        var resposta = new ApiResponse<IReadOnlyList<ProdutoDto>>
        {
            Success = true,
            Message = $"{produtos.Count} produto(s) encontrado(s).",
            Data = produtos
        };

        return Ok(resposta);
    }

    /// <summary>
    /// Retorna um produto pelo seu identificador.
    /// </summary>
    [HttpGet("{produtoId:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<ProdutoDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ProdutoDto>),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProdutoDto>>> ObterPorId(
        int produtoId,
        CancellationToken cancellationToken)
    {
        var produto = await _produtoService.ObterPorIdAsync(
            produtoId,
            cancellationToken);

        if (produto is null)
        {
            return NotFound(new ApiResponse<ProdutoDto>
            {
                Success = false,
                Message = "Produto não encontrado.",
                Data = null
            });
        }

        return Ok(new ApiResponse<ProdutoDto>
        {
            Success = true,
            Message = "Produto encontrado.",
            Data = produto
        });
    }

    /// <summary>
    /// Pesquisa produtos por nome, referência, GTIN/EAN ou marca.
    /// </summary>
    [HttpGet("pesquisar")]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<ProdutoDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<ProdutoDto>>),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProdutoDto>>>>
        Pesquisar(
            [FromQuery] string termo,
            [FromQuery] int? empresaId,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return BadRequest(
                new ApiResponse<IReadOnlyList<ProdutoDto>>
                {
                    Success = false,
                    Message = "Informe um termo para realizar a pesquisa.",
                    Data = []
                });
        }

        var produtos = await _produtoService.PesquisarAsync(
            termo,
            empresaId,
            cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ProdutoDto>>
        {
            Success = true,
            Message = $"{produtos.Count} produto(s) encontrado(s).",
            Data = produtos
        });
    }
}