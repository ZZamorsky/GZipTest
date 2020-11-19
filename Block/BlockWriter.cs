using System.Collections.Concurrent;
using System.Threading;

namespace GZipTest
{
    /// <summary>
    /// Manage creating of output file from block
    /// </summary>
    public class BlockWriter
    {
        private readonly object locker = new object();
        private ConcurrentDictionary<int, byte[]> dictionary = new ConcurrentDictionary<int, byte[]>();
        private bool completed = false;
        private int index = 0;

        /// <summary>
        /// Inserting incoming Block into ConcurrentDictionary 
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="bytes"></param>
        public void Add(int blockId, byte[] bytes)
        {
            lock (locker)
            {
                dictionary.TryAdd(blockId, bytes);
                Monitor.PulseAll(locker);
            }
        }

        /// <summary>
        /// Manage outgoing data from ConcurrentDictionary
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Indicates end of inserting into ConcurrentDictionary
        /// </summary>
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