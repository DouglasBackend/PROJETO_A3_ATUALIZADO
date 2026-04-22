using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

/// <summary>
/// Modelo para representar um inquilino (Tenant) no sistema
/// </summary>
public class Tenant
{
    public int Id { get; set; }
    
    [Required]
    public string Identificador { get; set; } = string.Empty;
    
    [Required]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    public string Descricao { get; set; } = string.Empty;
    
    [Required]
    public string ConexaoBancoDados { get; set; } = string.Empty;
    
    public bool Ativo { get; set; } = true;
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataAtualizacao { get; set; }
    
    // Relacionamentos
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
