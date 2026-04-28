using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TestFramework;

namespace TestRunner
{
    public class CustomThreadPool : IDisposable
    {
        private readonly ConcurrentQueue<Action> _taskQueue = new ConcurrentQueue<Action>();
        private readonly List<Thread> _threads = new List<Thread>();
        private readonly int _minThreads;
        private readonly int _maxThreads;
        private bool _disposed = false;
        private readonly object _lock = new object();
        private int _activeTasks = 0;
        private int _totalTasksExecuted = 0;
        
        public event ThreadPoolEventHandler? ThreadCreated;
        public event ThreadPoolEventHandler? ThreadTerminated;
        public event ThreadPoolEventHandler? QueueStatusChanged;
        public event ThreadErrorEventHandler? ThreadError;

        public bool HasWork => !_taskQueue.IsEmpty || Interlocked.CompareExchange(ref _activeTasks, 0, 0) > 0;
        
        public int ActiveTasks => Interlocked.CompareExchange(ref _activeTasks, 0, 0);
        public int QueueSize => _taskQueue.Count;
        public int TotalExecuted => _totalTasksExecuted;

        public CustomThreadPool(int minThreads, int maxThreads)
        {
            _minThreads = Math.Max(1, minThreads);
            _maxThreads = Math.Max(_minThreads, maxThreads);
            
            lock (_lock)
            {
                for (int i = 0; i < _minThreads; i++) 
                    CreateThread();
            }
            
            OnQueueStatusChanged($"Pool initialized: min={_minThreads}, max={_maxThreads}");
        }

        public void Enqueue(Action task)
        {
            lock (_lock)
            {
                _taskQueue.Enqueue(task);
                OnQueueStatusChanged($"Task enqueued. Queue size: {_taskQueue.Count}");
                
                if (_threads.Count < _maxThreads && _taskQueue.Count > (_threads.Count - _activeTasks))
                {
                    CreateThread();
                    OnQueueStatusChanged($"Load increased. Threads: {_threads.Count}");
                }
                Monitor.Pulse(_lock);
            }
        }

        private void CreateThread()
        {
            var thread = new Thread(Worker) 
            { 
                IsBackground = true,
                Name = $"PoolThread-{_threads.Count + 1}"
            };
            _threads.Add(thread);
            thread.Start();
            
            OnThreadCreated($"Thread created: {thread.Name}. Total threads: {_threads.Count}");
        }

        private void Worker()
        {
            while (!_disposed)
            {
                Action? task = null;
                lock (_lock)
                {
                    while (!_disposed && !_taskQueue.TryDequeue(out task))
                    {
                        if (_threads.Count > _minThreads)
                        {
                            if (!Monitor.Wait(_lock, 3000))
                            {
                                if (_taskQueue.IsEmpty && _threads.Count > _minThreads)
                                {
                                    var currentThread = Thread.CurrentThread;
                                    _threads.Remove(currentThread);
                                    OnThreadTerminated($"Thread terminated by timeout: {currentThread.Name}. Remaining: {_threads.Count}");
                                    return;
                                }
                            }
                        }
                        else 
                            Monitor.Wait(_lock);
                    }
                }
                
                if (_disposed) return;
                if (task == null) continue;
                
                Interlocked.Increment(ref _activeTasks);
                OnQueueStatusChanged($"Thread {Thread.CurrentThread.Name} started task. Active: {_activeTasks}");
                
                try 
                { 
                    task.Invoke(); 
                    Interlocked.Increment(ref _totalTasksExecuted);
                }
                catch (Exception ex)
                {
                    OnThreadError(Thread.CurrentThread.Name, ex);
                }
                finally 
                { 
                    Interlocked.Decrement(ref _activeTasks);
                    OnQueueStatusChanged($"Thread {Thread.CurrentThread.Name} completed task. Active: {_activeTasks}");
                }
            }
        }

        private void OnThreadCreated(string message) =>
            ThreadCreated?.Invoke(this, new ThreadPoolEventArgs 
            { 
                ThreadCount = _threads.Count, 
                QueueSize = _taskQueue.Count,
                ActiveTasks = ActiveTasks,
                Message = message 
            });

        private void OnThreadTerminated(string message) =>
            ThreadTerminated?.Invoke(this, new ThreadPoolEventArgs 
            { 
                ThreadCount = _threads.Count, 
                QueueSize = _taskQueue.Count,
                ActiveTasks = ActiveTasks,
                Message = message 
            });

        private void OnQueueStatusChanged(string message) =>
            QueueStatusChanged?.Invoke(this, new ThreadPoolEventArgs 
            { 
                ThreadCount = _threads.Count, 
                QueueSize = _taskQueue.Count,
                ActiveTasks = ActiveTasks,
                Message = message 
            });

        private void OnThreadError(string threadName, Exception ex) =>
            ThreadError?.Invoke(this, new ThreadErrorEventArgs 
            { 
                ThreadName = threadName, 
                Error = ex 
            });

        public void Dispose()
        {
            _disposed = true;
            lock (_lock) { Monitor.PulseAll(_lock); }
        }
    }
}