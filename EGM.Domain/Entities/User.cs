using EGM.Domain.Constants;

namespace EGM.Domain.Entities
{
    public class User
    {
        public int Id {get; set;}
        public int Sicil {get; set;}
        public string PasswordHash {get; set;} = string.Empty;
        public string Role {get; set;} = Roles.User;
        public string FullName {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public string GSM {get; set;} = string.Empty;

    }
}