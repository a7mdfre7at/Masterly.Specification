using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Entry point for temporal (date/time) specifications.
    /// </summary>
    public static class Temporal<T>
    {
        /// <summary>
        /// Creates a temporal specification for a DateTime property.
        /// </summary>
        public static TemporalSpecification<T> For(Expression<Func<T, DateTime>> propertySelector)
        {
            return new TemporalSpecification<T>(propertySelector);
        }

        /// <summary>
        /// Creates a temporal specification for a nullable DateTime property.
        /// </summary>
        public static NullableTemporalSpecification<T> For(Expression<Func<T, DateTime?>> propertySelector)
        {
            return new NullableTemporalSpecification<T>(propertySelector);
        }

        /// <summary>
        /// Creates a temporal specification for a DateTimeOffset property.
        /// </summary>
        public static DateTimeOffsetSpecification<T> For(Expression<Func<T, DateTimeOffset>> propertySelector)
        {
            return new DateTimeOffsetSpecification<T>(propertySelector);
        }
    }
}
