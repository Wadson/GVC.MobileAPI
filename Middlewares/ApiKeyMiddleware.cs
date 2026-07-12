using System.Security.Cryptography;
using System.Text;
using GVC.MobileAPI.Configuration;
using GVC.MobileAPI.Responses;
using Microsoft.Extensions.Options;

namespace GVC.MobileAPI.Middlewares;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeySettings _settings;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IOptions<ApiKeySettings> options,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.Enabled ||
            RotaLiberada(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(
                _settings.HeaderName,
                out var chaveRecebida))
        {
            await RetornarNaoAutorizadoAsync(
                context,
                "Chave de acesso não informada.");

            return;
        }

        if (!ChavesIguais(
                chaveRecebida.ToString(),
                _settings.Key))
        {
            _logger.LogWarning(
                "Tentativa de acesso com chave inválida. " +
                "Caminho: {Caminho}. IP: {IP}",
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            await RetornarNaoAutorizadoAsync(
                context,
                "Chave de acesso inválida.");

            return;
        }

        await _next(context);
    }

    private static bool RotaLiberada(PathString caminho)
    {
        return caminho.StartsWithSegments("/health") ||
               caminho.StartsWithSegments("/swagger") ||
               caminho.StartsWithSegments("/favicon.ico") ||
               caminho == "/";
    }

    private static bool ChavesIguais(
        string chaveRecebida,
        string chaveConfigurada)
    {
        if (string.IsNullOrWhiteSpace(chaveRecebida) ||
            string.IsNullOrWhiteSpace(chaveConfigurada))
        {
            return false;
        }

        var bytesRecebidos =
            Encoding.UTF8.GetBytes(chaveRecebida);

        var bytesConfigurados =
            Encoding.UTF8.GetBytes(chaveConfigurada);

        return bytesRecebidos.Length ==
               bytesConfigurados.Length &&
               CryptographicOperations.FixedTimeEquals(
                   bytesRecebidos,
                   bytesConfigurados);
    }

    private static async Task RetornarNaoAutorizadoAsync(
        HttpContext context,
        string mensagem)
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.ContentType =
            "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(
            new ApiErrorResponse
            {
                Message = mensagem,
                TraceId = context.TraceIdentifier
            });
    }
}