using TestFramework;

namespace TargetApp.Tests
{
    public class TestSharedContext
    {
        [SharedContext]
        public void GlobalSetup()
        {
            BankDbStorage.InitializeSeedData();
            
            SharedContextBase.SetData("GlobalKey", "Secret123");
            SharedContextBase.SetData("StartupTime", DateTime.Now.ToString("T"));
        }
    }
}