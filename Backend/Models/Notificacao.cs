using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// Modelo para representar uma notificação do sistema
/// </summary>
[Table("notificacoes")]
public class Notificacao
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("Usuario")]
    public int IdUsuario { get; set; }
    
    [Required]
    [ForeignKey("Tenant")]
    public int IdTenant { get; set; }
    
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "Título não pode exceder 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mensagem é obrigatória")]
    [StringLength(1000, ErrorMessage = "Mensagem não pode exceder 1000 caracteres")]
    public string Mensagem { get; set; } = string.Empty;
    
    public bool Lida { get; set; } = false;
    
    [StringLength(50)]
    public string? Tipo { get; set; } // ex: "ALERTA", "INFO", "SUCESSO"
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataLeitura { get; set; }
    
    // Relacionamentos
    public virtual Usuario? Usuario { get; set; }
    public virtual Tenant? Tenant { get; set; }
}
