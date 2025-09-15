namespace LoginSystem.DTOs
{
    // DTO a bejelentkezéshez
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;   // Email vagy felhasználónév
        public string Password { get; set; } = string.Empty; // Jelszó
    }

}
