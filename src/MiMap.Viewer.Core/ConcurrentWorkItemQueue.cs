using System.Threading.Tasks.Dataflow;

namespace MiMap.Viewer.Element.MiMapTiles
{
    public class ConcurrentWorkItemQueue<T> : IDisposable
    {
        public event EventHandler<T> ItemCompleted;
        public event EventHandler<T> ItemStarted;

        public bool TrackCompletedTasks { get; }

        private readonly int _threadCount;
        private readonly Action<T> _processWorkItem;

        //private Thread[] _threads;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public ConcurrentWorkItemQueue(Action<T> processWorkItem) : this(Environment.ProcessorCount, processWorkItem)
        {
        }

        public ConcurrentWorkItemQueue(int threadCount, Action<T> processWorkItem) : this(processWorkItem, threadCount, false)
        {
        }

        private BufferBlock<T> DataQueue { get; }
        public ConcurrentWorkItemQueue(Action<T> processWorkItem, int threadCount = -1, bool trackCompletedTasks = false)
        {
            threadCount = threadCount > 0 ? threadCount : Environment.ProcessorCount;
            TrackCompletedTasks = trackCompletedTasks;
            _threadCount = threadCount;
            _processWorkItem = processWorkItem;

            var blockOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellationTokenSource.Token,
                EnsureOrdered = true,
                NameFormat = "Chunker: {0}-{1}",
                MaxDegreeOfParallelism = threadCount
            };

            DataQueue = new BufferBlock<T>(blockOptions);
            var actionBlock = new ActionBlock<T>(Process, blockOptions);
            DataQueue.LinkTo(actionBlock, new DataflowLinkOptions() {PropagateCompletion = true, Append = true});
        }

        public bool TryEnqueue(T item)
        {
            while (!DataQueue.Post(item))
                Thread.Yield();
            
            return   true;
        }

        private void MarkComplete(T item)
        {
           // lock (_chunksSync)
            /*{
                if (_inProgress.Remove(item))
                {
                    if (TrackCompletedTasks)
                        _completed.Add(item);
                }
            }*/
        }

        private void Process(T c)
        {
            ItemStarted?.Invoke(this, c);

            _processWorkItem.Invoke(c);

            //MarkComplete(c);
            ItemCompleted?.Invoke(this, c);
        }
        
        public void ClearQueue()
        {
          //  lock (_chunksSync)
            {
              //  _pending.Clear();
            }
        }

        public void Reset()
        {
            DataQueue.TryReceiveAll(out _);
            
   //         lock (_chunksSync)
            {
             //   _pending.Clear();
           //     _inProgress.Clear();
          //      _completed.Clear();
            }
        }

        public void Dispose()
        {
            Reset();
       //     Stop();
         //   _trigger?.Dispose();
        }
    }
}