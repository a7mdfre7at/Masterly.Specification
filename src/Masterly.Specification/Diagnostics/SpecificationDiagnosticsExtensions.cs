namespace Masterly.Specification
{
    /// <summary>
    /// Extension methods for specification diagnostics.
    /// </summary>
    public static class SpecificationDiagnosticsExtensions
    {
        /// <summary>
        /// Generates a human-readable explanation of the specification.
        /// </summary>
        public static string Explain<T>(this ISpecification<T> specification)
        {
            return SpecificationExplainer.Explain(specification);
        }

        /// <summary>
        /// Evaluates the specification against an object and returns detailed results.
        /// </summary>
        public static EvaluationResult Evaluate<T>(this ISpecification<T> specification, T obj)
        {
            return SpecificationExplainer.Evaluate(specification, obj);
        }

        /// <summary>
        /// Gets a detailed breakdown of why the specification passed or failed.
        /// </summary>
        public static string GetDetailedResult<T>(this ISpecification<T> specification, T obj)
        {
            EvaluationResult result = specification.Evaluate(obj);
            System.Text.StringBuilder lines = new System.Text.StringBuilder();
            lines.AppendLine($"Result: {result.Summary}");
            lines.AppendLine("Details:");
            foreach (EvaluationDetail detail in result.Details)
            {
                lines.AppendLine($"  - {detail}");
            }
            return lines.ToString();
        }
    }
}