using System.Security.Claims;
using BrasilFit.API.DTOs.PlanoAlimentar;
using BrasilFit.API.Services.PlanosAlimentares;
using BrasilFit.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/planos-alimentares")]
[Authorize(Roles = nameof(PapelUsuario.Nutricionista))]
public class PlanosAlimentaresController : ControllerBase
{
    private readonly IPlanoAlimentarService _service;

    public PlanosAlimentaresController(IPlanoAlimentarService service) => _service = service;

    // ENDPOINT AUTENTICADO #2 - Criacao de Plano Alimentar (Role: Nutricionista).
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] CriarPlanoAlimentarDto dto, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var id = await _service.CriarAsync(dto, nutricionistaId, ct);
            return Created($"/api/planos-alimentares/{id}", new { id });
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
