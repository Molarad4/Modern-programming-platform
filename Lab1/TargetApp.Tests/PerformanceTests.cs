using TestFramework;

namespace TargetApp.Tests
{
    [Category("Performance")]
    [Author("Performance Team", Email = "perf-team@example.com")]
    public class PerformanceTests
    {
        [MyTest("Долгий тест 1")]
        [Priority(1)]
        [Category("LongRunning")]
        public async Task Test_LongRunningTask_1()
        {
            await Task.Delay(1000);
            Assert.IsTrue(true);
        }

        [MyTest("Долгий тест 2")]
        [Priority(1)]
        [Category("LongRunning")]
        public async Task Test_LongRunningTask_2()
        {
            await Task.Delay(1000);
            Assert.IsTrue(true);
        }

        [MyTest("Долгий тест 3")]
        [Priority(1)]
        [Category("LongRunning")]
        public async Task Test_LongRunningTask_3()
        {
            await Task.Delay(1000);
            Assert.IsTrue(true);
        }

        [MyTest("Тест с ограничением времени - Успех")]
        [Timeout(500)]
        [Priority(2)]
        [Category("Timeout")]
        public async Task Test_Timeout_Success()
        {
            await Task.Delay(200);
            Assert.IsTrue(true);
        }

        [MyTest("Тест с ограничением времени - Провал")]
        [Timeout(500)]
        [Priority(2)]
        [Category("Timeout")]
        public async Task Test_Timeout_Fail()
        {
            await Task.Delay(2000);
            Assert.IsTrue(true);
        }

        
        
        
        
        [MyTest("Тест с высоким приоритетом - всегда запускается")]
        [Priority(3)]
        [Category("Critical")]
        [Author("John Doe")]
        public void Test_HighPriority_AlwaysRuns()
        {
            Assert.IsTrue(true);
        }

        [MyTest("Тест со средним приоритетом")]
        [Priority(2)]
        [Category("Medium")]
        public void Test_MediumPriority_MayRun()
        {
            int[] numbers = { 1, 2, 3 };
            Assert.IsTrue(numbers.Length > 0);
        }

        [MyTest("Тест с низким приоритетом - может быть пропущен")]
        [Priority(0)]
        [Category("Low")]
        public void Test_LowPriority_MayBeSkipped()
        {
            var result = 5 * 5;
            Assert.AreEqual(25, result);
        }
    }
}