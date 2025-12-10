using ClubsApi.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASS");
var apiAddr = Environment.GetEnvironmentVariable("API_ADDR"); 
// ÖRNEK: http://localhost:5173

var cs = $"{builder.Configuration.GetConnectionString("DefaultConnection")}User Id={dbUser};Password={dbPass};";

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -----------------------------------------
// 🔥 CORS DÜZELTİLMİŞ
// -----------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins(apiAddr!)     // tek kaynak veya env üzerinden liste
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();       // parametresiz!
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// CORS middleware
app.UseCors("frontend");

app.MapControllers();

app.Run();
