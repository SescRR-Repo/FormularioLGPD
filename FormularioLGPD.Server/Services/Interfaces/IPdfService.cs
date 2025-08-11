// Services/Interfaces/IPdfService.cs
using FormularioLGPD.Server.Models;

namespace FormularioLGPD.Server.Services.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GerarPdfTermoAsync(TermoAceite termo);
        Task<string> SalvarPdfAsync(byte[] pdfBytes, string numeroTermo);
        string GerarHashPdf(byte[] pdfBytes);
    }
}