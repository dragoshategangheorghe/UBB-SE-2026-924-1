namespace BankApp.Models.DTOs.Transactions
{
    public class ExportedFileResult
    {
        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}
