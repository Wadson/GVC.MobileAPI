namespace GVC.MobileAPI.Configuration;

public sealed class ApiKeySettings
{
    public const string SectionName = "ApiKeySettings";

    public bool Enabled { get; set; } = true;

    public string HeaderName { get; set; } =
        "X-GVC-API-Key";

    public string Key { get; set; } = string.Empty;
}