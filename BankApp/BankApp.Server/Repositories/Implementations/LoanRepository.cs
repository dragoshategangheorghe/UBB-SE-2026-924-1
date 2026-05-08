using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.DataAccess;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Server.Repositories.Implementations
{
    /// <summary>
    /// EF Core-backed repository for loans and loan applications.
    /// </summary>
    public class LoanRepository : ILoanRepository
    {
        private const int EmptyCount = 0;
        private const int FirstIndex = 0;

        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoanRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's EF Core database context.</param>
        public LoanRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all loans from storage using the Loan DbSet.
        /// </summary>
        public async Task<List<Loan>> GetAllLoansAsync()
        {
            return await dbContext.Loans.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Retrieves a loan by its identifier using EF Core.
        /// </summary>
        public async Task<Loan> GetLoanByIdAsync(int id)
        {
            return await dbContext.Loans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Retrieves loans belonging to the specified user through the user navigation mapping.
        /// </summary>
        public async Task<List<Loan>> GetLoansByUserAsync(int userId)
        {
            return await dbContext.Loans
                .AsNoTracking()
                .Where(x => EF.Property<int>(x, "UserId") == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves loans filtered by type using LINQ.
        /// </summary>
        public async Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
        {
            return await dbContext.Loans
                .AsNoTracking()
                .Where(x => x.LoanType == loanType)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves loans filtered by status using LINQ.
        /// </summary>
        public async Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
        {
            return await dbContext.Loans
                .AsNoTracking()
                .Where(x => x.LoanStatus == loanStatus)
                .ToListAsync();
        }

        /// <summary>
        /// Saves an amortization schedule for a loan using EF Core entities.
        /// </summary>
        public async Task SaveAmortizationAsync(List<AmortizationRow> rows)
        {
            if (rows == null || rows.Count == EmptyCount)
            {
                return;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var loanId = rows[FirstIndex].LoanId;
                var existingRows = dbContext.AmortizationRows.Where(x => x.LoanId == loanId);
                dbContext.AmortizationRows.RemoveRange(existingRows);
                await dbContext.AmortizationRows.AddRangeAsync(rows);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves amortization rows for a loan using the EF Core DbSet.
        /// </summary>
        public async Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
        {
            return await dbContext.AmortizationRows
                .AsNoTracking()
                .Where(x => x.LoanId == loanId)
                .OrderBy(x => x.InstallmentNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new loan application using EF Core.
        /// </summary>
        public async Task<int> CreateLoanApplicationAsync(LoanApplicationRequest application)
        {
            var loanApplication = new LoanApplication
            {
                UserId = application.UserId,
                LoanType = application.LoanType,
                DesiredAmount = application.DesiredAmount,
                PreferredTermMonths = application.PreferredTermMonths,
                Purpose = application.Purpose,
                ApplicationStatus = LoanApplicationStatus.Pending,
                RejectionReason = null,
            };

            dbContext.LoanApplications.Add(loanApplication);
            await dbContext.SaveChangesAsync();
            return loanApplication.UserId;
        }

        /// <summary>
        /// Updates review status and optional rejection reason for an application using EF Core tracking.
        /// </summary>
        public async Task UpdateLoanApplicationStatusAsync(
            int id,
            LoanApplicationStatus loanApplicationStatus,
            string? reason)
        {
            var application = await dbContext.LoanApplications.FirstOrDefaultAsync(x => x.UserId == id);
            if (application == null)
            {
                return;
            }

            application.ApplicationStatus = loanApplicationStatus;
            application.RejectionReason = reason;
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a new loan record using the EF Core DbSet.
        /// </summary>
        public async Task<int> CreateLoanAsync(Loan loan)
        {
            dbContext.Loans.Add(loan);
            await dbContext.SaveChangesAsync();
            return loan.Id;
        }

        /// <summary>
        /// Updates a loan after a payment is processed using EF Core.
        /// </summary>
        public async Task UpdateLoanAfterPaymentAsync(
            int id,
            decimal newBalance,
            int newRemainingMonths,
            LoanStatus newStatus)
        {
            var loan = await dbContext.Loans.FirstOrDefaultAsync(x => x.Id == id);
            if (loan == null)
            {
                return;
            }

            loan.OutstandingBalance = newBalance;
            loan.RemainingMonths = newRemainingMonths;
            loan.LoanStatus = newStatus;
            await dbContext.SaveChangesAsync();
        }
    }
}