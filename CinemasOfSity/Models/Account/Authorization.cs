using System.ComponentModel.DataAnnotations;

namespace CinemasOfSity.Models.Account
{
    public class Authorization : IValidatableObject
    {
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Не указан пароль")]
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();
            return errors;
        }
    }
}
