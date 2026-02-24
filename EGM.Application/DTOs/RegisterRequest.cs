namespace EGM.Application.DTOs
{
    public class RegisterRequest
    {
        public int Sicil {get; set;}
        public string Password {get; set;} = string.Empty;
        public string Role {get; set;} = string.Empty;
        public string GSM {get; set;} = string.Empty;
        public string FullName {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
    }

    public class LoginRequest
    {
        public int Sicil {get; set;}
        public string Password {get; set;} = string.Empty;
    }
}