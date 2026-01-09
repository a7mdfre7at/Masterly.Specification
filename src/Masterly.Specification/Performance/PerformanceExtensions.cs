using System;

namespace Masterly.Specification
{
    /// <summary>
    /// Extension methods for specification performance optimization.
    /// </summary>
    public static class PerformanceExtensions
    {
        /// <summary>
        /// Wraps the specification in a cached version for improved performance when
        /// evaluating the same specification multiple times.
        /// </summary>
        public static CachedSpecification<T> Cached<T>(this ISpecification<T> specification)
        {
            if (specification is CachedSpecification<T> cached)
                return cached;

            return new CachedSpecification<T>(specification);
        }

        /// <summary>
        /// Compiles the specification to a delegate for maximum performance.
        /// Use this when you need to evaluate the specification many times on different objects.
        /// </summary>
        public static Func<T, bool> Compile<T>(this ISpecification<T> specification)
        {
            return specification.ToExpression().Compile();
        }

        /// <summary>
        /// Creates a memoized version of the specification that caches results by object.
        /// Useful when the same objects are evaluated multiple times.
        /// </summary>
        public static ISpecification<T> Memoized<T>(this ISpecification<T> specification)
            where T : class
        {
            return new MemoizedSpecification<T>(specification);
        }
    }

    /// <summary>
    /// A specification that caches evaluation results by object reference.
    /// </summary>
    internal class MemoizedSpecification<T> : Specification<T> where T : class
    {
        private readonly ISpecification<T> _inner;
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<T, ResultBox> _cache;
        private readonly Lazy<Func<T, bool>> _compiled;

        public MemoizedSpecification(ISpecification<T> specification)
        {
            _inner = specification;
            _cache = new System.Runtime.CompilerServices.ConditionalWeakTable<T, ResultBox>();
            _compiled = new Lazy<Func<T, bool>>(() => _inner.ToExpression().Compile());
        }

        public override bool IsSatisfiedBy(T obj)
        {
            if (obj == null)
                return _compiled.Value(obj);

            return _cache.GetOrCreateValue(obj).GetOrCompute(() => _compiled.Value(obj));
        }

        public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
        {
            return _inner.ToExpression();
        }

        private class ResultBox
        {
            private bool _hasValue;
            private bool _value;
            private readonly object _lock = new object();

            public bool GetOrCompute(Func<bool> compute)
            {
                if (_hasValue)
                    return _value;

                lock (_lock)
                {
                    if (_hasValue)
                        return _value;

                    _value = compute();
                    _hasValue = true;
                    return _value;
                }
            }
        }
    }
}