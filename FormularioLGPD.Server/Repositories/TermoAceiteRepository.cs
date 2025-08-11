// Repositories/TermoAceiteRepository.cs
using FormularioLGPD.Server.Repositories.Interfaces;
using FormularioLGPD.Server.Data;
using FormularioLGPD.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FormularioLGPD.Server.Repositories
{
    public class TermoAceiteRepository : ITermoAceiteRepository
    {
        private readonly ApplicationDbContext _context;

        public TermoAceiteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TermoAceite> CriarAsync(TermoAceite termo)
        {
            _context.TermosAceite.Add(termo);
            await _context.SaveChangesAsync();
            return termo;
        }

        public async Task<TermoAceite?> ObterPorIdAsync(int id)
        {
            return await _context.TermosAceite
                .Include(t => t.Titular)
                .ThenInclude(t => t.Dependentes)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TermoAceite> AtualizarAsync(TermoAceite termo)
        {
            _context.TermosAceite.Update(termo);
            await _context.SaveChangesAsync();
            return termo;
        }

        public async Task<bool> ExisteCpfAsync(string cpf)
        {
            return await _context.Titulares
                .AnyAsync(t => t.CPF == cpf && t.IsAtivo &&
                          t.TermoAceite != null && t.TermoAceite.StatusTermo == StatusTermo.Ativo);
        }

        public async Task<List<TermoAceite>> ListarAsync(int skip = 0, int take = 10)
        {
            return await _context.TermosAceite
                .Include(t => t.Titular)
                .OrderByDescending(t => t.DataHoraAceite)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}