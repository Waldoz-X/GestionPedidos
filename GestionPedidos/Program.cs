using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using GestionPedidos.Data;
using GestionPedidos.Services;
using GestionPedidos.Services.Auth;
using GestionPedidos.Security;
using GestionPedidos.Models;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);
const string localDevCorsPolicy = "LocalDevCorsPolicy";

// ── EF Core + SQL Server ──
builder.Services.AddDbContext<AppDbContext>(options =>
    // Cadena anterior (preservada comentada):
    // options.UseSqlServer(
    //     builder.Configuration.GetConnectionString("DefaultConnection"),
    //     sqlOptions => sqlOptions.EnableRetryOnFailure());

    // Nueva cadena de conexión fija (suministrada):
    options.UseSqlServer(
        "Data Source=SQL1002.site4now.net;Initial Catalog=db_acb151_rinatdb;User Id=db_acb151_rinatdb_admin;Password=NXn5jiEmBpax@Sh;Encrypt=True;TrustServerCertificate=True;",
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// ── ASP.NET Core Identity ──
builder.Services.AddIdentity<etUsuario, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("Falta configuración de JWT.");

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

// ── JWT Bearer Authentication ──
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// We will just register basic SwaggerGen for now, Swashbuckle requires OpenApi v1.x types which conflict with native OpenApi v2.x
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Escribe 'Bearer' [espacio] y luego tu token.\r\n\r\nEjemplo: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IProductoGuanteService, ProductoGuanteService>();
builder.Services.AddScoped<IProductoTextilService, ProductoTextilService>();
builder.Services.AddScoped<IProductoConoService, ProductoConoService>();
builder.Services.AddScoped<IProductoEspinilleraService, ProductoEspinilleraService>();
builder.Services.AddScoped<IProductoAccesorioService, ProductoAccesorioService>();
builder.Services.AddScoped<IProductoMochilaService, ProductoMochilaService>();
builder.Services.AddScoped<IProductoFitnessService, ProductoFitnessService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVarianteService, VarianteService>();
builder.Services.AddScoped<ISkuService, SkuService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAsignacionService, AsignacionService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPrecioService, PrecioService>();
builder.Services.AddScoped<IVisibilidadService, VisibilidadService>();
builder.Services.AddScoped<IPoliticaService, PoliticaService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddHostedService<GestionPedidos.Services.Workers.LiberadorPedidosExpiradosWorker>();

// ── Cloudinary Configuration ──
var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
Account account = new Account(
    cloudinaryConfig["CloudName"],
    cloudinaryConfig["ApiKey"],
    cloudinaryConfig["ApiSecret"]
);
Cloudinary cloudinary = new Cloudinary(account);
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ── CORS - Permitir origen de Angular ──
builder.Services.AddCors(options =>
{
    options.AddPolicy(localDevCorsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin()        // Permitir cualquier origen (desarrollo)
            .AllowAnyMethod()        // Permitir cualquier método (GET, POST, PUT, DELETE, etc)
            .AllowAnyHeader();       // Permitir cualquier header
    });
});

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

// ── Middleware Pipeline ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(localDevCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

