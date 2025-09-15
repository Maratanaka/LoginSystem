namespace LoginSystem.DTOs
{
    // DTO az elfelejtett jelszó folyamat indításához
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty; // Email cím, ahová a reset linket küldjük
    }

}
