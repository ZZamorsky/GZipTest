using System;
using System.IO;


namespace GZipTest
{
    /// <summary>
    /// Manage decompression of incoming Blocks
    /// </summary>
    class Decompress : AbstractCompress
    {
        public Decompress(BufferedStream inputFileStream, BufferedStream outputFileStream) : base(inputFileStream, outputFileStream)
        {
        }

        /// <summary>
        /// Read input and splitting incoming data to Blocks
        /// </summary>
        protected override void ReadInputFile()
        {
            try
            {
                using (var binaryReader = new BinaryReader(InputFileStream))
                {
                    var fileSize = InputFileStream.Length;

                    const int intSize = 4;
                    var blockId = 0;

                    while (fileSize > 0 && errorMessage == null)
                    {
                        var blockSize = binaryReader.ReadInt32();
                        var bytes = binaryReader.ReadBytes(blockSize);
                        InputQueue.InQueue(new Block(blockId++, bytes));
                        fileSize -= (blockSize + intSize);
                        if (fileSize == 0)
                        {
                            InputQueue.ReadComplete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e;
            }
        }

        /// <summary>
        /// Sending Block into GZip for decompression
        /// </summary>
        /// <param name="processEventId"></param>
        protected override void Process(int processEventId)
        {
            try
            {
                while (InputQueue.OutQueue(out Block block) && errorMessage == null)
                {
                    var decompressedBlockData = GZipCompress.DecompressByBlocks(block.Bytes);
                    if (decompressedBlockData == null) throw new OutOfMemoryException();
                    OutputDictionary.Add(block.Id, decompressedBlockData);
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