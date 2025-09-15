using LoginSystem.Data;
using LoginSystem.DTOs;
using LoginSystem.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LoginSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;       // Adatbázis kontextus
        private readonly IConfiguration _configuration;       // Alkalmazás konfiguráció
        private readonly ILogger<AuthController> _logger;     // Logger hibák és események naplózásához

        // Konstruktor: DI (Dependency Injection) használata
        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        
        // REGISZTRÁCIÓ
        // POST: /api/auth/register
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Ellenőrzi, hogy az email már foglalt-e
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return BadRequest(new { message = "Az email cím már használatban van." });

                // Ellenőrzi, hogy a felhasználónév már foglalt-e
                if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                    return BadRequest(new { message = "A felhasználónév már használatban van." });

                // Jelszó hash-elése biztonságosan
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Új User objektum létrehozása
                var user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = false,
                    Role = "User"
                };

                // Mentés az adatbázisba
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Új felhasználó regisztrálva: {user.Email}");

                return Ok(new { message = "Sikeres regisztráció!", userId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a regisztráció során");
                return StatusCode(500, new { message = "Belső szerver hiba" });
            }
        }

    
        // BEJELENTKEZÉS
        // POST: /api/auth/login
  
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Felhasználó keresése email vagy felhasználónév alapján
                var user = await _context.Users.FirstOrDefaultAsync(
                    u => u.Email == request.Email || u.UserName == request.Email
                );

                // Hibás login ellenőrzése
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized(new { message = "Hibás email/felhasználónév vagy jelszó." });

                if (!user.IsActive)
                    return Unauthorized(new { message = "A fiók inaktív." });

                // Access token és refresh token generálása
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // Refresh token mentése az adatbázisba
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays")
                );
                user.LastLoginAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Sikeres bejelentkezés: {user.Email}");

                // Visszatérési érték: tokenek + user adatok
                return Ok(new
                {
                    accessToken,
                    refreshToken,
                    user = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a bejelentkezés során");
                return StatusCode(500, new { message = "Belső szerver hiba" });
            }
        }

      
        // REFRESH TOKEN
        // POST: /api/auth/refresh
      
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(
                    u => u.RefreshToken == request.RefreshToken
                );

                // Érvénytelen vagy lejárt token
                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                    return Unauthorized(new { message = "Érvénytelen vagy lejárt refresh token." });

                // Új tokenek generálása
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays")
                );

                await _context.SaveChangesAsync();

                return Ok(new { accessToken, refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a token frissítés során");
                return StatusCode(500, new { message = "Belső szerver hiba" });
            }
        }

      
        // KIJELENTKEZÉS
        // POST: /api/auth/logout
     
        [HttpPost("logout")]
        [Authorize] // Csak bejelentkezett felhasználó hívhatja
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = GetUserIdFromToken(); // JWT-ből azonosító kinyerése
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Sikeres kijelentkezés" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a kijelentkezés során");
                return StatusCode(500, new { message = "Belső szerver hiba" });
            }
        }

      
        // JELENLEGI FELHASZNÁLÓ ADATAI
        // GET: /api/auth/me
 
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return NotFound(new { message = "Felhasználó nem található" });

                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.Role,
                    user.CreatedAt,
                    user.LastLoginAt,
                    user.EmailConfirmed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a felhasználó lekérdezése során");
                return StatusCode(500, new { message = "Belső szerver hiba" });
            }
        }

      
        // Privát segédfüggvények

        // Generál véletlenszerű token jelszó visszaállításhoz
        private string GeneratePasswordResetToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Generál véletlenszerű refresh tokent
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // JWT-ből kinyeri a user Id-t
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // JWT access token generálása
        private string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
            );
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpirationInMinutes")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
