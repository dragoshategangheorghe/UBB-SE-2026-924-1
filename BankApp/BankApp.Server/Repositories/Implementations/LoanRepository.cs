using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.DataAccess;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BankApp.Server.Repositories.Implementations
{
    /// <summary>
    /// SQL-backed repository for loans and loan applications.
    /// </summary>
    public class LoanRepository : ILoanRepository
    {
        private const int CommandTimeoutSeconds = 120;
        private const int StandardNVarCharLength = 50;
        private const int ExtendedNVarCharLength = 255;
        private const int EmptyCount = 0;
        private const int FirstIndex = 0;

        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoanRepository"/> class.
        /// </summary>
        /// <param name="dbContext"></param>
        public LoanRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all loans from storage.
        /// </summary>
        /// <returns>The complete list of loans.</returns>
        public async Task<List<Loan>> GetAllLoansAsync()
        {
            var query = "SELECT * FROM Loan";

            using var reader = await dbContext.ExecuteQueryAsync(query, []);

            List<Loan> loans = [];
            while (await reader.ReadAsync())
            {
                loans.Add(this.ReaderToLoan(reader));
            }

            return loans;
        }

        /// <summary>
        /// Retrieves a loan by its identifier.
        /// </summary>
        /// <param name="id">The loan identifier.</param>
        /// <returns>The matching loan, or <see langword="null"/> when not found.</returns>
        public async Task<Loan> GetLoanByIdAsync(int id)
        {
            var query = "SELECT * FROM Loan WHERE id = @id";

            using var reader = await dbContext.ExecuteQueryAsync(query, new object[] { id });

            if (await reader.ReadAsync())
            {
                return this.ReaderToLoan(reader);
            }

            return null;
        }

        /// <summary>
        /// Retrieves all loans belonging to a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The loans owned by the user.</returns>
        public async Task<List<Loan>> GetLoansByUserAsync(int userId)
        {
            var query = "SELECT * FROM Loan WHERE userId = @userId";

            using var reader = await dbContext.ExecuteQueryAsync(query, new object[] { userId });

            List<Loan> loans = [];
            while (await reader.ReadAsync())
            {
                loans.Add(this.ReaderToLoan(reader));
            }

            return loans;
        }

        /// <summary>
        /// Retrieves loans filtered by type.
        /// </summary>
        /// <param name="loanType">The loan type to filter by.</param>
        /// <returns>The loans matching the requested type.</returns>
        public async Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
        {
            var query = "SELECT * FROM Loan WHERE loanType = @loanType";

            using var reader = await dbContext.ExecuteQueryAsync(query, new object[] { loanType.ToString() });

            List<Loan> loans = [];
            while (await reader.ReadAsync())
            {
                loans.Add(this.ReaderToLoan(reader));
            }

            return loans;
        }

        /// <summary>
        /// Retrieves loans filtered by status.
        /// </summary>
        /// <param name="loanStatus">The loan status to filter by.</param>
        /// <returns>The loans matching the requested status.</returns>
        public async Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
        {
            var query = "SELECT * FROM Loan WHERE loanStatus = @loanStatus";

            using var reader = await dbContext.ExecuteQueryAsync(query, new object[] { loanStatus.ToString() });

            List<Loan> loans = [];
            while (await reader.ReadAsync())
            {
                loans.Add(this.ReaderToLoan(reader));
            }

            return loans;
        }

        /// <summary>
        /// Saves an amortization schedule for a loan.
        /// </summary>
        /// <param name="rows">The amortization rows to persist.</param>
        /// <returns>A task that completes when persistence finishes.</returns>
        public async Task SaveAmortizationAsync(List<AmortizationRow> rows)
        {
            if (rows == null || rows.Count == EmptyCount)
            {
                return;
            }

            await dbContext.BeginTransactionAsync();

            try
            {
                var loanId = rows[FirstIndex].LoanId;

                var deleteQuery = "DELETE FROM AmortizationRow WHERE loanId = @LoanId";

                await dbContext.ExecuteNonQueryAsync(deleteQuery, new object[] { loanId });

                var insertQuery = @"INSERT INTO AmortizationRow 
                        (loanId, installmentNumber, dueDate, principalPortion, interestPortion, remainingBalance) 
                        VALUES 
                        (@LoanId, @InstallmentNumber, @DueDate, @PrincipalPortion, @InterestPortion, @RemainingBalance)";

                foreach (var row in rows)
                {
                    await dbContext.ExecuteNonQueryAsync(insertQuery,
                        new object[] { row.LoanId, row.InstallmentNumber, row.DueDate, row.PrincipalPortion, row.InterestPortion, row.RemainingBalance, });
                }

                await dbContext.CommitTransactionAsync();
            }
            catch
            {
                await dbContext.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves amortization rows for a loan.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns>The amortization schedule rows.</returns>
        public async Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
        {
            var query = @"SELECT id, loanId, installmentNumber, dueDate, 
                             principalPortion, interestPortion, remainingBalance 
                             FROM AmortizationRow 
                             WHERE loanId = @LoanId 
                             ORDER BY installmentNumber ASC";


            var reader = await dbContext.ExecuteQueryAsync(query, new object[] { loanId });
            List<AmortizationRow> rows = [];

            while (await reader.ReadAsync())
            {
                var row = new AmortizationRow
                {
                    Id = (int)reader["id"],
                    LoanId = (int)reader["loanId"],
                    InstallmentNumber = (int)reader["installmentNumber"],
                    DueDate = (DateTime)reader["dueDate"],
                    PrincipalPortion = (decimal)reader["principalPortion"],
                    InterestPortion = (decimal)reader["interestPortion"],
                    RemainingBalance = (decimal)reader["remainingBalance"],
                };
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// Creates a new loan application.
        /// </summary>
        /// <param name="application">The application payload to persist.</param>
        /// <returns>The created loan application identifier.</returns>
        public async Task<int> CreateLoanApplicationAsync(LoanApplicationRequest application)
        {

            var query = @"INSERT INTO LoanApplication
                (loanType, desiredAmount, preferredTermMonths, purpose, applicationStatus, rejectionReason, userId)
                OUTPUT INSERTED.id
                VALUES
                (@loanType, @amount, @term, @purpose, @status, @reason, @userId)";

            var newIdentificationNumber = (int)(await dbContext.ExecuteScalarAsync(query, new object[]
            {
                application.LoanType.ToString(),
                application.DesiredAmount,
                application.PreferredTermMonths,
                application.Purpose,
                LoanApplicationStatus.Pending.ToString(),
                DBNull.Value,
                application.UserId,
            }))!;
            return newIdentificationNumber;
        }

        /// <summary>
        /// Updates review status and optional rejection reason for an application.
        /// </summary>
        /// <param name="id">The loan application identifier.</param>
        /// <param name="loanApplicationStatus">The new application status.</param>
        /// <param name="reason">The optional rejection reason.</param>
        /// <returns>A task that completes when the update is applied.</returns>
        public async Task UpdateLoanApplicationStatusAsync(
            int id,
            LoanApplicationStatus loanApplicationStatus,
            string? reason)
        {
            var query = @"UPDATE LoanApplication
                             SET rejectionReason = @rejectionReason,
                                 applicationStatus = @loanApplicationStatus
                             WHERE id = @id";

            await dbContext.ExecuteNonQueryAsync(query, new object[] { reason != null ? reason : DBNull.Value, loanApplicationStatus.ToString(), id });
        }

        /// <summary>
        /// Creates a new loan record.
        /// </summary>
        /// <param name="loan">The loan to persist.</param>
        /// <returns>The created loan identifier.</returns>
        public async Task<int> CreateLoanAsync(Loan loan)
        {
            var query = @"INSERT INTO Loan
                (userId, loanType, principal, outstandingBalance, interestRate, monthlyInstallment, remainingMonths, loanStatus, termInMonths ,startDate)
                OUTPUT INSERTED.id
                VALUES
                (@userId, @loanType, @principal, @outstandingBalance, @interestRate, @monthlyInstallment, @remainingMonths, @loanStatus, @termInMonths , @startDate)";

            var newId = (int)(await dbContext.ExecuteScalarAsync(query,
                new object[] { loan.UserId, loan.LoanType.ToString(), loan.Principal, loan.OutstandingBalance, loan.InterestRate,
                    loan.MonthlyInstallment, loan.RemainingMonths, loan.LoanStatus.ToString(), loan.TermInMonths, loan.StartDate, }))!;
            return newId;
        }

        /// <summary>
        /// Updates a loan after a payment is processed.
        /// </summary>
        /// <param name="id">The loan identifier.</param>
        /// <param name="newBalance">The updated outstanding balance.</param>
        /// <param name="newRemainingMonths">The updated remaining term.</param>
        /// <param name="newStatus">The updated loan status.</param>
        /// <returns>A task that completes when the update is applied.</returns>
        public async Task UpdateLoanAfterPaymentAsync(
            int id,
            decimal newBalance,
            int newRemainingMonths,
            LoanStatus newStatus)
        {
            var query = @"UPDATE Loan
                             SET outstandingBalance = @outstandingBalance,
                                 remainingMonths = @remainingMonths,
                                 loanStatus = @loanStatus
                             WHERE id = @id";

            await dbContext.ExecuteNonQueryAsync(query, new object[] { newBalance, newRemainingMonths, newStatus.ToString(), id });
        }

        private Loan ReaderToLoan(SqlDataReader reader)
        {
            return new Loan
            {
                Id = (int)reader["id"],
                UserId = (int)reader["userId"],
                LoanType = Enum.Parse<LoanType>(reader["loanType"].ToString()!, true),
                Principal = (decimal)reader["principal"],
                OutstandingBalance = (decimal)reader["outstandingBalance"],
                InterestRate = (decimal)reader["interestRate"],
                MonthlyInstallment = (decimal)reader["monthlyInstallment"],
                RemainingMonths = (int)reader["remainingMonths"],
                LoanStatus = Enum.Parse<LoanStatus>(reader["loanStatus"].ToString()!, true),
                TermInMonths = (int)reader["termInMonths"],
                StartDate = (DateTime)reader["startDate"],
            };
        }
    }
}