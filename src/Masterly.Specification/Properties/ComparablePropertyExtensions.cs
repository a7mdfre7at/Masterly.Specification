using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Extensions for comparable property specifications.
    /// </summary>
    public static class ComparablePropertyExtensions
    {
        /// <summary>
        /// Creates a specification where the property is greater than the specified value.
        /// </summary>
        public static ISpecification<T> GreaterThan<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty value)
            where TProperty : IComparable<TProperty>
        {
            return CreateComparisonSpec(property, value, Expression.GreaterThan);
        }

        /// <summary>
        /// Creates a specification where the property is greater than or equal to the specified value.
        /// </summary>
        public static ISpecification<T> GreaterThanOrEqual<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty value)
            where TProperty : IComparable<TProperty>
        {
            return CreateComparisonSpec(property, value, Expression.GreaterThanOrEqual);
        }

        /// <summary>
        /// Creates a specification where the property is less than the specified value.
        /// </summary>
        public static ISpecification<T> LessThan<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty value)
            where TProperty : IComparable<TProperty>
        {
            return CreateComparisonSpec(property, value, Expression.LessThan);
        }

        /// <summary>
        /// Creates a specification where the property is less than or equal to the specified value.
        /// </summary>
        public static ISpecification<T> LessThanOrEqual<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty value)
            where TProperty : IComparable<TProperty>
        {
            return CreateComparisonSpec(property, value, Expression.LessThanOrEqual);
        }

        /// <summary>
        /// Creates a specification where the property is within the specified range (inclusive).
        /// </summary>
        public static ISpecification<T> InRange<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty min, TProperty max)
            where TProperty : IComparable<TProperty>
        {
            return property.GreaterThanOrEqual(min).And(property.LessThanOrEqual(max));
        }

        /// <summary>
        /// Creates a specification where the property is within the specified exclusive range.
        /// </summary>
        public static ISpecification<T> InRangeExclusive<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty min, TProperty max)
            where TProperty : IComparable<TProperty>
        {
            return property.GreaterThan(min).And(property.LessThan(max));
        }

        /// <summary>
        /// Creates a specification where the property is outside the specified range.
        /// </summary>
        public static ISpecification<T> OutsideRange<T, TProperty>(
            this PropertySpecification<T, TProperty> property, TProperty min, TProperty max)
            where TProperty : IComparable<TProperty>
        {
            return property.LessThan(min).Or(property.GreaterThan(max));
        }

        private static ISpecification<T> CreateComparisonSpec<T, TProperty>(
            PropertySpecification<T, TProperty> property,
            TProperty value,
            Func<Expression, Expression, BinaryExpression> comparison)
            where TProperty : IComparable<TProperty>
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var body = comparison(propInfo.PropertyAccess, Expression.Constant(value, typeof(TProperty)));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        private static (ParameterExpression Parameter, Expression PropertyAccess) GetPropertyInfo<T, TProperty>(
            PropertySpecification<T, TProperty> property)
        {
            var selectorField = typeof(PropertySpecification<T, TProperty>)
                .GetField("_propertySelector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var selector = (Expression<Func<T, TProperty>>)selectorField.GetValue(property);
            return (selector.Parameters[0], selector.Body);
        }
    }
}
