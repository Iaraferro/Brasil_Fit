using BrasilFit.API.DTOs.Avaliacoes;
using BrasilFit.API.Services.Avaliacoes;
using BrasilFit.Domain.Enums;
using BrasilFit.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/avaliacoes")]
[Authorize(Roles = nameof(PapelUsuario.Nutricionista))]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _service;

    private readonly AppDbContext _context;  

    // Correção do construtor para a adição de _context = context;
    public AvaliacoesController(IAvaliacaoService service, AppDbContext context)
    {
        _service = service;
        _context = context;  
    }

    // ENDPOINT AUTENTICADO #3 - Registro de Avaliacao Antropometrica.
    [HttpPost]
    [ProducesResponseType(typeof(AvaliacaoResultadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Registrar([FromBody] CriarAvaliacaoDto dto, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resultado = await _service.RegistrarAsync(dto, nutricionistaId, ct);
        return Created($"/api/avaliacoes/{resultado.Id}", resultado);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AvaliacaoResultadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var avaliacao = await _context.Avaliacoes
            .AsNoTracking()
            .Include(a => a.MedidaCorporal)
            .Include(a => a.ComposicaoCorporal)
            .FirstOrDefaultAsync(a => a.Id == id && a.NutricionistaId == nutricionistaId, ct);

        if (avaliacao is null)
            return NotFound(new { mensagem = "Avaliacao nao encontrada." });

        return Ok(new AvaliacaoResultadoDto
        {
            Id = avaliacao.Id,
            Data = avaliacao.Data,
            Peso = avaliacao.MedidaCorporal?.Peso ?? 0,
            Altura = avaliacao.MedidaCorporal?.Altura ?? 0,
            Imc = avaliacao.ComposicaoCorporal?.Imc ?? 0,
            Classificacao = avaliacao.ComposicaoCorporal?.Classificacao
        });
    }

    // GET /api/avaliacoes/paciente/{pacienteId}
    [HttpGet("paciente/{pacienteId}")]
    [ProducesResponseType(typeof(List<AvaliacaoResultadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorPaciente(int pacienteId, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var avaliacoes = await _context.Avaliacoes
            .AsNoTracking()
            .Include(a => a.MedidaCorporal)
            .Include(a => a.ComposicaoCorporal)
            .Where(a => a.PacienteId == pacienteId && a.NutricionistaId == nutricionistaId)
            .OrderByDescending(a => a.Data)
            .Select(a => new AvaliacaoResultadoDto
            {
                Id = a.Id,
                Data = a.Data,
                Peso = a.MedidaCorporal != null ? a.MedidaCorporal.Peso : 0,
                Altura = a.MedidaCorporal != null ? a.MedidaCorporal.Altura : 0,
                Imc = a.ComposicaoCorporal != null ? a.ComposicaoCorporal.Imc : 0,
                Classificacao = a.ComposicaoCorporal != null ? a.ComposicaoCorporal.Classificacao : null
            })
            .ToListAsync(ct);

        return Ok(avaliacoes);
    }


    // DELETE /api/avaliacoes/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(int id, CancellationToken ct)
    {
        var nutricionistaId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var avaliacao = await _context.Avaliacoes
            .FirstOrDefaultAsync(a => a.Id == id && a.NutricionistaId == nutricionistaId, ct);

        if (avaliacao is null)
            return NotFound(new { mensagem = "Avaliação não encontrada." });

        _context.Avaliacoes.Remove(avaliacao);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }



}
