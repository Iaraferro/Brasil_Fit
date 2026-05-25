using BrasilFit.API.DTOs.PlanoAlimentar;
using BrasilFit.Domain.Entities;
using BrasilFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.API.Services.PlanosAlimentares;

public class PlanoAlimentarService : IPlanoAlimentarService
{
    private readonly AppDbContext _context;

    public PlanoAlimentarService(AppDbContext context) => _context = context;

    public async Task<int> CriarAsync(CriarPlanoAlimentarDto dto, int nutricionistaId, CancellationToken ct = default)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == dto.PacienteId, ct)
            ?? throw new InvalidOperationException("Paciente nao encontrado.");

        // Regra de negocio: nutricionista so pode criar plano para seus pacientes.
        if (paciente.NutricionistaId != nutricionistaId)
            throw new UnauthorizedAccessException("Paciente nao pertence a este nutricionista.");

        var plano = new PlanoAlimentar
        {
            Nome = dto.Nome,
            Objetivo = dto.Objetivo,
            DataInicio = dto.DataInicio,
            DuracaoDias = dto.DuracaoDias,
            Observacoes = dto.Observacoes,
            PacienteId = dto.PacienteId,
            NutricionistaId = nutricionistaId,
            Ativo = true,
            Refeicoes = dto.Refeicoes.Select(r => new Refeicao
            {
                Tipo = r.Tipo,
                Horario = r.Horario,
                Observacoes = r.Observacoes,
                Itens = r.Itens.Select(i => new ItemRefeicao
                {
                    AlimentoId = i.AlimentoId,
                    Quantidade = i.Quantidade,
                    Unidade = i.Unidade
                }).ToList()
            }).ToList()
        };

        _context.PlanosAlimentares.Add(plano);
        await _context.SaveChangesAsync(ct);

        return plano.Id;
    }
}
