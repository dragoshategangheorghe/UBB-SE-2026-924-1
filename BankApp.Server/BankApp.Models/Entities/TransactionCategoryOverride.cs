namespace BankApp.Models.Entities
{
    public class TransactionCategoryOverride
    {
        public int Id { get; set; }
        public virtual Transaction Transaction { get; set; }
        public virtual User User { get; set; }
        public virtual Category Category { get; set; }
    }
}