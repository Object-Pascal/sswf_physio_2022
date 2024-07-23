using System;
using System.ComponentModel.DataAnnotations;

namespace Web_App.Validation
{
    public class IsFutureDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime? StartDate = value as DateTime?;
            if (StartDate.HasValue)
            {
                return StartDate < DateTime.Now ? new ValidationResult($"{validationContext.DisplayName} must be higher than today") : ValidationResult.Success;
            }
            return new ValidationResult($"{validationContext.DisplayName} is required");
        }
    }
}