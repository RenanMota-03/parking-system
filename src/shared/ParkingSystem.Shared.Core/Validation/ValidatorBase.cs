using System.Linq.Expressions;

namespace ParkingSystem.Shared.Core.Validation;

public abstract class ValidatorBase<T>
{
    private readonly List<(Func<T, object?> getter, string propertyName, Func<T, string?> errorFn)> _rules = [];

    public ValidationResult Validate(T instance)
    {
        var result = new ValidationResult();
        foreach (var (_, propertyName, errorFn) in _rules)
        {
            var error = errorFn(instance);
            if (error is not null)
                result.AddFailure(propertyName, error);
        }
        return result;
    }

    protected void ValidateNotEmpty(Expression<Func<T, string?>> expr, string errorMessage)
    {
        var name = GetName(expr);
        var getter = expr.Compile();
        _rules.Add((x => getter(x), name, x =>
            string.IsNullOrWhiteSpace(getter(x)) ? errorMessage : null));
    }

    protected void ValidateMaxLength(Expression<Func<T, string?>> expr, int max, string errorMessage)
    {
        var name = GetName(expr);
        var getter = expr.Compile();
        _rules.Add((x => getter(x), name, x =>
        {
            var val = getter(x);
            return val is not null && val.Length > max ? errorMessage : null;
        }));
    }

    protected void ValidateIsInEnum<TEnum>(Expression<Func<T, TEnum>> expr, string errorMessage) where TEnum : struct, Enum
    {
        var name = GetName(expr);
        var getter = expr.Compile();
        _rules.Add((x => getter(x), name, x => !Enum.IsDefined(getter(x)) ? errorMessage : null));
    }

    protected void ValidateMust(Expression<Func<T, bool>> expr, string propertyName, string errorMessage)
    {
        var getter = expr.Compile();
        _rules.Add((x => getter(x), propertyName, x => !getter(x) ? errorMessage : null));
    }

    private static string GetName<TVal>(Expression<Func<T, TVal>> expr)
    {
        if (expr.Body is MemberExpression member) return member.Member.Name;
        return "Property";
    }
}
