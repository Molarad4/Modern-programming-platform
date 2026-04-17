using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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

        public bool HasWork => !_taskQueue.IsEmpty || Interlocked.CompareExchange(ref _activeTasks, 0, 0) > 0;

        public CustomThreadPool(int minThreads, int maxThreads)
        {
            _minThreads = minThreads;
            _maxThreads = maxThreads;
            lock (_lock)
            {
                for (int i = 0; i < _minThreads; i++) CreateThread();
            }
        }

        public void Enqueue(Action task)
        {
            lock (_lock)
            {
                _taskQueue.Enqueue(task);
                if (_threads.Count < _maxThreads && _taskQueue.Count > (_threads.Count - _activeTasks))
                {
                    CreateThread();
                    Program.Log($"[Pool] Нагрузка увеличилась. Потоков: {_threads.Count}");
                }
                Monitor.Pulse(_lock);
            }
        }

        private void CreateThread()
        {
            var thread = new Thread(Worker) { IsBackground = true };
            _threads.Add(thread);
            thread.Start();
        }

        private void Worker()
        {
            while (!_disposed)
            {
                Action task = null;
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
                                    _threads.Remove(Thread.CurrentThread);
                                    Program.Log($"[Pool] Поток завершен по таймауту простоя. Осталось: {_threads.Count}");
                                    return;
                                }
                            }
                        }
                        else Monitor.Wait(_lock);
                    }
                }

                if (_disposed) return;

                Interlocked.Increment(ref _activeTasks);
                try { task?.Invoke(); } 
                finally { Interlocked.Decrement(ref _activeTasks); }
            }
        }

        public void Dispose()
        {
            _disposed = true;
            lock (_lock) { Monitor.PulseAll(_lock); }
        }
    }
}