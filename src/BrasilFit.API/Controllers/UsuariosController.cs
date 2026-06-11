using BrasilFit.API.DTOs.Nutricionista;
using BrasilFit.Domain.Entities;
using BrasilFit.Domain.Enums;
using BrasilFit.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = nameof(PapelUsuario.Administrador))]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/usuarios/stats — resumo geral para o dashboard do admin
    [HttpGet("stats")]
    public async Task<IActionResult> ObterStats()
    {
        var hoje = DateTime.UtcNow;
        var inicioAno = new DateTime(hoje.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalNutricionistas = await _context.Nutricionistas.CountAsync();
        var totalPacientes      = await _context.Pacientes.CountAsync();
        var totalPlanos         = await _context.PlanosAlimentares.CountAsync(p => p.Ativo);
        var totalAvaliacoes     = await _context.Avaliacoes.CountAsync();

        // Novos usuários por mês no ano atual (índice 0 = janeiro)
        var usuariosPorMes = await _context.Usuarios
            .Where(u => u.CriadoEm >= inicioAno)
            .GroupBy(u => u.CriadoEm.Month)
            .Select(g => new { Mes = g.Key, Total = g.Count() })
            .ToListAsync();

        var crescimento = new int[12];
        foreach (var m in usuariosPorMes)
            crescimento[m.Mes - 1] = m.Total;

        return Ok(new
        {
            totalNutricionistas,
            totalPacientes,
            totalPlanos,
            totalAvaliacoes,
            crescimentoMensal = crescimento
        });
    }

    // GET /api/usuarios/nutricionistas
    [HttpGet("nutricionistas")]
    public async Task<IActionResult> ListarNutricionistas()
    {
        // Materializa sem projeções problemáticas (ToString/Count em navegação)
        // para evitar falha de tradução SQL no EF Core 10.
        var raw = await _context.Nutricionistas
            .Include(n => n.Pacientes)
            .AsNoTracking()
            .ToListAsync();

        var nutricionistas = raw.Select(n => new
        {
            n.Id,
            n.Nome,
            n.Email,
            n.Ativo,
            n.Crn,
            n.Especialidade,
            CriadoEm      = n.CriadoEm.ToString("yyyy-MM-dd"),
            TotalPacientes = n.Pacientes.Count
        }).ToList();

        return Ok(new { itens = nutricionistas, totalItens = nutricionistas.Count });
    }

    // GET /api/usuarios/planos — todos os planos alimentares (admin)
    [HttpGet("planos")]
    public async Task<IActionResult> ListarPlanos()
    {
        var planos = await _context.PlanosAlimentares
            .AsNoTracking()
            .Include(p => p.Paciente)
            .Include(p => p.Nutricionista)
            .OrderByDescending(p => p.CriadoEm)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.Objetivo,
                p.Ativo,
                p.DuracaoDias,
                DataInicio    = p.DataInicio.ToString("yyyy-MM-dd"),
                PacienteNome  = p.Paciente.Nome,
                NutricionistaNome = p.Nutricionista.Nome
            })
            .ToListAsync();

        return Ok(planos);
    }

    // GET /api/usuarios/avaliacoes — todas as avaliacoes (admin)
    [HttpGet("avaliacoes")]
    public async Task<IActionResult> ListarAvaliacoes()
    {
        var avaliacoes = await _context.Avaliacoes
            .AsNoTracking()
            .Include(a => a.Paciente)
            .Include(a => a.Nutricionista)
            .Include(a => a.MedidaCorporal)
            .Include(a => a.ComposicaoCorporal)
            .OrderByDescending(a => a.Data)
            .ToListAsync();

        var resultado = avaliacoes.Select(a => new
        {
            a.Id,
            Data              = a.Data.ToString("yyyy-MM-dd"),
            PacienteNome      = a.Paciente.Nome,
            NutricionistaNome = a.Nutricionista.Nome,
            Peso              = a.MedidaCorporal?.Peso,
            Altura            = a.MedidaCorporal?.Altura,
            Imc               = a.ComposicaoCorporal?.Imc,
            Classificacao     = a.ComposicaoCorporal?.Classificacao,
            a.ObservacoesClinicas
        }).ToList();

        return Ok(resultado);
    }

    // POST /api/usuarios/nutricionistas
    [HttpPost("nutricionistas")]
    public async Task<IActionResult> CriarNutricionista([FromBody] CriarNutricionistaDTO dto)
    {
        var existe = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
        if (existe)
            return BadRequest(new { mensagem = "E-mail já cadastrado." });

        var nutricionista = new Nutricionista
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Papel = PapelUsuario.Nutricionista,
            Ativo = true,
            Crn = dto.Crn,
            Especialidade = dto.Especialidade
        };

        _context.Usuarios.Add(nutricionista);
        await _context.SaveChangesAsync();

        return Ok(new { id = nutricionista.Id, nome = nutricionista.Nome, email = nutricionista.Email });
    }
}