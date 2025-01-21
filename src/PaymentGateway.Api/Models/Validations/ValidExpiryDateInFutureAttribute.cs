using System;
using System.ComponentModel.DataAnnotations;

public class ValidExpiryDateInFutureAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Access the object containing the properties
        var instance = validationContext.ObjectInstance;

        // Use reflection to get the expiry month and year properties
        var expiryMonthProperty = validationContext.ObjectType.GetProperty("ExpiryMonth");
        var expiryYearProperty = validationContext.ObjectType.GetProperty("ExpiryYear");

        if (expiryMonthProperty == null || expiryYearProperty == null)
        {
            return new ValidationResult("ExpiryMonth or ExpiryYear property is not found.");
        }

        int expiryMonth = (int)expiryMonthProperty.GetValue(instance);
        int expiryYear = (int)expiryYearProperty.GetValue(instance);

        // Validate expiry month and year
        if (expiryMonth < 1 || expiryMonth > 12)
        {
            return new ValidationResult("ExpiryMonth must be between 1 and 12.");
        }

        // Get the current date
        var now = DateTime.UtcNow;

        // Calculate the expiry date
        var expiryDate = new DateTime(expiryYear, expiryMonth, DateTime.DaysInMonth(expiryYear, expiryMonth));

        // Check if expiry date is in the future
        if (expiryDate < now)
        {
            return new ValidationResult("The expiry date must be in the future.");
        }

        return ValidationResult.Success;
    }
}