using System;
using System.IO;


namespace GZipTest
{
    /// <summary>
    /// Manage compression of incoming Blocks
    /// </summary>
    public class Compress : AbstractCompress
    {
        public static int blockSize = 1024 * 1024;
        public Compress(BufferedStream inputFileStream, BufferedStream outputFileStream) : base(inputFileStream, outputFileStream)
        {
        }
        
        /// <summary>
        /// Read input and splitting incoming data to Blocks
        /// </summary>
        protected override void ReadInputFile()
        {
            try
            {
                var fileSize = InputFileStream.Length;
                using (var binaryReader = new BinaryReader(InputFileStream))
                {
                    var blockId = 0;
                    while (fileSize > 0 && errorMessage == null)
                    {
                        var currentBlockSize = fileSize > blockSize ? blockSize : fileSize;
                        var bytes = binaryReader.ReadBytes((int)currentBlockSize);
                        InputQueue.InQueue(new Block(blockId++, bytes));
                        fileSize -= currentBlockSize;
                        if (fileSize == 0)
                        {
                            InputQueue.ReadComplete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cant read input file");
                errorMessage = e;
            }
        }

        /// <summary>
        /// Sending Block into GZip for compression
        /// </summary>
        /// <param name="processEventId"></param>
        protected override void Process(int processEventId)
        {
            try
            {
                while (InputQueue.OutQueue(out Block block) && errorMessage == null)
                {
                    var compressBlock = GZipCompress.CompressByBlocks(block.Bytes);
                    if (compressBlock == null) throw new OutOfMemoryException();
                    OutputDictionary.Add(block.Id, compressBlock);
                }
                ProcessEvents[processEventId].Set();
            }
            catch (Exception e)
            {
                errorMessage = e;
            }
        }

        /// <summary>
        /// Writing data into output file
        /// </summary>
        protected override void WriteOutputFile()
        {
            try
            {
                using (var binaryWriter = new BinaryWriter(OutputFileStream))
                {
                    while (OutputDictionary.GetValueByKey(out var data) && errorMessage == null)
                    {
                        binaryWriter.Write(data.Length);
                        binaryWriter.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e;
            }
        }
    }
}
