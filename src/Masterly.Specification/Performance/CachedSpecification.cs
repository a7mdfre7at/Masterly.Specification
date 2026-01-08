using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// A specification wrapper that caches the compiled expression for improved performance.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class CachedSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _innerSpecification;
        private readonly Lazy<Func<T, bool>> _compiledExpression;
        private readonly Expression<Func<T, bool>> _expression;

        public CachedSpecification(ISpecification<T> specification)
        {
            _innerSpecification = specification ?? throw new ArgumentNullException(nameof(specification));
            _expression = specification.ToExpression();
            _compiledExpression = new Lazy<Func<T, bool>>(() => _expression.Compile());
        }

        /// <summary>
        /// Gets the compiled delegate for direct invocation.
        /// </summary>
        public Func<T, bool> CompiledPredicate => _compiledExpression.Value;

        public override bool IsSatisfiedBy(T obj)
        {
            return _compiledExpression.Value(obj);
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return _expression;
        }
    }
}
