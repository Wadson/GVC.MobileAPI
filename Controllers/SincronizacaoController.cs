using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/sincronizacao")]
public sealed class SincronizacaoController : ControllerBase
{
    private readonly ISincronizacaoService _sincronizacaoService;
    private readonly ILogger<SincronizacaoController> _logger;

    public SincronizacaoController(
        ISincronizacaoService sincronizacaoService,
        ILogger<SincronizacaoController> logger)
    {
        _sincronizacaoService = sincronizacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Gera e baixa o pacote completo contendo produtos e imagens.
    /// </summary>
    [HttpGet("completa")]
    [Produces("application/zip")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [EnableRateLimiting("SyncDownload")]
    public async Task<IActionResult> BaixarCompleta(
    CancellationToken cancellationToken)
    {
        var resultado =
            await _sincronizacaoService.GerarPacoteCompletoAsync(
                empresaId: null,
                cancellationToken);

        return PhysicalFile(
            resultado.CaminhoArquivo,
            resultado.ContentType,
            resultado.NomeArquivo,
            enableRangeProcessing: true);
    }

    private void ExcluirArquivoTemporario(
        string caminhoArquivo)
    {
        try
        {
            if (System.IO.File.Exists(caminhoArquivo))
            {
                System.IO.File.Delete(caminhoArquivo);

                _logger.LogInformation(
                    "Arquivo temporário removido: {CaminhoArquivo}.",
                    caminhoArquivo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Não foi possível remover o arquivo temporário: {CaminhoArquivo}.",
                caminhoArquivo);
        }
    }
}