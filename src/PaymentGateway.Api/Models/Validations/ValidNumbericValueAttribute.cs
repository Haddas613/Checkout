using System;
using System.ComponentModel.DataAnnotations;

public class NumericValueAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is int number)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("The value must be an integer");
    }
}
