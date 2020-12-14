using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GZipTest
{
    /// <summary>
    ///  Abstract class for Compression data flow
    /// </summary>
    public abstract class AbstractCompress
    {
        protected BufferedStream InputFileStream { get; set; }

        protected BufferedStream OutputFileStream { get; set; }

        protected BlockReader InputQueue { get; set; }

        protected BlockWriter OutputDictionary { get; set; }

        protected AutoResetEvent[] ProcessEvents = new AutoResetEvent[CoreCounter()];

        protected Exception errorMessage = null;

        protected AbstractCompress(BufferedStream inputFileStream, BufferedStream outputFileStream)
        {
            InputFileStream = inputFileStream;
            OutputFileStream = outputFileStream;
            InputQueue = new BlockReader();
            OutputDictionary = new BlockWriter();
        }

        public void Run()
        {
            var readingThread = new Thread(new ThreadStart(ReadInputFile));
            var compressingThreads = new List<Thread>();
            for (var i = 0; i < CoreCounter(); i++)
            {
                var j = i;
                ProcessEvents[j] = new AutoResetEvent(false);
                compressingThreads.Add(new Thread(() => Process(j)));
            }
            var writingThread = new Thread(new ThreadStart(WriteOutputFile));

            readingThread.Start();

            foreach (var compressThread in compressingThreads)
            {
                compressThread.Start();
            }

            writingThread.Start();

            WaitHandle.WaitAll(ProcessEvents);
            OutputDictionary.SetCompleted();

            writingThread.Join();
            if (errorMessage != null)
            {
                Console.WriteLine("1");
                Console.WriteLine(errorMessage.Message);
            }
            else
            {
                Console.WriteLine("0");
            }
        }


        protected abstract void ReadInputFile();

        protected abstract void Process(int processEventId);

        protected abstract void WriteOutputFile();

        /// <summary>
        /// Getting number of Cores from System
        /// </summary>
        /// <returns></returns>
        private static int CoreCounter()
        {
            int coreCount = 0;
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            return coreCount;
        }
    }
}