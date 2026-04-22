using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

/// <summary>
/// DTO para criação de notificação
/// </summary>
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

/// <summary>
/// DTO para resposta de notificação
/// </summary>
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

/// <summary>
/// DTO para marcar notificação como lida
/// </summary>
public class DtoMarcarComoLido
{
    public int IdNotificacao { get; set; }
}
