using TestFramework;
using TargetApp;

namespace TargetApp.Tests
{
    public class SharedContextTests
    {
        [MyTest]
        public void Test_SharedContext_DataIntegrity()
        {
            var key = SharedContextBase.GetData("GlobalKey");
            var time = SharedContextBase.GetData("StartupTime");
            
            Assert.AreEqual("Secret123", key);
            Assert.IsNotNull(time);
            Assert.IsInstanceOf<string>(time);
        }
        
        [MyTest]
        public void Test_SharedContext_ClearEffect()
        {
            Assert.IsNotNull(SharedContextBase.GetData("GlobalKey"));
            
            SharedContextBase.Clear();
            
            Assert.IsNull(SharedContextBase.GetData("GlobalKey"));
            Assert.IsNull(SharedContextBase.GetData("StartupTime"));
        }
        
        [MyTest]
        public void Test_SharedContext_UpdateData()
        {
            SharedContextBase.SetData("DynamicKey", 100);
            Assert.AreEqual(100, SharedContextBase.GetData("DynamicKey"));
            
            SharedContextBase.SetData("DynamicKey", 200);
            Assert.AreEqual(200, SharedContextBase.GetData("DynamicKey"));
        }
    }
}