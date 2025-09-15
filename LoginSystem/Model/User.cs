using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginSystem.Model
{
    public class User
    {
        [Key] // Elsődleges kulcs (Id azonosító az adatbázisban)
        public int Id { get; set; }

        [Required] // Kötelező mező
        [StringLength(100)] // Max hossz 100 karakter
        public string UserName { get; set; } = string.Empty;

        [Required] // Kötelező mező
        [EmailAddress] // Email validáció
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required] // Kötelező mező
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty; // Jelszó hash-elve (nem nyers jelszó!)

        [StringLength(100)]
        public string? FirstName { get; set; } // Keresztnév (opcionális)

        [StringLength(100)]
        public string? LastName { get; set; } // Vezetéknév (opcionális)

        [StringLength(20)]
        public string? PhoneNumber { get; set; } // Telefonszám (opcionális)

        public bool IsActive { get; set; } = true; // Aktív-e a felhasználó

        public bool EmailConfirmed { get; set; } = false; // Megerősítette-e az email címét

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Létrehozás dátuma

        public DateTime? LastLoginAt { get; set; } // Utolsó belépés dátuma

        public DateTime? UpdatedAt { get; set; } // Utolsó módosítás dátuma

        // Opcionális szerepkör kezelés (pl. User / Admin)
        [StringLength(50)]
        public string Role { get; set; } = "User";

        // Refresh token kezeléshez (token frissítés JWT-hez)
        [StringLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; } // Meddig érvényes a refresh token

        // Email megerősítéshez (ellenőrző token)
        [StringLength(255)]
        public string? EmailConfirmationToken { get; set; }

        // Jelszó visszaállításhoz használt token
        [StringLength(255)]
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; } // Jelszó reset token lejárati ideje

        // Kizárt mezők az adatbázisból (nem lesz oszlop, csak a kódban érhető el)
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim(); // Teljes név összeállítva
    }
}

