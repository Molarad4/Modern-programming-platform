using System.Collections.Concurrent;

namespace TestFramework
{
    public abstract class SharedContextBase
    {
        private static readonly ConcurrentDictionary<string, object> _data = new();

        public static void SetData(string key, object value) => _data[key] = value;
        public static object GetData(string key) => _data.TryGetValue(key, out var value) ? value : null;
        public static void Clear() => _data.Clear();
    }
}