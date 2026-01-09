using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Entry point for fluent specification building.
    /// </summary>
    public static class Spec
    {
        /// <summary>
        /// Creates a new specification builder for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
        public static SpecificationBuilder<T> For<T>() => new SpecificationBuilder<T>();

        /// <summary>
        /// Creates a specification from a predicate expression.
        /// </summary>
        public static ISpecification<T> Where<T>(Expression<Func<T, bool>> predicate)
            => new ExpressionSpecification<T>(predicate);

        /// <summary>
        /// Creates a specification that always returns true.
        /// </summary>
        public static ISpecification<T> Any<T>() => new AnySpecification<T>();

        /// <summary>
        /// Creates a specification that always returns false.
        /// </summary>
        public static ISpecification<T> None<T>() => new NoneSpecification<T>();
    }
}