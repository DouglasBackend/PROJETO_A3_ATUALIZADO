using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/sistema")]
public class SistemaController : ControllerBase
{
    private readonly AppDbContext _contexto;

    public SistemaController(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    [HttpGet("cadastrado")]
    public async Task<IActionResult> VerificarCadastro()
    {
        var existeUsuario = await _contexto.Usuarios.AnyAsync();
        return Ok(new { cadastrado = existeUsuario });
    }
}
