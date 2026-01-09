using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masterly.Specification
{
    /// <summary>
    /// Represents a specification where ALL of the given specifications must be satisfied.
    /// </summary>
    /// <typeparam name="T">The type of the object to which the specification is applied.</typeparam>
    public class AllSpecification<T> : Specification<T>
    {
        private readonly IReadOnlyList<ISpecification<T>> _specifications;

        public AllSpecification(params ISpecification<T>[] specifications)
            : this((IEnumerable<ISpecification<T>>)specifications)
        {
        }

        public AllSpecification(IEnumerable<ISpecification<T>> specifications)
        {
            _specifications = specifications?.ToList() ?? throw new ArgumentNullException(nameof(specifications));

            if (_specifications.Count == 0)
                throw new ArgumentException("At least one specification is required.", nameof(specifications));
        }

        public IReadOnlyList<ISpecification<T>> Specifications => _specifications;

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> result = _specifications[0].ToExpression();

            for (int i = 1; i < _specifications.Count; i++)
            {
                result = result.And(_specifications[i].ToExpression());
            }

            return result;
        }
    }
}