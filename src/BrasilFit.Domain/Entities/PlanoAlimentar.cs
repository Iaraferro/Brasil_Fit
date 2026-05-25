namespace BrasilFit.Domain.Entities;

public class PlanoAlimentar
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Objetivo { get; set; }
    public DateTime DataInicio { get; set; }
    public int DuracaoDias { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public int NutricionistaId { get; set; }
    public Nutricionista Nutricionista { get; set; } = null!;

    // 1:N - um plano contem varias refeicoes.
    public ICollection<Refeicao> Refeicoes { get; set; } = new List<Refeicao>();
}
