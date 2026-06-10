using BrasilFit.API.DTOs.Avaliacoes;
using BrasilFit.API.DTOs.Common;
using BrasilFit.API.DTOs.Pacientes;
using BrasilFit.API.DTOs.PlanoAlimentar;
using BrasilFit.API.Services.Pacientes;
using BrasilFit.Domain.Enums;
using BrasilFit.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = $"{nameof(PapelUsuario.Nutricionista)},{nameof(PapelUsuario.Administrador)}")]

public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;
    private readonly AppDbContext _context;

    public PacientesController(IPacienteService pacienteService, AppDbContext context)
    {
        _pacienteService = pacienteService;
        _context = context; 
    }

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


    // PacientesController.cs - Adicionar este método

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var nutricionistaId = ObterUsuarioId();

        var paciente = await _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.NutricionistaId == nutricionistaId, ct);

        if (paciente is null)
            return NotFound();

        return Ok(new PacienteDto
        {
            Id = paciente.Id,
            Nome = paciente.Nome,
            Email = paciente.Email,
            DataNascimento = paciente.DataNascimento,
            Sexo = paciente.Sexo,
            Telefone = paciente.Telefone,
            Ativo = paciente.Ativo
        });
    }

    [HttpGet("{id}/avaliacoes")]
    [ProducesResponseType(typeof(List<AvaliacaoResultadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAvaliacoes(int id, CancellationToken ct)
    {
        var nutricionistaId = ObterUsuarioId();

        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.NutricionistaId == nutricionistaId, ct);

        if (paciente is null)
            return NotFound();

        var avaliacoes = await _context.Avaliacoes
            .AsNoTracking()
            .Include(a => a.MedidaCorporal)
            .Include(a => a.ComposicaoCorporal)
            .Where(a => a.PacienteId == id)
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

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        // Placeholder
        return Ok(new { id });
    }

    private int ObterUsuarioId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("Token sem identificador de usuario.");
        return int.Parse(sub);
    }

}
