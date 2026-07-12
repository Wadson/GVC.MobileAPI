using Dapper;
using GVC.MobileAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace GVC.MobileAPI.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseController(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
        
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var resultado = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT 'Conexão com SQL Server realizada com sucesso!'");
            return Ok(resultado ?? "Sem resposta do banco.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao conectar: {ex.Message}");
        }
    }

}