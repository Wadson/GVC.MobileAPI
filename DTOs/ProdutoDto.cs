namespace GVC.MobileAPI.DTOs;

public sealed class ProdutoDto
{
    public int ProdutoID { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public decimal? PrecoCompra { get; set; }

    public decimal PrecoCusto { get; set; }

    public decimal PrecoDeVenda { get; set; }

    public int Estoque { get; set; }

    public string? GtinEan { get; set; }

    public string? Imagem { get; set; }

    public int? MarcaID { get; set; }

    public string? Marca { get; set; }

    public int EmpresaID { get; set; }
}