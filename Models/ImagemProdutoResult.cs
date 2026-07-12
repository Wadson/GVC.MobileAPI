namespace GVC.MobileAPI.Models;

public sealed class ImagemProdutoResult
{
    public bool Encontrada { get; init; }

    public string? CaminhoFisico { get; init; }

    public string? NomeArquivo { get; init; }

    public string? CaminhoNoPacote { get; init; }

    public string? ContentType { get; init; }

    public bool UsaImagemPadrao { get; init; }
}