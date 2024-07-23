using System;
using System.ComponentModel.DataAnnotations;

namespace Web_App.Validation
{
    public class Age : ValidationAttribute
    {
        private int Minimum { get; set; }
        public Age(int minimum)
        {
            Minimum = minimum;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime? StartDate = value as DateTime?;
            if (StartDate.HasValue)
            {
                DateTime now = DateTime.Now;
                int age = now.Year - StartDate.Value.Year;
                if (now.Month < StartDate.Value.Month || (now.Month == StartDate.Value.Month && now.Day < StartDate.Value.Day))
                    age--;

                return age < Minimum ? new ValidationResult($"{validationContext.DisplayName} must have an age minimum of {Minimum}") : ValidationResult.Success;
            }
            return new ValidationResult($"{validationContext.DisplayName} is required");
        }
    }
}