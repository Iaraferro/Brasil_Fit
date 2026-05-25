namespace BrasilFit.Domain.Entities;

public class AvaliacaoAntropometrica
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string? ObservacoesClinicas { get; set; }

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public int NutricionistaId { get; set; }
    public Nutricionista Nutricionista { get; set; } = null!;

    // 1:1 - cada avaliacao tem uma medida corporal e uma composicao corporal.
    public MedidaCorporal? MedidaCorporal { get; set; }
    public ComposicaoCorporal? ComposicaoCorporal { get; set; }
}
