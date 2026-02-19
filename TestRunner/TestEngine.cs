using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestFramework;

namespace MyTestRunner
{
    public class TestEngine
    {
        public async Task RunAllTests(string dllPath, Action<string, bool, string> onTestFinished)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var types = assembly.GetTypes();

            // 1. Shared Context
            foreach (var type in types)
            {
                var sharedMethods = type.GetMethods()
                    .Where(m => m.GetCustomAttributes<SharedContextAttribute>().Any()); // Исправлено здесь

                foreach (var m in sharedMethods)
                {
                    var instance = Activator.CreateInstance(type);
                    m.Invoke(instance, null);
                }
            }

            // 2. Тестовые методы
            foreach (var type in types)
            {
                var methods = type.GetMethods();
        
                // Исправленный поиск: проверяем наличие хотя бы одного из атрибутов
                var testMethods = methods.Where(m => 
                    m.GetCustomAttributes<MyTestAttribute>().Any() || 
                    m.GetCustomAttributes<MyTestCaseAttribute>().Any());

                if (!testMethods.Any()) continue;

                var beforeMethod = methods.FirstOrDefault(m => m.GetCustomAttributes<BeforeEachAttribute>().Any());
                var afterMethod = methods.FirstOrDefault(m => m.GetCustomAttributes<AfterEachAttribute>().Any());

                foreach (var method in testMethods)
                {
                    // Берем ВСЕ тест-кейсы (их может быть много)
                    var testCases = method.GetCustomAttributes<MyTestCaseAttribute>().ToList();

                    if (testCases.Any())
                    {
                        foreach (var tc in testCases)
                        {
                            await InvokeTestMethod(type, method, beforeMethod, afterMethod, tc.Params, onTestFinished);
                        }
                    }
                    else
                    {
                        await InvokeTestMethod(type, method, beforeMethod, afterMethod, null, onTestFinished);
                    }
                }
            }
        }

        private async Task InvokeTestMethod(Type type, MethodInfo method, MethodInfo before, MethodInfo after, object[] args, Action<string, bool, string> callback)
        {
            var instance = Activator.CreateInstance(type);
            string testName = $"{type.Name}.{method.Name}" + (args != null ? $"({string.Join(", ", args)})" : "");

            try
            {
                before?.Invoke(instance, null);
                object result = method.Invoke(instance, args);
                if (result is Task task) await task;
                after?.Invoke(instance, null);
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