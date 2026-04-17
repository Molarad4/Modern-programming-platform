using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TestFramework;

namespace TestRunner
{
    public class TestEngine : IDisposable
    {
        private static int _testCounter = 0;
        public static int TotalExecuted => _testCounter;
        private readonly CustomThreadPool _pool;

        public TestEngine(int min, int max) => _pool = new CustomThreadPool(min, max);

        public async Task WaitTasks()
        {
            while (_pool.HasWork) await Task.Delay(50);
            await Task.Delay(200);
        }

        public async Task RunAllTests(string dllPath, Action<string, bool, string> onTestFinished)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            foreach (var type in assembly.GetTypes())
            {
                var methods = type.GetMethods();
                var testMethods = methods.Where(m => m.GetCustomAttributes<MyTestAttribute>().Any() || m.GetCustomAttributes<MyTestCaseAttribute>().Any());
                var before = methods.FirstOrDefault(m => m.GetCustomAttributes<BeforeEachAttribute>().Any());
                var after = methods.FirstOrDefault(m => m.GetCustomAttributes<AfterEachAttribute>().Any());

                foreach (var method in testMethods)
                {
                    var cases = method.GetCustomAttributes<MyTestCaseAttribute>().ToList();
                    int timeout = method.GetCustomAttribute<TimeoutAttribute>()?.Milliseconds ?? Timeout.Infinite;

                    if (cases.Any())
                        foreach (var tc in cases) _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, tc.Params, timeout, onTestFinished));
                    else
                        _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, null, timeout, onTestFinished));
                }
            }
        }

        public void RunSingleTest(string dllPath, string className, string methodName, Action<string, bool, string> onTestFinished)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name == className);
            var method = type?.GetMethod(methodName);
            if (method == null) return;

            var before = type.GetMethods().FirstOrDefault(m => m.GetCustomAttributes<BeforeEachAttribute>().Any());
            var after = type.GetMethods().FirstOrDefault(m => m.GetCustomAttributes<AfterEachAttribute>().Any());

            _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, null, Timeout.Infinite, onTestFinished));
        }

        private void InvokeTestMethodSync(Type type, MethodInfo method, MethodInfo before, MethodInfo after, object[] args, int timeoutMs, Action<string, bool, string> callback)
        {
            int num = Interlocked.Increment(ref _testCounter);
            string testName = $"#{num:D3} {type.Name}.{method.Name}" + (args != null ? $"({string.Join(", ", args)})" : "");
            var instance = Activator.CreateInstance(type);
            Exception threadEx = null;

            Thread testThread = new Thread(() => {
                try {
                    before?.Invoke(instance, null);
                    var result = method.Invoke(instance, args);
                    if (result is Task t) t.GetAwaiter().GetResult();
                    after?.Invoke(instance, null);
                }
                catch (TargetInvocationException ex) { threadEx = ex.InnerException; }
                catch (Exception ex) { threadEx = ex; }
            }) { IsBackground = true };

            testThread.Start();
            if (!testThread.Join(timeoutMs == Timeout.Infinite ? int.MaxValue : timeoutMs))
                callback(testName, false, "Timeout");
            else
                callback(testName, threadEx == null, threadEx?.Message);
        }

        public void Dispose() => _pool.Dispose();
    }
}