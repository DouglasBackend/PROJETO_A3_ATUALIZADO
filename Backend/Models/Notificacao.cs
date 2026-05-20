using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("notificacoes")]
public class Notificacao
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Mensagem { get; set; } = string.Empty;

    public bool Lida { get; set; } = false;

    [StringLength(50)]
    public string? Tipo { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataLeitura { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
