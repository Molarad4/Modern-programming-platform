namespace TestFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MyTestAttribute : Attribute
    {
        public string Description { get; set; }
        public MyTestAttribute(string description = "") 
        {
            Description = description;
        }
    }
    
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

   
    [AttributeUsage(AttributeTargets.Method)]
    public class TimeoutAttribute : Attribute
    {
        public int Milliseconds { get; }
        public TimeoutAttribute(int milliseconds) => Milliseconds = milliseconds;
    }
    
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CategoryAttribute : Attribute
    {
        public string Category { get; set; }
        public CategoryAttribute(string category) => Category = category;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PriorityAttribute : Attribute
    {
        public int Priority { get; set; }
        public PriorityAttribute(int priority) => Priority = priority;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorAttribute : Attribute
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public AuthorAttribute(string name) => Name = name;
    }
}