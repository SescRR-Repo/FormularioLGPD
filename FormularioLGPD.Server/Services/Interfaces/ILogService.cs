// Services/Interfaces/ILogService.cs
using FormularioLGPD.Server.Models;

namespace FormularioLGPD.Server.Services.Interfaces
{
    public interface ILogService
    {
        Task RegistrarOperacaoAsync(int? termoId, string tipoOperacao, string descricao, string ipOrigem, string? userAgent, StatusOperacao status, object? dadosAntes = null, object? dadosDepois = null);
        Task<List<LogAuditoria>> ConsultarLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null, int skip = 0, int take = 50);
    }
}