namespace GVC.MobileAPI.Configuration;

public sealed class ImageSettings
{
    public const string SectionName = "ImageSettings";

    public string ImageFolder { get; set; } = string.Empty;
    public string DefaultImage { get; set; } = string.Empty;
}