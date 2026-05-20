using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class DtoCriarNotificacao
{
    [Required]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Mensagem { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Tipo { get; set; }
}

public class DtoRespostaNotificacao
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public bool Lida { get; set; }
    public string? Tipo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataLeitura { get; set; }
}

public class DtoMarcarComoLido
{
    public int IdNotificacao { get; set; }
}
