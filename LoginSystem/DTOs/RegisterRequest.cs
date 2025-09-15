namespace LoginSystem.DTOs
{
    // DTO a regisztrációhoz
    public class RegisterRequest
    {
        public string UserName { get; set; }           // Felhasználónév
        public string Email { get; set; }              // Email cím
        public string Password { get; set; } = string.Empty; // Jelszó
        public string? FirstName { get; set; }         // Keresztnév (opcionális)
        public string LastName { get; set; }           // Vezetéknév
        public string? PhoneNumber { get; set; }       // Telefonszám (opcionális)
    }

}
