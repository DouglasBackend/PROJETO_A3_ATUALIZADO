using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

/// <summary>
/// DTO para criação de registro de água
/// </summary>
public class DtoCriarRegistroAqua
{
    [Required(ErrorMessage = "Consumo em litros é obrigatório")]
    [Range(0.1, double.MaxValue, ErrorMessage = "Consumo deve ser maior que 0")]
    public double ConsumoLitros { get; set; }

    public DateTime? Data { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para atualizar registro de água
/// </summary>
public class DtoAtualizarRegistroAqua
{
    [Range(0.1, double.MaxValue, ErrorMessage = "Consumo deve ser maior que 0")]
    public double? ConsumoLitros { get; set; }

    public DateTime? Data { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

/// <summary>
/// DTO para resposta de registro de água
/// </summary>
public class DtoRespostaRegistroAqua
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public double ConsumoLitros { get; set; }
    public DateTime Data { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
}

/// <summary>
/// DTO para resposta de resumo de registros
/// </summary>
public class DtoResumoCheio
{
    public int TotalRegistros { get; set; }
    public double ConsumoTotal { get; set; }
    public double ConsumoMedio { get; set; }
    public double ConsumoDiaAtual { get; set; }
    public double ConsumoMesAtual { get; set; }
    public List<DtoRespostaRegistroAqua> RegistrosRecentes { get; set; } = new();
}
