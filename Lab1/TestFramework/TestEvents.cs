namespace TestFramework
{
    public class ThreadPoolEventArgs : EventArgs
    {
        public int ThreadCount { get; set; }
        public int QueueSize { get; set; }
        public int ActiveTasks { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Message { get; set; } = string.Empty;
    }

    public class ThreadErrorEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public string ThreadName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class TestEventArgs : EventArgs
    {
        public string TestName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public long DurationMs { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public delegate void ThreadPoolEventHandler(object sender, ThreadPoolEventArgs e);
    public delegate void ThreadErrorEventHandler(object sender, ThreadErrorEventArgs e);
    public delegate void TestEventHandler(object sender, TestEventArgs e);
}