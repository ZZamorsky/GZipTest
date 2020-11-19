using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest
{
    public class BlockWriter
    {
        private readonly object locker = new object();
        private ConcurrentDictionary<int, byte[]> dictionary = new ConcurrentDictionary<int, byte[]>();
        private bool completed = false;
        private int index = 0;

        public void Add(int blockId, byte[] bytes)
        {
            lock (locker)
            {
                dictionary.TryAdd(blockId, bytes);
                Monitor.PulseAll(locker);
            }
        }

        public bool GetValueByKey(out byte[] data)
        {
            lock (locker)
            {
                while (!dictionary.ContainsKey(index))
                {
                    if (completed)
                    {
                        data = new byte[0];
                        return false;
                    }
                    Monitor.Wait(locker);
                }
                data = dictionary[index++];
                Monitor.PulseAll(locker);
                return true;
            }
        }

        public void SetCompleted()
        {
            lock (locker)
            {
                completed = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}