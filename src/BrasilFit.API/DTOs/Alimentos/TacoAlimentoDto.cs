namespace BrasilFit.API.DTOs.Alimentos;

public class TacoAlimentoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal? Kcal { get; set; }
    public decimal? Proteinas { get; set; }
    public decimal? Carboidratos { get; set; }
    public decimal? Lipidios { get; set; }
    public decimal? Fibras { get; set; }
    public string? Categoria { get; set; }
}
