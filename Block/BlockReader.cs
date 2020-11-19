using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class BlockReader
    {
        private object locker = new object();
        private bool complete = false;
        private Queue<Block> queue = new Queue<Block>();

        public void InQueue(Block block)
        {
            lock (locker)
            {
                queue.Enqueue(block);
                Monitor.PulseAll(locker);
            }
        }

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