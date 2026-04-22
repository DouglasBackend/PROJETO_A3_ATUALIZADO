using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

/// <summary>
/// DTO para registro de novo usuário
/// </summary>
public class DtoRegistro
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Identificador do tenant é obrigatório")]
    public string IdentificadorTenant { get; set; } = string.Empty;
}

/// <summary>
/// DTO para login
/// </summary>
public class DtoLogin
{
    [Required(ErrorMessage = "Email é obrigatório")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Identificador do tenant é obrigatório")]
    public string IdentificadorTenant { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de autenticação
/// </summary>
public class DtoRespostaAutenticacao
{
    public string Token { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public int IdTenant { get; set; }
}

/// <summary>
/// DTO para resposta de erro
/// </summary>
public class DtoRespostaErro
{
    public string Mensagem { get; set; } = string.Empty;
    public string? Detalhes { get; set; }
}

/// <summary>
/// DTO para resposta de sucesso
/// </summary>
public class DtoRespostaSucesso
{
    public string Mensagem { get; set; } = string.Empty;
}
