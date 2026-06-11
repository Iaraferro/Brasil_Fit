using BrasilFit.API.DTOs.Common;
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

    public async Task<PaginacaoResultadoDto<PacienteDto>> ListarPorNutricionistaAsync(
        int nutricionistaId,
        PaginacaoQuery query,
        bool? somenteAtivos,
        CancellationToken ct = default)
    {
        // Comecamos com o IQueryable - o EF so executa a query no banco quando
        // chamamos ToListAsync/CountAsync. Isso permite compor filtros sem trazer
        // tudo para a memoria.
        var consulta = _context.Pacientes
            .AsNoTracking()
            .Where(p => p.NutricionistaId == nutricionistaId);

        if (somenteAtivos is not null)
            consulta = consulta.Where(p => p.Ativo == somenteAtivos.Value);

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            var termo = query.Busca.Trim();
            // SQL Server usa LIKE - EF traduz Contains para isso.
            consulta = consulta.Where(p =>
                EF.Functions.Like(p.Nome, $"%{termo}%") ||
                EF.Functions.Like(p.Email, $"%{termo}%"));
        }

        consulta = (query.OrdenarPor?.ToLowerInvariant()) switch
        {
            "email"          => query.Decrescente ? consulta.OrderByDescending(p => p.Email)         : consulta.OrderBy(p => p.Email),
            "datanascimento" => query.Decrescente ? consulta.OrderByDescending(p => p.DataNascimento): consulta.OrderBy(p => p.DataNascimento),
            _                => query.Decrescente ? consulta.OrderByDescending(p => p.Nome)          : consulta.OrderBy(p => p.Nome)
        };

        // Count e a query paginada vao em duas idas ao banco. Para conjuntos pequenos
        // o overhead e irrelevante; para listas gigantes vale considerar cache do total.
        var total = await consulta.CountAsync(ct);

        var itens = await consulta
            .Skip((query.Pagina - 1) * query.TamanhoPagina)
            .Take(query.TamanhoPagina)
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

        return PaginacaoResultadoDto<PacienteDto>.Criar(itens, query.Pagina, query.TamanhoPagina, total);
    }

    public async Task<PaginacaoResultadoDto<PacienteDto>> ListarTodosAsync(
        PaginacaoQuery query,
        bool? somenteAtivos,
        CancellationToken ct = default)
    {
        var consulta = _context.Pacientes.AsNoTracking();

        if (somenteAtivos is not null)
            consulta = consulta.Where(p => p.Ativo == somenteAtivos.Value);

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            var termo = query.Busca.Trim();
            consulta = consulta.Where(p =>
                EF.Functions.Like(p.Nome, $"%{termo}%") ||
                EF.Functions.Like(p.Email, $"%{termo}%"));
        }

        consulta = (query.OrdenarPor?.ToLowerInvariant()) switch
        {
            "email" => query.Decrescente ? consulta.OrderByDescending(p => p.Email) : consulta.OrderBy(p => p.Email),
            _       => query.Decrescente ? consulta.OrderByDescending(p => p.Nome)  : consulta.OrderBy(p => p.Nome)
        };

        var total = await consulta.CountAsync(ct);
        var itens = await consulta
            .Skip((query.Pagina - 1) * query.TamanhoPagina)
            .Take(query.TamanhoPagina)
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

        return PaginacaoResultadoDto<PacienteDto>.Criar(itens, query.Pagina, query.TamanhoPagina, total);
    }
}
