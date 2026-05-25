using System.ComponentModel.DataAnnotations;
using BrasilFit.Domain.Enums;

namespace BrasilFit.API.DTOs.PlanoAlimentar;

public class CriarPlanoAlimentarDto
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Objetivo { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    [Range(1, 365)]
    public int DuracaoDias { get; set; }

    [MaxLength(2000)]
    public string? Observacoes { get; set; }

    [Required]
    public int PacienteId { get; set; }

    public List<CriarRefeicaoDto> Refeicoes { get; set; } = new();
}

public class CriarRefeicaoDto
{
    [Required] public TipoRefeicao Tipo { get; set; }
    [Required] public TimeOnly Horario { get; set; }
    public string? Observacoes { get; set; }
    public List<CriarItemRefeicaoDto> Itens { get; set; } = new();
}

public class CriarItemRefeicaoDto
{
    [Required] public int AlimentoId { get; set; }
    [Range(0.01, 9999)] public decimal Quantidade { get; set; }
    [Required, MaxLength(20)] public string Unidade { get; set; } = "g";
}
