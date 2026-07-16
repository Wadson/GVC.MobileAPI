namespace GVC.MobileAPI.DTOs;

public sealed class EmpresaSyncDto
{
    public int EmpresaID { get; set; }

    public string RazaoSocial { get; set; } = string.Empty;

    public string? NomeFantasia { get; set; }

    public string CNPJ { get; set; } = string.Empty;

    public string? InscricaoEstadual { get; set; }

    public string? Telefone { get; set; }

    public string? Email { get; set; }

    public string? Site { get; set; }

    public byte[]? Logo { get; set; }

    public string NomeExibicao =>
        !string.IsNullOrWhiteSpace(NomeFantasia)
            ? NomeFantasia
            : RazaoSocial;
}