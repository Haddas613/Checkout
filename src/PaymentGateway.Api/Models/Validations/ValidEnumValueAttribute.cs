using System;
using System.ComponentModel.DataAnnotations;

public class ValidEnumValueAttribute : ValidationAttribute
{
    private readonly Type _enumType;

    public ValidEnumValueAttribute(Type enumType)
    {
        _enumType = enumType;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Check if the value is not null and if it's a valid enum value
        if (value == null || !Enum.IsDefined(_enumType, value))
        {
            return new ValidationResult($"The value '{value}' is not valid for {validationContext.DisplayName}. Please select a valid option.");
        }

        return ValidationResult.Success;
    }
}