// Models/Enums.cs
namespace FormularioLGPD.Server.Models
{
    public enum QualificacaoLegal
    {
        Titular = 1,
        DependenteMaior = 2,
        ResponsavelMenor = 3,
        ResponsavelOrfao = 4,
        CuradorTutor = 5
    }

    public enum StatusTermo
    {
        Ativo = 1,
        Revogado = 2,
        Expirado = 3,
        Cancelado = 4
    }

    public enum StatusOperacao
    {
        Sucesso = 1,
        Erro = 2,
        Pendente = 3,
        Cancelado = 4
    }
}