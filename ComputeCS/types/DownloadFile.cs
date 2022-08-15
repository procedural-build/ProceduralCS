namespace ComputeCS.types
{
    public class DownloadFile
    {
        public string FilePathUnix;

        public string Hash;

        public byte[] Content;

        public int Size => Content.Length;
    }
}
