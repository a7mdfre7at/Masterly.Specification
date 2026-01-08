using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents NAND (NOT AND) - false only when both are true.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class NandSpecification<T> : CompositeSpecification<T>
    {
        public NandSpecification(ISpecification<T> left, ISpecification<T> right) : base(left, right)
        {
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var combined = Left.ToExpression().And(Right.ToExpression());

            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(combined.Body), combined.Parameters);
        }
    }
}
