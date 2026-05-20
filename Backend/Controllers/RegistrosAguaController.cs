using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/registros-agua")]
[Authorize]
public class RegistrosAguaController : ControllerBase
{
    private readonly AppDbContext _contexto;

    public RegistrosAguaController(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    private int ObterIdUsuario()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idStr ?? "0");
    }

    [HttpGet]
    public async Task<IActionResult> ListarRegistros([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var idUsuario = ObterIdUsuario();

        var registros = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario)
            .OrderByDescending(r => r.Data)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(r => new DtoRespostaRegistroAqua
            {
                Id = r.Id,
                IdUsuario = r.IdUsuario,
                ConsumoLitros = r.ConsumoLitros,
                Data = r.Data,
                Observacoes = r.Observacoes,
                DataCriacao = r.DataCriacao
            })
            .ToListAsync();

        return Ok(registros);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterRegistro(int id)
    {
        var idUsuario = ObterIdUsuario();

        var registro = await _contexto.RegistrosAgua
            .FirstOrDefaultAsync(r => r.Id == id && r.IdUsuario == idUsuario);

        if (registro == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Registro não encontrado" });

        return Ok(new DtoRespostaRegistroAqua
        {
            Id = registro.Id,
            IdUsuario = registro.IdUsuario,
            ConsumoLitros = registro.ConsumoLitros,
            Data = registro.Data,
            Observacoes = registro.Observacoes,
            DataCriacao = registro.DataCriacao
        });
    }

    [HttpPost]
    public async Task<IActionResult> CriarRegistro([FromBody] DtoCriarRegistroAqua dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos" });

        var idUsuario = ObterIdUsuario();

        var registro = new RegistroAqua
        {
            IdUsuario = idUsuario,
            ConsumoLitros = dto.ConsumoLitros,
            Data = dto.Data ?? DateTime.UtcNow,
            Observacoes = dto.Observacoes
        };

        _contexto.RegistrosAgua.Add(registro);
        await _contexto.SaveChangesAsync();

        var resposta = new DtoRespostaRegistroAqua
        {
            Id = registro.Id,
            IdUsuario = registro.IdUsuario,
            ConsumoLitros = registro.ConsumoLitros,
            Data = registro.Data,
            Observacoes = registro.Observacoes,
            DataCriacao = registro.DataCriacao
        };

        return CreatedAtAction(nameof(ObterRegistro), new { id = registro.Id }, resposta);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarRegistro(int id, [FromBody] DtoAtualizarRegistroAqua dto)
    {
        var idUsuario = ObterIdUsuario();

        var registro = await _contexto.RegistrosAgua
            .FirstOrDefaultAsync(r => r.Id == id && r.IdUsuario == idUsuario);

        if (registro == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Registro não encontrado" });

        if (dto.ConsumoLitros.HasValue)
            registro.ConsumoLitros = dto.ConsumoLitros.Value;

        if (dto.Data.HasValue)
            registro.Data = dto.Data.Value;

        if (!string.IsNullOrEmpty(dto.Observacoes))
            registro.Observacoes = dto.Observacoes;

        _contexto.RegistrosAgua.Update(registro);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaSucesso { Mensagem = "Registro atualizado com sucesso" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarRegistro(int id)
    {
        var idUsuario = ObterIdUsuario();

        var registro = await _contexto.RegistrosAgua
            .FirstOrDefaultAsync(r => r.Id == id && r.IdUsuario == idUsuario);

        if (registro == null)
            return NotFound(new DtoRespostaErro { Mensagem = "Registro não encontrado" });

        _contexto.RegistrosAgua.Remove(registro);
        await _contexto.SaveChangesAsync();

        return Ok(new DtoRespostaSucesso { Mensagem = "Registro deletado com sucesso" });
    }
}
