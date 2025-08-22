// DTOs/TermoAceiteCreateDTO.cs
using FormularioLGPD.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace FormularioLGPD.Server.DTOs
{
    public class TermoAceiteCreateDTO
    {
        [Required(ErrorMessage = "Tipo de cadastro é obrigatório")]
        public string TipoCadastro { get; set; } = "cadastro"; // NOVO CAMPO ADICIONADO

        public TitularDTO Titular { get; set; } = new();
        public List<DependenteDTO> Dependentes { get; set; } = new();
        public bool AceiteConfirmado { get; set; }
    }

    public class TitularDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
        public string CPF { get; set; } = string.Empty;

        public string? RG { get; set; }

        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        public DateTime DataNascimento { get; set; }

        [Required(ErrorMessage = "Estado civil é obrigatório")]
        public string EstadoCivil { get; set; } = string.Empty;

        [Required(ErrorMessage = "Naturalidade é obrigatória")]
        public string Naturalidade { get; set; } = string.Empty;

        public string? Endereco { get; set; }
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        public string? Escolaridade { get; set; }
        public string? SerieSemestre { get; set; }

        [Required(ErrorMessage = "Qualificação legal é obrigatória")]
        public QualificacaoLegal QualificacaoLegal { get; set; }
    }

    public class DependenteDTO
    {
        [Required(ErrorMessage = "Nome do dependente é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF do dependente é obrigatório")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de nascimento do dependente é obrigatória")]
        public DateTime DataNascimento { get; set; }

        [Required(ErrorMessage = "Grau de parentesco é obrigatório")]
        public string GrauParentesco { get; set; } = string.Empty;
    }
}
