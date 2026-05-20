using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly AppDbContext _contexto;

    public AutenticacaoController(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] DtoRegistro dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos" });

        if (await _contexto.Usuarios.AnyAsync())
            return BadRequest(new DtoRespostaErro { Mensagem = "Já existe um usuário cadastrado no sistema" });

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        _contexto.Usuarios.Add(usuario);
        await _contexto.SaveChangesAsync();

        var notificacao = new Notificacao
        {
            IdUsuario = usuario.Id,
            Titulo = "Bem-vindo ao AquaMonitor!",
            Mensagem = "Obrigado por se cadastrar. Comece a monitorar seu consumo de água hoje.",
            Tipo = "INFO"
        };

        _contexto.Notificacoes.Add(notificacao);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaSucesso { Mensagem = "Usuário registrado com sucesso" });
    }

    [HttpPost("entrar")]
    public async Task<IActionResult> Entrar([FromBody] DtoLogin dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos" });

        var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return Unauthorized(new DtoRespostaErro { Mensagem = "Email ou senha inválidos" });

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email)
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidade);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new DtoRespostaAutenticacao
        {
            Nome = usuario.Nome,
            Email = usuario.Email,
            IdUsuario = usuario.Id
        });
    }

    [HttpPost("sair")]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new DtoRespostaSucesso { Mensagem = "Logout realizado com sucesso" });
    }
}
