using System.ComponentModel.DataAnnotations;

namespace WorkLog.Models
{
    public class RegisterVM
    {
        [Required, MaxLength(50)]
        public string Username { get; set; }
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        /*[RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\]).{8,32}$")]*/
        public string Password { get; set; }
/*
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }*/
        
    }
}
