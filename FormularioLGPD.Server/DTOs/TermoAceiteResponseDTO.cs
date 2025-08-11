// DTOs/TermoAceiteResponseDTO.cs
namespace FormularioLGPD.Server.DTOs
{
    public class TermoAceiteResponseDTO
    {
        public int Id { get; set; }
        public string NumeroTermo { get; set; } = string.Empty;
        public DateTime DataHoraAceite { get; set; }
        public string HashIntegridade { get; set; } = string.Empty;
        public string CaminhoArquivoPDF { get; set; } = string.Empty;
        public bool PodeDownloadPdf { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}