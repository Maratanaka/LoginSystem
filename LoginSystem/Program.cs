using LoginSystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Alkalmazás builder létrehozása
// Ez kezeli a konfigurációt, szolgáltatásokat és a middleware pipeline-t


// Add services to the container.
builder.Services.AddControllers();
// MVC Controller-ek regisztrálása, JSON API végpontokhoz

// Swagger/OpenAPI dokumentáció
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });

    // JWT Authentication beállítása Swagger-ben
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    // Swagger UI-ban lehetőség a JWT token bevitelére az API teszteléshez
});


// Database Context regisztrálása MySQL-lel

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!));


// JWT Authentication konfigurálása

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Csak a megadott issuer-t fogadja el
        ValidateAudience = true, // Csak a megadott audience-t fogadja el
        ValidateLifetime = true, // Lejárt token nem érvényes
        ValidateIssuerSigningKey = true, // Ellenőrzi a titkos kulcs aláírást
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero // Nincs időeltolás a lejárat ellenőrzésben
    };
});

// Authorization hozzáadása
builder.Services.AddAuthorization();
// Engedélyezi a [Authorize] attribútum használatát a controllerekben


// CORS konfiguráció (frontend integrációhoz)

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    // Fejlesztéshez minden origin engedélyezett, éles környezetben korlátozni kell
});

var app = builder.Build();


// Middleware pipeline konfigurálása

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();    // Swagger dokumentáció elérhető fejlesztésben
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // HTTP → HTTPS átirányítás
app.UseCors("AllowFrontend"); // CORS policy alkalmazása

app.UseAuthentication(); // JWT Authentication middleware
app.UseAuthorization();  // Authorization middleware

app.MapControllers();    // Controller útvonalak regisztrálása


// Adatbázis inicializálás / migrációk futtatása

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
        // vagy context.Database.Migrate(); ha migration-őket használsz
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.Run(); // Alkalmazás futtatása
