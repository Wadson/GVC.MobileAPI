using GVC.MobileAPI.Builders;
using GVC.MobileAPI.Builders.Interfaces;
using GVC.MobileAPI.DTOs;
using GVC.MobileAPI.Models;
using GVC.MobileAPI.Services.Interfaces;

namespace GVC.MobileAPI.Services;

public sealed class SincronizacaoService : ISincronizacaoService
{
    private readonly IProdutoService _produtoService;
    private readonly IImagemService _imagemService;
    private readonly ISyncPackageBuilder _packageBuilder;
    private readonly ILogger<SincronizacaoService> _logger;

    private readonly IClienteService _clienteService;
    private readonly IContaReceberService _contaReceberService;
    private readonly IEmpresaService _empresaService;

    public SincronizacaoService(
       IProdutoService produtoService,
       IClienteService clienteService,
       IContaReceberService contaReceberService,
       IEmpresaService empresaService,
       IImagemService imagemService,
       ISyncPackageBuilder packageBuilder,
       ILogger<SincronizacaoService> logger)
    {
        _produtoService = produtoService;
        _clienteService = clienteService;
        _contaReceberService = contaReceberService;
        _empresaService = empresaService;
        _imagemService = imagemService;
        _packageBuilder = packageBuilder;
        _logger = logger;
    }

    public async Task<SyncPackageResult> GerarPacoteCompletoAsync( int? empresaId, CancellationToken cancellationToken = default)
    {
        if (empresaId.HasValue && empresaId.Value <= 0)
        {
            throw new ArgumentException(
                "O identificador da empresa deve ser maior que zero.",
                nameof(empresaId));
        }

        _logger.LogInformation( "Iniciando geração do pacote completo. EmpresaID: {EmpresaID}.", empresaId);

        
        var produtosOriginais = await _produtoService.ObterTodosAsync(
            empresaId,
            cancellationToken);

        var clientes =  await _clienteService.ObterTodosAsync( empresaId,  cancellationToken);
        var contasReceber = await _contaReceberService.ObterTodasAsync(  empresaId, cancellationToken);

        var empresas = await _empresaService.ObterTodasAsync(cancellationToken);

        var produtosSync = new List<ProdutoSyncDto>(
            produtosOriginais.Count);

        var imagens = new List<SyncImageFile>();

        var caminhosAdicionados = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var quantidadeImagensPadrao = 0;
        var quantidadeImagensAusentes = 0;

        foreach (var produto in produtosOriginais)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var imagem = _imagemService.LocalizarImagem(
                produto.ProdutoID,
                produto.Imagem);

            string? caminhoImagemNoPacote = null;

            if (imagem.Encontrada &&
                !string.IsNullOrWhiteSpace(imagem.CaminhoFisico))
            {
                if (imagem.UsaImagemPadrao)
                {
                    quantidadeImagensPadrao++;

                    var extensaoPadrao = Path
                        .GetExtension(imagem.CaminhoFisico)
                        .ToLowerInvariant();

                    if (string.IsNullOrWhiteSpace(extensaoPadrao))
                        extensaoPadrao = ".png";

                    caminhoImagemNoPacote =
                        $"imagens/sem-imagem{extensaoPadrao}";
                }
                else
                {
                    caminhoImagemNoPacote =
                        imagem.CaminhoNoPacote;
                }

                if (!string.IsNullOrWhiteSpace(caminhoImagemNoPacote))
                {
                    var chaveImagem =
                        $"{imagem.CaminhoFisico}|{caminhoImagemNoPacote}";

                    if (caminhosAdicionados.Add(chaveImagem))
                    {
                        imagens.Add(new SyncImageFile
                        {
                            CaminhoFisico = imagem.CaminhoFisico,
                            CaminhoNoPacote = caminhoImagemNoPacote
                        });
                    }
                }
            }
            else
            {
                quantidadeImagensAusentes++;
            }

            produtosSync.Add(new ProdutoSyncDto
            {
                ProdutoID = produto.ProdutoID,
                NomeProduto = produto.NomeProduto,
                Referencia = produto.Referencia,
                PrecoCompra = produto.PrecoCompra,
                PrecoCusto = produto.PrecoCusto,
                PrecoDeVenda = produto.PrecoDeVenda,
                Estoque = produto.Estoque,
                GtinEan = produto.GtinEan,
                MarcaID = produto.MarcaID,
                Marca = produto.Marca,
                EmpresaID = produto.EmpresaID,
                ImagemLocal = caminhoImagemNoPacote
            });
        }

        var dataGeracaoUtc = DateTime.UtcNow;

        var manifest = new SyncManifestDto
        {
            Versao = GerarVersao(dataGeracaoUtc),
            DataGeracaoUtc = dataGeracaoUtc,

            QuantidadeEmpresas = empresas.Count,
            QuantidadeProdutos = produtosSync.Count,
            QuantidadeClientes = clientes.Count,
            QuantidadeContasReceber = contasReceber.Count,

            QuantidadeImagens = imagens.Count,
            QuantidadeImagensPadrao = quantidadeImagensPadrao,
            QuantidadeImagensAusentes = quantidadeImagensAusentes,

            EmpresaID = empresaId,

            Escopo = empresaId.HasValue
        ? "EmpresaEspecifica"
        : "TodasEmpresas",

            FormatoPacote = "GVC-SYNC-1.0"
        };

        var resultado =
      await _packageBuilder.CriarPacoteAsync(
          empresas,
          produtosSync,
          clientes,
          contasReceber,
          manifest,
          imagens,
          cancellationToken);

        _logger.LogInformation(
            "Pacote completo finalizado. Produtos: {Produtos}. " +
            "Imagens únicas: {Imagens}. Imagens padrão utilizadas: {Padrao}. " +
            "Produtos sem imagem: {Ausentes}.",
            produtosSync.Count,
            imagens.Count,
            quantidadeImagensPadrao,
            quantidadeImagensAusentes);

        return resultado;
    }

    private static string GerarVersao(DateTime dataGeracaoUtc)
    {
        return dataGeracaoUtc.ToString(
            "yyyy.MM.dd.HHmmss");
    }
}