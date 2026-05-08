namespace BankApp.Server.Services.Implementations
{
    using System;
    using System.Globalization;

    public class PaymentCalculationService
    {
        private const decimal ZeroAmount = 0m;
        private const int ZeroMonths = 0;
        private const int SingleMonth = 1;
        private const string CurrencyInputFormat = "0.##";

        public (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(
            decimal monthlyInstallment,
            decimal outstandingBalance,
            int remainingMonths,
            bool isStandardPayment,
            decimal customPaymentAmount = ZeroAmount)
        {
            var paymentAmount = isStandardPayment ? monthlyInstallment : customPaymentAmount;
            var balanceAfterPayment = Math.Max(ZeroAmount, outstandingBalance - paymentAmount);

            var monthsPaid = isStandardPayment
                ? SingleMonth
                : paymentAmount <= ZeroAmount
                    ? ZeroMonths
                    : (int)Math.Floor(paymentAmount / monthlyInstallment);

            var newRemainingMonths = Math.Max(ZeroMonths, remainingMonths - monthsPaid);
            return (balanceAfterPayment, newRemainingMonths);
        }

        public (bool Success, decimal Amount) ParsePaymentAmount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return (false, ZeroAmount);
            }

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureResult))
            {
                return (true, currentCultureResult);
            }

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantCultureResult))
            {
                return (true, invariantCultureResult);
            }

            return (false, ZeroAmount);
        }

        public (bool IsValid, string ValidationMessage) ValidatePaymentAmount(
            decimal paymentAmount,
            decimal outstandingBalance)
        {
            if (paymentAmount <= ZeroAmount)
            {
                return (false, "Payment amount must be greater than zero.");
            }

            if (paymentAmount > outstandingBalance)
            {
                return (false, $"Payment amount cannot exceed outstanding balance of {outstandingBalance:C2}.");
            }

            return (true, string.Empty);
        }

        public decimal GetInitialCustomAmount(
            decimal monthlyInstallment,
            decimal outstandingBalance,
            double? currentCustomAmount)
        {
            var amount = currentCustomAmount.HasValue ? (decimal)currentCustomAmount.Value : monthlyInstallment;
            return amount > outstandingBalance ? outstandingBalance : amount;
        }

        public string FormatCustomAmount(decimal amount)
        {
            return amount.ToString(CurrencyInputFormat, CultureInfo.CurrentCulture);
        }
    }
}