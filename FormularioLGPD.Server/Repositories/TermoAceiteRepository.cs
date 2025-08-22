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
            return await _context.TermosAceite
                .AnyAsync(ta => ta.Titular.CPF == cpf && 
                               ta.Titular.IsAtivo && 
                               ta.StatusTermo == StatusTermo.Ativo);
        }

        // ✅ NOVO MÉTODO: Contar quantos termos um CPF possui (para logs)
        public async Task<int> ContarTermosPorCpfAsync(string cpf)
        {
            return await _context.Titulares
                .Where(t => t.CPF == cpf)
                .CountAsync();
        }

        // ✅ NOVO MÉTODO: Obter histórico de termos por CPF
        public async Task<List<TermoAceite>> ObterTermosPorCpfAsync(string cpf)
        {
            return await _context.TermosAceite
                .Include(t => t.Titular)
                .ThenInclude(t => t.Dependentes)
                .Where(t => t.Titular.CPF == cpf)
                .OrderByDescending(t => t.DataHoraAceite)
                .ToListAsync();
        }

        // ✅ NOVO MÉTODO: Obter titular por CPF
        public async Task<Titular?> ObterTitularPorCpfAsync(string cpf)
        {
            return await _context.Titulares
                .Include(t => t.Dependentes)
                .FirstOrDefaultAsync(t => t.CPF == cpf);
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