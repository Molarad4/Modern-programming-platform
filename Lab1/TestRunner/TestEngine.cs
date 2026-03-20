using System.Reflection;
using TestFramework;

namespace TestRunner
{
    public class TestEngine
    {
        public async Task RunAllTests(string dllPath, int maxDegreeOfParallelism, Action<string, bool, string> onTestFinished)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var types = assembly.GetTypes();
            
            // Инициализация Shared Context
            foreach (var type in types)
            {
                var sharedMethods = type.GetMethods().Where(m => m.GetCustomAttributes<SharedContextAttribute>().Any());
                foreach (var m in sharedMethods)
                {
                    var instance = Activator.CreateInstance(type);
                    m.Invoke(instance, null);
                }
            }
            
            // Собираем все тесты в единый список делегатов для параллельного выполнения
            var testTasksToRun = new List<Func<Task>>();

            foreach (var type in types)
            {
                var methods = type.GetMethods();
                var testMethods = methods.Where(m => 
                    m.GetCustomAttributes<MyTestAttribute>().Any() || 
                    m.GetCustomAttributes<MyTestCaseAttribute>().Any());

                var beforeMethod = methods.FirstOrDefault(m => m.GetCustomAttributes<BeforeEachAttribute>().Any());
                var afterMethod = methods.FirstOrDefault(m => m.GetCustomAttributes<AfterEachAttribute>().Any());

                foreach (var method in testMethods)
                {
                    var testCases = method.GetCustomAttributes<MyTestCaseAttribute>().ToList();
                    var timeoutAttr = method.GetCustomAttribute<TimeoutAttribute>();
                    int timeoutMs = timeoutAttr?.Milliseconds ?? Timeout.Infinite;

                    if (testCases.Any())
                    {
                        foreach (var tc in testCases)
                        {
                            testTasksToRun.Add(() => InvokeTestMethodAsync(type, method, beforeMethod, afterMethod, tc.Params, timeoutMs, onTestFinished));
                        }
                    }
                    else
                    {
                        testTasksToRun.Add(() => InvokeTestMethodAsync(type, method, beforeMethod, afterMethod, null, timeoutMs, onTestFinished));
                    }
                }
            }

            // ПАРАЛЛЕЛИЗМ: Ограничиваем количество потоков с помощью SemaphoreSlim
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var runningTasks = testTasksToRun.Select(async testFunc =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await testFunc(); 
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(runningTasks);
        }

        private async Task InvokeTestMethodAsync(Type type, MethodInfo method, MethodInfo before, MethodInfo after, object[] args, int timeoutMs, Action<string, bool, string> callback)
        {
            string testName = $"{type.Name}.{method.Name}" + (args != null ? $"({string.Join(", ", args)})" : "");

            try
            {
                // Запускаем сам тест в отдельной таске
                var executionTask = Task.Run(async () =>
                {
                    // Создаем изолированный инстанс класса тестов для потокобезопасности
                    var instance = Activator.CreateInstance(type);
                    
                    before?.Invoke(instance, null);
                    object result = method.Invoke(instance, args);
                    if (result is Task task) await task;
                    after?.Invoke(instance, null);
                });

                // ЛОГИКА ТАЙМАУТА: Ждем либо завершения теста, либо истечения времени
                if (timeoutMs != Timeout.Infinite)
                {
                    var delayTask = Task.Delay(timeoutMs);
                    var completedTask = await Task.WhenAny(executionTask, delayTask);
                    
                    if (completedTask == delayTask)
                    {
                        throw new Exception($"Превышено время ожидания ({timeoutMs} мс). Тест принудительно прерван.");
                    }
                    await executionTask;
                }
                else
                {
                    await executionTask;
                }

                callback(testName, true, null);
            }
            catch (Exception ex)
            {
                var actualException = ex.InnerException ?? ex;
                callback(testName, false, actualException.Message);
            }
        }
    }
}