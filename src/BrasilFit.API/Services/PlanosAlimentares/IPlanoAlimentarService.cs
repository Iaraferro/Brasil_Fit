using BrasilFit.API.DTOs.PlanoAlimentar;

namespace BrasilFit.API.Services.PlanosAlimentares;

public interface IPlanoAlimentarService
{
    Task<int> CriarAsync(CriarPlanoAlimentarDto dto, int nutricionistaId, CancellationToken ct = default);
}
