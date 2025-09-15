using LoginSystem.Model;
using Microsoft.EntityFrameworkCore;

namespace LoginSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Konstruktor: az EF Core DbContext inicializálása a konfigurációs opciókkal
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        // DbSet a felhasználókhoz → ez hozza létre a Users táblát az adatbázisban
        public DbSet<User> Users { get; set; }

        // Modell konfigurációk (táblák, mezők, indexek)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e => {

             
                // Tábla beállítások
          
                e.ToTable("users"); // Tábla neve az adatbázisban
                e.HasKey(e => e.Id); // Elsődleges kulcs
                e.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Users_Email"); // Egyedi email
                e.HasIndex(e => e.UserName).IsUnique().HasDatabaseName("IX_Users_UserName"); // Egyedi felhasználónév

        
                // Felhasználói adatok
          
                e.Property(e => e.Id).ValueGeneratedOnAdd(); // Auto increment
                e.Property(e => e.UserName).IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
                e.Property(e => e.Email).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                e.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");

          
                // Általános adatok
            
                e.Property(e => e.FirstName).HasMaxLength(100).HasColumnType("varchar(100)");
                e.Property(e => e.LastName).HasMaxLength(100).HasColumnType("varchar(100)");
                e.Property(e => e.PhoneNumber).HasMaxLength(20).HasColumnType("varchar(20)");

       
                // Szerepkörök
          
                e.Property(e => e.Role)
                 .IsRequired()
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)")
                 .HasDefaultValue("User"); // Alapértelmezett: "User"

       
                // Tokenek

                e.Property(e => e.RefreshToken).HasMaxLength(500).HasColumnType("varchar(500)");
                e.Property(e => e.EmailConfirmationToken).HasMaxLength(255).HasColumnType("varchar(255)");
                e.Property(e => e.PasswordResetToken).HasMaxLength(255).HasColumnType("varchar(255)");

          
                // Idő mezők
         
                e.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(e => e.LastLoginAt).HasColumnType("datetime");
                e.Property(e => e.UpdatedAt).HasColumnType("datetime");
                e.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
                e.Property(e => e.PasswordResetTokenExpiry).HasColumnType("datetime");

      
                // Logikai mezők
         
                e.Property(e => e.IsActive).HasDefaultValue(true); // Aktív alapértelmezett: true
                e.Property(e => e.EmailConfirmed).HasDefaultValue(false); // Email megerősítés alap: false

            });
        }
    }
}
