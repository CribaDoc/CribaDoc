using CribaDoc.Server.Auth;
using CribaDoc.Server.Negocio;
using CribaDoc.Server.Persistencia.Repositorios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings.json");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<CribaDoc.Server.Persistencia.FabricaConexion>();

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IProyectoRepositorio, ProyectoRepositorio>();
builder.Services.AddScoped<IAccesoProyectoRepositorio, AccesoProyectoRepositorio>();

builder.Services.AddScoped<IBusquedaRepositorio, BusquedaRepositorio>();
builder.Services.AddScoped<IPaperRepositorio, PaperRepositorio>();
builder.Services.AddScoped<ICriterioRepositorio, CriterioRepositorio>();
builder.Services.AddScoped<IDecisionRepositorio, DecisionRepositorio>();
builder.Services.AddScoped<IDecisionCriterioRepositorio, DecisionCriterioRepositorio>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProyectoService>();
builder.Services.AddScoped<BusquedaService>();
builder.Services.AddScoped<CriterioService>();
builder.Services.AddScoped<CribadoService>();

builder.Services.AddScoped<CribaDoc.Core.Ris.IConversorRis, CribaDoc.Core.Ris.ConversorRis>();
builder.Services.AddScoped<CribaDoc.Core.ExportRis.IExportadorRis, CribaDoc.Core.ExportRis.ExportadorRis>();
builder.Services.AddScoped<CribaDoc.Core.ExporExcel.IExportadorExcel, CribaDoc.Core.ExporExcel.ExportadorExcel>();

builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<AppTokenService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();