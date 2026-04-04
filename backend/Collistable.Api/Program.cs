using System.Text;
using Collistable.Api.Data;
using Collistable.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load .env file — only in local development (Railway injects env vars directly)
if (File.Exists(".env"))
    DotNetEnv.Env.Load();

// Load environment variables into configuration
builder.Configuration.AddEnvironmentVariables();

// Configure Entity Framework — SQL Server locally, PostgreSQL in production
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var usePostgres = builder.Configuration["USE_POSTGRES"] == "true";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (usePostgres)
        options.UseNpgsql(connectionString);
    else
        options.UseSqlServer(connectionString);
});

// Configure CORS to allow requests from the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                builder.Configuration["Cors:AllowedOrigin"] ?? "")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (without 'Bearer' prefix)",
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

builder.Services.AddHttpClient<IGDBService>();
builder.Services.AddScoped<IGDBService>();
builder.Services.AddScoped<TokenService>();

// Named HttpClient for GitHub OAuth API calls — sets required User-Agent and JSON Accept headers
builder.Services.AddHttpClient("github", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Collistable");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Initialise the database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (usePostgres)
        // EnsureCreated creates the schema from the EF Core model — no migrations needed for a fresh deployment
        db.Database.EnsureCreated();
    else
        // SQL Server uses migrations to support incremental schema updates
        db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Pipeline order matters: CORS must come before auth so preflight OPTIONS
// requests get CORS headers before the 401 response would block them
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
