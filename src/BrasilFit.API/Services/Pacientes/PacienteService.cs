using BrasilFit.API.DTOs.Pacientes;
using BrasilFit.API.Services.Auth;
using BrasilFit.Domain.Entities;
using BrasilFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.API.Services.Pacientes;

public class PacienteService : IPacienteService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public PacienteService(AppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<PacienteDto> CadastrarAsync(CriarPacienteDto dto, int nutricionistaId, CancellationToken ct = default)
    {
        var emailJaUsado = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email, ct);
        if (emailJaUsado)
            throw new InvalidOperationException("E-mail ja cadastrado.");

        var paciente = new Paciente
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = _hasher.Hash(dto.Senha),
            DataNascimento = dto.DataNascimento,
            Sexo = dto.Sexo,
            Telefone = dto.Telefone,
            HistoricoClinico = dto.HistoricoClinico,
            NutricionistaId = nutricionistaId,
            Ativo = true
        };

        if (dto.Endereco is not null)
        {
            paciente.Endereco = new Endereco
            {
                Cep = dto.Endereco.Cep,
                Logradouro = dto.Endereco.Logradouro,
                Numero = dto.Endereco.Numero,
                Complemento = dto.Endereco.Complemento,
                Bairro = dto.Endereco.Bairro,
                Cidade = dto.Endereco.Cidade,
                Uf = dto.Endereco.Uf.ToUpperInvariant()
            };
        }

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync(ct);

        return new PacienteDto
        {
            Id = paciente.Id,
            Nome = paciente.Nome,
            Email = paciente.Email,
            DataNascimento = paciente.DataNascimento,
            Sexo = paciente.Sexo,
            Telefone = paciente.Telefone,
            Ativo = paciente.Ativo
        };
    }

    public async Task<IReadOnlyList<PacienteDto>> ListarPorNutricionistaAsync(int nutricionistaId, CancellationToken ct = default)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .Where(p => p.NutricionistaId == nutricionistaId)
            .Select(p => new PacienteDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Email = p.Email,
                DataNascimento = p.DataNascimento,
                Sexo = p.Sexo,
                Telefone = p.Telefone,
                Ativo = p.Ativo
            })
            .ToListAsync(ct);
    }
}
