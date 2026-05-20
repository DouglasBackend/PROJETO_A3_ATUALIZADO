using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/contas-agua")]
[Authorize]
public class ContasAguaController : ControllerBase
{
    private readonly AppDbContext _contexto;

    public ContasAguaController(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    private int ObterIdUsuario()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idStr ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> ListarContas()
    {
        var idUsuario = ObterIdUsuario();

        var contas = await _contexto.ContasAgua
            .Where(c => c.IdUsuario == idUsuario)
            .OrderByDescending(c => c.MesReferencia)
            .Select(c => new DtoRespostaContaAgua
            {
                Id = c.Id,
                MesReferencia = c.MesReferencia,
                LitrosConsumidos = c.LitrosConsumidos,
                ValorPago = c.ValorPago,
                DataCriacao = c.DataCriacao
            })
            .ToListAsync();

        return Ok(contas);
    }

    [HttpPost]
    public async Task<IActionResult> CriarConta([FromBody] DtoCriarContaAgua dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos" });

        var idUsuario = ObterIdUsuario();

        var conta = new ContaAgua
        {
            IdUsuario = idUsuario,
            MesReferencia = new DateTime(dto.MesReferencia.Year, dto.MesReferencia.Month, 1),
            LitrosConsumidos = dto.LitrosConsumidos,
            ValorPago = dto.ValorPago
        };

        _contexto.ContasAgua.Add(conta);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaContaAgua
        {
            Id = conta.Id,
            MesReferencia = conta.MesReferencia,
            LitrosConsumidos = conta.LitrosConsumidos,
            ValorPago = conta.ValorPago,
            DataCriacao = conta.DataCriacao
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarConta(int id)
    {
        var idUsuario = ObterIdUsuario();

        var conta = await _contexto.ContasAgua
            .FirstOrDefaultAsync(c => c.Id == id && c.IdUsuario == idUsuario);

        if (conta == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Conta não encontrada" });

        _contexto.ContasAgua.Remove(conta);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaSucesso { Mensagem = "Conta removida com sucesso" });
    }
}
