namespace TestFramework
{
    public abstract class SharedContextBase
    {
        // Хранилище для любых данных контекста (ключ-значение)
        private static readonly Dictionary<string, object> _data = new();

        public static void SetData(string key, object value) => _data[key] = value;
        public static object GetData(string key) => _data.TryGetValue(key, out var value) ? value : null;
        public static void Clear() => _data.Clear();
    }
}