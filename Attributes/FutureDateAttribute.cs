using System;
using System.ComponentModel.DataAnnotations;

namespace AdminDashboard.Attributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                if (date < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? "The date must not be in the past.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
