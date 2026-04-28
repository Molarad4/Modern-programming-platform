using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestFramework;

namespace TestRunner
{
    class Program
    {
        private static readonly object _consoleLock = new object();
        
        static async Task Main(string[] args)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TargetApp.Tests.dll");
            if (!File.Exists(fullPath))
            {
                Console.WriteLine("Test DLL not found!");
                return;
            }

            using var engine = new TestEngine(2, 10);
            
            engine.TestCompleted += (sender, e) =>
            {
                Log($"[Event] Test {e.TestName} completed: {(e.Success ? "PASS" : "FAIL")} in {e.DurationMs}ms");
            };
            
            engine.ThreadCreated += (sender, e) =>
                Log($"[Event] 🟢 {e.Message}");
            
            engine.ThreadTerminated += (sender, e) =>
                Log($"[Event] 🔴 {e.Message}");
            
            engine.QueueStatusChanged += (sender, e) =>
                Log($"[Event] 📊 {e.Message}");
            
            engine.ThreadError += (sender, e) =>
                Log($"[Event] ❌ Error in {e.ThreadName}: {e.Error.Message}");

            
            Log("\n>>> ДЕМОНСТРАЦИЯ ФИЛЬТРАЦИИ: Только тесты с Priority >= 2");
            
            await engine.RunAllTests(fullPath, PrintTestResult, FilterByPriority);
            await engine.WaitTasks();
            
            Log("\n>>> ДЕМОНСТРАЦИЯ YIELD-ТЕСТОВ (BankTests.YieldMoneyTransferTest)");
            engine.RunSingleTest(fullPath, "BankTests", "YieldMoneyTransferTest", PrintTestResult);
            await engine.WaitTasks();
            
            Log("\n>>> ДЕМОНСТРАЦИЯ Assert.Explain (детальный разбор выражения при ошибке)");
            await engine.RunAllTests(fullPath, PrintTestResult, FilterForExplainTests);
            await engine.WaitTasks();
            
            Log($"\n✅ Всего выполнено тестов: {TestEngine.TotalExecuted}");
            Log("Нажмите Enter для выхода...");
            Console.ReadLine();
        }
        
        private static bool FilterByPriority(MethodInfo method)
        {
            var priority = method.GetCustomAttribute<PriorityAttribute>();
            if (priority == null) return true;
            return priority.Priority >= 2;
        }
        
        private static bool FilterForExplainTests(MethodInfo method)
        {
            return method.Name.Contains("Explain", StringComparison.OrdinalIgnoreCase);
        }
        
        public static void Log(string message)
        {
            lock (_consoleLock)
            {
                Console.WriteLine(message);
            }
        }
        
        private static void PrintTestResult(string name, bool isSuccess, string error)
        {
            lock (_consoleLock)
            {
                Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write($"{(isSuccess ? "[PASS]" : "[FAIL]")} ");
                Console.ResetColor();
                Console.WriteLine(name + (string.IsNullOrEmpty(error) ? "" : " -> " + error));
            }
        }
    }
}