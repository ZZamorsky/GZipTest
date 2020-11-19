using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    /// <summary>
    /// Managing flow of Block in Queue 
    /// </summary>
    public class BlockReader
    {
        private object locker = new object();
        private bool complete = false;
        private Queue<Block> queue = new Queue<Block>();

        /// <summary>
        /// Inserting Block into Queue 
        /// </summary>
        /// <param name="block"></param>
        public void InQueue(Block block)
        {
            lock (locker)
            {
                queue.Enqueue(block);
                Monitor.PulseAll(locker);
            }
        }

        /// <summary>
        /// Removing Block from Queue
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool OutQueue(out Block block)
        {
            lock (locker)
            {
                while (queue.Count == 0)
                {
                    if (complete)
                    {
                        block = new Block();
                        return false;
                    }
                    Monitor.Wait(locker);
                }
                block = queue.Dequeue();
                Monitor.PulseAll(locker);
                return true;
            }
        }
        /// <summary>
        /// Indicate end of reading into Queue
        /// </summary>
        public void ReadComplete()
        {
            lock (locker)
            {
                complete = true;
                Monitor.PulseAll(locker);
            }
        }
    }
}