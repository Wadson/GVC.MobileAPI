using GVC.MobileAPI.Configuration;
using GVC.MobileAPI.Models;
using GVC.MobileAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace GVC.MobileAPI.Services;

public sealed class ImagemService : IImagemService
{
    private static readonly Dictionary<string, string> ContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif",
            [".bmp"] = "image/bmp"
        };

    private readonly ImageSettings _settings;
    private readonly ILogger<ImagemService> _logger;

    public ImagemService(
        IOptions<ImageSettings> options,
        ILogger<ImagemService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public ImagemProdutoResult LocalizarImagem(
        int produtoId,
        string? caminhoImagem)
    {
        var caminhoLocalizado = ResolverCaminho(caminhoImagem);

        if (caminhoLocalizado is not null)
        {
            return CriarResultado(
                caminhoLocalizado,
                produtoId,
                usaImagemPadrao: false);
        }

        var caminhoPadrao = ResolverImagemPadrao();

        if (caminhoPadrao is not null)
        {
            _logger.LogWarning(
                "Imagem do produto {ProdutoID} não encontrada. " +
                "A imagem padrão será utilizada. Caminho registrado: {CaminhoImagem}",
                produtoId,
                caminhoImagem);

            return CriarResultado(
                caminhoPadrao,
                produtoId,
                usaImagemPadrao: true);
        }

        _logger.LogWarning(
            "Imagem do produto {ProdutoID} não encontrada e não há imagem padrão disponível. " +
            "Caminho registrado: {CaminhoImagem}",
            produtoId,
            caminhoImagem);

        return new ImagemProdutoResult
        {
            Encontrada = false
        };
    }

    private string? ResolverCaminho(string? caminhoImagem)
    {
        if (string.IsNullOrWhiteSpace(caminhoImagem))
            return null;

        var caminhoLimpo = caminhoImagem.Trim()
            .Trim('"');

        string caminhoCompleto;

        if (Path.IsPathRooted(caminhoLimpo))
        {
            caminhoCompleto = Path.GetFullPath(caminhoLimpo);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_settings.ImageFolder))
                return null;

            caminhoCompleto = Path.GetFullPath(
                Path.Combine(
                    _settings.ImageFolder,
                    caminhoLimpo));
        }

        if (!EstaDentroDaPastaPermitida(caminhoCompleto))
        {
            _logger.LogWarning(
                "Caminho de imagem recusado por estar fora da pasta permitida: {Caminho}",
                caminhoCompleto);

            return null;
        }

        return File.Exists(caminhoCompleto)
            ? caminhoCompleto
            : null;
    }

    private string? ResolverImagemPadrao()
    {
        if (string.IsNullOrWhiteSpace(_settings.DefaultImage))
            return null;

        var caminhoPadrao = Path.IsPathRooted(_settings.DefaultImage)
            ? Path.GetFullPath(_settings.DefaultImage)
            : Path.GetFullPath(
                Path.Combine(
                    _settings.ImageFolder,
                    _settings.DefaultImage));

        if (!EstaDentroDaPastaPermitida(caminhoPadrao))
            return null;

        return File.Exists(caminhoPadrao)
            ? caminhoPadrao
            : null;
    }

    private bool EstaDentroDaPastaPermitida(string caminhoCompleto)
    {
        if (string.IsNullOrWhiteSpace(_settings.ImageFolder))
            return false;

        var pastaPermitida = Path.GetFullPath(
            _settings.ImageFolder);

        var separador = Path.DirectorySeparatorChar.ToString();

        if (!pastaPermitida.EndsWith(separador))
            pastaPermitida += separador;

        return caminhoCompleto.StartsWith(
            pastaPermitida,
            StringComparison.OrdinalIgnoreCase);
    }

    private static ImagemProdutoResult CriarResultado(
        string caminhoFisico,
        int produtoId,
        bool usaImagemPadrao)
    {
        var extensao = Path.GetExtension(caminhoFisico);

        if (string.IsNullOrWhiteSpace(extensao))
            extensao = ".jpg";

        var nomeArquivo = $"{produtoId}{extensao.ToLowerInvariant()}";

        var contentType = ContentTypes.TryGetValue(
            extensao,
            out var tipo)
            ? tipo
            : "application/octet-stream";

        return new ImagemProdutoResult
        {
            Encontrada = true,
            CaminhoFisico = caminhoFisico,
            NomeArquivo = nomeArquivo,
            CaminhoNoPacote = $"imagens/{nomeArquivo}",
            ContentType = contentType,
            UsaImagemPadrao = usaImagemPadrao
        };
    }
}