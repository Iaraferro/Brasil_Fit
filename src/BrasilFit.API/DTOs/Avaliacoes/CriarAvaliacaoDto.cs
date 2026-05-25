using System.ComponentModel.DataAnnotations;

namespace BrasilFit.API.DTOs.Avaliacoes;

public class CriarAvaliacaoDto
{
    [Required] public int PacienteId { get; set; }
    [Required] public DateTime Data { get; set; }
    public string? ObservacoesClinicas { get; set; }

    [Required] public MedidaCorporalDto Medida { get; set; } = new();
}

public class MedidaCorporalDto
{
    [Range(20, 400)] public decimal Peso { get; set; }
    [Range(0.5, 2.5)] public decimal Altura { get; set; }
    public string? CircunferenciasJson { get; set; }
    public string? DobrasCutaneasJson { get; set; }
}

public class AvaliacaoResultadoDto
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
    public decimal Imc { get; set; }
    public string? Classificacao { get; set; }
}
