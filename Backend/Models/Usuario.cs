using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public virtual ICollection<RegistroAqua> RegistrosAgua { get; set; } = new List<RegistroAqua>();
    public virtual ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    public virtual ICollection<ContaAgua> ContasAgua { get; set; } = new List<ContaAgua>();
}
