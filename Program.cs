using System;
using System.IO;

namespace GZipTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (InputArgsValidator(args))
            {
                using (var sourceStream = new BufferedStream(new FileStream(args[1], FileMode.OpenOrCreate, FileAccess.ReadWrite)))
                using (var destinationStream = new BufferedStream(new FileStream(args[2], FileMode.OpenOrCreate, FileAccess.ReadWrite)))
                {
                    AbstractCompress compressor;
                    if (args[0].ToLower() == "compress")
                    {
                        compressor = new Compress(sourceStream, destinationStream);
                        compressor.Run();
                    }
                    else
                    {
                        compressor = new Decompress(sourceStream, destinationStream);
                        compressor.Run();
                    }
                }
            }
            Console.WriteLine("press any key for exit");
            Console.ReadKey();
        }

        /// <summary>
        /// Validate input arguments
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool InputArgsValidator(string[] args)
        {
            if (args.Length != 3)

            {
                Console.WriteLine("Incorrect number of parameters");
                return false;
            }
            var inputFileInfo = new FileInfo(args[1]);
            var outputFileInfo = new FileInfo(args[2]);

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                Console.WriteLine("Check first argument [compress or decompress]");
            }

            if (!inputFileInfo.Exists || inputFileInfo.Length == 0)
            {
                Console.WriteLine("Check second argument, wrong the file name: {0} or wrong or missing path: {1}",
                    inputFileInfo.Name, inputFileInfo.DirectoryName);
                return false;
            }
            return true;
        }
    }
}

