using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class DtoCriarContaAgua
{
    [Required]
    public DateTime MesReferencia { get; set; }

    [Required]
    [Range(0.1, double.MaxValue)]
    public double LitrosConsumidos { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public double ValorPago { get; set; }
}

public class DtoRespostaContaAgua
{
    public int Id { get; set; }
    public DateTime MesReferencia { get; set; }
    public double LitrosConsumidos { get; set; }
    public double ValorPago { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class DtoEstimativa
{
    public double PrecoMedioPorLitro { get; set; }
    public double ConsumoMesAtual { get; set; }
    public double EstimativaMesAtual { get; set; }
    public double ProjecaoProximoMes { get; set; }
    public List<DtoRespostaContaAgua> HistoricoCustos { get; set; } = new();
}
