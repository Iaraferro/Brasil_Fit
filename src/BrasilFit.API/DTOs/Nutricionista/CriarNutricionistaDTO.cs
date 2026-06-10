namespace BrasilFit.API.DTOs.Nutricionista
{
    public class CriarNutricionistaDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Crn { get; set; } = string.Empty;
        public string? Especialidade { get; set; }
    }
}
