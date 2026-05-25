using BrasilFit.Domain.Enums;

namespace BrasilFit.Domain.Entities;

public class Paciente : Usuario
{
    public Paciente()
    {
        Papel = PapelUsuario.Paciente;
    }

    public DateTime DataNascimento { get; set; }
    public Sexo Sexo { get; set; }
    public string? Telefone { get; set; }
    public string? HistoricoClinico { get; set; }

    // Nutricionista responsavel (relacionamento N:1).
    public int? NutricionistaId { get; set; }
    public Nutricionista? Nutricionista { get; set; }

    // Endereco 1:1 (paciente possui um endereco principal).
    public Endereco? Endereco { get; set; }

    public ICollection<PlanoAlimentar> PlanosAlimentares { get; set; } = new List<PlanoAlimentar>();
    public ICollection<AvaliacaoAntropometrica> Avaliacoes { get; set; } = new List<AvaliacaoAntropometrica>();
    public ICollection<Meta> Metas { get; set; } = new List<Meta>();
}
