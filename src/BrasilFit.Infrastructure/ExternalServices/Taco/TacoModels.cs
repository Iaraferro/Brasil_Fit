using System.Text.Json.Serialization;

namespace BrasilFit.Infrastructure.ExternalServices.Taco;

public class TacoAlimento
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("energy_kcal")]
    public decimal? Kcal { get; set; }

    [JsonPropertyName("protein")]
    public decimal? Proteinas { get; set; }

    [JsonPropertyName("carbohydrate")]
    public decimal? Carboidratos { get; set; }

    [JsonPropertyName("lipids")]
    public decimal? Lipidios { get; set; }

    [JsonPropertyName("dietary_fiber")]
    public decimal? FibrasDieteticas { get; set; }

    [JsonPropertyName("category")]
    public TacoCategory? Categoria { get; set; }
}

public class TacoCategory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("description")]
    public string Nome { get; set; } = string.Empty;
}
