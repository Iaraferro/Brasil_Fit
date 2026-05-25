using System.Text.Json.Serialization;

namespace BrasilFit.Infrastructure.ExternalServices.ViaCep;

// Espelha o payload retornado pela API publica ViaCEP.
public class ViaCepResponse
{
    [JsonPropertyName("cep")]        public string? Cep { get; set; }
    [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
    [JsonPropertyName("complemento")]public string? Complemento { get; set; }
    [JsonPropertyName("bairro")]     public string? Bairro { get; set; }
    [JsonPropertyName("localidade")] public string? Localidade { get; set; }
    [JsonPropertyName("uf")]         public string? Uf { get; set; }
    [JsonPropertyName("erro")]       public bool? Erro { get; set; }
}
