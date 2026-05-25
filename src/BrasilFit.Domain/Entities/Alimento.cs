namespace BrasilFit.Domain.Entities;

public class Alimento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // Valores nutricionais por 100g/100ml de referencia.
    public decimal Kcal { get; set; }
    public decimal Carboidratos { get; set; }
    public decimal Proteinas { get; set; }
    public decimal Lipidios { get; set; }

    // Codigo de barras do OpenFoodFacts (quando importado).
    public string? CodigoBarrasExterno { get; set; }
    public string? Marca { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<ItemRefeicao> ItensRefeicao { get; set; } = new List<ItemRefeicao>();
}
