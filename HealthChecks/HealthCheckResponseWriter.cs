using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GVC.MobileAPI.HealthChecks;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static async Task WriteResponseAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType =
            "application/json; charset=utf-8";

        var resposta = new
        {
            status = report.Status.ToString(),
            duracaoTotalMs = report.TotalDuration.TotalMilliseconds,

            verificacoes = report.Entries.Select(item => new
            {
                nome = item.Key,
                status = item.Value.Status.ToString(),
                descricao = item.Value.Description,
                duracaoMs = item.Value.Duration.TotalMilliseconds
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(
                resposta,
                JsonOptions));
    }
}