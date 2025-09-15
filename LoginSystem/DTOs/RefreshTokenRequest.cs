namespace LoginSystem.DTOs
{
    // DTO a JWT refresh token kéréshez
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty; // Kliens által kapott refresh token
    }

}
