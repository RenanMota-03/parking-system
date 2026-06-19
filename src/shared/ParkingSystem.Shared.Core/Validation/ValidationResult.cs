namespace ParkingSystem.Shared.Core.Validation;

public class ValidationResult
{
    private readonly List<ValidationFailure> _errors = [];
    public IList<ValidationFailure> Errors => _errors;
    public bool IsValid => _errors.Count == 0;
    public dynamic Data { get; set; } = default!;

    public void AddFailure(string propertyName, string errorMessage)
        => _errors.Add(new ValidationFailure(propertyName, errorMessage));

    public void AddFailures(IEnumerable<ValidationFailure> failures)
        => _errors.AddRange(failures);

    public static ValidationResult Success => new();
}

public class ValidationFailure(string propertyName, string errorMessage)
{
    public string PropertyName { get; } = propertyName;
    public string ErrorMessage { get; } = errorMessage;
}
