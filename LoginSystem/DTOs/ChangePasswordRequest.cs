namespace LoginSystem.DTOs
{
    // DTO a jelszó megváltoztatásához (bejelentkezett felhasználó)
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty; // Jelenlegi jelszó
        public string NewPassword { get; set; } = string.Empty;     // Új jelszó
    }

}
