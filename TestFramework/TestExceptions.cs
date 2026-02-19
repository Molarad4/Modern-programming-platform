using System;

namespace TestFramework
{
    public class TestFailedException : Exception
    {
        public TestFailedException(string message) : base(message) { }
    }
}