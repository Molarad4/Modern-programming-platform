using System.Diagnostics;

namespace TestRunner
{
    class Program
    {
        private static readonly object _consoleLock = new object();

        static async Task Main(string[] args)
        {
            Console.Title = "Parallel Test Runner v2.0";
            
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dllName = "TargetApp.Tests.dll";
            string fullPath = Path.Combine(baseDirectory, dllName);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Файл {dllName} не найден.");
                return;
            }

            var engine = new TestEngine();

            Console.WriteLine("==================================================");
            Console.WriteLine("ЗАПУСК В ПОСЛЕДОВАТЕЛЬНОМ РЕЖИМЕ (1 ПОТОК)");
            Console.WriteLine("==================================================");
            
            var swSequential = Stopwatch.StartNew();
            await engine.RunAllTests(fullPath, 1, PrintTestResult);
            swSequential.Stop();

            Console.WriteLine("\n==================================================");
            Console.WriteLine("ЗАПУСК В ПАРАЛЛЕЛЬНОМ РЕЖИМЕ (4 ПОТОКА)");
            Console.WriteLine("==================================================");
            
            var swParallel = Stopwatch.StartNew();
            await engine.RunAllTests(fullPath, 4, PrintTestResult);
            swParallel.Stop();

            Console.WriteLine("\n==================================================");
            Console.WriteLine("СРАВНЕНИЕ ЭФФЕКТИВНОСТИ:");
            Console.WriteLine($"Время последовательного запуска : {swSequential.ElapsedMilliseconds} мс");
            Console.WriteLine($"Время параллельного запуска     : {swParallel.ElapsedMilliseconds} мс");
            Console.WriteLine("==================================================");
            
            if (swParallel.ElapsedMilliseconds < swSequential.ElapsedMilliseconds)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("УСПЕХ: Параллельный запуск выполнился быстрее!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: Параллельный запуск не быстрее. Возможно, тесты слишком быстрые.");
            }
            Console.ResetColor();
        }

        private static void PrintTestResult(string name, bool isSuccess, string error)
        {
            lock (_consoleLock)
            {
                if (isSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"[{Thread.CurrentThread.ManagedThreadId:D2}] [PASS] ");
                    Console.ResetColor();
                    Console.WriteLine(name);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[{Thread.CurrentThread.ManagedThreadId:D2}] [FAIL] ");
                    Console.ResetColor();
                    Console.WriteLine($"{name} -> {error}");
                }
            }
        }
    }
}