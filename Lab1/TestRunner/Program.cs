using System;
using System.IO;
using System.Threading.Tasks;

namespace TestRunner
{
    class Program
    {
        private static readonly object _consoleLock = new object();

        static async Task Main(string[] args)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TargetApp.Tests.dll");
            if (!File.Exists(fullPath)) return;

            using var engine = new TestEngine(2, 10);

            Log("=== ЛАБОРАТОРНАЯ РАБОТА 3 ===");

            Log("\n>>> ЭТАП 1: ПИКОВАЯ НАГРУЗКА");
            for (int i = 0; i < 3; i++) 
            {
                await engine.RunAllTests(fullPath, PrintTestResult);
            }
            await engine.WaitTasks();

            Log("\n>>> ЭТАП 2: ИНТЕРВАЛ БЕЗДЕЙСТВИЯ (5 сек)");
            Log("Сейчас должны пойти сообщения о завершении потоков...");
            await Task.Delay(5000); 

            Log("\n>>> ЭТАП 3: ЕДИНИЧНЫЕ ПОДАЧИ");
            engine.RunSingleTest(fullPath, "BankTests", "Test_OwnerName_Validation", PrintTestResult);
            await engine.WaitTasks();
            
            engine.RunSingleTest(fullPath, "BankTests", "Test_CheckDescription", PrintTestResult);
            await engine.WaitTasks();

            Log("\n>>> ЭТАП 4: ФИНАЛЬНОЕ СЖАТИЕ");
            await Task.Delay(4000); 

            Log($"\nГотово. Тестов: {TestEngine.TotalExecuted}");
            Console.ReadLine();
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
                Console.WriteLine(name + (isSuccess ? "" : " -> " + error));
            }
        }
    }
}