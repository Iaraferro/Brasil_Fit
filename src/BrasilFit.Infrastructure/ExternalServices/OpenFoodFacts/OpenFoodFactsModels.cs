using System.Text.Json.Serialization;

namespace BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;

// Modelos minimos para deserializar o que precisamos da API do OpenFoodFacts.
// A resposta real e bem maior - mapeamos apenas os campos uteis.

public class OffProductResponse
{
    [JsonPropertyName("status")] public int Status { get; set; }
    [JsonPropertyName("code")]   public string? Code { get; set; }
    [JsonPropertyName("product")] public OffProduct? Product { get; set; }
}

public class OffSearchResponse
{
    [JsonPropertyName("count")]    public int Count { get; set; }
    [JsonPropertyName("page")]     public int Page { get; set; }
    [JsonPropertyName("page_size")] public int PageSize { get; set; }
    [JsonPropertyName("products")] public List<OffProduct> Products { get; set; } = new();
}

public class OffProduct
{
    [JsonPropertyName("code")]            public string? Code { get; set; }
    [JsonPropertyName("product_name")]    public string? ProductName { get; set; }
    [JsonPropertyName("product_name_pt")] public string? ProductNamePt { get; set; }
    [JsonPropertyName("brands")]          public string? Brands { get; set; }
    [JsonPropertyName("nutriments")]      public OffNutriments? Nutriments { get; set; }
}

public class OffNutriments
{
    // O OpenFoodFacts ja entrega valores por 100g nessas chaves.
    [JsonPropertyName("energy-kcal_100g")] public decimal? EnergyKcal100g { get; set; }
    [JsonPropertyName("carbohydrates_100g")] public decimal? Carbohydrates100g { get; set; }
    [JsonPropertyName("proteins_100g")] public decimal? Proteins100g { get; set; }
    [JsonPropertyName("fat_100g")] public decimal? Fat100g { get; set; }
}
