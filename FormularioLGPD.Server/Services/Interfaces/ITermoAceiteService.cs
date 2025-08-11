// Services/Interfaces/ITermoAceiteService.cs
using FormularioLGPD.Server.DTOs;
using FormularioLGPD.Server.Models;

namespace FormularioLGPD.Server.Services.Interfaces
{
    public interface ITermoAceiteService
    {
        Task<TermoAceiteResponseDTO> CriarTermoAceiteAsync(TermoAceiteCreateDTO dto, string ipOrigem, string userAgent);
        Task<byte[]> GerarPdfTermoAsync(int termoId);
        Task<bool> ValidarCpfExistenteAsync(string cpf);
        Task<TermoAceite?> ObterTermoPorIdAsync(int id);
        string GerarNumeroTermo();
    }
}