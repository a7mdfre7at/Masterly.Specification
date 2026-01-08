using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Masterly.Specification
{
    /// <summary>
    /// Extensions for collection property specifications.
    /// </summary>
    public static class CollectionPropertyExtensions
    {
        /// <summary>
        /// Creates a specification where the collection contains the specified value.
        /// </summary>
        public static ISpecification<T> Contains<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property, TElement value)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var containsMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TElement));

            var body = Expression.Call(containsMethod, propInfo.PropertyAccess, Expression.Constant(value, typeof(TElement)));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the collection is empty.
        /// </summary>
        public static ISpecification<T> IsEmpty<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var anyMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TElement));

            var anyCall = Expression.Call(anyMethod, propInfo.PropertyAccess);
            var body = Expression.Not(anyCall);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the collection is not empty.
        /// </summary>
        public static ISpecification<T> IsNotEmpty<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property)
        {
            return property.IsEmpty().Not();
        }

        /// <summary>
        /// Creates a specification where the collection has the exact count.
        /// </summary>
        public static ISpecification<T> HasCount<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property, int count)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var countMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TElement));

            var countCall = Expression.Call(countMethod, propInfo.PropertyAccess);
            var body = Expression.Equal(countCall, Expression.Constant(count));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the collection count is at least the specified value.
        /// </summary>
        public static ISpecification<T> HasMinCount<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property, int minCount)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var countMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TElement));

            var countCall = Expression.Call(countMethod, propInfo.PropertyAccess);
            var body = Expression.GreaterThanOrEqual(countCall, Expression.Constant(minCount));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the collection count is at most the specified value.
        /// </summary>
        public static ISpecification<T> HasMaxCount<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property, int maxCount)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var countMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Count" && m.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TElement));

            var countCall = Expression.Call(countMethod, propInfo.PropertyAccess);
            var body = Expression.LessThanOrEqual(countCall, Expression.Constant(maxCount));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where any element in the collection satisfies the predicate.
        /// </summary>
        public static ISpecification<T> Any<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property,
            Expression<Func<TElement, bool>> predicate)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var anyMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TElement));

            var body = Expression.Call(anyMethod, propInfo.PropertyAccess, predicate);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where all elements in the collection satisfy the predicate.
        /// </summary>
        public static ISpecification<T> All<T, TElement>(
            this PropertySpecification<T, IEnumerable<TElement>> property,
            Expression<Func<TElement, bool>> predicate)
        {
            var propInfo = GetPropertyInfo(property);
            var param = propInfo.Parameter;
            var allMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "All" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TElement));

            var body = Expression.Call(allMethod, propInfo.PropertyAccess, predicate);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        private static (ParameterExpression Parameter, Expression PropertyAccess) GetPropertyInfo<T, TElement>(
            PropertySpecification<T, IEnumerable<TElement>> property)
        {
            var selectorField = typeof(PropertySpecification<T, IEnumerable<TElement>>)
                .GetField("_propertySelector", BindingFlags.NonPublic | BindingFlags.Instance);
            var selector = (Expression<Func<T, IEnumerable<TElement>>>)selectorField.GetValue(property);
            return (selector.Parameters[0], selector.Body);
        }
    }
}
