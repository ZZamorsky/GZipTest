using System;
using System.IO;


namespace GZipTest
{
    public class Compress : AbstractCompress
    {
        public static int blockSize = 1024 * 1024;
        public Compress(Stream inputFileStream, Stream outputFileStream) : base(inputFileStream, outputFileStream)
        {
        }
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
