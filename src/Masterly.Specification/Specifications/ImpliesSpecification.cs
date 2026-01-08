using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents logical implication (A implies B = !A || B).
    /// If A is true, then B must be true. If A is false, result is true regardless of B.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class ImpliesSpecification<T> : CompositeSpecification<T>
    {
        public ImpliesSpecification(ISpecification<T> left, ISpecification<T> right) : base(left, right)
        {
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpr = Left.ToExpression();
            var rightExpr = Right.ToExpression();

            // Implies: !A || B
            var notLeft = Expression.Lambda<Func<T, bool>>(
                Expression.Not(leftExpr.Body), leftExpr.Parameters);

            return notLeft.Or(rightExpr);
        }
    }
}
