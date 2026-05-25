using BrasilFit.Domain.Enums;

namespace BrasilFit.API.DTOs.Pacientes;

public class PacienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public Sexo Sexo { get; set; }
    public string? Telefone { get; set; }
    public bool Ativo { get; set; }
}
