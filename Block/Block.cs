namespace GZipTest
{
    /// <summary>
    /// Define Block achritecture
    /// </summary>
    public class Block
    {
        public Block()
        {
        }

        public Block(int id, byte[] bytes)
        {
            Id = id;
            Bytes = bytes;
        }

        public int Id { get; set; }

        public byte[] Bytes { get; set; }
    }
}
