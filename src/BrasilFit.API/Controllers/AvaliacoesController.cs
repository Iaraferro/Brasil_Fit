using System.Security.Claims;
using BrasilFit.API.DTOs.Avaliacoes;
using BrasilFit.API.Services.Avaliacoes;
using BrasilFit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/avaliacoes")]
[Authorize(Roles = nameof(PapelUsuario.Nutricionista))]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _service;

    public AvaliacoesController(IAvaliacaoService service) => _service = service;

    // ENDPOINT AUTENTICADO #3 - Registro de Avaliacao Antropometrica (Role: Nutricionista).
    [HttpPost]
    [ProducesResponseType(typeof(AvaliacaoResultadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Registrar([FromBody] CriarAvaliacaoDto dto, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var resultado = await _service.RegistrarAsync(dto, nutricionistaId, ct);
            return Created($"/api/avaliacoes/{resultado.Id}", resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
