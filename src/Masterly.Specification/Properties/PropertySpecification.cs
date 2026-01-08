using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Property-based specification with type-safe comparisons.
    /// </summary>
    public class PropertySpecification<T, TProperty>
    {
        private readonly Expression<Func<T, TProperty>> _propertySelector;

        public PropertySpecification(Expression<Func<T, TProperty>> propertySelector)
        {
            _propertySelector = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));
        }

        /// <summary>
        /// Creates a specification where the property equals the specified value.
        /// </summary>
        public ISpecification<T> EqualTo(TProperty value)
        {
            var param = _propertySelector.Parameters[0];
            var body = Expression.Equal(_propertySelector.Body, Expression.Constant(value, typeof(TProperty)));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the property does not equal the specified value.
        /// </summary>
        public ISpecification<T> NotEqualTo(TProperty value)
        {
            var param = _propertySelector.Parameters[0];
            var body = Expression.NotEqual(_propertySelector.Body, Expression.Constant(value, typeof(TProperty)));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the property is null.
        /// </summary>
        public ISpecification<T> IsNull()
        {
            var param = _propertySelector.Parameters[0];
            var body = Expression.Equal(_propertySelector.Body, Expression.Constant(null, typeof(TProperty)));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the property is not null.
        /// </summary>
        public ISpecification<T> IsNotNull()
        {
            var param = _propertySelector.Parameters[0];
            var body = Expression.NotEqual(_propertySelector.Body, Expression.Constant(null, typeof(TProperty)));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the property is in the specified collection.
        /// </summary>
        public ISpecification<T> In(params TProperty[] values) => In((IEnumerable<TProperty>)values);

        /// <summary>
        /// Creates a specification where the property is in the specified collection.
        /// </summary>
        public ISpecification<T> In(IEnumerable<TProperty> values)
        {
            var valuesList = values.ToList();
            var param = _propertySelector.Parameters[0];
            var containsMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TProperty));

            var body = Expression.Call(containsMethod, Expression.Constant(valuesList), _propertySelector.Body);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the property is not in the specified collection.
        /// </summary>
        public ISpecification<T> NotIn(params TProperty[] values) => In(values).Not();

        /// <summary>
        /// Creates a specification where the property is not in the specified collection.
        /// </summary>
        public ISpecification<T> NotIn(IEnumerable<TProperty> values) => In(values).Not();

        /// <summary>
        /// Creates a specification using a custom predicate on the property value.
        /// </summary>
        public ISpecification<T> Matches(Expression<Func<TProperty, bool>> predicate)
        {
            var param = _propertySelector.Parameters[0];
            var propertyAccess = _propertySelector.Body;
            var predicateBody = new ParameterReplacer(predicate.Parameters[0], propertyAccess).Visit(predicate.Body);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(predicateBody, param));
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly Expression _replacement;

            public ParameterReplacer(ParameterExpression oldParam, Expression replacement)
            {
                _oldParam = oldParam;
                _replacement = replacement;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParam ? _replacement : base.VisitParameter(node);
            }
        }
    }
}
