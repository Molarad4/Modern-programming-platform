using System;

namespace TestFramework
{
    // Маркер обычного теста
    [AttributeUsage(AttributeTargets.Method)]
    public class MyTestAttribute : Attribute { }

    // Для тестов с параметрами
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MyTestCaseAttribute : Attribute 
    {
        public object[] Params { get; }
        public MyTestCaseAttribute(params object[] parameters) => Params = parameters;
    }

    // Подготовка перед каждым тестом
    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeEachAttribute : Attribute { }

    // Очистка после каждого теста
    [AttributeUsage(AttributeTargets.Method)]
    public class AfterEachAttribute : Attribute { }

    // Маркер для метода инициализации общего контекста
    [AttributeUsage(AttributeTargets.Method)]
    public class SharedContextAttribute : Attribute { }
}