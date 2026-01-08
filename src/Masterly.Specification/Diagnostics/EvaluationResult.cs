using System.Collections.Generic;
using System.Linq;

namespace Masterly.Specification
{
    /// <summary>
    /// Contains detailed evaluation results with reasons for pass/fail.
    /// </summary>
    public class EvaluationResult
    {
        public bool IsSatisfied { get; }
        public IReadOnlyList<EvaluationDetail> Details { get; }
        public string Summary => IsSatisfied ? "PASSED" : "FAILED";

        public EvaluationResult(bool isSatisfied, IEnumerable<EvaluationDetail> details)
        {
            IsSatisfied = isSatisfied;
            Details = details?.ToList() ?? new List<EvaluationDetail>();
        }

        public IEnumerable<string> GetFailureReasons()
        {
            return Details.Where(d => !d.Passed).Select(d => d.ToString());
        }

        public IEnumerable<string> GetPassedConditions()
        {
            return Details.Where(d => d.Passed).Select(d => d.ToString());
        }
    }

    /// <summary>
    /// Details about a single condition evaluation.
    /// </summary>
    public class EvaluationDetail
    {
        public string Condition { get; }
        public bool Passed { get; }
        public object ActualValue { get; }
        public object ExpectedValue { get; }

        public EvaluationDetail(string condition, bool passed, object actualValue = null, object expectedValue = null)
        {
            Condition = condition;
            Passed = passed;
            ActualValue = actualValue;
            ExpectedValue = expectedValue;
        }

        public override string ToString()
        {
            var status = Passed ? "PASSED" : "FAILED";
            if (ActualValue != null && !Passed)
                return $"{Condition}: {status} (actual: {ActualValue})";
            return $"{Condition}: {status}";
        }
    }
}
