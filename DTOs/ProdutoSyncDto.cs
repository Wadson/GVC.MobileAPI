namespace GVC.MobileAPI.DTOs;

public sealed class ProdutoSyncDto
{
    public int ProdutoID { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public decimal? PrecoCompra { get; set; }

    public decimal PrecoCusto { get; set; }

    public decimal PrecoDeVenda { get; set; }

    public int Estoque { get; set; }

    public string? GtinEan { get; set; }

    public int? MarcaID { get; set; }

    public string? Marca { get; set; }

    public int EmpresaID { get; set; }

    /// <summary>
    /// Caminho relativo da imagem dentro do pacote de sincronização.
    /// Exemplo: imagens/125.jpg
    /// </summary>
    public string? ImagemLocal { get; set; }
}