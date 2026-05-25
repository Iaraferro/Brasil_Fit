using BrasilFit.API.DTOs.Common;
using BrasilFit.API.DTOs.Pacientes;

namespace BrasilFit.API.Services.Pacientes;

public interface IPacienteService
{
    Task<PacienteDto> CadastrarAsync(CriarPacienteDto dto, int nutricionistaId, CancellationToken ct = default);

    Task<PaginacaoResultadoDto<PacienteDto>> ListarPorNutricionistaAsync(
        int nutricionistaId,
        PaginacaoQuery query,
        bool? somenteAtivos,
        CancellationToken ct = default);
}
