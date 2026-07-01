using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskEasy.Data;
using TaskEasy.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext")));

builder.Services.AddScoped<TokenService>();
// PayPal service
builder.Services.AddHttpClient();
builder.Services.AddScoped<PayPalService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirCliente", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var frontendPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "frontend"));
var frontendProvider = new PhysicalFileProvider(frontendPath);
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = frontendProvider });
app.UseStaticFiles(new StaticFileOptions { FileProvider = frontendProvider, RequestPath = "" });
app.UseCors("PermitirCliente");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallback(async context =>
{
    var file = frontendProvider.GetFileInfo("index.html");
    if (file.Exists)
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(file);
    }
    else
    {
        context.Response.StatusCode = 404;
    }
});
app.Run();