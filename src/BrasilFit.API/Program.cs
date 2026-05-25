using System.Text;
using BrasilFit.API.Services.Auth;
using BrasilFit.API.Services.Avaliacoes;
using BrasilFit.API.Services.Pacientes;
using BrasilFit.API.Services.PlanosAlimentares;
using BrasilFit.Infrastructure.Data;
using BrasilFit.Infrastructure.Data.Seeders;
using BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;
using BrasilFit.Infrastructure.ExternalServices.ViaCep;
using BrasilFit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// 1) EF Core + SQL Server
// =============================================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

// =============================================================================
// 2) JWT Authentication + Authorization
// =============================================================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Secao 'Jwt' nao configurada em appsettings.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// =============================================================================
// 3) HttpClientFactory para APIs externas (ViaCEP e OpenFoodFacts)
// =============================================================================
builder.Services.AddHttpClient<IViaCepService, ViaCepService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:ViaCep:BaseUrl"]
        ?? "https://viacep.com.br/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BrasilFit-API/1.0");
});

builder.Services.AddHttpClient<IOpenFoodFactsService, OpenFoodFactsService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:OpenFoodFacts:BaseUrl"]
        ?? "https://world.openfoodfacts.org/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BrasilFit-API/1.0 (contato@brasilfit.local)");
});

// =============================================================================
// 4) Application Services + Repositorios
// =============================================================================
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IPlanoAlimentarService, PlanoAlimentarService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();

// Seeder - registrado com o delegate de hashing apontando para o IPasswordHasher.
builder.Services.AddScoped(sp => new DataSeeder(
    sp.GetRequiredService<AppDbContext>(),
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<ILoggerFactory>().CreateLogger<DataSeeder>(),
    sp.GetRequiredService<IPasswordHasher>().Hash));

// =============================================================================
// 5) Controllers + Swagger
// =============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BrasilFit / NutriLife API",
        Version = "v1",
        Description = "API de planejamento nutricional - Projeto Faculdade."
    });

    // Botao Authorize do Swagger UI.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole o token JWT (sem o prefixo 'Bearer')."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// CORS minimo para desenvolvimento (qualquer origem).
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// =============================================================================
// 6) Pipeline
// =============================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =============================================================================
// 7) Aplicar migrations e rodar o seeder na inicializacao
// =============================================================================
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
