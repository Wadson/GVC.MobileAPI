using Dapper;
using GVC.MobileAPI.Data;
using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;

namespace GVC.MobileAPI.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClienteRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ClienteSyncDto>> ObterTodosAsync(
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                c.ClienteID,
                c.Nome,
                c.Cpf,
                c.Cnpj,
                c.Telefone,
                c.Email,
                c.TipoCliente,
                c.Status,
                c.LimiteCredito,
                c.DataUltimaCompra,
                c.EmpresaID
            FROM dbo.Clientes AS c
            WHERE
                (@EmpresaID IS NULL OR c.EmpresaID = @EmpresaID)
            ORDER BY
                c.Nome;
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

        var clientes =
            await connection.QueryAsync<ClienteSyncDto>(command);

        return clientes.AsList();
    }

    public async Task<ClienteSyncDto?> ObterPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                c.ClienteID,
                c.Nome,
                c.Cpf,
                c.Cnpj,
                c.Telefone,
                c.Email,
                c.TipoCliente,
                c.Status,
                c.LimiteCredito,
                c.DataUltimaCompra,
                c.EmpresaID
            FROM dbo.Clientes AS c
            WHERE
                c.ClienteID = @ClienteID;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                ClienteID = clienteId
            },
            cancellationToken: cancellationToken);

        return await connection
            .QuerySingleOrDefaultAsync<ClienteSyncDto>(command);
    }

    public async Task<IReadOnlyList<ClienteSyncDto>> PesquisarAsync(
        string termo,
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                c.ClienteID,
                c.Nome,
                c.Cpf,
                c.Cnpj,
                c.Telefone,
                c.Email,
                c.TipoCliente,
                c.Status,
                c.LimiteCredito,
                c.DataUltimaCompra,
                c.EmpresaID
            FROM dbo.Clientes AS c
            WHERE
                (@EmpresaID IS NULL OR c.EmpresaID = @EmpresaID)
                AND
                (
                    c.Nome LIKE '%' + @Termo + '%'
                    OR c.Cpf LIKE '%' + @Termo + '%'
                    OR c.Cnpj LIKE '%' + @Termo + '%'
                    OR c.Telefone LIKE '%' + @Termo + '%'
                )
            ORDER BY
                c.Nome;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                EmpresaID = empresaId,
                Termo = termo
            },
            cancellationToken: cancellationToken);

        var clientes =
            await connection.QueryAsync<ClienteSyncDto>(command);

        return clientes.AsList();
    }
}