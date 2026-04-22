using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// Modelo para representar um usuário do sistema
/// </summary>
[Table("usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("Tenant")]
    public int IdTenant { get; set; }
    
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string SenhaHash { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Telefone { get; set; }
    
    public bool Ativo { get; set; } = true;
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataAtualizacao { get; set; }
    
    // Relacionamentos
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<RegistroAqua> RegistrosAgua { get; set; } = new List<RegistroAqua>();
    public virtual ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
}
