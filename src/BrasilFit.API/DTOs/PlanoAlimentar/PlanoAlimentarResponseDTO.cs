using BrasilFit.Domain.Enums;

namespace BrasilFit.API.DTOs.PlanoAlimentar
{
    public class PlanoAlimentarResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Objetivo { get; set; }
        public DateTime DataInicio { get; set; }
        public int DuracaoDias { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
        public int PacienteId { get; set; }
        public string PacienteNome { get; set; } = string.Empty;
        public List<RefeicaoResponseDto> Refeicoes { get; set; } = new();
    }

    public class RefeicaoResponseDto
    {
        public int Id { get; set; }
        public TipoRefeicao Tipo { get; set; }
        public TimeOnly Horario { get; set; }
        public string? Observacoes { get; set; }
        public List<ItemRefeicaoResponseDto> Itens { get; set; } = new();
    }

    public class ItemRefeicaoResponseDto
    {
        public int Id { get; set; }
        public int AlimentoId { get; set; }
        public string AlimentoNome { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public string Unidade { get; set; } = string.Empty;
    }
}
