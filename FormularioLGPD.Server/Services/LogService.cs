// Services/LogService.cs
using FormularioLGPD.Server.Services.Interfaces;
using FormularioLGPD.Server.Data;
using FormularioLGPD.Server.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace FormularioLGPD.Server.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LogService> _logger;

        public LogService(ApplicationDbContext context, ILogger<LogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegistrarOperacaoAsync(int? termoId, string tipoOperacao, string descricao, string ipOrigem, string? userAgent, StatusOperacao status, object? dadosAntes = null, object? dadosDepois = null)
        {
            try
            {
                var logAuditoria = new LogAuditoria
                {
                    TermoAceiteId = termoId,
                    TipoOperacao = tipoOperacao,
                    Descricao = descricao,
                    IpOrigem = ipOrigem,
                    UserAgent = userAgent ?? string.Empty,
                    DataHoraOperacao = DateTime.UtcNow,
                    StatusOperacao = status,
                    DadosAntes = dadosAntes != null ? JsonSerializer.Serialize(dadosAntes) : null,
                    DadosDepois = dadosDepois != null ? JsonSerializer.Serialize(dadosDepois) : null
                };

                _context.LogsAuditoria.Add(logAuditoria);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Log de auditoria registrado: {TipoOperacao} - {Status}", tipoOperacao, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar log de auditoria: {TipoOperacao}", tipoOperacao);
                // Não relançar a exceção para não interromper o fluxo principal
            }
        }

        public async Task<List<LogAuditoria>> ConsultarLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null, int skip = 0, int take = 50)
        {
            var query = _context.LogsAuditoria.AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(l => l.DataHoraOperacao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(l => l.DataHoraOperacao <= dataFim.Value);

            return await query
                .OrderByDescending(l => l.DataHoraOperacao)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}