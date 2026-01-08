using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents NOR (NOT OR) - true only when both are false.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class NorSpecification<T> : CompositeSpecification<T>
    {
        public NorSpecification(ISpecification<T> left, ISpecification<T> right) : base(left, right)
        {
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var combined = Left.ToExpression().Or(Right.ToExpression());

            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(combined.Body), combined.Parameters);
        }
    }
}
