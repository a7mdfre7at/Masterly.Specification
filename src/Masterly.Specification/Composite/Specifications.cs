using System.Collections.Generic;

namespace Masterly.Specification
{
    /// <summary>
    /// Static factory for creating N-ary composite specifications.
    /// </summary>
    public static class Specifications
    {
        /// <summary>
        /// Creates a specification where ALL of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> All<T>(params ISpecification<T>[] specifications)
            => new AllSpecification<T>(specifications);

        /// <summary>
        /// Creates a specification where ALL of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> All<T>(IEnumerable<ISpecification<T>> specifications)
            => new AllSpecification<T>(specifications);

        /// <summary>
        /// Creates a specification where ANY of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> AnyOf<T>(params ISpecification<T>[] specifications)
            => new AnyOfSpecification<T>(specifications);

        /// <summary>
        /// Creates a specification where ANY of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> AnyOf<T>(IEnumerable<ISpecification<T>> specifications)
            => new AnyOfSpecification<T>(specifications);

        /// <summary>
        /// Creates a specification where EXACTLY N of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> Exactly<T>(int count, params ISpecification<T>[] specifications)
            => new ExactlySpecification<T>(count, specifications);

        /// <summary>
        /// Creates a specification where EXACTLY N of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> Exactly<T>(int count, IEnumerable<ISpecification<T>> specifications)
            => new ExactlySpecification<T>(count, specifications);

        /// <summary>
        /// Creates a specification where AT LEAST N of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> AtLeast<T>(int count, params ISpecification<T>[] specifications)
            => new AtLeastSpecification<T>(count, specifications);

        /// <summary>
        /// Creates a specification where AT LEAST N of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> AtLeast<T>(int count, IEnumerable<ISpecification<T>> specifications)
            => new AtLeastSpecification<T>(count, specifications);

        /// <summary>
        /// Creates a specification where AT MOST N of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> AtMost<T>(int count, params ISpecification<T>[] specifications)
            => new AtMostSpecification<T>(count, specifications);

        /// <summary>
        /// Creates a specification where AT MOST N of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> AtMost<T>(int count, IEnumerable<ISpecification<T>> specifications)
            => new AtMostSpecification<T>(count, specifications);

        /// <summary>
        /// Creates a specification where NONE of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> NoneOf<T>(params ISpecification<T>[] specifications)
            => new AnyOfSpecification<T>(specifications).Not();

        /// <summary>
        /// Creates a specification where NONE of the given specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> NoneOf<T>(IEnumerable<ISpecification<T>> specifications)
            => new AnyOfSpecification<T>(specifications).Not();
    }
}