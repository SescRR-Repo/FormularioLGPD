// Models/Titular.cs
using System.ComponentModel.DataAnnotations;

namespace FormularioLGPD.Server.Models
{
    public class Titular
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string CPF { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? RG { get; set; }

        public DateTime DataNascimento { get; set; }

        [Required]
        [MaxLength(50)]
        public string EstadoCivil { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Naturalidade { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Endereco { get; set; }

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Escolaridade { get; set; }

        [MaxLength(50)]
        public string? SerieSemestre { get; set; }

        [Required]
        public QualificacaoLegal QualificacaoLegal { get; set; }

        public bool IsAtivo { get; set; } = true;

        public DateTime DataCadastro { get; set; }

        // Relacionamentos
        public ICollection<Dependente> Dependentes { get; set; } = new List<Dependente>();
        public TermoAceite? TermoAceite { get; set; }
    }
}