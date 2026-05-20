namespace Backend.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}
