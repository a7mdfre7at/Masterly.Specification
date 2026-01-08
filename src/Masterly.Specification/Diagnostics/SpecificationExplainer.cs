using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Masterly.Specification
{
    /// <summary>
    /// Generates human-readable explanations of specifications.
    /// </summary>
    public static class SpecificationExplainer
    {
        /// <summary>
        /// Generates a human-readable explanation of the specification.
        /// </summary>
        public static string Explain<T>(ISpecification<T> specification)
        {
            var expression = specification.ToExpression();
            return ExplainExpression(expression.Body);
        }

        /// <summary>
        /// Evaluates the specification and returns detailed results.
        /// </summary>
        public static EvaluationResult Evaluate<T>(ISpecification<T> specification, T obj)
        {
            var expression = specification.ToExpression();
            var details = new List<EvaluationDetail>();
            var result = EvaluateExpression(expression.Body, expression.Parameters[0], obj, details);
            return new EvaluationResult(result, details);
        }

        private static string ExplainExpression(Expression expression)
        {
            return expression switch
            {
                BinaryExpression binary => ExplainBinary(binary),
                UnaryExpression unary => ExplainUnary(unary),
                MethodCallExpression methodCall => ExplainMethodCall(methodCall),
                MemberExpression member => ExplainMember(member),
                ConstantExpression constant => constant.Value?.ToString() ?? "null",
                LambdaExpression lambda => ExplainExpression(lambda.Body),
                ConditionalExpression conditional => ExplainConditional(conditional),
                _ => expression.ToString()
            };
        }

        private static string ExplainBinary(BinaryExpression binary)
        {
            var left = ExplainExpression(binary.Left);
            var right = ExplainExpression(binary.Right);
            var op = binary.NodeType switch
            {
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                ExpressionType.Equal => "==",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.Add => "+",
                _ => binary.NodeType.ToString()
            };

            if (binary.NodeType == ExpressionType.AndAlso || binary.NodeType == ExpressionType.OrElse)
                return $"({left}) {op} ({right})";

            return $"{left} {op} {right}";
        }

        private static string ExplainUnary(UnaryExpression unary)
        {
            var operand = ExplainExpression(unary.Operand);
            return unary.NodeType switch
            {
                ExpressionType.Not => $"NOT ({operand})",
                ExpressionType.Convert => operand,
                _ => $"{unary.NodeType}({operand})"
            };
        }

        private static string ExplainMethodCall(MethodCallExpression methodCall)
        {
            var methodName = methodCall.Method.Name;
            if (methodCall.Object != null)
            {
                var obj = ExplainExpression(methodCall.Object);
                var args = string.Join(", ", methodCall.Arguments.Select(ExplainExpression));
                return string.IsNullOrEmpty(args) ? $"{obj}.{methodName}()" : $"{obj}.{methodName}({args})";
            }
            else
            {
                var args = string.Join(", ", methodCall.Arguments.Select(ExplainExpression));
                return $"{methodName}({args})";
            }
        }

        private static string ExplainMember(MemberExpression member)
        {
            if (member.Expression is ParameterExpression)
                return member.Member.Name;

            if (member.Expression != null)
                return $"{ExplainExpression(member.Expression)}.{member.Member.Name}";

            return member.Member.Name;
        }

        private static string ExplainConditional(ConditionalExpression conditional)
        {
            var test = ExplainExpression(conditional.Test);
            var ifTrue = ExplainExpression(conditional.IfTrue);
            var ifFalse = ExplainExpression(conditional.IfFalse);
            return $"IF ({test}) THEN {ifTrue} ELSE {ifFalse}";
        }

        private static bool EvaluateExpression<T>(Expression expression, ParameterExpression param, T obj,
            List<EvaluationDetail> details)
        {
            switch (expression)
            {
                case BinaryExpression binary when binary.NodeType == ExpressionType.AndAlso:
                    var leftResult = EvaluateExpression(binary.Left, param, obj, details);
                    var rightResult = EvaluateExpression(binary.Right, param, obj, details);
                    return leftResult && rightResult;

                case BinaryExpression binary when binary.NodeType == ExpressionType.OrElse:
                    leftResult = EvaluateExpression(binary.Left, param, obj, details);
                    rightResult = EvaluateExpression(binary.Right, param, obj, details);
                    return leftResult || rightResult;

                case UnaryExpression unary when unary.NodeType == ExpressionType.Not:
                    var innerResult = EvaluateExpression(unary.Operand, param, obj, details);
                    return !innerResult;

                default:
                    var lambda = Expression.Lambda<Func<T, bool>>(expression, param);
                    var compiled = lambda.Compile();
                    var result = compiled(obj);
                    var explanation = ExplainExpression(expression);
                    details.Add(new EvaluationDetail(explanation, result));
                    return result;
            }
        }
    }
}
