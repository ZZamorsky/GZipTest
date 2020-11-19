using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    public class GZipCompress
    {
        public static byte[] CompressByBlocks(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var compressStream = new GZipStream(output, CompressionMode.Compress))
                {
                    compressStream.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }
        public static byte[] DecompressByBlocks(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    using (var decompressStream = new GZipStream(input, CompressionMode.Decompress))
                    {
                        var buffer = new byte[1024 * 1024];
                        int bytesRead;

                        while ((bytesRead = decompressStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                        }
                    }
                    return output.ToArray();
                }
            }
        }
    }
}
