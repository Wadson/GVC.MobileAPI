using Dapper;
using GVC.MobileAPI.Data;
using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;

namespace GVC.MobileAPI.Repositories;

public sealed class ContaReceberRepository : IContaReceberRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ContaReceberRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ContaReceberSyncDto>> ObterTodasAsync(
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.ParcelaID,
                p.VendaID,
                v.ClienteID,
                c.Nome AS NomeCliente,
                p.NumeroParcela,
                v.DataVenda,
                p.DataVencimento,

                ISNULL(p.ValorParcela, 0) AS ValorParcela,
                ISNULL(p.ValorRecebido, 0) AS ValorRecebido,
                ISNULL(p.Juros, 0) AS Juros,
                ISNULL(p.Multa, 0) AS Multa,

                (
                    ISNULL(p.ValorParcela, 0)
                    + ISNULL(p.Juros, 0)
                    + ISNULL(p.Multa, 0)
                    - ISNULL(p.ValorRecebido, 0)
                ) AS Saldo,

                CASE
                    WHEN p.Status NOT IN ('Pago', 'Cancelada')
                         AND p.DataVencimento < CAST(GETDATE() AS DATE)
                         AND (
                             ISNULL(p.ValorParcela, 0)
                             + ISNULL(p.Juros, 0)
                             + ISNULL(p.Multa, 0)
                             - ISNULL(p.ValorRecebido, 0)
                         ) > 0
                    THEN 'Atrasada'
                    ELSE p.Status
                END AS StatusParcela,

                v.FormaPgtoID,
                fp.NomeFormaPagamento,
                p.DataPagamento,
                p.Observacao AS ObservacaoParcela,
                v.Observacoes AS ObservacaoVenda,

                ISNULL(v.TotalBruto, 0) AS TotalBrutoVenda,
                ISNULL(v.TotalDesconto, 0) AS TotalDescontoVenda,
                ISNULL(v.TotalLiquido, 0) AS TotalLiquidoVenda,

                v.StatusVenda,
                p.EmpresaID
            FROM dbo.Parcela AS p
            INNER JOIN dbo.Venda AS v
                ON v.VendaID = p.VendaID
                AND v.EmpresaID = p.EmpresaID
            INNER JOIN dbo.Clientes AS c
                ON c.ClienteID = v.ClienteID
                AND c.EmpresaID = v.EmpresaID
            LEFT JOIN dbo.FormaPagamento AS fp
                ON fp.FormaPgtoID = v.FormaPgtoID
            WHERE
                (@EmpresaID IS NULL OR p.EmpresaID = @EmpresaID)
            ORDER BY
                c.Nome,
                p.DataVencimento,
                p.NumeroParcela;
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

        var contas =
            await connection.QueryAsync<ContaReceberSyncDto>(command);

        return contas.AsList();
    }

    public async Task<IReadOnlyList<ContaReceberSyncDto>>
        ObterPorClienteAsync(
            int clienteId,
            int? empresaId,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.ParcelaID,
                p.VendaID,
                v.ClienteID,
                c.Nome AS NomeCliente,
                p.NumeroParcela,
                v.DataVenda,
                p.DataVencimento,

                ISNULL(p.ValorParcela, 0) AS ValorParcela,
                ISNULL(p.ValorRecebido, 0) AS ValorRecebido,
                ISNULL(p.Juros, 0) AS Juros,
                ISNULL(p.Multa, 0) AS Multa,

                (
                    ISNULL(p.ValorParcela, 0)
                    + ISNULL(p.Juros, 0)
                    + ISNULL(p.Multa, 0)
                    - ISNULL(p.ValorRecebido, 0)
                ) AS Saldo,

                CASE
                    WHEN p.Status NOT IN ('Pago', 'Cancelada')
                         AND p.DataVencimento < CAST(GETDATE() AS DATE)
                         AND (
                             ISNULL(p.ValorParcela, 0)
                             + ISNULL(p.Juros, 0)
                             + ISNULL(p.Multa, 0)
                             - ISNULL(p.ValorRecebido, 0)
                         ) > 0
                    THEN 'Atrasada'
                    ELSE p.Status
                END AS StatusParcela,

                v.FormaPgtoID,
                fp.NomeFormaPagamento,
                p.DataPagamento,
                p.Observacao AS ObservacaoParcela,
                v.Observacoes AS ObservacaoVenda,

                ISNULL(v.TotalBruto, 0) AS TotalBrutoVenda,
                ISNULL(v.TotalDesconto, 0) AS TotalDescontoVenda,
                ISNULL(v.TotalLiquido, 0) AS TotalLiquidoVenda,

                v.StatusVenda,
                p.EmpresaID
            FROM dbo.Parcela AS p
            INNER JOIN dbo.Venda AS v
                ON v.VendaID = p.VendaID
                AND v.EmpresaID = p.EmpresaID
            INNER JOIN dbo.Clientes AS c
                ON c.ClienteID = v.ClienteID
                AND c.EmpresaID = v.EmpresaID
            LEFT JOIN dbo.FormaPagamento AS fp
                ON fp.FormaPgtoID = v.FormaPgtoID
            WHERE
                v.ClienteID = @ClienteID
                AND (@EmpresaID IS NULL OR p.EmpresaID = @EmpresaID)
            ORDER BY
                p.DataVencimento,
                p.NumeroParcela;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                ClienteID = clienteId,
                EmpresaID = empresaId
            },
            cancellationToken: cancellationToken);

        var contas =
            await connection.QueryAsync<ContaReceberSyncDto>(command);

        return contas.AsList();
    }
}