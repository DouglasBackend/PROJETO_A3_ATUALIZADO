using Backend.Data;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly AppDbContext _contexto;

    public NotificacoesController(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    private int ObterIdUsuario()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idStr ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> ListarNotificacoes([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var idUsuario = ObterIdUsuario();
        var dataLimite = DateTime.UtcNow.AddDays(-2);

        var notificacoes = await _contexto.Notificacoes
            .Where(n => n.IdUsuario == idUsuario)
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

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterNotificacao(int id)
    {
        var idUsuario = ObterIdUsuario();

        var notificacao = await _contexto.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario);

        if (notificacao == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Notificação não encontrada" });

        return Ok(new DtoRespostaNotificacao
        {
            Id = notificacao.Id,
            Titulo = notificacao.Titulo,
            Mensagem = notificacao.Mensagem,
            Lida = notificacao.Lida,
            Tipo = notificacao.Tipo,
            DataCriacao = notificacao.DataCriacao,
            DataLeitura = notificacao.DataLeitura
        });
    }

    [HttpPost("{id}/marcar-como-lida")]
    public async Task<IActionResult> MarcarComoLida(int id)
    {
        var idUsuario = ObterIdUsuario();

        var notificacao = await _contexto.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario);

        if (notificacao == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Notificação não encontrada" });

        notificacao.Lida = true;
        notificacao.DataLeitura = DateTime.UtcNow;

        _contexto.Notificacoes.Update(notificacao);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaSucesso { Mensagem = "Notificação marcada como lida" });
    }

    [HttpPost("marcar-todas-como-lidas")]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        var idUsuario = ObterIdUsuario();

        var notificacoes = await _contexto.Notificacoes
            .Where(n => n.IdUsuario == idUsuario && !n.Lida)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarNotificacao(int id)
    {
        var idUsuario = ObterIdUsuario();

        var notificacao = await _contexto.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == idUsuario);

        if (notificacao == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Notificação não encontrada" });

        _contexto.Notificacoes.Remove(notificacao);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaSucesso { Mensagem = "Notificação deletada com sucesso" });
    }

    [HttpGet("nao-lidas/contar")]
    public async Task<IActionResult> ContarNaoLidas()
    {
        var idUsuario = ObterIdUsuario();
        var contar = await _contexto.Notificacoes.CountAsync(n => n.IdUsuario == idUsuario && !n.Lida);
        return Ok(new { contar });
    }
}
