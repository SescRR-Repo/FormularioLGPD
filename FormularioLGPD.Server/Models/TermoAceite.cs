// Models/TermoAceite.cs
using System.ComponentModel.DataAnnotations;

namespace FormularioLGPD.Server.Models
{
    public class TermoAceite
    {
        public int Id { get; set; }

        public int TitularId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroTermo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TipoCadastro { get; set; } = "cadastro"; // NOVO CAMPO ADICIONADO

        [Required]
        public string ConteudoTermo { get; set; } = string.Empty;

        public bool AceiteConfirmado { get; set; }

        public DateTime DataHoraAceite { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpOrigem { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string HashIntegridade { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CaminhoArquivoPDF { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string VersaoTermo { get; set; } = "1.0";

        public StatusTermo StatusTermo { get; set; } = StatusTermo.Ativo;

        public DateTime DataCriacao { get; set; }

        // Relacionamento
        public Titular Titular { get; set; } = null!;
    }
}
