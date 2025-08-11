// Models/Dependente.cs
using System.ComponentModel.DataAnnotations;

namespace FormularioLGPD.Server.Models
{
    public class Dependente
    {
        public int Id { get; set; }

        public int TitularId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string CPF { get; set; } = string.Empty;

        public DateTime DataNascimento { get; set; }

        [Required]
        [MaxLength(50)]
        public string GrauParentesco { get; set; } = string.Empty;

        public bool IsMenorIdade => CalcularIdade() < 18;

        public bool IsAtivo { get; set; } = true;

        public DateTime DataCadastro { get; set; }

        // Relacionamento
        public Titular Titular { get; set; } = null!;

        public int CalcularIdade()
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - DataNascimento.Year;
            if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
            return idade;
        }
    }
}