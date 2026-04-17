using TestFramework;

namespace TargetApp.Tests
{
    public class PerformanceTests
    {
        [MyTest("Долгий тест 1")]
        public async Task Test_LongRunningTask_1()
        {
            await Task.Delay(1000);
            Assert.IsTrue(true);
        }

        [MyTest("Долгий тест 2")]
        public async Task Test_LongRunningTask_2()
        {
            await Task.Delay(1000); 
            Assert.IsTrue(true);
        }

        [MyTest("Долгий тест 3")]
        public async Task Test_LongRunningTask_3()
        {
            await Task.Delay(1000); 
            Assert.IsTrue(true);
        }

        [MyTest("Тест с ограничением времени - Успех")]
        [Timeout(500)]
        public async Task Test_Timeout_Success()
        {
            await Task.Delay(200); // Успеет
            Assert.IsTrue(true);
        }

        [MyTest("Тест с ограничением времени - Провал")]
        [Timeout(500)]
        public async Task Test_Timeout_Fail()
        {
            await Task.Delay(2000); // Не успеет
            Assert.IsTrue(true);
        }
    }
}