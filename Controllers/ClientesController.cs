using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(
        IClienteService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos os clientes.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int? empresaId,
        CancellationToken cancellationToken)
    {
        var clientes =
            await _service.ObterTodosAsync(
                empresaId,
                cancellationToken);

        return Ok(clientes);
    }

    /// <summary>
    /// Obtém um cliente pelo código.
    /// </summary>
    [HttpGet("{clienteId:int}")]
    public async Task<IActionResult> GetById(
        int clienteId,
        CancellationToken cancellationToken)
    {
        var cliente =
            await _service.ObterPorIdAsync(
                clienteId,
                cancellationToken);

        if (cliente is null)
            return NotFound();

        return Ok(cliente);
    }

    /// <summary>
    /// Pesquisa clientes.
    /// </summary>
    [HttpGet("pesquisar")]
    public async Task<IActionResult> Pesquisar(
        [FromQuery] string termo,
        [FromQuery] int? empresaId,
        CancellationToken cancellationToken)
    {
        var clientes =
            await _service.PesquisarAsync(
                termo,
                empresaId,
                cancellationToken);

        return Ok(clientes);
    }
}