using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// Modelo para representar um registro de consumo de água
/// </summary>
[Table("registros_agua")]
public class RegistroAqua
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("Usuario")]
    public int IdUsuario { get; set; }
    
    [Required]
    [ForeignKey("Tenant")]
    public int IdTenant { get; set; }
    
    [Required(ErrorMessage = "Consumo em litros é obrigatório")]
    [Range(0.1, double.MaxValue, ErrorMessage = "Consumo deve ser maior que 0")]
    public double ConsumoLitros { get; set; }
    
    [Required]
    public DateTime Data { get; set; } = DateTime.UtcNow;
    
    [StringLength(500)]
    public string? Observacoes { get; set; }
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    // Relacionamentos
    public virtual Usuario? Usuario { get; set; }
    public virtual Tenant? Tenant { get; set; }
}
