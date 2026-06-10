using BrasilFit.API.DTOs.PlanoAlimentar;
using BrasilFit.API.Services.PlanosAlimentares;
using BrasilFit.Domain.Enums;
using BrasilFit.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/planos-alimentares")]
[Authorize(Roles = nameof(PapelUsuario.Nutricionista))]
public class PlanosAlimentaresController : ControllerBase
{
    private readonly IPlanoAlimentarService _service;

    private readonly AppDbContext _context;  

   //Contrutor corrigido para a adição de _context = context;
    public PlanosAlimentaresController(IPlanoAlimentarService service, AppDbContext context)
    {
        _service = service;
        _context = context;  
    }


    // ENDPOINT AUTENTICADO #2 - Criacao de Plano Alimentar.
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] CriarPlanoAlimentarDto dto, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await _service.CriarAsync(dto, nutricionistaId, ct);
        return Created($"/api/planos-alimentares/{id}", new { id });
    }


    // GET /api/planos-alimentares/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CriarPlanoAlimentarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var plano = await _context.PlanosAlimentares
            .AsNoTracking()
            .Include(p => p.Paciente)
            .Include(p => p.Refeicoes)
                .ThenInclude(r => r.Itens)
                    .ThenInclude(i => i.Alimento)
            .FirstOrDefaultAsync(p => p.Id == id && p.NutricionistaId == nutricionistaId, ct);

        if (plano is null)
            return NotFound();

        return Ok(plano);
    }

    // GET /api/planos-alimentares/paciente/{pacienteId}
    [HttpGet("paciente/{pacienteId}")]
    [ProducesResponseType(typeof(List<CriarPlanoAlimentarDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorPaciente(int pacienteId, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var planos = await _context.PlanosAlimentares
            .AsNoTracking()
            .Where(p => p.PacienteId == pacienteId && p.NutricionistaId == nutricionistaId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync(ct);

        return Ok(planos);
    }

    // DELETE /api/planos-alimentares/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(int id, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var plano = await _context.PlanosAlimentares
            .FirstOrDefaultAsync(p => p.Id == id && p.NutricionistaId == nutricionistaId, ct);

        if (plano is null)
            return NotFound(new { mensagem = "Plano não encontrado." });

        _context.PlanosAlimentares.Remove(plano);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }


}
