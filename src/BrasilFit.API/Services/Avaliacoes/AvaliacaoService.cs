using BrasilFit.API.DTOs.Avaliacoes;
using BrasilFit.Domain.Entities;
using BrasilFit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BrasilFit.API.Services.Avaliacoes;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly AppDbContext _context;

    public AvaliacaoService(AppDbContext context) => _context = context;

    public async Task<AvaliacaoResultadoDto> RegistrarAsync(CriarAvaliacaoDto dto, int nutricionistaId, CancellationToken ct = default)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == dto.PacienteId, ct)
            ?? throw new InvalidOperationException("Paciente nao encontrado.");

        if (paciente.NutricionistaId != nutricionistaId)
            throw new UnauthorizedAccessException("Paciente nao pertence a este nutricionista.");

        var imc = CalcularImc(dto.Medida.Peso, dto.Medida.Altura);
        var classificacao = ClassificarImc(imc);

        var avaliacao = new AvaliacaoAntropometrica
        {
            Data = dto.Data,
            ObservacoesClinicas = dto.ObservacoesClinicas,
            PacienteId = dto.PacienteId,
            NutricionistaId = nutricionistaId,
            MedidaCorporal = new MedidaCorporal
            {
                Peso = dto.Medida.Peso,
                Altura = dto.Medida.Altura,
                CircunferenciasJson = dto.Medida.CircunferenciasJson,
                DobrasCutaneasJson = dto.Medida.DobrasCutaneasJson
            },
            ComposicaoCorporal = new ComposicaoCorporal
            {
                Imc = imc,
                Classificacao = classificacao
            }
        };

        _context.Avaliacoes.Add(avaliacao);
        await _context.SaveChangesAsync(ct);

        return new AvaliacaoResultadoDto
        {
            Id = avaliacao.Id,
            Data = avaliacao.Data,
            Peso = dto.Medida.Peso,
            Altura = dto.Medida.Altura,
            Imc = imc,
            Classificacao = classificacao
        };
    }

    // IMC = peso / (altura * altura). Altura em metros.
    private static decimal CalcularImc(decimal peso, decimal altura)
    {
        if (altura <= 0) return 0m;
        return Math.Round(peso / (altura * altura), 2);
    }

    // Faixas OMS para adultos.
    private static string ClassificarImc(decimal imc) => imc switch
    {
        < 18.5m => "Baixo peso",
        < 25m   => "Eutrofico",
        < 30m   => "Sobrepeso",
        < 35m   => "Obesidade grau I",
        < 40m   => "Obesidade grau II",
        _       => "Obesidade grau III"
    };
}
