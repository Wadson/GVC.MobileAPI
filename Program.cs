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
                        PermitLimit = 3,

                        Window =
                            TimeSpan.FromMinutes(10),

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

// ---------------------------------------------------------
// PIPELINE HTTP
// ---------------------------------------------------------

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
    () => Results.Ok(new
    {
        api = "GVC Mobile API",
        status = "Online",
        versao = "1.0.0"
    }));

app.Run();