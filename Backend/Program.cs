using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Configurações de Banco de Dados ---
var stringConexao = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("String de conexão 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao)));

// --- Serviços de Tenant ---
builder.Services.AddScoped<IProvedorTenant, ProvedorTenantService>();

// --- CORS ---
builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermitirFrontend", politica => politica
        .WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// --- Autenticação JWT ---
var chaveJwt = builder.Configuration["ChaveJwt"] 
    ?? throw new InvalidOperationException("Chave JWT não configurada.");
var bytesChave = Encoding.ASCII.GetBytes(chaveJwt);

builder.Services.AddAuthentication(opcoes =>
{
    opcoes.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opcoes.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opcoes =>
{
    opcoes.RequireHttpsMetadata = false;
    opcoes.SaveToken = true;
    opcoes.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(bytesChave),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

// --- Controllers e OpenAPI ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// --- Inicializar Banco de Dados ---
try
{
    using (var escopo = app.Services.CreateScope())
    {
        var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Verificar se o banco existe, se não, criar
        await contexto.Database.EnsureCreatedAsync();
        
        // Se não houver tenant padrão, criar um
        if (!await contexto.Tenants.AnyAsync())
        {
            var tenantPadrao = new Tenant
            {
                Identificador = "padrao",
                Nome = "Tenant Padrão",
                Descricao = "Tenant padrão do sistema",
                ConexaoBancoDados = "DefaultConnection",
                Ativo = true
            };
            
            contexto.Tenants.Add(tenantPadrao);
            await contexto.SaveChangesAsync();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao inicializar banco de dados: {ex.Message}");
}

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("PermitirFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();