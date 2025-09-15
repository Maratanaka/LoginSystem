namespace LoginSystem.DTOs
{
    // DTO az elfelejtett jelszó visszaállításához
    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;       // Emailben kapott reset token
        public string NewPassword { get; set; } = string.Empty; // Új jelszó
    }

}
