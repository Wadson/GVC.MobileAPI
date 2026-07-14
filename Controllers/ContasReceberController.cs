using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/contasreceber")]
public sealed class ContasReceberController : ControllerBase
{
    private readonly IContaReceberService _service;

    public ContasReceberController(
        IContaReceberService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todas as contas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? empresaId,
        CancellationToken cancellationToken)
    {
        var contas =
            await _service.ObterTodasAsync(
                empresaId,
                cancellationToken);

        return Ok(contas);
    }

    /// <summary>
    /// Lista contas de um cliente.
    /// </summary>
    [HttpGet("cliente/{clienteId:int}")]
    public async Task<IActionResult> PorCliente(
        int clienteId,
        [FromQuery] int? empresaId,
        CancellationToken cancellationToken)
    {
        var contas =
            await _service.ObterPorClienteAsync(
                clienteId,
                empresaId,
                cancellationToken);

        return Ok(contas);
    }
}