using System.Security.Claims;
using BrasilFit.API.DTOs.Pacientes;
using BrasilFit.API.Services.Pacientes;
using BrasilFit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = nameof(PapelUsuario.Nutricionista))]
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;

    public PacientesController(IPacienteService pacienteService)
        => _pacienteService = pacienteService;

    // ENDPOINT AUTENTICADO #1 - Cadastro de Paciente pelo Nutricionista (Role: Nutricionista).
    [HttpPost]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cadastrar([FromBody] CriarPacienteDto dto, CancellationToken ct)
    {
        var nutricionistaId = ObterUsuarioId();
        try
        {
            var paciente = await _pacienteService.CadastrarAsync(dto, nutricionistaId, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = paciente.Id }, paciente);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PacienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var nutricionistaId = ObterUsuarioId();
        var pacientes = await _pacienteService.ListarPorNutricionistaAsync(nutricionistaId, ct);
        return Ok(pacientes);
    }

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        // Placeholder - mantido para o CreatedAtAction.
        return Ok(new { id });
    }

    private int ObterUsuarioId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("Token sem identificador de usuario.");
        return int.Parse(sub);
    }
}
