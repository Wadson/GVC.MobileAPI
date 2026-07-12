namespace GVC.MobileAPI.Configuration;

public sealed class SyncSettings
{
    public const string SectionName = "SyncSettings";

    public int HorasRetencaoArquivos { get; set; } = 6;

    public int IntervaloLimpezaMinutos { get; set; } = 30;
}