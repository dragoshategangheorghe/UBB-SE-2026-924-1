using BankApp.Models.Features.Savings;

namespace BankApp.Models.DTOs.Savings
{
    public class GetTransactionsResponse
    {
        public List<SavingsTransaction> Items { get; set; }

        public int TotalCount { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class ValidationResponse
    {
        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; }
    }
}
