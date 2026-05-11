namespace BankApp.Models.DTOs.Transactions
{
    public class AccountFilterOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
    }
}
