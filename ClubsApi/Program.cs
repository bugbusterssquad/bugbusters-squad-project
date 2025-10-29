using ClubsApi.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASS");
var apiAddr = Environment.GetEnvironmentVariable("API_ADDR");

var cs = $"{builder.Configuration.GetConnectionString("DefaultConnection")}User Id={dbUser};Password={dbPass};";

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Frontend (Vite varsayılan: 5173) için CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("frontend", p => p
        .WithOrigins(apiAddr)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// HTTP kullanıyoruz; UseHttpsRedirection'ı kaldırdık
app.UseCors("frontend");

app.MapControllers();

app.Run();
