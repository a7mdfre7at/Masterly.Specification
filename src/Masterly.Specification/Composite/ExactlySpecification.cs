using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents a specification where EXACTLY N of the given specifications must be satisfied.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class ExactlySpecification<T> : Specification<T>
    {
        private readonly IReadOnlyList<ISpecification<T>> _specifications;
        private readonly int _count;
        private Func<T, bool> _compiled;

        public ExactlySpecification(int count, params ISpecification<T>[] specifications)
            : this(count, (IEnumerable<ISpecification<T>>)specifications)
        {
        }

        public ExactlySpecification(int count, IEnumerable<ISpecification<T>> specifications)
        {
            _specifications = specifications?.ToList() ?? throw new ArgumentNullException(nameof(specifications));
            _count = count;

            if (_specifications.Count == 0)
                throw new ArgumentException("At least one specification is required.", nameof(specifications));

            if (count < 0 || count > _specifications.Count)
                throw new ArgumentOutOfRangeException(nameof(count), $"Count must be between 0 and {_specifications.Count}.");
        }

        public int Count => _count;
        public IReadOnlyList<ISpecification<T>> Specifications => _specifications;

        public override bool IsSatisfiedBy(T obj)
        {
            if (_compiled == null)
            {
                List<Func<T, bool>> compiledSpecs = _specifications.Select(s => s.ToExpression().Compile()).ToList();
                _compiled = o => compiledSpecs.Count(f => f(o)) == _count;
            }
            return _compiled(obj);
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            // For expression trees, we need to build the counting logic
            // This creates a complex expression that counts matching specifications
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression countExpr = BuildCountExpression(param);
            BinaryExpression comparison = Expression.Equal(countExpr, Expression.Constant(_count));

            return Expression.Lambda<Func<T, bool>>(comparison, param);
        }

        private Expression BuildCountExpression(ParameterExpression param)
        {
            Expression sum = Expression.Constant(0);

            foreach (ISpecification<T> spec in _specifications)
            {
                Expression<Func<T, bool>> specExpr = spec.ToExpression();
                Expression body = new ParameterReplacer(specExpr.Parameters[0], param).Visit(specExpr.Body);
                ConditionalExpression conditional = Expression.Condition(body, Expression.Constant(1), Expression.Constant(0));
                sum = Expression.Add(sum, conditional);
            }

            return sum;
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
}