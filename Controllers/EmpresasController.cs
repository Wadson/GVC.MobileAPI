using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/empresas")]
public sealed class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _service;

    public EmpresasController(
        IEmpresaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var empresas =
            await _service.ObterTodasAsync(
                cancellationToken);

        return Ok(empresas);
    }

    [HttpGet("{empresaId:int}")]
    public async Task<IActionResult> GetById(
        int empresaId,
        CancellationToken cancellationToken)
    {
        var empresa =
            await _service.ObterPorIdAsync(
                empresaId,
                cancellationToken);

        if (empresa is null)
        {
            return NotFound();
        }

        return Ok(empresa);
    }
}