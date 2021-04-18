using FluentValidation;

namespace OtherExternalDependency
{
    public sealed class AdditionValidator : AbstractValidator<Addition>
    {
        public AdditionValidator()
        {
            RuleFor(a => a.A).Must(a => a >= 0);
            RuleFor(a => a.B).Must(b => b >= 0);
        }
    }
}
