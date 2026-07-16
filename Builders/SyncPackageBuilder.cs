using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using GVC.MobileAPI.Builders.Interfaces;
using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Models;

namespace GVC.MobileAPI.Builders;

public sealed class SyncPackageBuilder : ISyncPackageBuilder
{
    private readonly ILogger<SyncPackageBuilder> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SyncPackageBuilder(
        ILogger<SyncPackageBuilder> logger)
    {
        _logger = logger;
    }

    public async Task<SyncPackageResult> CriarPacoteAsync(
    IReadOnlyList<EmpresaSyncDto> empresas,
    IReadOnlyList<ProdutoSyncDto> produtos,
    IReadOnlyList<ClienteSyncDto> clientes,
    IReadOnlyList<ContaReceberSyncDto> contasReceber,
    SyncManifestDto manifest,
    IReadOnlyList<SyncImageFile> imagens,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(produtos);
        ArgumentNullException.ThrowIfNull(clientes);
        ArgumentNullException.ThrowIfNull(contasReceber);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(imagens);

        var pastaTemporaria = Path.Combine(
            Path.GetTempPath(),
            "GVC.MobileAPI",
            "Sync");

        Directory.CreateDirectory(pastaTemporaria);

        var identificador =
            Guid.NewGuid().ToString("N");

       
        var nomeArquivo =
    $"GVC_SYNC_TODAS_EMPRESAS_" +  $"{DateTime.Now:yyyyMMdd_HHmmss}_" +  $"{identificador[..8]}.zip";
        var caminhoArquivo = Path.Combine(
            pastaTemporaria,
            nomeArquivo);

        try
        {
            await using var fileStream = new FileStream(
                caminhoArquivo,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 128 * 1024,
                useAsync: true);

            using (var zipArchive = new ZipArchive(
                fileStream,
                ZipArchiveMode.Create,
                leaveOpen: true))
            {
                await AdicionarJsonAsync(
                    zipArchive,
                    "empresas.json",
                    empresas,
                    cancellationToken);

                await AdicionarJsonAsync(
                    zipArchive,
                    "produtos.json",
                    produtos,
                    cancellationToken);

                await AdicionarJsonAsync(
                    zipArchive,
                    "clientes.json",
                    clientes,
                    cancellationToken);

                await AdicionarJsonAsync(
                    zipArchive,
                    "contas-receber.json",
                    contasReceber,
                    cancellationToken);

                await AdicionarJsonAsync(
                    zipArchive,
                    "manifest.json",
                    manifest,
                    cancellationToken);

                foreach (var imagem in imagens)
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    await AdicionarImagemAsync(
                        zipArchive,
                        imagem,
                        cancellationToken);
                }
            }

            await fileStream.FlushAsync(
                cancellationToken);

            var tamanhoBytes =
                fileStream.Length;

            _logger.LogInformation(
                "Pacote de sincronização criado. " +
                "Arquivo: {NomeArquivo}. " +
                "Produtos: {QuantidadeProdutos}. " +
                "Clientes: {QuantidadeClientes}. " +
                "Contas a receber: {QuantidadeContas}. " +
                "Imagens: {QuantidadeImagens}. " +
                "Tamanho: {TamanhoBytes} bytes.",
                nomeArquivo,
                produtos.Count,
                clientes.Count,
                contasReceber.Count,
                imagens.Count,
                tamanhoBytes);

            return new SyncPackageResult
            {
                CaminhoArquivo = caminhoArquivo,
                NomeArquivo = nomeArquivo,
                ContentType = "application/zip",
                TamanhoBytes = tamanhoBytes,
                QuantidadeProdutos = produtos.Count,
                QuantidadeImagens = imagens.Count
            };
        }
        catch
        {
            ExcluirArquivoSilenciosamente(
                caminhoArquivo);

            throw;
        }
    }

    private static async Task AdicionarJsonAsync<T>(
        ZipArchive zipArchive,
        string nomeEntrada,
        T dados,
        CancellationToken cancellationToken)
    {
        var entrada = zipArchive.CreateEntry(
            nomeEntrada,
            CompressionLevel.Optimal);

        await using var stream =
            entrada.Open();

        await JsonSerializer.SerializeAsync(
            stream,
            dados,
            JsonOptions,
            cancellationToken);
    }

    private static async Task AdicionarImagemAsync(
        ZipArchive zipArchive,
        SyncImageFile imagem,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                imagem.CaminhoFisico))
        {
            return;
        }

        if (!File.Exists(
                imagem.CaminhoFisico))
        {
            return;
        }

        var caminhoNoPacote =
            imagem.CaminhoNoPacote
                .Replace('\\', '/')
                .TrimStart('/');

        var entrada =
            zipArchive.CreateEntry(
                caminhoNoPacote,
                CompressionLevel.Fastest);

        await using var destino =
            entrada.Open();

        await using var origem =
            new FileStream(
                imagem.CaminhoFisico,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 128 * 1024,
                useAsync: true);

        await origem.CopyToAsync(
            destino,
            cancellationToken);
    }

    private static void ExcluirArquivoSilenciosamente(
        string caminhoArquivo)
    {
        try
        {
            if (File.Exists(caminhoArquivo))
            {
                File.Delete(caminhoArquivo);
            }
        }
        catch
        {
            // Não substitui a exceção original.
        }
    }
}