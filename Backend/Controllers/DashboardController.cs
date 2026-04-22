using Backend.Data;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Controller para fornecer dados do dashboard
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _contexto;
    private readonly IProvedorTenant _provedorTenant;

    public DashboardController(AppDbContext contexto, IProvedorTenant provedorTenant)
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
    /// Obtém o resumo completo do consumo de água do usuário
    /// </summary>
    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo()
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var hoje = DateTime.UtcNow.Date;
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var registros = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant)
                .ToListAsync();

            var totalRegistros = registros.Count;
            var consumoTotal = registros.Sum(r => r.ConsumoLitros);
            var consumoMedio = totalRegistros > 0 ? consumoTotal / totalRegistros : 0;

            var consumoDiaAtual = registros
                .Where(r => r.Data.Date == hoje)
                .Sum(r => r.ConsumoLitros);

            var consumoMesAtual = registros
                .Where(r => r.Data >= inicioMes && r.Data < inicioMes.AddMonths(1))
                .Sum(r => r.ConsumoLitros);

            var registrosRecentes = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant)
                .OrderByDescending(r => r.Data)
                .Take(10)
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

            var resumo = new DtoResumoCheio
            {
                TotalRegistros = totalRegistros,
                ConsumoTotal = consumoTotal,
                ConsumoMedio = consumoMedio,
                ConsumoDiaAtual = consumoDiaAtual,
                ConsumoMesAtual = consumoMesAtual,
                RegistrosRecentes = registrosRecentes
            };

            return Ok(resumo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter resumo do dashboard",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém consumo diário por dia do mês
    /// </summary>
    [HttpGet("consumo-diario")]
    public async Task<IActionResult> ObterConsumoDiario([FromQuery] int mes = 0, [FromQuery] int ano = 0)
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            if (mes == 0) mes = DateTime.UtcNow.Month;
            if (ano == 0) ano = DateTime.UtcNow.Year;

            var inicioMes = new DateTime(ano, mes, 1);
            var fimMes = inicioMes.AddMonths(1);

            var registros = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant &&
                            r.Data >= inicioMes && r.Data < fimMes)
                .GroupBy(r => r.Data.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    ConsumoTotal = g.Sum(r => r.ConsumoLitros)
                })
                .OrderBy(r => r.Data)
                .ToListAsync();

            return Ok(registros);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter consumo diário",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém consumo semanal
    /// </summary>
    [HttpGet("consumo-semanal")]
    public async Task<IActionResult> ObterConsumoSemanal()
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var dataInicio = DateTime.UtcNow.AddDays(-7);

            var registros = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant &&
                            r.Data >= dataInicio)
                .GroupBy(r => r.Data.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    ConsumoTotal = g.Sum(r => r.ConsumoLitros)
                })
                .OrderBy(r => r.Data)
                .ToListAsync();

            return Ok(registros);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter consumo semanal",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém consumo mensal (últimos 12 meses)
    /// </summary>
    [HttpGet("consumo-mensal")]
    public async Task<IActionResult> ObterConsumoMensal()
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var dataInicio = DateTime.UtcNow.AddMonths(-12);

            var registros = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant &&
                            r.Data >= dataInicio)
                .GroupBy(r => new { r.Data.Year, r.Data.Month })
                .Select(g => new
                {
                    Mes = g.Key.Month,
                    Ano = g.Key.Year,
                    ConsumoTotal = g.Sum(r => r.ConsumoLitros)
                })
                .OrderBy(r => r.Ano)
                .ThenBy(r => r.Mes)
                .ToListAsync();

            return Ok(registros);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter consumo mensal",
                Detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Obtém estatísticas gerais
    /// </summary>
    [HttpGet("estatisticas")]
    public async Task<IActionResult> ObterEstatisticas()
    {
        try
        {
            var idUsuario = ObterIdUsuario();
            var idTenant = ObterIdTenant();
            _provedorTenant.DefinirIdTenantAtual(idTenant);

            var registros = await _contexto.RegistrosAgua
                .Where(r => r.IdUsuario == idUsuario && r.IdTenant == idTenant)
                .ToListAsync();

            if (registros.Count == 0)
                return Ok(new
                {
                    maiorConsumo = 0,
                    menorConsumo = 0,
                    mediaConsumoDia = 0,
                    desviaoPadrao = 0
                });

            var consumos = registros.Select(r => r.ConsumoLitros).ToList();
            var maiorConsumo = consumos.Max();
            var menorConsumo = consumos.Min();
            var mediaConsumoDia = consumos.Average();

            var variancia = consumos.Sum(x => Math.Pow(x - mediaConsumoDia, 2)) / consumos.Count;
            var desviaoPadrao = Math.Sqrt(variancia);

            return Ok(new
            {
                maiorConsumo,
                menorConsumo,
                mediaConsumoDia,
                desviaoPadrao
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DtoRespostaErro
            {
                Mensagem = "Erro ao obter estatísticas",
                Detalhes = ex.Message
            });
        }
    }
}
