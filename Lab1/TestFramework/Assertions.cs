using System.Collections;
using System.Linq.Expressions;
using System.Text;

namespace TestFramework
{
    public static class Assert
    {
        public static void AreEqual(object expected, object actual)
        {
            if (!Equals(expected, actual))
                throw new TestFailedException($"Expected: {expected}, but was: {actual}");
        }

        public static void AreNotEqual(object val1, object val2)
        {
            if (Equals(val1, val2))
                throw new TestFailedException($"Values are equal, but expected not equal: {val1}");
        }

        public static void IsTrue(bool condition)
        {
            if (!condition) throw new TestFailedException("Expected: True, but was: False");
        }

        public static void IsFalse(bool condition)
        {
            if (condition) throw new TestFailedException("Expected: False, but was: True");
        }

        public static void IsNull(object obj)
        {
            if (obj != null) throw new TestFailedException("Object was not null");
        }

        public static void IsNotNull(object obj)
        {
            if (obj == null) throw new TestFailedException("Object was null");
        }

        public static void StringContains(string substring, string fullString)
        {
            if (string.IsNullOrEmpty(fullString) || !fullString.Contains(substring))
                throw new TestFailedException($"String '{fullString}' does not contain '{substring}'");
        }

        public static void IsEmpty(IEnumerable collection)
        {
            if (collection == null || collection.Cast<object>().Any())
                throw new TestFailedException("Collection is not empty");
        }

        public static void IsInstanceOf<T>(object obj)
        {
            if (!(obj is T))
                throw new TestFailedException($"Object is not {typeof(T).Name}");
        }

        public static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            catch (Exception ex) { throw new TestFailedException($"Expected {typeof(T).Name}, but got {ex.GetType().Name}"); }
            throw new TestFailedException($"Expected {typeof(T).Name} but no exception was thrown");
        }
        
        public static void Explain(Expression<Func<bool>> expression)
        {
            try
            {
                if (!expression.Compile()())
                {
                    var detail = ExplainExpression(expression.Body);
                    throw new TestFailedException($"Assertion failed: {detail}");
                }
            }
            catch (TestFailedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TestFailedException($"Exception while evaluating expression: {ex.Message}");
            }
        }

        private static string ExplainExpression(Expression expr)
        {
            if (expr is BinaryExpression binary)
            {
                var left = ExplainExpression(binary.Left);
                var right = ExplainExpression(binary.Right);
                var op = GetOperatorSymbol(binary.NodeType);
                
                var leftVal = EvaluateExpression(binary.Left);
                var rightVal = EvaluateExpression(binary.Right);
                
                return $"{left} {op} {right} | ({leftVal} {op} {rightVal} = false)";
            }
            
            if (expr is MemberExpression member)
            {
                return member.Member.Name;
            }
            
            if (expr is ConstantExpression constant)
            {
                return constant.Value?.ToString() ?? "null";
            }
            
            if (expr is MethodCallExpression methodCall)
            {
                var target = methodCall.Object != null ? ExplainExpression(methodCall.Object) : "result";
                var args = string.Join(", ", methodCall.Arguments.Select(ExplainExpression));
                return $"{target}.{methodCall.Method.Name}({args})";
            }

            return expr.ToString();
        }

        private static object EvaluateExpression(Expression expr)
        {
            try
            {
                var lambda = Expression.Lambda<Func<object>>(Expression.Convert(expr, typeof(object)));
                return lambda.Compile()();
            }
            catch
            {
                return "?";
            }
        }

        private static string GetOperatorSymbol(ExpressionType nodeType)
        {
            return nodeType switch
            {
                ExpressionType.Equal => "==",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "&&",
                ExpressionType.OrElse => "||",
                ExpressionType.Add => "+",
                ExpressionType.Subtract => "-",
                ExpressionType.Multiply => "*",
                ExpressionType.Divide => "/",
                _ => nodeType.ToString()
            };
        }
    }
}