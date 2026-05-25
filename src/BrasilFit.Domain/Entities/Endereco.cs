namespace BrasilFit.Domain.Entities;

public class Endereco
{
    public int Id { get; set; }
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;

    // 1:1 com Paciente.
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
}
