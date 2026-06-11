using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BrasilFit.API.Middlewares;
using BrasilFit.API.Services.Auth;
using BrasilFit.API.Services.Avaliacoes;
using BrasilFit.API.Services.Pacientes;
using BrasilFit.API.Services.PlanosAlimentares;
using BrasilFit.Infrastructure.Data;
using BrasilFit.Infrastructure.Data.Seeders;
using BrasilFit.Infrastructure.ExternalServices.OpenFoodFacts;
using BrasilFit.Infrastructure.ExternalServices.Taco;
using BrasilFit.Infrastructure.ExternalServices.ViaCep;
using BrasilFit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

const string CorsFrontendPolicy = "FrontendCors";

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
// 3) CORS - politica nomeada com origens vindas do appsettings.
//    NUNCA usar AllowAnyOrigin + AllowCredentials em producao (e ilegal por spec).
// =============================================================================
var origensPermitidas = builder.Configuration
    .GetSection("Cors:OrigensPermitidas")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsFrontendPolicy, policy =>
    {
        if (origensPermitidas.Length == 0)
        {
            // Fallback so para desenvolvimento local.
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origensPermitidas)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials() // necessario se o front for usar cookies/refresh token
                  .WithExposedHeaders("Content-Disposition"); // libera headers extras se voce devolver downloads
        }
    });
});

// =============================================================================
// 4) HttpClientFactory para APIs externas (ViaCEP e OpenFoodFacts)
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

builder.Services.AddHttpClient<ITacoService, TacoService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:Taco:BaseUrl"]
        ?? "https://taco-food-api.vercel.app/api/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("BrasilFit-API/1.0");
});

// =============================================================================
// 5) Application Services + Repositorios
// =============================================================================
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IPlanoAlimentarService, PlanoAlimentarService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();

builder.Services.AddScoped(sp => new DataSeeder(
    sp.GetRequiredService<AppDbContext>(),
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<ILoggerFactory>().CreateLogger<DataSeeder>(),
    sp.GetRequiredService<IPasswordHasher>().Hash));

// =============================================================================
// 6) ProblemDetails + Global Exception Handler (RFC 7807)
// =============================================================================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    // Enriquecimento padrao - acrescenta TraceId e o path em TODO ProblemDetails
    // gerado pelo ASP.NET (inclusive 400 de model validation).
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Instance ??= ctx.HttpContext.Request.Path;
    };
});

// =============================================================================
// 7) Controllers + JSON + Swagger
// =============================================================================
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Evita StackOverflow quando o EF traz objetos com referencias circulares
        // (ex.: Paciente -> Nutricionista -> Pacientes -> ...).
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Padroniza nomes em camelCase (default do ASP.NET, mas deixo explicito).
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        // Enums viram string ("Nutricionista" no lugar de 2) - muito mais legivel
        // no front e no Swagger.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Nao envia campos null no payload.
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BrasilFit / NutriLife API",
        Version = "v1",
        Description = "API de planejamento nutricional - Projeto Faculdade."
    });

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

var app = builder.Build();

// =============================================================================
// 8) Pipeline (ordem importa!)
// =============================================================================

// UseExceptionHandler PRIMEIRO - para capturar excecoes de qualquer middleware/abaixo.
app.UseExceptionHandler();

// StatusCodePages converte 401/403/404 sem body em ProblemDetails.
app.UseStatusCodePages();

// Swagger disponivel em desenvolvimento e tambem em producao (para demonstracao).
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BrasilFit API v1"));

// HTTPS redirect apenas em desenvolvimento local; Azure gerencia SSL na camada de proxy.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(CorsFrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// =============================================================================
// 9) Migrations + Seed na inicializacao
// =============================================================================
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
