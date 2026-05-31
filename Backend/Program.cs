using Backend.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var stringConexao = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=aquamonitor.db";

builder.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseSqlite(stringConexao));

builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy("PermitirFrontend", politica => politica
        .WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:8080")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.Cookie.HttpOnly = true;
        opcoes.Cookie.SameSite = SameSiteMode.Lax;
        opcoes.ExpireTimeSpan = TimeSpan.FromDays(7);
        opcoes.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        opcoes.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await contexto.Database.EnsureCreatedAsync();
    await contexto.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ""contas_agua"" (
            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_contas_agua"" PRIMARY KEY AUTOINCREMENT,
            ""IdUsuario"" INTEGER NOT NULL,
            ""MesReferencia"" TEXT NOT NULL,
            ""LitrosConsumidos"" REAL NOT NULL,
            ""ValorPago"" REAL NOT NULL,
            ""DataCriacao"" TEXT NOT NULL,
            CONSTRAINT ""FK_contas_agua_usuarios_IdUsuario"" FOREIGN KEY (""IdUsuario"") REFERENCES ""usuarios"" (""Id"") ON DELETE CASCADE
        )
    ");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PermitirFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
