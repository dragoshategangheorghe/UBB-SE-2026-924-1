namespace BankApp.Client.Utilities
{
    public class DownloadResponse
    {
        public byte[] Content { get; set; } = System.Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
