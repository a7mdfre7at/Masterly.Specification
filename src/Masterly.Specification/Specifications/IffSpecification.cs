using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents bi-conditional (A if and only if B).
    /// Both must be true or both must be false.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class IffSpecification<T> : CompositeSpecification<T>
    {
        public IffSpecification(ISpecification<T> left, ISpecification<T> right) : base(left, right)
        {
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpr = Left.ToExpression();
            var rightExpr = Right.ToExpression();

            // IFF: (A && B) || (!A && !B)
            var notLeft = Expression.Lambda<Func<T, bool>>(
                Expression.Not(leftExpr.Body), leftExpr.Parameters);
            var notRight = Expression.Lambda<Func<T, bool>>(
                Expression.Not(rightExpr.Body), rightExpr.Parameters);

            return leftExpr.And(rightExpr).Or(notLeft.And(notRight));
        }
    }
}
