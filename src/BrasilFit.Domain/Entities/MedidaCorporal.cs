namespace BrasilFit.Domain.Entities;

// Medidas brutas coletadas em uma avaliacao.
// Strings simples (CircunferenciasJson/DobrasJson) guardam mapas chave-valor serializados
// para evitar tabelas auxiliares para cada local de medida.
public class MedidaCorporal
{
    public int Id { get; set; }
    public decimal Peso { get; set; }       // kg
    public decimal Altura { get; set; }     // m
    public string? CircunferenciasJson { get; set; } // { "cintura": 80, "quadril": 95, ... }
    public string? DobrasCutaneasJson { get; set; }  // { "triciptal": 12, "subescapular": 18, ... }

    public int AvaliacaoAntropometricaId { get; set; }
    public AvaliacaoAntropometrica AvaliacaoAntropometrica { get; set; } = null!;
}
