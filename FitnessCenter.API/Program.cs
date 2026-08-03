using FitnessCenterr.Core.Services.Interfaces;
using FitnessCenterr.Infrastructure.Data;
using FitnessCenterr.Infrastructure.Services;
using FitnessCenterr.Infrastructure.Services.MySQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── MySQL ─────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// ── MongoDB ───────────────────────────────────────────────────
builder.Services.AddSingleton<MongoDbContext>();

// ── Neo4j ─────────────────────────────────────────────────────
builder.Services.AddSingleton<Neo4jContext>();

// ── Services (MySQL) ──────────────────────────────────────────
builder.Services.AddScoped<IMemberService,  MemberService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<IClassService,   ClassService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ── AI Enrichment (Ollama) ────────────────────────────────────
builder.Services.AddHttpClient("OllamaClient", client =>
{
    var baseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout     = TimeSpan.FromSeconds(120); // Ollama kan være langsom første gang
});
builder.Services.AddScoped<IAiEnrichmentService, AiEnrichmentService>();

// ── JWT Authentication ────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "fitness-center-super-secret-key-2024!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = "FitnessAPI",
            ValidAudience            = "FitnessAPIUsers",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ── Fix circular reference ────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ── Swagger med JWT ───────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitness Center API", Version = "v1" });
    c.TagActionsBy(api =>
    {
        if (api.RelativePath!.Contains("mysql"))   return new[] { "MySQL" };
        if (api.RelativePath!.Contains("mongodb")) return new[] { "MongoDB" };
        if (api.RelativePath!.Contains("neo4j"))   return new[] { "Neo4j" };
        if (api.RelativePath!.Contains("ai"))      return new[] { "AI Enrichment" };
        return new[] { "General" };
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Skriv: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fitness Center API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
