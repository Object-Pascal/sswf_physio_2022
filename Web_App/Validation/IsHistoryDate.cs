using System;
using System.ComponentModel.DataAnnotations;

namespace Web_App.Validation
{
    public class IsHistoryDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime? StartDate = value as DateTime?;
            if (StartDate.HasValue)
            {
                return StartDate > DateTime.Now ? new ValidationResult($"{validationContext.DisplayName} must be lower than today") : ValidationResult.Success;
            }
            return new ValidationResult($"{validationContext.DisplayName} is required");
        }
    }
}