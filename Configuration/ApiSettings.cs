namespace GVC.MobileAPI.Configuration;

public sealed class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string Nome { get; set; } = "GVC Mobile API";

    public string Versao { get; set; } = "1.0.0";
}