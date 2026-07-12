using GVC.MobileAPI.Models;

namespace GVC.MobileAPI.Services.Interfaces;

public interface IImagemService
{
    ImagemProdutoResult LocalizarImagem(
        int produtoId,
        string? caminhoImagem);
}