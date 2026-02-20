namespace TestRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Custom Test Runner v1.0";
            
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
       
            string dllName = "TargetApp.Tests.dll";
            string fullPath = Path.Combine(baseDirectory, dllName);

            if (!File.Exists(fullPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Файл {dllName} не найден в папке приложения.");
                Console.WriteLine($"Ожидаемый путь: {fullPath}");
                Console.ResetColor();
                Console.WriteLine("\nПожалуйста, убедитесь, что проект TargetApp.Tests собран.");
                return;
            }

            Console.WriteLine($"Обнаружена библиотека тестов: {dllName}");
            
            var engine = new TestEngine();
            int passed = 0;
            int failed = 0;

            Console.WriteLine("\nВыполнение тестов...\n");
            Console.WriteLine("--------------------------------------------------");

            await engine.RunAllTests(fullPath, (name, isSuccess, error) => 
            {
                if (isSuccess)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[PASS] ");
                    Console.ResetColor();
                    Console.WriteLine(name);
                    passed++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[FAIL] ");
                    Console.ResetColor();
                    Console.WriteLine($"{name} -> Ошибка: {error}");
                    failed++;
                }
            });

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"\nИТОГО: Всего {passed + failed} | Прошло: {passed} | Провалено: {failed}");
            
            if (failed == 0) Console.WriteLine("\n🎉 ВСЕ ТЕСТЫ ПРОЙДЕНЫ!");

            //Console.WriteLine("\nНажмите любую клавишу для выхода...");
            //Console.ReadKey();
        }
    }
}