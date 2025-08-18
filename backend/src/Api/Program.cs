using Api.Middleware;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con autenticación JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "DoubleV Partners API", 
        Version = "v1",
        Description = "API para gestión de facturas y productos",
        Contact = new OpenApiContact
        {
            Name = "DoubleV Partners",
            Email = "welcome@doublevpartners.com"
        }
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
            new string[] {}
        }
    });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configurar autenticación JWT
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<Common.Models.JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    });
}

// Configurar servicios de la aplicación
// builder.Services.AddApplicationServices(builder.Configuration);

// Configurar repositorios
builder.Services.Configure<Infrastructure.Db.DatabaseConfig>(builder.Configuration.GetSection("Database"));
builder.Services.AddScoped<Infrastructure.Db.IDbConnectionFactory, Infrastructure.Db.SqlConnectionFactory>();
builder.Services.AddScoped<Application.Interfaces.ICatalogRepository, Infrastructure.Repositories.CatalogRepo>();
builder.Services.AddScoped<Application.Interfaces.IInvoiceRepository, Infrastructure.Repositories.InvoiceRepository>();
builder.Services.AddScoped<Application.Interfaces.IUserRepository, Infrastructure.Repositories.UserRepository>();

// Configurar servicios de aplicación
builder.Services.AddScoped<Application.Interfaces.IInvoiceService, Application.Services.InvoiceService>();
builder.Services.AddScoped<Application.Interfaces.IProductService, Application.Services.ProductService>();
builder.Services.AddScoped<Application.Interfaces.IAuthService, Application.Services.AuthService>();

// Logger personalizado
builder.Services.AddSingleton<Common.Interfaces.ILogger, Common.Interfaces.ConsoleLogger>();

// Configuración de JWT
builder.Services.Configure<Common.Models.JwtSettings>(builder.Configuration.GetSection("Jwt"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DoubleV Partners API v1");
        c.RoutePrefix = string.Empty; // Hacer que Swagger esté disponible en la raíz
    });
}

// Middleware de logging de requests (debe ir antes de CORS)
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAngularApp");

// Middleware de validación de modelos (temporalmente comentado)
// app.UseMiddleware<ModelValidationMiddleware>();

// Middleware de manejo de excepciones (debe ir después de la validación)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
