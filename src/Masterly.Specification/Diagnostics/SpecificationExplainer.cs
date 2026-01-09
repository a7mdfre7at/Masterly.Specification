using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
            Expression<Func<T, bool>> expression = specification.ToExpression();
            return ExplainExpression(expression.Body);
        }

        /// <summary>
        /// Evaluates the specification and returns detailed results.
        /// </summary>
        public static EvaluationResult Evaluate<T>(ISpecification<T> specification, T obj)
        {
            Expression<Func<T, bool>> expression = specification.ToExpression();
            List<EvaluationDetail> details = new List<EvaluationDetail>();
            bool result = EvaluateExpression(expression.Body, expression.Parameters[0], obj, details);
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
            string left = ExplainExpression(binary.Left);
            string right = ExplainExpression(binary.Right);
            string op = binary.NodeType switch
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
            string operand = ExplainExpression(unary.Operand);
            return unary.NodeType switch
            {
                ExpressionType.Not => $"NOT ({operand})",
                ExpressionType.Convert => operand,
                _ => $"{unary.NodeType}({operand})"
            };
        }

        private static string ExplainMethodCall(MethodCallExpression methodCall)
        {
            string methodName = methodCall.Method.Name;
            if (methodCall.Object != null)
            {
                string obj = ExplainExpression(methodCall.Object);
                string args = string.Join(", ", methodCall.Arguments.Select(ExplainExpression));
                return string.IsNullOrEmpty(args) ? $"{obj}.{methodName}()" : $"{obj}.{methodName}({args})";
            }
            else
            {
                string args = string.Join(", ", methodCall.Arguments.Select(ExplainExpression));
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
            string test = ExplainExpression(conditional.Test);
            string ifTrue = ExplainExpression(conditional.IfTrue);
            string ifFalse = ExplainExpression(conditional.IfFalse);
            return $"IF ({test}) THEN {ifTrue} ELSE {ifFalse}";
        }

        private static bool EvaluateExpression<T>(Expression expression, ParameterExpression param, T obj,
            List<EvaluationDetail> details)
        {
            switch (expression)
            {
                case BinaryExpression binary when binary.NodeType == ExpressionType.AndAlso:
                    bool leftResultAnd = EvaluateExpression(binary.Left, param, obj, details);
                    bool rightResultAnd = EvaluateExpression(binary.Right, param, obj, details);
                    return leftResultAnd && rightResultAnd;

                case BinaryExpression binary when binary.NodeType == ExpressionType.OrElse:
                    bool leftResultOr = EvaluateExpression(binary.Left, param, obj, details);
                    bool rightResultOr = EvaluateExpression(binary.Right, param, obj, details);
                    return leftResultOr || rightResultOr;

                case UnaryExpression unary when unary.NodeType == ExpressionType.Not:
                    bool innerResult = EvaluateExpression(unary.Operand, param, obj, details);
                    return !innerResult;

                default:
                    Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expression, param);
                    Func<T, bool> compiled = lambda.Compile();
                    bool result = compiled(obj);
                    string explanation = ExplainExpression(expression);
                    details.Add(new EvaluationDetail(explanation, result));
                    return result;
            }
        }
    }
}