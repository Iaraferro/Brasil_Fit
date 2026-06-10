namespace BrasilFit.API.DTOs.Alimentos;

public class AlimentoExternoDto
{
    public string? CodigoBarras { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public decimal? Kcal { get; set; }
    public decimal? Carboidratos { get; set; }
    public decimal? Proteinas { get; set; }
    public decimal? Lipidios { get; set; }

    public string? ImagemUrl { get; set; }
}
