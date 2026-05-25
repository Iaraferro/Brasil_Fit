using BrasilFit.API.DTOs.Avaliacoes;

namespace BrasilFit.API.Services.Avaliacoes;

public interface IAvaliacaoService
{
    Task<AvaliacaoResultadoDto> RegistrarAsync(CriarAvaliacaoDto dto, int nutricionistaId, CancellationToken ct = default);
}
