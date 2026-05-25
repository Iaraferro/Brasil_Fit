using System.ComponentModel.DataAnnotations;
using BrasilFit.Domain.Enums;

namespace BrasilFit.API.DTOs.Pacientes;

public class CriarPacienteDto
{
    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public DateTime DataNascimento { get; set; }

    [Required]
    public Sexo Sexo { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(2000)]
    public string? HistoricoClinico { get; set; }

    public EnderecoDto? Endereco { get; set; }
}

public class EnderecoDto
{
    [Required, MaxLength(9)]   public string Cep { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Logradouro { get; set; } = string.Empty;
    [Required, MaxLength(20)]  public string Numero { get; set; } = string.Empty;
    [MaxLength(100)]           public string? Complemento { get; set; }
    [Required, MaxLength(100)] public string Bairro { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Cidade { get; set; } = string.Empty;
    [Required, MaxLength(2)]   public string Uf { get; set; } = string.Empty;
}
