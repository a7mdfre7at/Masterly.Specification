using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Fluent builder for creating specifications without defining classes.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class SpecificationBuilder<T>
    {
        private ISpecification<T> _specification;

        internal SpecificationBuilder()
        {
        }

        internal SpecificationBuilder(ISpecification<T> specification)
        {
            _specification = specification;
        }

        /// <summary>
        /// Starts the specification with a predicate.
        /// </summary>
        public SpecificationBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (_specification == null)
                _specification = new ExpressionSpecification<T>(predicate);
            else
                _specification = _specification.And(new ExpressionSpecification<T>(predicate));

            return this;
        }

        /// <summary>
        /// Adds an AND condition to the specification.
        /// </summary>
        public SpecificationBuilder<T> And(Expression<Func<T, bool>> predicate)
        {
            EnsureStarted();
            _specification = _specification.And(new ExpressionSpecification<T>(predicate));
            return this;
        }

        /// <summary>
        /// Adds an AND condition with another specification.
        /// </summary>
        public SpecificationBuilder<T> And(ISpecification<T> specification)
        {
            EnsureStarted();
            _specification = _specification.And(specification);
            return this;
        }

        /// <summary>
        /// Adds an OR condition to the specification.
        /// </summary>
        public SpecificationBuilder<T> Or(Expression<Func<T, bool>> predicate)
        {
            EnsureStarted();
            _specification = _specification.Or(new ExpressionSpecification<T>(predicate));
            return this;
        }

        /// <summary>
        /// Adds an OR condition with another specification.
        /// </summary>
        public SpecificationBuilder<T> Or(ISpecification<T> specification)
        {
            EnsureStarted();
            _specification = _specification.Or(specification);
            return this;
        }

        /// <summary>
        /// Adds an AND NOT condition to the specification.
        /// </summary>
        public SpecificationBuilder<T> AndNot(Expression<Func<T, bool>> predicate)
        {
            EnsureStarted();
            _specification = _specification.AndNot(new ExpressionSpecification<T>(predicate));
            return this;
        }

        /// <summary>
        /// Adds an AND NOT condition with another specification.
        /// </summary>
        public SpecificationBuilder<T> AndNot(ISpecification<T> specification)
        {
            EnsureStarted();
            _specification = _specification.AndNot(specification);
            return this;
        }

        /// <summary>
        /// Negates the current specification.
        /// </summary>
        public SpecificationBuilder<T> Not()
        {
            EnsureStarted();
            _specification = _specification.Not();
            return this;
        }

        /// <summary>
        /// Adds an XOR condition to the specification.
        /// </summary>
        public SpecificationBuilder<T> Xor(Expression<Func<T, bool>> predicate)
        {
            EnsureStarted();
            _specification = _specification.Xor(new ExpressionSpecification<T>(predicate));
            return this;
        }

        /// <summary>
        /// Adds an XOR condition with another specification.
        /// </summary>
        public SpecificationBuilder<T> Xor(ISpecification<T> specification)
        {
            EnsureStarted();
            _specification = _specification.Xor(specification);
            return this;
        }

        /// <summary>
        /// Groups the current specification with parentheses (for clarity in complex expressions).
        /// </summary>
        public SpecificationBuilder<T> Group(Func<SpecificationBuilder<T>, SpecificationBuilder<T>> groupBuilder)
        {
            var innerBuilder = new SpecificationBuilder<T>();
            var result = groupBuilder(innerBuilder);
            var innerSpec = result.Build();

            if (_specification == null)
                _specification = innerSpec;
            else
                _specification = _specification.And(innerSpec);

            return this;
        }

        /// <summary>
        /// Creates a grouped OR expression.
        /// </summary>
        public SpecificationBuilder<T> OrGroup(Func<SpecificationBuilder<T>, SpecificationBuilder<T>> groupBuilder)
        {
            EnsureStarted();
            var innerBuilder = new SpecificationBuilder<T>();
            var result = groupBuilder(innerBuilder);
            _specification = _specification.Or(result.Build());
            return this;
        }

        /// <summary>
        /// Builds the final specification.
        /// </summary>
        public ISpecification<T> Build()
        {
            return _specification ?? new AnySpecification<T>();
        }

        /// <summary>
        /// Implicitly converts the builder to a specification.
        /// </summary>
        public static implicit operator Specification<T>(SpecificationBuilder<T> builder)
        {
            return new ExpressionSpecification<T>(builder.Build().ToExpression());
        }

        private void EnsureStarted()
        {
            if (_specification == null)
                throw new InvalidOperationException("Specification not started. Call Where() first.");
        }
    }
}
