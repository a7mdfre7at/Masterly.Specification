using Ardalis.GuardClauses;
using JetBrains.Annotations;

namespace Masterly.Specification
{
    public static class SpecificationExtensions
    {
        /// <summary>
        /// Combines the current specification instance with another specification instance
        /// and returns the combined specification which represents that both the current and
        /// the given specification must be satisfied by the given object.
        /// </summary>
        /// <param name="specification">The specification</param>
        /// <param name="other">The specification instance with which the current specification is combined.</param>
        /// <returns>The combined specification instance.</returns>
        public static ISpecification<T> And<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new AndSpecification<T>(specification, other);
        }

        /// <summary>
        /// Combines the current specification instance with another specification instance
        /// and returns the combined specification which represents that either the current or
        /// the given specification should be satisfied by the given object.
        /// </summary>
        /// <param name="specification">The specification</param>
        /// <param name="other">The specification instance with which the current specification
        /// is combined.</param>
        /// <returns>The combined specification instance.</returns>
        public static ISpecification<T> Or<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new OrSpecification<T>(specification, other);
        }

        /// <summary>
        /// Combines the current specification instance with another specification instance
        /// and returns the combined specification which represents that the current specification
        /// should be satisfied by the given object but the specified specification should not.
        /// </summary>
        /// <param name="specification">The specification</param>
        /// <param name="other">The specification instance with which the current specification
        /// is combined.</param>
        /// <returns>The combined specification instance.</returns>
        public static ISpecification<T> AndNot<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new AndNotSpecification<T>(specification, other);
        }

        /// <summary>
        /// Reverses the current specification instance and returns a specification which represents
        /// the semantics opposite to the current specification.
        /// </summary>
        /// <returns>The reversed specification instance.</returns>
        public static ISpecification<T> Not<T>([NotNull] this ISpecification<T> specification)
        {
            Guard.Against.Null(specification, nameof(specification));

            return new NotSpecification<T>(specification);
        }

        /// <summary>
        /// Combines two specifications with XOR (exclusive OR).
        /// Exactly one of the specifications must be satisfied.
        /// </summary>
        public static ISpecification<T> Xor<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new XorSpecification<T>(specification, other);
        }

        /// <summary>
        /// Creates a logical implication (A implies B).
        /// If the current specification is satisfied, the other must also be satisfied.
        /// </summary>
        public static ISpecification<T> Implies<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new ImpliesSpecification<T>(specification, other);
        }

        /// <summary>
        /// Creates a bi-conditional (A if and only if B).
        /// Both specifications must have the same result (both true or both false).
        /// </summary>
        public static ISpecification<T> Iff<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new IffSpecification<T>(specification, other);
        }

        /// <summary>
        /// Creates a NAND specification (NOT AND).
        /// Returns false only when both specifications are satisfied.
        /// </summary>
        public static ISpecification<T> Nand<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new NandSpecification<T>(specification, other);
        }

        /// <summary>
        /// Creates a NOR specification (NOT OR).
        /// Returns true only when both specifications are not satisfied.
        /// </summary>
        public static ISpecification<T> Nor<T>([NotNull] this ISpecification<T> specification,
            [NotNull] ISpecification<T> other)
        {
            Guard.Against.Null(specification, nameof(specification));
            Guard.Against.Null(other, nameof(other));

            return new NorSpecification<T>(specification, other);
        }
    }
}