using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web_App.Models
{
    public class LoginModel
    {
        [Required]
        [DisplayName("Username*")]
        public string UserName { get; set; }

        [Required]
        [DisplayName("Password*")]
        public string Password { get; set; }

        public LoginModel() { }
        public LoginModel(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }
    }
}