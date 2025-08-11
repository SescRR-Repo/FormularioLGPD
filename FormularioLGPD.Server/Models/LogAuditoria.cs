// Models/LogAuditoria.cs
using System.ComponentModel.DataAnnotations;

namespace FormularioLGPD.Server.Models
{
    public class LogAuditoria
    {
        public int Id { get; set; }

        public int? TermoAceiteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoOperacao { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [MaxLength(45)]
        public string IpOrigem { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime DataHoraOperacao { get; set; }

        public string? DadosAntes { get; set; } // JSON

        public string? DadosDepois { get; set; } // JSON

        [Required]
        public StatusOperacao StatusOperacao { get; set; }

        // Relacionamento
        public TermoAceite? TermoAceite { get; set; }
    }
}
