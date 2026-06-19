using ParkingSystem.Shared.Core.Exceptions;

namespace ParkingSystem.Shared.Core.Validation;

public static class DomainValidation
{
    public static void NotNull(object? target, string fieldName)
    {
        if (target is null)
            throw new EntityValidationException($"{fieldName} não pode ser nulo.");
    }

    public static void NotNullOrEmpty(string? target, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(target))
            throw new EntityValidationException($"{fieldName} não pode ser nulo ou vazio.");
    }

    public static void NotZeroOrNegative(long target, string fieldName)
    {
        if (target <= 0)
            throw new EntityValidationException($"{fieldName} deve ser maior que zero.");
    }

    public static void MinLength(string target, int minLength, string fieldName)
    {
        if (target.Length < minLength)
            throw new EntityValidationException($"{fieldName} deve ter pelo menos {minLength} caracteres.");
    }

    public static void MaxLength(string target, int maxLength, string fieldName)
    {
        if (target.Length > maxLength)
            throw new EntityValidationException($"{fieldName} deve ter no máximo {maxLength} caracteres.");
    }

    public static void PositiveDecimal(decimal target, string fieldName)
    {
        if (target <= 0)
            throw new EntityValidationException($"{fieldName} deve ser um valor positivo.");
    }

    public static void That(bool condition, string message)
    {
        if (!condition)
            throw new EntityValidationException(message);
    }
}
