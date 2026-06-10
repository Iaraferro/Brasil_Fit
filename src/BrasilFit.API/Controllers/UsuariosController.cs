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

    // GET /api/usuarios/nutricionistas
    [HttpGet("nutricionistas")]
    public async Task<IActionResult> ListarNutricionistas()
    {
        var nutricionistas = await _context.Usuarios
            .Where(u => u.Papel == PapelUsuario.Nutricionista)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Ativo,
                CriadoEm = u.CriadoEm.ToString("yyyy-MM-dd")
            })
            .ToListAsync();

        return Ok(new { itens = nutricionistas, totalItens = nutricionistas.Count });
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