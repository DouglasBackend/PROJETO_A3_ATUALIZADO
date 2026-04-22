using Backend.Data;
using Backend.Models;
using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000") // Common Vite/React ports
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// JWT Authentication
var jwtKey = "SupeeeeerSecrreeeeeetKeyyyyyy123456789"; // Minimum 32 chars for HMAC-SHA256
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-create Database for simplicity (No migrations needed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// --- AUTH ENDPOINTS ---
app.MapPost("/api/auth/register", async (RegisterDto dto, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
        return Results.BadRequest(new { message = "Email já em uso." });

    var user = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Create a welcome notification
    db.Notifications.Add(new Notification
    {
        UserId = user.Id,
        Title = "Bem-vindo ao AquaMonitor!",
        Message = "Obrigado por se cadastrar. Comece a monitorar seu consumo de água hoje."
    });
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Usuário registrado com sucesso." });
});

app.MapPost("/api/auth/login", async (LoginDto dto, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Results.Unauthorized();

    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return Results.Ok(new AuthResponse
    {
        Token = tokenHandler.WriteToken(token),
        Name = user.Name
    });
});

// --- NOTIFICATIONS ENDPOINTS ---
app.MapGet("/api/notifications", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdStr == null) return Results.Unauthorized();
    int userId = int.Parse(userIdStr);

    var notifications = await db.Notifications
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.CreatedAt)
        .ToListAsync();

    return Results.Ok(notifications);
}).RequireAuthorization();

app.MapPost("/api/notifications/read/{id}", async (int id, AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdStr == null) return Results.Unauthorized();
    int userId = int.Parse(userIdStr);

    var notif = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    if (notif == null) return Results.NotFound();

    notif.IsRead = true;
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

// --- DASHBOARD ENDPOINTS ---
app.MapGet("/api/dashboard/summary", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdStr == null) return Results.Unauthorized();
    int userId = int.Parse(userIdStr);

    var currentConsumption = 234; // mock
    var averageConsumption = 220;
    var monthTotal = 6500;
    var monthGoal = 7000;
    var savings = ((monthGoal - monthTotal) / (double)monthGoal * 100).ToString("F1");

    return Results.Ok(new {
        currentConsumption,
        averageConsumption,
        monthTotal,
        monthGoal,
        savings,
        dailyData = new[] {
            new { time = "00h", consumption = 12 },
            new { time = "04h", consumption = 8 },
            new { time = "08h", consumption = 45 },
            new { time = "12h", consumption = 38 },
            new { time = "16h", consumption = 28 },
            new { time = "20h", consumption = 42 },
            new { time = "23h", consumption = 15 }
        },
        weeklyData = new[] {
            new { day = "Seg", consumption = 245, average = 220 },
            new { day = "Ter", consumption = 198, average = 220 },
            new { day = "Qua", consumption = 312, average = 220 },
            new { day = "Qui", consumption = 256, average = 220 },
            new { day = "Sex", consumption = 189, average = 220 },
            new { day = "Sáb", consumption = 278, average = 220 },
            new { day = "Dom", consumption = 234, average = 220 },
        },
        monthlyData = new[] {
            new { month = "Out", consumption = 6200, goal = 7000 },
            new { month = "Nov", consumption = 6800, goal = 7000 },
            new { month = "Dez", consumption = 7500, goal = 7000 },
            new { month = "Jan", consumption = 8200, goal = 7000 },
            new { month = "Fev", consumption = 7100, goal = 7000 },
            new { month = "Mar", consumption = 6500, goal = 7000 },
        }
    });
}).RequireAuthorization();

app.Run();

