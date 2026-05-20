using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class DtoCriarRegistroAqua
{
    [Required]
    [Range(0.1, double.MaxValue)]
    public double ConsumoLitros { get; set; }

    public DateTime? Data { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

public class DtoAtualizarRegistroAqua
{
    [Range(0.1, double.MaxValue)]
    public double? ConsumoLitros { get; set; }

    public DateTime? Data { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

public class DtoRespostaRegistroAqua
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public double ConsumoLitros { get; set; }
    public DateTime Data { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class DtoResumoCheio
{
    public int TotalRegistros { get; set; }
    public double ConsumoTotal { get; set; }
    public double ConsumoMedio { get; set; }
    public double ConsumoDiaAtual { get; set; }
    public double ConsumoMesAtual { get; set; }
    public List<DtoRespostaRegistroAqua> RegistrosRecentes { get; set; } = new();
}
