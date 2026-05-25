using BrasilFit.Domain.Enums;

namespace BrasilFit.Domain.Entities;

public class Nutricionista : Usuario
{
    public Nutricionista()
    {
        Papel = PapelUsuario.Nutricionista;
    }

    public string Crn { get; set; } = string.Empty;
    public string? Especialidade { get; set; }

    public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
    public ICollection<PlanoAlimentar> PlanosAlimentares { get; set; } = new List<PlanoAlimentar>();
    public ICollection<AvaliacaoAntropometrica> Avaliacoes { get; set; } = new List<AvaliacaoAntropometrica>();
}
