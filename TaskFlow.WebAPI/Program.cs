using Serilog;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Services;
using TaskFlow.WebAPI.Hubs;
using TaskFlow.WebAPI.Services; // NotificationBackgroundService namespace'i

var builder = WebApplication.CreateBuilder(args);

// Serilog Yapılandırması
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/taskflow-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Servis Kayıtları ve Konfigürasyonlar (builder.Build() öncesi)
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IStorageService, LocalStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<TaskFlow.Application.Validators.CreateIssueDtoValidator>();

// SignalR
builder.Services.AddSignalR();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };

        // YENİ EKLENEN KISIM: SignalR için URL'den JWT Okuma (15.4 Güvenlik Kuralı)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                // Eğer istek SignalR hub'ına geliyorsa ve token URL parametresi olarak gelmişse onu yakala
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/taskflow"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Swagger Yapılandırması
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskFlow API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Örn: 'Bearer TOKEN_DEGERI'",
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

// CORS Politikaları (Frontend portları ve SignalR için AllowCredentials)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5175", "http://localhost:5177", "http://localhost:5178")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware Sıralaması
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
        c.RoutePrefix = string.Empty; // Swagger'ı kök dizine (localhost:5045) sabitler
    });
}

app.UseHttpsRedirection();

// Statik Dosyalar
app.UseStaticFiles();

// CORS Middleware (Tüm isteklerden önce çalışmalı)
app.UseCors("AllowFrontend");

// Kimlik Doğrulama ve Yetkilendirme
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub Endpoint Bağlantısı
app.MapHub<TaskFlow.WebAPI.Hubs.TaskFlowHub>("/hubs/taskflow");

app.Run();