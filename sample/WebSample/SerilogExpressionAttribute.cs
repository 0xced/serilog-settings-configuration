using System.ComponentModel.DataAnnotations;
using Serilog.Expressions;

namespace WebSample;

class SerilogExpressionAttribute : RequiredAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string expression)
        {
            return SerilogExpression.TryCompile(expression, out _, out var error) ? ValidationResult.Success : new ValidationResult(error);
        }
        return base.IsValid(value, validationContext);
    }
}
