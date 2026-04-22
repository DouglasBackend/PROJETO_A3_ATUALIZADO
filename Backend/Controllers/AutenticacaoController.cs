using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace Backend.Controllers;

/// <summary>
/// Controller para gerenciar autenticação e autorização de usuários
/// </summary>
[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly AppDbContext _contexto;
    private readonly IConfiguration _configuracao;
    private readonly IProvedorTenant _provedorTenant;

    public AutenticacaoController(AppDbContext contexto, IConfiguration configuracao, IProvedorTenant provedorTenant)
    {
        _contexto = contexto;
        _configuracao = configuracao;
        _provedorTenant = provedorTenant;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] DtoRegistro dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos", Detalhes = "Verifique os valores fornecidos" });

            // Obter ou criar tenant
            var tenant = await _contexto.Tenants
                .FirstOrDefaultAsync(t => t.Identificador == dto.IdentificadorTenant);

            if (tenant == null)
            {
                tenant = new Tenant
                {
                    Identificador = dto.IdentificadorTenant,
                    Nome = dto.IdentificadorTenant,
                    Descricao = $"Tenant para {dto.Nome}",
                    ConexaoBancoDados = "DefaultConnection",
                    Ativo = true
                };

                _contexto.Tenants.Add(tenant);
                await _contexto.SaveChangesAsync();
            }

            _provedorTenant.DefinirIdTenantAtual(tenant.Id);

            // Verificar se email já existe para este tenant
            if (await _contexto.Usuarios.AnyAsync(u => u.Email == dto.Email && u.IdTenant == tenant.Id))
                return BadRequest(new DtoRespostaErro { Mensagem = "Email já cadastrado neste tenant" });

            // Criar novo usuário
            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                IdTenant = tenant.Id,
                Ativo = true
            };

            _contexto.Usuarios.Add(usuario);
            await _contexto.SaveChangesAsync();

            // Criar notificação de boas-vindas
            var notificacao = new Notificacao
            {
                IdUsuario = usuario.Id,
                IdTenant = tenant.Id,
                Titulo = "Bem-vindo ao AquaMonitor!",
                Mensagem = "Obrigado por se cadastrar. Comece a monitorar seu consumo de água hoje.",
                Tipo = "INFO"
            };

            _contexto.Notificacoes.Add(notificacao);
            await _contexto.SaveChangesAsync();

            return Ok(new DtoRespostaSucesso { Mensagem = "Usuário registrado com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao registrar usuário",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Autentica um usuário e retorna token JWT
    /// </summary>
    [HttpPost("entrar")]
    public async Task<IActionResult> Entrar([FromBody] DtoLogin dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos" });

            // Obter tenant
            var tenant = await _contexto.Tenants
                .FirstOrDefaultAsync(t => t.Identificador == dto.IdentificadorTenant);

            if (tenant == null)
                return Unauthorized(new DtoRespostaErro { Mensagem = "Tenant não encontrado" });

            _provedorTenant.DefinirIdTenantAtual(tenant.Id);

            // Procurar usuário
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IdTenant == tenant.Id);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
                return Unauthorized(new DtoRespostaErro { Mensagem = "Email ou senha inválidos" });

            if (!usuario.Ativo)
                return Unauthorized(new DtoRespostaErro { Mensagem = "Usuário inativo" });

            // Gerar token JWT
            var token = GerarToken(usuario, tenant);

            return Ok(new DtoRespostaAutenticacao
            {
                Token = token,
                Nome = usuario.Nome!,
                Email = usuario.Email!,
                IdUsuario = usuario.Id,
                IdTenant = tenant.Id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao autenticar",
                Detalhes = ex.Message
            });
        }
    }

    private string GerarToken(Usuario usuario, Tenant tenant)
    {
        var chaveSecreta = _configuracao["ChaveJwt"] ?? "ChaveSecretaPadraoPorFavorMudeNaProducao";
        var bytesChave = Encoding.ASCII.GetBytes(chaveSecreta);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("TenantId", tenant.Id.ToString()),
                new Claim("TenantIdentificador", tenant.Identificador)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(bytesChave), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
