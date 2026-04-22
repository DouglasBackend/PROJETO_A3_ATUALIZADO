using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Controller para gerenciar registros de consumo de água
/// </summary>
[ApiController]
[Route("api/registros-agua")]
[Authorize]
public class RegistrosAguaController : ControllerBase
{
    private readonly AppDbContext _contexto;
    private readonly IProvedorTenant _provedorTenant;

    public RegistrosAguaController(AppDbContext contexto, IProvedorTenant provedorTenant)
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
    /// Lista todos os registros de água do usuário autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarRegistros([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var registros = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant)
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
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao listar registros",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém um registro específico de água pelo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterRegistro(int id)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var registro = await _contexto.RegistrosAgua
                .FirstOrDefaultAsync(r => r.Id == id && r.IdUsuario == idUsuario && r.IdTenant == idTenant);

            if (registro == null)
                return NotFound(new DtoRespostaErro { Mensagem = "Registro não encontrado" });

            var dto = new DtoRespostaRegistroAqua
            {
                Id = registro.Id,
                IdUsuario = registro.IdUsuario,
                ConsumoLitros = registro.ConsumoLitros,
                Data = registro.Data,
                Observacoes = registro.Observacoes,
                DataCriacao = registro.DataCriacao
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter registro",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Cria um novo registro de consumo de água
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarRegistro([FromBody] DtoCriarRegistroAqua dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new DtoRespostaErro { Mensagem = "Dados inválidos" });

            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            // Verificar se usuário existe
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Id == idUsuario && u.IdTenant == idTenant);

            if (usuario == null)
                return NotFound(new DtoRespostaErro { Mensagem = "Usuário não encontrado" });

            var registro = new RegistroAqua
            {
                IdUsuario = idUsuario,
                IdTenant = idTenant,
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
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao criar registro",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Atualiza um registro de consumo de água
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarRegistro(int id, [FromBody] DtoAtualizarRegistroAqua dto)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var registro = await _contexto.RegistrosAgua
                .FirstOrDefaultAsync(r => r.Id == id && r.IdUsuario == idUsuario && r.IdTenant == idTenant);

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
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao atualizar registro",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Deleta um registro de consumo de água
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletarRegistro(int id)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var registro = await _contexto.RegistrosAgua
                .FirstOrDefaultAsync(r => r.Id == id && r.IdUsuario == idUsuario && r.IdTenant == idTenant);

            if (registro == null)
                return NotFound(new DtoRespostaErro { Mensagem = "Registro não encontrado" });

            _contexto.RegistrosAgua.Remove(registro);
            await _contexto.SaveChangesAsync();

            return Ok(new DtoRespostaSucesso { Mensagem = "Registro deletado com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao deletar registro",
                Detalhes = ex.Message
            });
        }
    }
}
