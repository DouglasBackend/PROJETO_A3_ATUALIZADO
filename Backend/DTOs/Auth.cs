using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class DtoRegistro
{
    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}

public class DtoLogin
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public class DtoRespostaAutenticacao
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
}

public class DtoRespostaErro
{
    public string Mensagem { get; set; } = string.Empty;
    public string? Detalhes { get; set; }
}

public class DtoRespostaSucesso
{
    public string Mensagem { get; set; } = string.Empty;
}
