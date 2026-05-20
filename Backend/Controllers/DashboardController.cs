using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _contexto;

    public DashboardController(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    private int ObterIdUsuario()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idStr ?? "0");
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo()
    {
        var idUsuario = ObterIdUsuario();
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var registros = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario)
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
            .Where(r => r.IdUsuario == idUsuario)
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

        return Ok(new DtoResumoCheio
        {
            TotalRegistros = totalRegistros,
            ConsumoTotal = consumoTotal,
            ConsumoMedio = consumoMedio,
            ConsumoDiaAtual = consumoDiaAtual,
            ConsumoMesAtual = consumoMesAtual,
            RegistrosRecentes = registrosRecentes
        });
    }

    [HttpGet("consumo-diario")]
    public async Task<IActionResult> ObterConsumoDiario([FromQuery] int mes = 0, [FromQuery] int ano = 0)
    {
        var idUsuario = ObterIdUsuario();

        if (mes == 0) mes = DateTime.UtcNow.Month;
        if (ano == 0) ano = DateTime.UtcNow.Year;

        var inicioMes = new DateTime(ano, mes, 1);
        var fimMes = inicioMes.AddMonths(1);

        var registros = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario && r.Data >= inicioMes && r.Data < fimMes)
            .GroupBy(r => r.Data.Date)
            .Select(g => new { Data = g.Key, ConsumoTotal = g.Sum(r => r.ConsumoLitros) })
            .OrderBy(r => r.Data)
            .ToListAsync();

        return Ok(registros);
    }

    [HttpGet("consumo-semanal")]
    public async Task<IActionResult> ObterConsumoSemanal()
    {
        var idUsuario = ObterIdUsuario();
        var dataInicio = DateTime.UtcNow.AddDays(-7);

        var registros = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario && r.Data >= dataInicio)
            .GroupBy(r => r.Data.Date)
            .Select(g => new { Data = g.Key, ConsumoTotal = g.Sum(r => r.ConsumoLitros) })
            .OrderBy(r => r.Data)
            .ToListAsync();

        return Ok(registros);
    }

    [HttpGet("consumo-mensal")]
    public async Task<IActionResult> ObterConsumoMensal()
    {
        var idUsuario = ObterIdUsuario();
        var dataInicio = DateTime.UtcNow.AddMonths(-12);

        var registros = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario && r.Data >= dataInicio)
            .GroupBy(r => new { r.Data.Year, r.Data.Month })
            .Select(g => new { Mes = g.Key.Month, Ano = g.Key.Year, ConsumoTotal = g.Sum(r => r.ConsumoLitros) })
            .OrderBy(r => r.Ano)
            .ThenBy(r => r.Mes)
            .ToListAsync();

        return Ok(registros);
    }

    [HttpGet("estatisticas")]
    public async Task<IActionResult> ObterEstatisticas()
    {
        var idUsuario = ObterIdUsuario();

        var registros = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario)
            .ToListAsync();

        if (registros.Count == 0)
            return Ok(new { maiorConsumo = 0, menorConsumo = 0, mediaConsumoDia = 0, desviaoPadrao = 0 });

        var consumos = registros.Select(r => r.ConsumoLitros).ToList();
        var maiorConsumo = consumos.Max();
        var menorConsumo = consumos.Min();
        var mediaConsumoDia = consumos.Average();
        var variancia = consumos.Sum(x => Math.Pow(x - mediaConsumoDia, 2)) / consumos.Count;
        var desviaoPadrao = Math.Sqrt(variancia);

        return Ok(new { maiorConsumo, menorConsumo, mediaConsumoDia, desviaoPadrao });
    }

    [HttpGet("estimativa")]
    public async Task<IActionResult> ObterEstimativa()
    {
        var idUsuario = ObterIdUsuario();
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var contas = await _contexto.ContasAgua
            .Where(c => c.IdUsuario == idUsuario)
            .ToListAsync();

        var consumoMesAtual = await _contexto.RegistrosAgua
            .Where(r => r.IdUsuario == idUsuario && r.Data >= inicioMes)
            .SumAsync(r => r.ConsumoLitros);

        double precoMedioPorLitro = 0;
        if (contas.Any())
        {
            var totalLitros = contas.Sum(c => c.LitrosConsumidos);
            var totalValor = contas.Sum(c => c.ValorPago);
            precoMedioPorLitro = totalLitros > 0 ? totalValor / totalLitros : 0;
        }

        var diasNoMes = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        var diaAtual = DateTime.UtcNow.Day;
        var consumoProjetadoMes = diaAtual > 0 ? (consumoMesAtual / diaAtual) * diasNoMes : 0;

        var historico = contas
            .OrderByDescending(c => c.MesReferencia)
            .Take(6)
            .Select(c => new DtoRespostaContaAgua
            {
                Id = c.Id,
                MesReferencia = c.MesReferencia,
                LitrosConsumidos = c.LitrosConsumidos,
                ValorPago = c.ValorPago,
                DataCriacao = c.DataCriacao
            })
            .ToList();

        return Ok(new DtoEstimativa
        {
            PrecoMedioPorLitro = Math.Round(precoMedioPorLitro, 4),
            ConsumoMesAtual = consumoMesAtual,
            EstimativaMesAtual = Math.Round(consumoMesAtual * precoMedioPorLitro, 2),
            ProjecaoProximoMes = Math.Round(consumoProjetadoMes * precoMedioPorLitro, 2),
            HistoricoCustos = historico
        });
    }
}
