using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class SignInVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        /*[RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\]).{8,32}$")]*/
        public string Password { get; set; }
        public bool RememberMe
        {
            get;
            set;
        }
    }
}
