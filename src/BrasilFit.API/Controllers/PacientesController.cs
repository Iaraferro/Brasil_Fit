using System.Security.Claims;
using BrasilFit.API.DTOs.Common;
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

    // ENDPOINT AUTENTICADO #1 - Cadastro de Paciente pelo Nutricionista.
    [HttpPost]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cadastrar([FromBody] CriarPacienteDto dto, CancellationToken ct)
    {
        var nutricionistaId = ObterUsuarioId();
        // Excecoes sobem para o GlobalExceptionHandler - sem try/catch aqui.
        var paciente = await _pacienteService.CadastrarAsync(dto, nutricionistaId, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = paciente.Id }, paciente);
    }

    // GET /api/pacientes?pagina=1&tamanhoPagina=20&busca=joao&ordenarPor=nome&decrescente=false&somenteAtivos=true
    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResultadoDto<PacienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] PaginacaoQuery query,
        [FromQuery] bool? somenteAtivos,
        CancellationToken ct)
    {
        var nutricionistaId = ObterUsuarioId();
        var resultado = await _pacienteService.ListarPorNutricionistaAsync(nutricionistaId, query, somenteAtivos, ct);
        return Ok(resultado);
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
