using GVC.MobileAPI.Configuration;
using Microsoft.Extensions.Options;

namespace GVC.MobileAPI.Services;

public sealed class SyncTempCleanupService : BackgroundService
{
    private readonly SyncSettings _settings;
    private readonly ILogger<SyncTempCleanupService> _logger;

    public SyncTempCleanupService(
        IOptions<SyncSettings> options,
        ILogger<SyncTempCleanupService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromMinutes(
            Math.Max(
                _settings.IntervaloLimpezaMinutos,
                5));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                LimparArquivosAntigos();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao limpar arquivos temporários de sincronização.");
            }

            await Task.Delay(
                intervalo,
                stoppingToken);
        }
    }

    private void LimparArquivosAntigos()
    {
        var pasta = Path.Combine(
            Path.GetTempPath(),
            "GVC.MobileAPI",
            "Sync");

        if (!Directory.Exists(pasta))
            return;

        var limite = DateTime.UtcNow.AddHours(
            -Math.Max(
                _settings.HorasRetencaoArquivos,
                1));

        var arquivos = Directory.EnumerateFiles(
            pasta,
            "*.zip",
            SearchOption.TopDirectoryOnly);

        var removidos = 0;

        foreach (var arquivo in arquivos)
        {
            try
            {
                var ultimaAlteracao =
                    File.GetLastWriteTimeUtc(arquivo);

                if (ultimaAlteracao >= limite)
                    continue;

                File.Delete(arquivo);
                removidos++;
            }
            catch (IOException)
            {
                // O arquivo pode estar sendo enviado neste momento.
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Sem permissão para remover o arquivo {Arquivo}.",
                    arquivo);
            }
        }

        if (removidos > 0)
        {
            _logger.LogInformation(
                "{Quantidade} arquivo(s) temporário(s) antigo(s) removido(s).",
                removidos);
        }
    }
}