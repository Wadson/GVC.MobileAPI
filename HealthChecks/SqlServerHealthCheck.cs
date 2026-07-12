using Dapper;
using GVC.MobileAPI.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GVC.MobileAPI.HealthChecks;

public sealed class SqlServerHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<SqlServerHealthCheck> _logger;

    public SqlServerHealthCheck(
        IDbConnectionFactory connectionFactory,
        ILogger<SqlServerHealthCheck> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                commandText: "SELECT 1;",
                cancellationToken: cancellationToken);

            var resultado =
                await connection.ExecuteScalarAsync<int>(command);

            if (resultado != 1)
            {
                return HealthCheckResult.Unhealthy(
                    "O SQL Server respondeu com um resultado inesperado.");
            }

            return HealthCheckResult.Healthy(
                "Conexão com o banco bdgvc realizada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha no Health Check do SQL Server.");

            return HealthCheckResult.Unhealthy(
                "Não foi possível acessar o banco bdgvc.",
                ex);
        }
    }
}