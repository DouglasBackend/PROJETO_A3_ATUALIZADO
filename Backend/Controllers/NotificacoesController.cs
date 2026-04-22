using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Controller para gerenciar notificações dos usuários
/// </summary>
[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly AppDbContext _contexto;
    private readonly IProvedorTenant _provedorTenant;

    public NotificacoesController(AppDbContext contexto, IProvedorTenant provedorTenant)
    {
        _contexto = contexto;
        _provedorTenant = provedorTenant;
    }

    /// <summary>
    /// Obtém o ID do usuário autenticado a partir do token JWT
    /// </summary>
    private int ObterIdUsuario()
    {
        var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idUsuarioStr ?? "0");
    }

    /// <summary>
    /// Obtém o ID do tenant a partir do token JWT
    /// </summary>
    private int ObterIdTenant()
    {
        var idTenantStr = User.FindFirst("TenantId")?.Value;
        return int.Parse(idTenantStr ?? "0");
    }

    /// <summary>
    /// Lista todas as notificações do usuário autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarNotificacoes([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var dataLimite = DateTime.UtcNow.AddDays(-2);

            var notificacoes = await _contexto.Notificacoes
                .Where(n => n.IdUsuario == idUsuario && n.IdTenant == idTenant)
                .Where(n => !n.Lida || (n.Lida && n.DataLeitura >= dataLimite))
                .OrderByDescending(n => n.DataCriacao)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(n => new DtoRespostaNotificacao
                {
                    Id = n.Id,
                    Titulo = n.Titulo,
                    Mensagem = n.Mensagem,
                    Lida = n.Lida,
                    Tipo = n.Tipo,
                    DataCriacao = n.DataCriacao,
                    DataLeitura = n.DataLeitura
                })
                .ToListAsync();

            return Ok(notificacoes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao listar notificações",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém uma notificação específica pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterNotificacao(int id)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var notificacao = await _contexto.Notificacoes
                .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario && n.IdTenant == idTenant);

            if (notificacao == null)
                return NotFound(new DtoRespostaErro { Mensagem = "Notificação não encontrada" });

            var dto = new DtoRespostaNotificacao
            {
                Id = notificacao.Id,
                Titulo = notificacao.Titulo,
                Mensagem = notificacao.Mensagem,
                Lida = notificacao.Lida,
                Tipo = notificacao.Tipo,
                DataCriacao = notificacao.DataCriacao,
                DataLeitura = notificacao.DataLeitura
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter notificação",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Marca uma notificação como lida
    /// </summary>
    [HttpPost("{id}/marcar-como-lida")]
    public async Task<IActionResult> MarcarComoLida(int id)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var notificacao = await _contexto.Notificacoes
                .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario && n.IdTenant == idTenant);

            if (notificacao == null)
                return NotFound(new DtoRespostaErro { Mensagem = "Notificação não encontrada" });

            notificacao.Lida = true;
            notificacao.DataLeitura = DateTime.UtcNow;

            _contexto.Notificacoes.Update(notificacao);
            await _contexto.SaveChangesAsync();

            return Ok(new DtoRespostaSucesso { Mensagem = "Notificação marcada como lida" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao marcar notificação como lida",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Marca todas as notificações como lidas
    /// </summary>
    [HttpPost("marcar-todas-como-lidas")]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var notificacoes = await _contexto.Notificacoes
                .Where(n => n.IdUsuario == idUsuario && n.IdTenant == idTenant && !n.Lida)
                .ToListAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
                notificacao.DataLeitura = DateTime.UtcNow;
            }

            _contexto.Notificacoes.UpdateRange(notificacoes);
            await _contexto.SaveChangesAsync();

            return Ok(new DtoRespostaSucesso { Mensagem = "Todas as notificações marcadas como lidas" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao marcar notificações como lidas",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Deleta uma notificação
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarNotificacao(int id)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var notificacao = await _contexto.Notificacoes
                .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario && n.IdTenant == idTenant);

            if (notificacao == null)
                return NotFound(new DtoRespostaErro { Mensagem = "Notificação não encontrada" });

            _contexto.Notificacoes.Remove(notificacao);
            await _contexto.SaveChangesAsync();

            return Ok(new DtoRespostaSucesso { Mensagem = "Notificação deletada com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao deletar notificação",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém contagem de notificações não lidas
    /// </summary>
    [HttpGet("nao-lidas/contar")]
    public async Task<IActionResult> ContarNaoLidas()
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var contar = await _contexto.Notificacoes
                .CountAsync(n => n.IdUsuario == idUsuario && n.IdTenant == idTenant && !n.Lida);

            return Ok(new { contar });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao contar notificações",
                Detalhes = ex.Message
            });
        }
    }
}
