using System;
using System.Collections;
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
        
        // Публичные события-обертки для доступа к событиям пула (ЛР4)
        public event ThreadPoolEventHandler? ThreadCreated;
        public event ThreadPoolEventHandler? ThreadTerminated;
        public event ThreadPoolEventHandler? QueueStatusChanged;
        public event ThreadErrorEventHandler? ThreadError;
        public event TestEventHandler? TestCompleted;

        public TestEngine(int min, int max)
        {
            _pool = new CustomThreadPool(min, max);
            
            // Подписываемся на события пула и перенаправляем их наружу
            _pool.ThreadCreated += (sender, e) => ThreadCreated?.Invoke(sender, e);
            _pool.ThreadTerminated += (sender, e) => ThreadTerminated?.Invoke(sender, e);
            _pool.QueueStatusChanged += (sender, e) => QueueStatusChanged?.Invoke(sender, e);
            _pool.ThreadError += (sender, e) => ThreadError?.Invoke(sender, e);
        }

        public async Task WaitTasks()
        {
            while (_pool.HasWork) await Task.Delay(50);
            await Task.Delay(200);
        }

        // Фильтрация через делегаты (ЛР4)
        public async Task RunAllTests(string dllPath, Action<string, bool, string> onTestFinished, 
                                       Func<MethodInfo, bool>? filter = null)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            foreach (var type in assembly.GetTypes())
            {
                var methods = type.GetMethods();
                var testMethods = methods.Where(m => 
                    (m.GetCustomAttributes<MyTestAttribute>().Any() || 
                     m.GetCustomAttributes<MyTestCaseAttribute>().Any() ||
                     m.GetCustomAttributes<YieldTestCaseAttribute>().Any()) &&
                    (filter == null || filter(m)));
                
                var before = methods.FirstOrDefault(m => m.GetCustomAttributes<BeforeEachAttribute>().Any());
                var after = methods.FirstOrDefault(m => m.GetCustomAttributes<AfterEachAttribute>().Any());
                
                foreach (var method in testMethods)
                {
                    // Проверка на yield генератор
                    var yieldAttr = method.GetCustomAttribute<YieldTestCaseAttribute>();
                    if (yieldAttr != null)
                    {
                        // Запуск yield-параметризованных тестов
                        await RunYieldTests(type, method, yieldAttr, before, after, onTestFinished);
                        continue;
                    }
                    
                    var cases = method.GetCustomAttributes<MyTestCaseAttribute>().ToList();
                    int timeout = method.GetCustomAttribute<TimeoutAttribute>()?.Milliseconds ?? Timeout.Infinite;
                    
                    if (cases.Any())
                    {
                        foreach (var tc in cases)
                        {
                            _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, 
                                tc.Params, timeout, onTestFinished));
                        }
                    }
                    else
                    {
                        _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, 
                            null, timeout, onTestFinished));
                    }
                }
            }
            await WaitTasks();
        }

        private async Task RunYieldTests(Type type, MethodInfo method, YieldTestCaseAttribute yieldAttr,
                                          MethodInfo? before, MethodInfo? after,
                                          Action<string, bool, string> onTestFinished)
        {
            var generator = (YieldTestGenerator)Activator.CreateInstance(yieldAttr.GeneratorType)!;
            var testCases = generator.GenerateTestCases();
            
            foreach (var testCase in testCases)
            {
                var args = testCase as object[];
                if (args == null && testCase != null)
                {
                    // Если возвращает не массив, оборачиваем в массив
                    args = new[] { testCase };
                }
                
                _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, 
                    args, Timeout.Infinite, onTestFinished));
            }
        }

        public void RunSingleTest(string dllPath, string className, string methodName, 
                                   Action<string, bool, string> onTestFinished)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name == className);
            var method = type?.GetMethod(methodName);
            if (method == null) return;
            
            var before = type.GetMethods().FirstOrDefault(m => m.GetCustomAttributes<BeforeEachAttribute>().Any());
            var after = type.GetMethods().FirstOrDefault(m => m.GetCustomAttributes<AfterEachAttribute>().Any());
            
            _pool.Enqueue(() => InvokeTestMethodSync(type, method, before, after, 
                null, Timeout.Infinite, onTestFinished));
        }

        private void InvokeTestMethodSync(Type type, MethodInfo method, MethodInfo? before, 
                                          MethodInfo? after, object[]? args, int timeoutMs, 
                                          Action<string, bool, string> callback)
        {
            int num = Interlocked.Increment(ref _testCounter);
            string testName = $"#{num:D3} {type.Name}.{method.Name}" + 
                              (args != null ? $"({string.Join(", ", args)})" : "");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Exception? threadEx = null;
            
            var instance = Activator.CreateInstance(type);
            Thread? testThread = null;
            
            try
            {
                testThread = new Thread(() => {
                    try {
                        before?.Invoke(instance, null);
                        var result = method.Invoke(instance, args);
                        if (result is Task t) t.GetAwaiter().GetResult();
                        after?.Invoke(instance, null);
                    }
                    catch (TargetInvocationException ex) { threadEx = ex.InnerException; }
                    catch (Exception ex) { threadEx = ex; }
                }) { IsBackground = true, Name = $"Test-{method.Name}" };
                
                testThread.Start();
                
                bool completed = timeoutMs == Timeout.Infinite 
                    ? testThread.Join(int.MaxValue) 
                    : testThread.Join(timeoutMs);
                    
                stopwatch.Stop();
                
                if (!completed)
                {
                    testThread.Interrupt();
                    callback(testName, false, $"Timeout after {timeoutMs}ms");
                    TestCompleted?.Invoke(this, new TestEventArgs 
                    { 
                        TestName = testName, 
                        Success = false, 
                        ErrorMessage = "Timeout",
                        DurationMs = stopwatch.ElapsedMilliseconds 
                    });
                    return;
                }
                
                callback(testName, threadEx == null, threadEx?.Message ?? "");
                TestCompleted?.Invoke(this, new TestEventArgs 
                { 
                    TestName = testName, 
                    Success = threadEx == null, 
                    ErrorMessage = threadEx?.Message ?? "",
                    DurationMs = stopwatch.ElapsedMilliseconds 
                });
            }
            catch (Exception ex)
            {
                callback(testName, false, $"Execution error: {ex.Message}");
                TestCompleted?.Invoke(this, new TestEventArgs 
                { 
                    TestName = testName, 
                    Success = false, 
                    ErrorMessage = ex.Message,
                    DurationMs = stopwatch.ElapsedMilliseconds 
                });
            }
            finally
            {
                if (testThread?.IsAlive == true)
                {
                    try { testThread.Interrupt(); } catch { }
                }
            }
        }

        public void Dispose() => _pool.Dispose();
    }
}