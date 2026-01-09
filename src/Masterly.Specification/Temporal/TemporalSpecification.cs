using System;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Specification builder for DateTime properties.
    /// </summary>
    public class TemporalSpecification<T>
    {
        private readonly Expression<Func<T, DateTime>> _propertySelector;

        public TemporalSpecification(Expression<Func<T, DateTime>> propertySelector)
        {
            _propertySelector = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));
        }

        /// <summary>
        /// Creates a specification where the date is before the specified date.
        /// </summary>
        public ISpecification<T> Before(DateTime date)
        {
            return CreateComparison(Expression.LessThan, date);
        }

        /// <summary>
        /// Creates a specification where the date is after the specified date.
        /// </summary>
        public ISpecification<T> After(DateTime date)
        {
            return CreateComparison(Expression.GreaterThan, date);
        }

        /// <summary>
        /// Creates a specification where the date is on or before the specified date.
        /// </summary>
        public ISpecification<T> OnOrBefore(DateTime date)
        {
            return CreateComparison(Expression.LessThanOrEqual, date);
        }

        /// <summary>
        /// Creates a specification where the date is on or after the specified date.
        /// </summary>
        public ISpecification<T> OnOrAfter(DateTime date)
        {
            return CreateComparison(Expression.GreaterThanOrEqual, date);
        }

        /// <summary>
        /// Creates a specification where the date is between two dates (inclusive).
        /// </summary>
        public ISpecification<T> Between(DateTime start, DateTime end)
        {
            return OnOrAfter(start).And(OnOrBefore(end));
        }

        /// <summary>
        /// Creates a specification where the date is within the specified timespan from now.
        /// </summary>
        public ISpecification<T> Within(TimeSpan timeSpan)
        {
            DateTime now = DateTime.UtcNow;
            return Between(now.Subtract(timeSpan), now.Add(timeSpan));
        }

        /// <summary>
        /// Creates a specification where the date is in the past.
        /// </summary>
        public ISpecification<T> InThePast()
        {
            return Before(DateTime.UtcNow);
        }

        /// <summary>
        /// Creates a specification where the date is in the future.
        /// </summary>
        public ISpecification<T> InTheFuture()
        {
            return After(DateTime.UtcNow);
        }

        /// <summary>
        /// Creates a specification where the date is today.
        /// </summary>
        public ISpecification<T> Today()
        {
            DateTime today = DateTime.Today;
            return Between(today, today.AddDays(1).AddTicks(-1));
        }

        /// <summary>
        /// Creates a specification where the date is in the specified year.
        /// </summary>
        public ISpecification<T> InYear(int year)
        {
            DateTime start = new DateTime(year, 1, 1);
            DateTime end = new DateTime(year, 12, 31, 23, 59, 59, 999);
            return Between(start, end);
        }

        /// <summary>
        /// Creates a specification where the date is in the specified month of the specified year.
        /// </summary>
        public ISpecification<T> InMonth(int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddTicks(-1);
            return Between(start, end);
        }

        /// <summary>
        /// Creates a specification where the date is on the specified day of the week.
        /// </summary>
        public ISpecification<T> OnDayOfWeek(DayOfWeek dayOfWeek)
        {
            ParameterExpression param = _propertySelector.Parameters[0];
            MemberExpression dayOfWeekProperty = Expression.Property(_propertySelector.Body, "DayOfWeek");
            BinaryExpression body = Expression.Equal(dayOfWeekProperty, Expression.Constant(dayOfWeek));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        /// <summary>
        /// Creates a specification where the date is on a weekend.
        /// </summary>
        public ISpecification<T> OnWeekend()
        {
            return OnDayOfWeek(DayOfWeek.Saturday).Or(OnDayOfWeek(DayOfWeek.Sunday));
        }

        /// <summary>
        /// Creates a specification where the date is on a weekday.
        /// </summary>
        public ISpecification<T> OnWeekday()
        {
            return OnWeekend().Not();
        }

        /// <summary>
        /// Creates a specification where the date is within the last N days.
        /// </summary>
        public ISpecification<T> WithinLastDays(int days)
        {
            return After(DateTime.UtcNow.AddDays(-days));
        }

        /// <summary>
        /// Creates a specification where the date is within the next N days.
        /// </summary>
        public ISpecification<T> WithinNextDays(int days)
        {
            DateTime now = DateTime.UtcNow;
            return After(now).And(Before(now.AddDays(days)));
        }

        /// <summary>
        /// Creates a specification where the time portion is between the specified times.
        /// </summary>
        public ISpecification<T> TimeBetween(TimeSpan start, TimeSpan end)
        {
            ParameterExpression param = _propertySelector.Parameters[0];
            MemberExpression timeOfDayProperty = Expression.Property(_propertySelector.Body, "TimeOfDay");
            BinaryExpression startCheck = Expression.GreaterThanOrEqual(timeOfDayProperty, Expression.Constant(start));
            BinaryExpression endCheck = Expression.LessThanOrEqual(timeOfDayProperty, Expression.Constant(end));
            BinaryExpression body = Expression.AndAlso(startCheck, endCheck);
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }

        private ISpecification<T> CreateComparison(
            Func<Expression, Expression, BinaryExpression> comparison, DateTime value)
        {
            ParameterExpression param = _propertySelector.Parameters[0];
            BinaryExpression body = comparison(_propertySelector.Body, Expression.Constant(value));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }
    }

    /// <summary>
    /// Specification builder for nullable DateTime properties.
    /// </summary>
    public class NullableTemporalSpecification<T>
    {
        private readonly Expression<Func<T, DateTime?>> _propertySelector;

        public NullableTemporalSpecification(Expression<Func<T, DateTime?>> propertySelector)
        {
            _propertySelector = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));
        }

        /// <summary>
        /// Creates a specification where the date has a value.
        /// </summary>
        public ISpecification<T> HasValue()
        {
            ParameterExpression param = _propertySelector.Parameters[0];
            MemberExpression hasValueProperty = Expression.Property(_propertySelector.Body, "HasValue");
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(hasValueProperty, param));
        }

        /// <summary>
        /// Creates a specification where the date has no value (is null).
        /// </summary>
        public ISpecification<T> IsNull()
        {
            return HasValue().Not();
        }

        /// <summary>
        /// Creates a specification where the date has a value and satisfies the inner specification.
        /// </summary>
        public ISpecification<T> HasValueAnd(Func<TemporalSpecification<T>, ISpecification<T>> configure)
        {
            ParameterExpression param = _propertySelector.Parameters[0];
            MemberExpression valueProperty = Expression.Property(_propertySelector.Body, "Value");
            Expression<Func<T, DateTime>> innerSelector = Expression.Lambda<Func<T, DateTime>>(valueProperty, param);
            ISpecification<T> innerSpec = configure(new TemporalSpecification<T>(innerSelector));
            return HasValue().And(innerSpec);
        }
    }

    /// <summary>
    /// Specification builder for DateTimeOffset properties.
    /// </summary>
    public class DateTimeOffsetSpecification<T>
    {
        private readonly Expression<Func<T, DateTimeOffset>> _propertySelector;

        public DateTimeOffsetSpecification(T propertySelector)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffsetSpecification(Expression<Func<T, DateTimeOffset>> propertySelector)
        {
            _propertySelector = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));
        }

        /// <summary>
        /// Creates a specification where the date is before the specified date.
        /// </summary>
        public ISpecification<T> Before(DateTimeOffset date)
        {
            return CreateComparison(Expression.LessThan, date);
        }

        /// <summary>
        /// Creates a specification where the date is after the specified date.
        /// </summary>
        public ISpecification<T> After(DateTimeOffset date)
        {
            return CreateComparison(Expression.GreaterThan, date);
        }

        /// <summary>
        /// Creates a specification where the date is between two dates (inclusive).
        /// </summary>
        public ISpecification<T> Between(DateTimeOffset start, DateTimeOffset end)
        {
            ISpecification<T> afterStart = CreateComparison(Expression.GreaterThanOrEqual, start);
            ISpecification<T> beforeEnd = CreateComparison(Expression.LessThanOrEqual, end);
            return afterStart.And(beforeEnd);
        }

        private ISpecification<T> CreateComparison(
            Func<Expression, Expression, BinaryExpression> comparison, DateTimeOffset value)
        {
            ParameterExpression param = _propertySelector.Parameters[0];
            BinaryExpression body = comparison(_propertySelector.Body, Expression.Constant(value));
            return new ExpressionSpecification<T>(Expression.Lambda<Func<T, bool>>(body, param));
        }
    }
}