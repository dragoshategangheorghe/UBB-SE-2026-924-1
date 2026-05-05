namespace BankApp.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public bool IsSystem { get; set; } = true;
    }
}