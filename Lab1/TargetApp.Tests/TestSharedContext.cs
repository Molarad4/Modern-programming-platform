using TestFramework;
using TargetApp;

namespace TargetApp.Tests
{
    public class TestSharedContext
    {
        [SharedContext]
        public void GlobalSetup()
        {
            // Инициализируем БД
            BankDbStorage.InitializeSeedData();
            
            // Записываем данные, которые проверим в тестах
            SharedContextBase.SetData("GlobalKey", "Secret123");
            SharedContextBase.SetData("StartupTime", DateTime.Now.ToString("T"));
        }
    }
}