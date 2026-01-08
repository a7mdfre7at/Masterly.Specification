using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Masterly.Specification
{
    /// <summary>
    /// Extensions for string property specifications.
    /// </summary>
    public static class StringPropertyExtensions
    {
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        private static readonly MethodInfo IsNullOrEmptyMethod = typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) });
        private static readonly MethodInfo IsNullOrWhiteSpaceMethod = typeof(string).GetMethod("IsNullOrWhiteSpace", new[] { typeof(string) });

        /// <summary>
        /// Creates a specification where the string starts with the specified value.
        /// </summary>
        public static ISpecification<T> StartsWith<T>(this PropertySpecification<T, string> property, string value)
        {
            return CreateStringMethodSpec(property, StartsWithMethod, value);
        }

        /// <summary>
        /// Creates a specification where the string ends with the specified value.
        /// </summary>
        public static ISpecification<T> EndsWith<T>(this PropertySpecification<T, string> property, string value)
        {
            return CreateStringMethodSpec(property, EndsWithMethod, value);
        }

        /// <summary>
        /// Creates a specification where the string contains the specified value.
        /// </summary>
        public static ISpecification<T> Contains<T>(this PropertySpecification<T, string> property, string value)
        {
            return CreateStringMethodSpec(property, ContainsMethod, value);
        }

        /// <summary>
        /// Creates a specification where the string equals the value (case-insensitive).
        /// </summary>
        public static ISpecification<T> EqualsIgnoreCase<T>(this PropertySpecification<T, string> property, string value)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var toLower = Expression.Call(propInfo.PropertyAccess, ToLowerMethod);
            var body = Expression.Equal(toLower, Expression.Constant(value?.ToLower()));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the string contains the value (case-insensitive).
        /// </summary>
        public static ISpecification<T> ContainsIgnoreCase<T>(this PropertySpecification<T, string> property, string value)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var toLower = Expression.Call(propInfo.PropertyAccess, ToLowerMethod);
            var body = Expression.Call(toLower, ContainsMethod, Expression.Constant(value?.ToLower()));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the string is null or empty.
        /// </summary>
        public static ISpecification<T> IsNullOrEmpty<T>(this PropertySpecification<T, string> property)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var body = Expression.Call(IsNullOrEmptyMethod, propInfo.PropertyAccess);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the string is not null or empty.
        /// </summary>
        public static ISpecification<T> IsNotNullOrEmpty<T>(this PropertySpecification<T, string> property)
        {
            return property.IsNullOrEmpty().Not();
        }

        /// <summary>
        /// Creates a specification where the string is null or whitespace.
        /// </summary>
        public static ISpecification<T> IsNullOrWhiteSpace<T>(this PropertySpecification<T, string> property)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var body = Expression.Call(IsNullOrWhiteSpaceMethod, propInfo.PropertyAccess);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the string has content (not null or whitespace).
        /// </summary>
        public static ISpecification<T> HasContent<T>(this PropertySpecification<T, string> property)
        {
            return property.IsNullOrWhiteSpace().Not();
        }

        /// <summary>
        /// Creates a specification where the string length equals the specified value.
        /// </summary>
        public static ISpecification<T> HasLength<T>(this PropertySpecification<T, string> property, int length)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var lengthProperty = Expression.Property(propInfo.PropertyAccess, "Length");
            var body = Expression.Equal(lengthProperty, Expression.Constant(length));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the string length is within the specified range.
        /// </summary>
        public static ISpecification<T> HasLengthBetween<T>(this PropertySpecification<T, string> property, int min, int max)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var lengthProperty = Expression.Property(propInfo.PropertyAccess, "Length");
            var minCheck = Expression.GreaterThanOrEqual(lengthProperty, Expression.Constant(min));
            var maxCheck = Expression.LessThanOrEqual(lengthProperty, Expression.Constant(max));
            var body = Expression.AndAlso(minCheck, maxCheck);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        private static ISpecification<T> CreateStringMethodSpec<T>(
            PropertySpecification<T, string> property, MethodInfo method, string value)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var body = Expression.Call(propInfo.PropertyAccess, method, Expression.Constant(value));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        private static (ParameterExpression Parameter, Expression PropertyAccess) GetPropertyInfo<T>(
            PropertySpecification<T, string> property)
        {
            var selectorField = typeof(PropertySpecification<T, string>)
                .GetField("_propertySelector", BindingFlags.NonPublic | BindingFlags.Instance);
            var selector = (Expression<Func<T, string>>)selectorField.GetValue(property);
            return (selector.Parameters[0], selector.Body);
        }
    }
}
