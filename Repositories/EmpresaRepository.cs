using Dapper;
using GVC.MobileAPI.Data;
using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;

namespace GVC.MobileAPI.Repositories;

public sealed class EmpresaRepository : IEmpresaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EmpresaRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<EmpresaSyncDto>> ObterTodasAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                e.EmpresaID,
                e.RazaoSocial,
                e.NomeFantasia,
                e.CNPJ,
                e.InscricaoEstadual,
                e.Telefone,
                e.Email,
                e.Site,
                e.Logo
            FROM dbo.Empresa AS e
            ORDER BY
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(e.NomeFantasia)), '') IS NOT NULL
                        THEN e.NomeFantasia
                    ELSE e.RazaoSocial
                END;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            cancellationToken: cancellationToken);

        var empresas =
            await connection.QueryAsync<EmpresaSyncDto>(
                command);

        return empresas.AsList();
    }

    public async Task<EmpresaSyncDto?> ObterPorIdAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                e.EmpresaID,
                e.RazaoSocial,
                e.NomeFantasia,
                e.CNPJ,
                e.InscricaoEstadual,
                e.Telefone,
                e.Email,
                e.Site,
                e.Logo
            FROM dbo.Empresa AS e
            WHERE e.EmpresaID = @EmpresaID;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                EmpresaID = empresaId
            },
            cancellationToken: cancellationToken);

        return await connection
            .QuerySingleOrDefaultAsync<EmpresaSyncDto>(
                command);
    }
}