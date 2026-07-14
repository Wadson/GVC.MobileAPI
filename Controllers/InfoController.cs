using GVC.MobileAPI.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/info")]
public sealed class InfoController : ControllerBase
{
    private readonly ApiSettings _apiSettings;
    private readonly IWebHostEnvironment _environment;

    public InfoController(
        IOptions<ApiSettings> apiOptions,
        IWebHostEnvironment environment)
    {
        _apiSettings = apiOptions.Value;
        _environment = environment;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ObterInformacoes()
    {
        return Ok(new
        {
            nome = _apiSettings.Nome,
            versao = _apiSettings.Versao,
            ambiente = _environment.EnvironmentName,
            status = "Online",
            dataHoraUtc = DateTime.UtcNow
        });
    }
}