using BrasilFit.API.DTOs.Pacientes;

namespace BrasilFit.API.Services.Pacientes;

public interface IPacienteService
{
    Task<PacienteDto> CadastrarAsync(CriarPacienteDto dto, int nutricionistaId, CancellationToken ct = default);
    Task<IReadOnlyList<PacienteDto>> ListarPorNutricionistaAsync(int nutricionistaId, CancellationToken ct = default);
}
