using System.Collections;

namespace TestFramework
{
    public abstract class YieldTestGenerator
    {
        public abstract IEnumerable GenerateTestCases();
    }
    
    public abstract class YieldTestGenerator<T> : YieldTestGenerator
    {
        public abstract override IEnumerable<T> GenerateTestCases();
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class YieldTestCaseAttribute : Attribute
    {
        public Type GeneratorType { get; }
        
        public YieldTestCaseAttribute(Type generatorType)
        {
            if (!typeof(YieldTestGenerator).IsAssignableFrom(generatorType))
                throw new ArgumentException($"{generatorType.Name} must inherit from YieldTestGenerator");
            GeneratorType = generatorType;
        }
    }
}