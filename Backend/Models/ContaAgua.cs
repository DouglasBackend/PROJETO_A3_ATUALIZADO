using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("contas_agua")]
public class ContaAgua
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Usuario")]
    public int IdUsuario { get; set; }

    [Required]
    public DateTime MesReferencia { get; set; }

    [Required]
    [Range(0.1, double.MaxValue)]
    public double LitrosConsumidos { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public double ValorPago { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public virtual Usuario? Usuario { get; set; }
}
