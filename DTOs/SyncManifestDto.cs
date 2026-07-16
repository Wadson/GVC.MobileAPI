namespace GVC.MobileAPI.DTOs;

public sealed class SyncManifestDto
{
    public string Versao { get; set; } = string.Empty;

    public DateTime DataGeracaoUtc { get; set; }

    public int QuantidadeProdutos { get; set; }

    public int QuantidadeImagens { get; set; }

    public int QuantidadeImagensPadrao { get; set; }

    public int QuantidadeImagensAusentes { get; set; }

    public int? EmpresaID { get; set; }

    public string FormatoPacote { get; set; } = "GVC-SYNC-1.0";

    public int QuantidadeClientes { get; set; }

    public int QuantidadeContasReceber { get; set; }

    public string Escopo { get; set; } = "TodasEmpresas";

    public int QuantidadeEmpresas { get; set; }
    

}