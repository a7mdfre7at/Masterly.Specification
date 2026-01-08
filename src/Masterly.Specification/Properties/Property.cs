using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Entry point for property-based specifications with rich comparison operators.
    /// </summary>
    public static class Property<T>
    {
        /// <summary>
        /// Creates a property specification builder for the specified property.
        /// </summary>
        public static PropertySpecification<T, TProperty> For<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            return new PropertySpecification<T, TProperty>(propertySelector);
        }
    }
}
