namespace GVC.MobileAPI.Models;

public sealed class SyncPackageResult
{
    public string CaminhoArquivo { get; init; } = string.Empty;

    public string NomeArquivo { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/zip";

    public long TamanhoBytes { get; init; }

    public int QuantidadeProdutos { get; init; }

    public int QuantidadeImagens { get; init; }
}