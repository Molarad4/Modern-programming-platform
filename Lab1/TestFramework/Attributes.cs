namespace TestFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MyTestAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MyTestCaseAttribute : Attribute 
    {
        public object[] Params { get; }
        public MyTestCaseAttribute(params object[] parameters) => Params = parameters;
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeEachAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class AfterEachAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class SharedContextAttribute : Attribute { }
}