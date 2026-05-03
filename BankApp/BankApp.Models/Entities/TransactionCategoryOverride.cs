namespace BankApp.Models.Entities
{
    public class TransactionCategoryOverride
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
    }
}