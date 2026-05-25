namespace BrasilFit.Domain.Entities;

// Calculada a partir da MedidaCorporal correspondente.
public class ComposicaoCorporal
{
    public int Id { get; set; }
    public decimal Imc { get; set; }
    public decimal? PercentualGordura { get; set; }
    public decimal? MassaMagra { get; set; }
    public string? Classificacao { get; set; } // "Eutrofico", "Sobrepeso", etc.

    public int AvaliacaoAntropometricaId { get; set; }
    public AvaliacaoAntropometrica AvaliacaoAntropometrica { get; set; } = null!;
}
