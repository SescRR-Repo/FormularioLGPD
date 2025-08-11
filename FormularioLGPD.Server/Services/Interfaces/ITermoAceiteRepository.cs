// Repositories/Interfaces/ITermoAceiteRepository.cs
using FormularioLGPD.Server.Models;

namespace FormularioLGPD.Server.Repositories.Interfaces
{
    public interface ITermoAceiteRepository
    {
        Task<TermoAceite> CriarAsync(TermoAceite termo);
        Task<TermoAceite?> ObterPorIdAsync(int id);
        Task<TermoAceite> AtualizarAsync(TermoAceite termo);
        Task<bool> ExisteCpfAsync(string cpf);
        Task<List<TermoAceite>> ListarAsync(int skip = 0, int take = 10);
    }
}