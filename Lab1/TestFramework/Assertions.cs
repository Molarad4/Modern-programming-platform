using System.Collections;

namespace TestFramework
{
    public static class Assert
    {
        // 1
        public static void AreEqual(object expected, object actual)
        {
            if (!Equals(expected, actual))
                throw new TestFailedException($"Expected: {expected}, but was: {actual}");
        }
        
        // 2
        public static void AreNotEqual(object val1, object val2)
        {
            if (Equals(val1, val2))
                throw new TestFailedException($"Values are equal, but expected not equal: {val1}");
        }
        
        // 3
        public static void IsTrue(bool condition)
        {
            if (!condition) throw new TestFailedException("Expected: True, but was: False");
        }
        
        // 4
        public static void IsFalse(bool condition)
        {
            if (condition) throw new TestFailedException("Expected: False, but was: True");
        }

        // 5
        public static void IsNull(object obj)
        {
            if (obj != null) throw new TestFailedException("Object was not null");
        }

        // 6
        public static void IsNotNull(object obj)
        {
            if (obj == null) throw new TestFailedException("Object was null");
        }

        // 7
        public static void StringContains(string substring, string fullString)
        {
            if (string.IsNullOrEmpty(fullString) || !fullString.Contains(substring))
                throw new TestFailedException($"String '{fullString}' does not contain '{substring}'");
        }

        // 8
        public static void IsEmpty(IEnumerable collection)
        {
            if (collection == null || collection.Cast<object>().Any())
                throw new TestFailedException("Collection is not empty");
        }

        // 9
        public static void IsInstanceOf<T>(object obj)
        {
            if (!(obj is T))
                throw new TestFailedException($"Object is not {typeof(T).Name}");
        }

        // 10
        public static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            catch (Exception ex) { throw new TestFailedException($"Expected {typeof(T).Name}, but got {ex.GetType().Name}"); }
            throw new TestFailedException($"Expected {typeof(T).Name} but no exception was thrown");
        }
    }
}