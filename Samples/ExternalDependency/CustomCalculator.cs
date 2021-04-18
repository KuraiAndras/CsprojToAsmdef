using OtherExternalDependency;
using System;
using System.Linq;
using System.Text;

namespace ExternalDependency
{
    public static class CustomCalculator
    {
        public static int Add(int a, int b)
        {
            var addition = new Addition(a, b);

            var validator = new AdditionValidator();
            var validation = validator.Validate(addition);

            if (!validation.IsValid)
            {
                var messages = validation.Errors
                    .Aggregate(new StringBuilder(), (sb, e) => sb.AppendLine(e.ErrorMessage))
                    .ToString();

                throw new InvalidOperationException(messages);
            }

            return addition.Execute();
        }
    }
}
