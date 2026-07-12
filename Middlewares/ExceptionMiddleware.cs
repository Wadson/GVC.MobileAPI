using System.Net;
using GVC.MobileAPI.Responses;
using Microsoft.Data.SqlClient;

namespace GVC.MobileAPI.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "A requisição foi cancelada pelo cliente. TraceId: {TraceId}",
                context.TraceIdentifier);

            context.Response.StatusCode = 499;
        }
        catch (Exception exception)
        {
            await TratarExcecaoAsync(context, exception);
        }
    }

    private async Task TratarExcecaoAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException =>
                HttpStatusCode.BadRequest,

            UnauthorizedAccessException =>
                HttpStatusCode.Forbidden,

            FileNotFoundException =>
                HttpStatusCode.NotFound,

            DirectoryNotFoundException =>
                HttpStatusCode.NotFound,

            SqlException =>
                HttpStatusCode.ServiceUnavailable,

            TimeoutException =>
                HttpStatusCode.GatewayTimeout,

            _ =>
                HttpStatusCode.InternalServerError
        };

        _logger.LogError(
            exception,
            "Erro não tratado. StatusCode: {StatusCode}. " +
            "Método: {Metodo}. Caminho: {Caminho}. TraceId: {TraceId}",
            (int)statusCode,
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Não foi possível retornar uma resposta de erro porque " +
                "os cabeçalhos já foram enviados. TraceId: {TraceId}",
                context.TraceIdentifier);

            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var resposta = new ApiErrorResponse
        {
            Message = ObterMensagemPublica(statusCode),
            TraceId = context.TraceIdentifier,

            Errors = _environment.IsDevelopment()
                ? [exception.Message]
                : []
        };

        await context.Response.WriteAsJsonAsync(resposta);
    }

    private static string ObterMensagemPublica(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Os dados enviados são inválidos.",

            HttpStatusCode.Forbidden =>
                "Acesso não autorizado ao recurso solicitado.",

            HttpStatusCode.NotFound =>
                "O recurso solicitado não foi encontrado.",

            HttpStatusCode.ServiceUnavailable =>
                "O banco de dados está temporariamente indisponível.",

            HttpStatusCode.GatewayTimeout =>
                "A operação excedeu o tempo máximo permitido.",

            _ =>
                "Ocorreu um erro interno ao processar a solicitação."
        };
    }
}