using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CustomerApp.Api.Validations
{
    public class ValidEmailAttribute : ValidationAttribute
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled);

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value == null) return ValidationResult.Success; 

            string email = value.ToString();
            if (!EmailRegex.IsMatch(email))
                return new ValidationResult("Email format is invalid");

            return ValidationResult.Success;
        }
    }
}
