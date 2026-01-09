using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// A specification that applies different specifications based on a condition.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class ConditionalSpecification<T> : Specification<T>
    {
        private readonly Func<T, bool> _condition;
        private readonly ISpecification<T> _whenTrue;
        private readonly ISpecification<T> _whenFalse;

        public ConditionalSpecification(
            Func<T, bool> condition,
            ISpecification<T> whenTrue,
            ISpecification<T> whenFalse = null)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _whenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
            _whenFalse = whenFalse ?? new AnySpecification<T>();
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _condition(obj) ? _whenTrue.IsSatisfiedBy(obj) : _whenFalse.IsSatisfiedBy(obj);
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            // Build conditional expression: condition ? whenTrue : whenFalse
            ParameterExpression param = Expression.Parameter(typeof(T), "x");

            // We need to inline the condition as an expression
            // For complex conditions, fall back to compiled delegate
            Expression<Func<T, bool>> whenTrueExpr = _whenTrue.ToExpression();
            Expression<Func<T, bool>> whenFalseExpr = _whenFalse.ToExpression();

            // Rebind parameters
            Expression trueBody = new ParameterReplacer(whenTrueExpr.Parameters[0], param).Visit(whenTrueExpr.Body);
            Expression falseBody = new ParameterReplacer(whenFalseExpr.Parameters[0], param).Visit(whenFalseExpr.Body);

            // For the condition, we invoke the compiled delegate
            InvocationExpression conditionInvoke = Expression.Invoke(Expression.Constant(_condition), param);
            ConditionalExpression conditional = Expression.Condition(conditionInvoke, trueBody, falseBody);

            return Expression.Lambda<Func<T, bool>>(conditional, param);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? _newParam : base.VisitParameter(node);
            }
        }
    }

    /// <summary>
    /// Builder for conditional specifications.
    /// </summary>
    public class ConditionalSpecificationBuilder<T>
    {
        private readonly ISpecification<T> _baseSpec;
        private readonly Func<T, bool> _condition;
        private readonly ISpecification<T> _whenTrue;

        internal ConditionalSpecificationBuilder(ISpecification<T> baseSpec, Func<T, bool> condition, ISpecification<T> whenTrue)
        {
            _baseSpec = baseSpec;
            _condition = condition;
            _whenTrue = whenTrue;
        }

        /// <summary>
        /// Specifies the specification to apply when the condition is false.
        /// </summary>
        public ISpecification<T> Otherwise(ISpecification<T> specification)
        {
            ConditionalSpecification<T> conditional = new ConditionalSpecification<T>(_condition, _whenTrue, specification);
            return _baseSpec == null ? conditional : _baseSpec.And(conditional);
        }

        /// <summary>
        /// Uses AnySpecification when the condition is false (always passes).
        /// </summary>
        public ISpecification<T> OtherwisePass()
        {
            return Otherwise(new AnySpecification<T>());
        }

        /// <summary>
        /// Uses NoneSpecification when the condition is false (always fails).
        /// </summary>
        public ISpecification<T> OtherwiseFail()
        {
            return Otherwise(new NoneSpecification<T>());
        }
    }
}