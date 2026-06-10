using BrasilFit.API.DTOs.Metas;
using BrasilFit.Domain.Entities;
using BrasilFit.Domain.Enums;
using BrasilFit.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BrasilFit.API.Controllers
{
    [ApiController]
    [Route("api/metas")]
    [Authorize(Roles = nameof(PapelUsuario.Nutricionista))]
    public class MetaController : ControllerBase
    {

        private readonly AppDbContext _context;

        public MetaController(AppDbContext context)
        {
            _context = context;
        }

        // POST /api/metas
        [HttpPost]
        [ProducesResponseType(typeof(MetaResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> Criar([FromBody] MetaRequestDTO dto, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == dto.PacienteId && p.NutricionistaId == nutricionistaId, ct);

            if (paciente is null)
                return NotFound(new { mensagem = "Paciente nao encontrado." });

            var meta = new Meta
            {
                Tipo = dto.Tipo,
                ValorAlvo = dto.ValorAlvo,
                Unidade = dto.Unidade,
                Prazo = dto.Prazo,
                Descricao = dto.Descricao,
                Status = StatusMeta.EmAndamento,
                CriadaEm = DateTime.UtcNow,
                PacienteId = dto.PacienteId
            };

            _context.Metas.Add(meta);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(ObterPorId), new { id = meta.Id }, new MetaResponseDTO
            {
                Id = meta.Id,
                Tipo = meta.Tipo,
                ValorAlvo = meta.ValorAlvo,
                Unidade = meta.Unidade,
                Prazo = meta.Prazo,
                Status = meta.Status,
                Descricao = meta.Descricao,
                CriadaEm = meta.CriadaEm,
                PacienteId = meta.PacienteId,
                PacienteNome = paciente.Nome
            });
        }

        // GET /api/metas/paciente/{pacienteId}
        [HttpGet("paciente/{pacienteId}")]
        [ProducesResponseType(typeof(List<MetaResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorPaciente(int pacienteId, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var metas = await _context.Metas
                .AsNoTracking()
                .Include(m => m.Paciente)
                .Where(m => m.PacienteId == pacienteId && m.Paciente.NutricionistaId == nutricionistaId)
                .OrderByDescending(m => m.CriadaEm)
                .Select(m => new MetaResponseDTO
                {
                    Id = m.Id,
                    Tipo = m.Tipo,
                    ValorAlvo = m.ValorAlvo,
                    Unidade = m.Unidade,
                    Prazo = m.Prazo,
                    Status = m.Status,
                    Descricao = m.Descricao,
                    CriadaEm = m.CriadaEm,
                    PacienteId = m.PacienteId,
                    PacienteNome = m.Paciente.Nome
                })
                .ToListAsync(ct);

            return Ok(metas);
        }

        // GET /api/metas/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MetaResponseDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var meta = await _context.Metas
                .AsNoTracking()
                .Include(m => m.Paciente)
                .Include(m => m.Progressos)
                .FirstOrDefaultAsync(m => m.Id == id && m.Paciente.NutricionistaId == nutricionistaId, ct);

            if (meta is null)
                return NotFound(new { mensagem = "Meta nao encontrada." });

            var response = new MetaResponseDTO
            {
                Id = meta.Id,
                Tipo = meta.Tipo,
                ValorAlvo = meta.ValorAlvo,
                Unidade = meta.Unidade,
                Prazo = meta.Prazo,
                Status = meta.Status,
                Descricao = meta.Descricao,
                CriadaEm = meta.CriadaEm,
                PacienteId = meta.PacienteId,
                PacienteNome = meta.Paciente.Nome,
                Progressos = meta.Progressos.OrderByDescending(p => p.DataVerificacao).Select(p => new ProgressoMetaResponseDTO
                {
                    Id = p.Id,
                    DataVerificacao = p.DataVerificacao,
                    ValorAtual = p.ValorAtual,
                    Observacao = p.Observacao
                }).ToList()
            };

            return Ok(response);
        }

        // PUT /api/metas/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] MetaRequestDTO dto, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var meta = await _context.Metas
                .Include(m => m.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id && m.Paciente.NutricionistaId == nutricionistaId, ct);

            if (meta is null)
                return NotFound(new { mensagem = "Meta nao encontrada." });

            meta.Tipo = dto.Tipo;
            meta.ValorAlvo = dto.ValorAlvo;
            meta.Unidade = dto.Unidade;
            meta.Prazo = dto.Prazo;
            meta.Descricao = dto.Descricao;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }


        // DELETE /api/metas/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var meta = await _context.Metas
                .Include(m => m.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id && m.Paciente.NutricionistaId == nutricionistaId, ct);

            if (meta is null)
                return NotFound(new { mensagem = "Meta nao encontrada." });

            meta.Status = StatusMeta.Cancelada;
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE FÍSICO - Remove a meta completamente
        [HttpDelete("{id}/remover")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarFisicamente(int id, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var meta = await _context.Metas
                .Include(m => m.Progressos) // Incluir progressos para deletar junto
                .FirstOrDefaultAsync(m => m.Id == id && m.Paciente.NutricionistaId == nutricionistaId, ct);

            if (meta is null)
                return NotFound(new { mensagem = "Meta não encontrada." });

            // Remover progressos primeiro (por causa da chave estrangeira)
            if (meta.Progressos != null && meta.Progressos.Any())
            {
                _context.ProgressosMeta.RemoveRange(meta.Progressos);
            }

            // Remover a meta
            _context.Metas.Remove(meta);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        // POST /api/metas/{id}/progresso
        [HttpPost("{id}/progresso")]
        [ProducesResponseType(typeof(ProgressoMetaResponseDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> RegistrarProgresso(int id, [FromBody] ProgressoMetaRequestDTO dto, CancellationToken ct)
        {
            var nutricionistaId = ObterUsuarioId();

            var meta = await _context.Metas
                .Include(m => m.Paciente)
                .FirstOrDefaultAsync(m => m.Id == id && m.Paciente.NutricionistaId == nutricionistaId, ct);

            if (meta is null)
                return NotFound(new { mensagem = "Meta nao encontrada." });

            var progresso = new ProgressoMeta
            {
                DataVerificacao = DateTime.UtcNow,
                ValorAtual = dto.ValorAtual,
                Observacao = dto.Observacao,
                MetaId = id
            };

            // Verificar se a meta foi atingida
            if (meta.Tipo == TipoMeta.PerdaDePeso && dto.ValorAtual <= meta.ValorAlvo)
                meta.Status = StatusMeta.Atingida;
            else if (dto.ValorAtual >= meta.ValorAlvo)
                meta.Status = StatusMeta.Atingida;

            _context.ProgressosMeta.Add(progresso);
            await _context.SaveChangesAsync(ct);

            return Created($"/api/metas/{id}/progresso/{progresso.Id}", new ProgressoMetaResponseDTO
            {
                Id = progresso.Id,
                DataVerificacao = progresso.DataVerificacao,
                ValorAtual = progresso.ValorAtual,
                Observacao = progresso.Observacao
            });
        }

        private int ObterUsuarioId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? throw new UnauthorizedAccessException("Token sem identificador de usuario.");
            return int.Parse(sub);
        }


    }



}
