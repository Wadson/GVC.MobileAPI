using Dapper;
using GVC.MobileAPI.Data;
using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Repositories.Interfaces;

namespace GVC.MobileAPI.Repositories;

public sealed class ProdutoRepository : IProdutoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProdutoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProdutoDto>> ObterTodosAsync(
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.ProdutoID,
                p.NomeProduto,
                p.Referencia,
                p.PrecoCompra,
                p.PrecoCusto,
                p.PrecoDeVenda,
                p.Estoque,
                p.GtinEan,
                p.Imagem,
                p.MarcaID,
                m.NomeMarca AS Marca,
                p.EmpresaID
            FROM dbo.Produtos AS p
            LEFT JOIN dbo.Marca AS m
                ON m.MarcaID = p.MarcaID
            WHERE
                (@EmpresaID IS NULL OR p.EmpresaID = @EmpresaID)
            ORDER BY
                p.NomeProduto;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                EmpresaID = empresaId
            },
            cancellationToken: cancellationToken);

        var produtos = await connection.QueryAsync<ProdutoDto>(command);

        return produtos.AsList();
    }

    public async Task<ProdutoDto?> ObterPorIdAsync(
        int produtoId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.ProdutoID,
                p.NomeProduto,
                p.Referencia,
                p.PrecoCompra,
                p.PrecoCusto,
                p.PrecoDeVenda,
                p.Estoque,
                p.GtinEan,
                p.Imagem,
                p.MarcaID,
                m.NomeMarca AS Marca,
                p.EmpresaID
            FROM dbo.Produtos AS p
            LEFT JOIN dbo.Marca AS m
                ON m.MarcaID = p.MarcaID
            WHERE
                p.ProdutoID = @ProdutoID;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                ProdutoID = produtoId
            },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<ProdutoDto>(command);
    }

    public async Task<IReadOnlyList<ProdutoDto>> PesquisarAsync(
        string termo,
        int? empresaId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                p.ProdutoID,
                p.NomeProduto,
                p.Referencia,
                p.PrecoCompra,
                p.PrecoCusto,
                p.PrecoDeVenda,
                p.Estoque,
                p.GtinEan,
                p.Imagem,
                p.MarcaID,
                m.NomeMarca AS Marca,
                p.EmpresaID
            FROM dbo.Produtos AS p
            LEFT JOIN dbo.Marca AS m
                ON m.MarcaID = p.MarcaID
            WHERE
                (@EmpresaID IS NULL OR p.EmpresaID = @EmpresaID)
                AND
                (
                    p.NomeProduto LIKE '%' + @Termo + '%'
                    OR p.Referencia LIKE '%' + @Termo + '%'
                    OR p.GtinEan LIKE '%' + @Termo + '%'
                    OR m.NomeMarca LIKE '%' + @Termo + '%'
                )
            ORDER BY
                p.NomeProduto;
            """;

        using var connection = _connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            commandText: sql,
            parameters: new
            {
                Termo = termo,
                EmpresaID = empresaId
            },
            cancellationToken: cancellationToken);

        var produtos = await connection.QueryAsync<ProdutoDto>(command);

        return produtos.AsList();
    }
}