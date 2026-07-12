using GVC.MobileAPI.Repositories.Interfaces;
using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/imagens")]
public sealed class ImagensController : ControllerBase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IImagemService _imagemService;

    public ImagensController(
        IProdutoRepository produtoRepository,
        IImagemService imagemService)
    {
        _produtoRepository = produtoRepository;
        _imagemService = imagemService;
    }

    [HttpGet("produto/{produtoId:int}")]
    public async Task<IActionResult> ObterImagemProduto(
        int produtoId,
        CancellationToken cancellationToken)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(
            produtoId,
            cancellationToken);

        if (produto is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Produto não encontrado."
            });
        }

        var imagem = _imagemService.LocalizarImagem(
            produto.ProdutoID,
            produto.Imagem);

        if (!imagem.Encontrada ||
            string.IsNullOrWhiteSpace(imagem.CaminhoFisico))
        {
            return NotFound(new
            {
                success = false,
                message = "Imagem do produto não encontrada."
            });
        }

        var stream = new FileStream(
            imagem.CaminhoFisico,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true);

        return File(
            stream,
            imagem.ContentType ?? "application/octet-stream",
            enableRangeProcessing: true);
    }
}