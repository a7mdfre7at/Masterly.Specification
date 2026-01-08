using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents exclusive OR - exactly one of the specifications must be satisfied.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class XorSpecification<T> : CompositeSpecification<T>
    {
        public XorSpecification(ISpecification<T> left, ISpecification<T> right) : base(left, right)
        {
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpr = Left.ToExpression();
            var rightExpr = Right.ToExpression();

            // XOR: (A && !B) || (!A && B)
            var leftTrue = leftExpr;
            var rightTrue = rightExpr;
            var leftFalse = Expression.Lambda<Func<T, bool>>(
                Expression.Not(leftExpr.Body), leftExpr.Parameters);
            var rightFalse = Expression.Lambda<Func<T, bool>>(
                Expression.Not(rightExpr.Body), rightExpr.Parameters);

            return leftTrue.And(rightFalse).Or(leftFalse.And(rightTrue));
        }
    }
}
