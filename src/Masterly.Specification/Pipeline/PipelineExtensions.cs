using System;

namespace Masterly.Specification
{
    /// <summary>
    /// Extension methods for specification pipelines and conditional specifications.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Applies an additional specification only when the condition is met.
        /// </summary>
        public static ConditionalSpecificationBuilder<T> When<T>(
            this ISpecification<T> specification,
            Func<T, bool> condition,
            ISpecification<T> thenSpec)
        {
            return new ConditionalSpecificationBuilder<T>(specification, condition, thenSpec);
        }

        /// <summary>
        /// Applies an additional specification only when the condition is met (starting fresh).
        /// </summary>
        public static ConditionalSpecificationBuilder<T> When<T>(
            Func<T, bool> condition,
            ISpecification<T> thenSpec)
        {
            return new ConditionalSpecificationBuilder<T>(null, condition, thenSpec);
        }

        /// <summary>
        /// Applies the specification only when a boolean condition is true, otherwise passes.
        /// </summary>
        public static ISpecification<T> OnlyWhen<T>(
            this ISpecification<T> specification,
            bool condition)
        {
            return condition ? specification : new AnySpecification<T>();
        }

        /// <summary>
        /// Applies the specification only when a boolean condition is true, otherwise passes.
        /// </summary>
        public static ISpecification<T> OnlyWhen<T>(
            this ISpecification<T> specification,
            Func<bool> condition)
        {
            return condition() ? specification : new AnySpecification<T>();
        }

        /// <summary>
        /// Skips this specification when the condition is true.
        /// </summary>
        public static ISpecification<T> SkipWhen<T>(
            this ISpecification<T> specification,
            bool condition)
        {
            return condition ? new AnySpecification<T>() : specification;
        }

        /// <summary>
        /// Combines specifications with short-circuit OR logic.
        /// If the first specification passes, the second is not evaluated.
        /// </summary>
        public static ISpecification<T> OrElse<T>(
            this ISpecification<T> first,
            Func<ISpecification<T>> second)
        {
            return new LazyOrSpecification<T>(first, second);
        }

        /// <summary>
        /// Combines specifications with short-circuit AND logic.
        /// If the first specification fails, the second is not evaluated.
        /// </summary>
        public static ISpecification<T> AndThen<T>(
            this ISpecification<T> first,
            Func<ISpecification<T>> second)
        {
            return new LazyAndSpecification<T>(first, second);
        }

        /// <summary>
        /// Converts a specification to an optional specification that can be null.
        /// When null, the specification passes.
        /// </summary>
        public static ISpecification<T> AsOptional<T>(this ISpecification<T> specification)
        {
            return specification ?? new AnySpecification<T>();
        }

        /// <summary>
        /// Chains multiple specifications together, applying them in sequence.
        /// </summary>
        public static ISpecification<T> Chain<T>(
            this ISpecification<T> first,
            params ISpecification<T>[] others)
        {
            var result = first;
            foreach (var spec in others)
            {
                result = result.And(spec);
            }
            return result;
        }
    }

    internal class LazyOrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _first;
        private readonly Func<ISpecification<T>> _secondFactory;

        public LazyOrSpecification(ISpecification<T> first, Func<ISpecification<T>> secondFactory)
        {
            _first = first;
            _secondFactory = secondFactory;
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _first.IsSatisfiedBy(obj) || _secondFactory().IsSatisfiedBy(obj);
        }

        public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
        {
            return _first.ToExpression().Or(_secondFactory().ToExpression());
        }
    }

    internal class LazyAndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _first;
        private readonly Func<ISpecification<T>> _secondFactory;

        public LazyAndSpecification(ISpecification<T> first, Func<ISpecification<T>> secondFactory)
        {
            _first = first;
            _secondFactory = secondFactory;
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _first.IsSatisfiedBy(obj) && _secondFactory().IsSatisfiedBy(obj);
        }

        public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
        {
            return _first.ToExpression().And(_secondFactory().ToExpression());
        }
    }
}
