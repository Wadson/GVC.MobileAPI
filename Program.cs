using GVC.MobileAPI.Builders;
using GVC.MobileAPI.Builders.Interfaces;
using GVC.MobileAPI.Configuration;
using GVC.MobileAPI.Data;
using GVC.MobileAPI.HealthChecks;
using GVC.MobileAPI.Middlewares;
using GVC.MobileAPI.Repositories;
using GVC.MobileAPI.Repositories.Interfaces;
using GVC.MobileAPI.Services;
using GVC.MobileAPI.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// SERILOG
// ---------------------------------------------------------

builder.Host.UseSerilog((context, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration);
});

// ---------------------------------------------------------
// CONTROLLERS E SWAGGER
// ---------------------------------------------------------

builder.Services.AddControllers();



builder.Services.AddEndpointsApiExplorer();

var apiKeyHeaderName =
    builder.Configuration[
        $"{ApiKeySettings.SectionName}:HeaderName"]
    ?? "X-GVC-API-Key";

builder.Services.AddSwaggerGen(options =>
{
    const string securitySchemeId = "ApiKey";

    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "GVC Mobile API",
            Version = "v1",
            Description = "API oficial do Sistema GVC"
        });

    options.AddSecurityDefinition(
        securitySchemeId,
        new OpenApiSecurityScheme
        {
            Name = apiKeyHeaderName,
            Description =
                $"Informe a chave da API. Exemplo para testes: 123456",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    securitySchemeId,
                    document)
            ] = []
        });
});

// ---------------------------------------------------------
// CONFIGURAÇÕES TIPADAS
// ---------------------------------------------------------

builder.Services
    .AddOptions<ApiSettings>()
    .Bind(builder.Configuration.GetSection(
        ApiSettings.SectionName))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.Nome) &&
            !string.IsNullOrWhiteSpace(settings.Versao),
        "ApiSettings:Nome e ApiSettings:Versao são obrigatórios.")
    .ValidateOnStart();



builder.Services
    .AddOptions<ImageSettings>()
    .Bind(builder.Configuration.GetSection(
        ImageSettings.SectionName))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.ImageFolder),
        "ImageSettings:ImageFolder é obrigatório.")
    .ValidateOnStart();

builder.Services
    .AddOptions<SyncSettings>()
    .Bind(builder.Configuration.GetSection(
        SyncSettings.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<ApiKeySettings>()
    .Bind(builder.Configuration.GetSection(
        ApiKeySettings.SectionName))
    .Validate(
        settings =>
            !settings.Enabled ||
            !string.IsNullOrWhiteSpace(settings.Key),
        "ApiKeySettings:Key é obrigatório quando a proteção está habilitada.")
    .ValidateOnStart();

// ---------------------------------------------------------
// INJEÇÃO DE DEPENDÊNCIA
// ---------------------------------------------------------

builder.Services.AddScoped<
    IDbConnectionFactory,
    SqlConnectionFactory>();

builder.Services.AddScoped<
    IProdutoRepository,
    ProdutoRepository>();

builder.Services.AddScoped<
    IProdutoService,
    ProdutoService>();

builder.Services.AddScoped<
    IImagemService,
    ImagemService>();

builder.Services.AddScoped<
    ISyncPackageBuilder,
    SyncPackageBuilder>();

builder.Services.AddScoped<
    ISincronizacaoService,
    SincronizacaoService>();

builder.Services.AddHostedService<
    SyncTempCleanupService>();

//Contas a Receber

builder.Services.AddScoped<
    IClienteRepository,
    ClienteRepository>();

builder.Services.AddScoped<
    IClienteService,
    ClienteService>();

builder.Services.AddScoped<
    IContaReceberRepository,
    ContaReceberRepository>();

builder.Services.AddScoped<
    IContaReceberService,
    ContaReceberService>();






// ---------------------------------------------------------
// HEALTH CHECKS
// ---------------------------------------------------------

builder.Services
    .AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>(
        "sql-server-bdgvc",
        tags: ["ready"]);

// ---------------------------------------------------------
// RATE LIMITING
// ---------------------------------------------------------

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (
    contexto,
    cancellationToken) =>
    {
        contexto.HttpContext.Response.ContentType =
            "application/json; charset=utf-8";

        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                success = false,
                message =
                    "Limite de solicitações atingido. Aguarde antes de tentar novamente.",
                traceId =
                    contexto.HttpContext.TraceIdentifier
            },
            cancellationToken);
    };


    options.AddPolicy(
        "SyncDownload",
        httpContext =>
        {
            var chaveParticao =
                httpContext.Connection.RemoteIpAddress?
                    .ToString()
                ?? "desconhecido";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: chaveParticao,
                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,

                        Window =
                            TimeSpan.FromMinutes(1),

                        QueueLimit = 0,

                        AutoReplenishment = true
                    });
        });
});

// ---------------------------------------------------------
// CORS
// ---------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DefaultPolicy",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

     //app.UseHttpsRedirection();



app.UseRateLimiter();

app.UseCors("DefaultPolicy");

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            HealthCheckResponseWriter.WriteResponseAsync
    });

app.MapGet(
    "/",
    (Microsoft.Extensions.Options.IOptions<ApiSettings> options) =>
    {
        var settings = options.Value;

        return Results.Ok(new
        {
            api = settings.Nome,
            status = "Online",
            versao = settings.Versao
        });
    });

app.Run();