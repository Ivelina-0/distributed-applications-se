using DentalClinicApp.Data;
using DentalClinicApp.Middleware;
using DentalClinicApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dental Clinic Appointment Management API", Version = "1.0.0" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Description = "JWT Authorization header. Example: Bearer {token}", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer" });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
using (var scope = app.Services.CreateScope()) await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
app.Run();
