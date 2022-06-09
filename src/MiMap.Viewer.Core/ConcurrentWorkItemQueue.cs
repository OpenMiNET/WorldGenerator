namespace MiMap.Viewer.Element.MiMapTiles
{
    public class ConcurrentWorkItemQueue<T> : IDisposable
    {
        public event EventHandler<T> ItemCompleted;
        public event EventHandler<T> ItemStarted;

        public bool TrackCompletedTasks { get; }

        private readonly int _threadCount;
        private readonly Action<T> _processWorkItem;
        private object _chunksSync = new object();
        private Queue<T> _pending;
        private HashSet<T> _inProgress;
        private HashSet<T> _completed;

        private Thread[] _threads;
        private AutoResetEvent _trigger;
        private bool _running;

        public ConcurrentWorkItemQueue(Action<T> processWorkItem) : this(Environment.ProcessorCount, processWorkItem)
        {
        }

        public ConcurrentWorkItemQueue(int threadCount, Action<T> processWorkItem) : this(processWorkItem, threadCount, false)
        {
        }

        public ConcurrentWorkItemQueue(Action<T> processWorkItem, int threadCount = -1, bool trackCompletedTasks = false)
        {
            threadCount = threadCount > 0 ? threadCount : Environment.ProcessorCount;
            TrackCompletedTasks = trackCompletedTasks;
            _threadCount = threadCount;
            _processWorkItem = processWorkItem;
            _pending = new Queue<T>();
            _inProgress = new HashSet<T>();
            _completed = new HashSet<T>();
            _trigger = new AutoResetEvent(false);
            _threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                _threads[i] = new Thread(WorkItemThreadRun)
                {
                    Name = "WorldGenerator",
                    IsBackground = false
                };
            }
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            for (int i = 0; i < _threadCount; i++)
            {
                _threads[i].Start();
            }
        }

        public void Stop()
        {
            if (!_running) return;

            _running = false;
            _trigger.Set();
            for (int i = 0; i < _threads.Length; i++)
            {
                _threads[i].Join();
            }

            _trigger?.Dispose();
        }

        public bool TryEnqueue(T item)
        {
            lock (_chunksSync)
            {
                if (_pending.Contains(item) || _inProgress.Contains(item))
                    return false;

                if (TrackCompletedTasks && _completed.Contains(item))
                    return false;

                _pending.Enqueue(item);
                return true;
            }
        }

        private bool TryDequeue(int timeout, out T item)
        {
            item = default;
            if (!Monitor.TryEnter(_chunksSync, timeout))
                return false;
            try
            {
                if (_pending.TryDequeue(out item))
                {
                    _inProgress.Add(item);
                    return true;
                }

                return false;
            }
            finally
            {
                Monitor.Exit(_chunksSync);
            }
        }

        private void MarkComplete(T item)
        {
            lock (_chunksSync)
            {
                _inProgress.Remove(item);
                if (TrackCompletedTasks)
                    _completed.Add(item);
            }
        }

        public void WorkItemThreadRun()
        {
            while (_running)
            {
                _trigger.WaitOne(1000);

                while (_running && TryDequeue(50, out var c))
                {
                    ItemStarted?.Invoke(this, c);

                    _processWorkItem.Invoke(c);

                    MarkComplete(c);
                    ItemCompleted?.Invoke(this, c);
                }
            }
        }

        public void ClearQueue()
        {
            lock (_chunksSync)
            {
                _pending.Clear();
            }
        }

        public void Reset()
        {
            lock (_chunksSync)
            {
                _pending.Clear();
                _inProgress.Clear();
                _completed.Clear();
            }
        }

        public void Dispose()
        {
            Reset();
            Stop();
            _trigger?.Dispose();
        }
    }
}