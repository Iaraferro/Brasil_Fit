using BrasilFit.API.DTOs.Avaliacoes;
using BrasilFit.Domain.Enums;
using BrasilFit.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BrasilFit.API.Controllers
{
    [ApiController]
    [Route("api/paciente")]
    [Authorize(Roles = nameof(PapelUsuario.Paciente))]
    public class PacienteController: ControllerBase
    {
        private readonly AppDbContext _context;

        public PacienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/paciente/meu-plano
        [HttpGet("meu-plano")]
        public async Task<IActionResult> GetMeuPlano(CancellationToken ct)
        {
            var pacienteId = ObterPacienteId();

            var plano = await _context.PlanosAlimentares
                .AsNoTracking()
                .Include(p => p.Refeicoes)
                    .ThenInclude(r => r.Itens)
                        .ThenInclude(i => i.Alimento)
                .Where(p => p.PacienteId == pacienteId && p.Ativo)
                .FirstOrDefaultAsync(ct);

            if (plano == null)
                return Ok(new { mensagem = "Nenhum plano ativo encontrado." });

            return Ok(plano);
        }

        // GET /api/paciente/minhas-avaliacoes
        [HttpGet("minhas-avaliacoes")]
        public async Task<IActionResult> GetMinhasAvaliacoes(CancellationToken ct)
        {
            var pacienteId = ObterPacienteId();

            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Include(a => a.MedidaCorporal)
                .Include(a => a.ComposicaoCorporal)
                .Where(a => a.PacienteId == pacienteId)
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

        // GET /api/paciente/minhas-metas
        [HttpGet("minhas-metas")]
        public async Task<IActionResult> GetMinhasMetas(CancellationToken ct)
        {
            var pacienteId = ObterPacienteId();

            var metas = await _context.Metas
                .AsNoTracking()
                .Where(m => m.PacienteId == pacienteId)
                .OrderByDescending(m => m.CriadaEm)
                .ToListAsync(ct);

            return Ok(metas);
        }

        // GET /api/paciente/minha-evolucao
        [HttpGet("minha-evolucao")]
        public async Task<IActionResult> GetMinhaEvolucao(CancellationToken ct)
        {
            var pacienteId = ObterPacienteId();

            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Include(a => a.MedidaCorporal)
                .Include(a => a.ComposicaoCorporal)
                .Where(a => a.PacienteId == pacienteId)
                .OrderBy(a => a.Data)
                .Select(a => new
                {
                    Data = a.Data,
                    Peso = a.MedidaCorporal != null ? a.MedidaCorporal.Peso : 0,
                    Imc = a.ComposicaoCorporal != null ? a.ComposicaoCorporal.Imc : 0
                })
                .ToListAsync(ct);

            return Ok(avaliacoes);
        }

        // DELETE /api/pacientes/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deletar(int id, CancellationToken ct)
        {
            var nutricionistaId = ObterPacienteId();

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == id && p.NutricionistaId == nutricionistaId, ct);

            if (paciente is null)
                return NotFound(new { mensagem = "Paciente não encontrado." });

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        private int ObterPacienteId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? throw new UnauthorizedAccessException("Token sem identificador de usuario.");
            return int.Parse(sub);
        }
    }
}
