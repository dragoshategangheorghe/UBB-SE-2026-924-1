using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;
using BankApp.Server.Configuration;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using NUnit.Framework;

namespace BankApp.Server.Tests;

[TestFixture]
public class TransactionHistoryServiceTests
{
    private ITransactionHistoryRepository mockTransactionHistoryRepository;

    private ITransactionExportService mockTransactionExportService;
    private TransactionHistoryService transactionHistoryService;

    [SetUp]
    public void SetUp()
    {
        mockTransactionHistoryRepository = Substitute.For<ITransactionHistoryRepository>();
        mockTransactionExportService = Substitute.For<ITransactionExportService>();

        transactionHistoryService = new TransactionHistoryService(
            mockTransactionHistoryRepository, mockTransactionExportService);
    }

    [Test]
    public void GetHistory_UserExists_ReturnsUserHistory()
    {
        User user = CardServiceTests.CreateUser(false);
        mockTransactionHistoryRepository.GetTransactionsByUserId(user.Id).Returns([]);

        TransactionHistoryResponse transactionHistoryResponse =
            transactionHistoryService.GetHistory(user.Id, new TransactionHistoryRequest());

        Assert.That(transactionHistoryResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionHistoryResponse.Success, Is.True);
            Assert.That(transactionHistoryResponse.Message, Is.EqualTo("Transaction history loaded successfully."));
        }
    }

    [Test]
    public void GetTransaction_TransactionDoesNotExist_ReturnsFailure()
    {
        User user = CardServiceTests.CreateUser(false);
        mockTransactionHistoryRepository.GetTransactionById(user.Id, 1).Returns((TransactionHistoryItemDto)null!);

        TransactionDetailsResponse transactionDetailsResponse = transactionHistoryService.GetTransaction(user.Id, 1);

        Assert.That(transactionDetailsResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionDetailsResponse.Success, Is.False);
            Assert.That(transactionDetailsResponse.Message, Is.EqualTo("Transaction not found."));
        }
    }

    [Test]
    public void GetTransaction_TransactionExists_ReturnsDetails()
    {
        User user = CardServiceTests.CreateUser(false);
        TransactionHistoryItemDto transaction = new TransactionHistoryItemDto();
        transaction.Id = 1;
        mockTransactionHistoryRepository.GetTransactionById(user.Id, transaction.Id).Returns(transaction);

        TransactionDetailsResponse transactionDetailsResponse =
            transactionHistoryService.GetTransaction(user.Id, transaction.Id);

        Assert.That(transactionDetailsResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionDetailsResponse.Success, Is.True);
            Assert.That(transactionDetailsResponse.Message, Is.EqualTo("Transaction details loaded successfully."));
            Assert.That(transactionDetailsResponse.Transaction, Is.EqualTo(transaction));
        }
    }

    [Test]
    public void ExportReceipt_TransactionDoesNotExist_ReturnsNewTransactionExportResult()
    {
        User user = CardServiceTests.CreateUser(false);
        TransactionHistoryItemDto transaction = new TransactionHistoryItemDto();
        transaction.Id = 1;
        mockTransactionHistoryRepository.GetTransactionById(user.Id, transaction.Id)
            .Returns((TransactionHistoryItemDto)null!);

        TransactionExportResult transactionExportResult =
            transactionHistoryService.ExportReceipt(user.Id, transaction.Id);

        Assert.That(transactionExportResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionExportResult.ContentType, Is.EqualTo(string.Empty));
            Assert.That(transactionExportResult.FileName, Is.EqualTo(string.Empty));
            Assert.That(transactionExportResult.Content, Is.EqualTo(Array.Empty<byte>()));
        }
    }

    [Test]
    public void ExportReceipt_TransactionExists_ReturnsExistingTransactionExportResult()
    {
        User user = CardServiceTests.CreateUser(false);
        TransactionHistoryItemDto transaction = new TransactionHistoryItemDto();
        transaction.Id = 1;
        mockTransactionHistoryRepository.GetTransactionById(user.Id, transaction.Id).Returns(transaction);
        mockTransactionExportService.ExportReceipt(transaction)
            .Returns(new TransactionExportResult { FileName = "Exists.txt" });

        TransactionExportResult transactionExportResult =
            transactionHistoryService.ExportReceipt(user.Id, transaction.Id);

        Assert.That(transactionExportResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionExportResult.FileName, Is.EqualTo("Exists.txt"));
        }
    }

    [Test]
    public void GetHistory_FiltersAndSortsTransactions_ByCompletedAscendingAndAmount()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());

        TransactionHistoryResponse transactionHistoryResponse = transactionHistoryService.GetHistory(userId, new TransactionHistoryRequest
        {
            SearchTerm = "market",
            Status = "Completed",
            Direction = "Debit",
            SortField = TransactionSortFields.Amount,
            SortDirection = SortDirections.Asc
        });

        Assert.That(transactionHistoryResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionHistoryResponse.Success, Is.True);
            Assert.That(transactionHistoryResponse.Transactions.Count, Is.EqualTo(2));
            Assert.That(transactionHistoryResponse.Transactions[0].ReferenceNumber, Is.EqualTo("REF-002"));
            Assert.That(transactionHistoryResponse.Transactions[1].ReferenceNumber, Is.EqualTo("REF-001"));
        }
    }

    [Test]
    public void GetHistory_FiltersAndSortsTransactions_ByCompletedDescendingAndAmount()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());

        TransactionHistoryResponse transactionHistoryResponse = transactionHistoryService.GetHistory(userId, new TransactionHistoryRequest
        {
            SearchTerm = "market",
            Status = "Completed",
            Direction = "Debit",
            SortField = TransactionSortFields.Amount,
            SortDirection = SortDirections.Desc
        });

        Assert.That(transactionHistoryResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionHistoryResponse.Success, Is.True);
            Assert.That(transactionHistoryResponse.Transactions.Count, Is.EqualTo(2));
            Assert.That(transactionHistoryResponse.Transactions[1].ReferenceNumber, Is.EqualTo("REF-002"));
            Assert.That(transactionHistoryResponse.Transactions[0].ReferenceNumber, Is.EqualTo("REF-001"));
        }
    }
    [Test]
    public void GetHistory_FiltersAndSortsTransactions_ByCompletedAndAmount()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());

        TransactionHistoryResponse transactionHistoryResponse = transactionHistoryService.GetHistory(userId, new TransactionHistoryRequest
        {
            SearchTerm = "market",
            Status = "Completed",
            Direction = "Debit",
            SortField = TransactionSortFields.Date
        });

        Assert.That(transactionHistoryResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionHistoryResponse.Success, Is.True);
            Assert.That(transactionHistoryResponse.Transactions.Count, Is.EqualTo(2));
            Assert.That(transactionHistoryResponse.Transactions[1].ReferenceNumber, Is.EqualTo("REF-002"));
            Assert.That(transactionHistoryResponse.Transactions[0].ReferenceNumber, Is.EqualTo("REF-001"));
        }
    }

    [Test]
    public void GetFilterMetadata_TransactionsByUserExists_ReturnsCardFilterOptions()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());
        mockTransactionHistoryRepository.GetAccountsByUserId(userId).Returns(new List<Account>
        {
            new () { Id = 2, AccountName = "Savings", IBAN = "RO49AAAA1B31007593840000" },
            new () { Id = 1, AccountName = "Everyday", IBAN = "RO49AAAA1B31007593840001" }
        });
        mockTransactionHistoryRepository.GetCardsByUserId(10)
            .Returns(new List<Card>
            {
                new () { Id = 8, CardBrand = "Visa", CardType = "Debit", CardNumber = "4111111111111111" },
                new () { Id = 7, CardBrand = "Mastercard", CardType = "Debit", CardNumber = "5555444433331111" }
            });

        TransactionFilterMetadataResponse transactionFilterMetadataResponse = transactionHistoryService.GetFilterMetadata(userId);

        Assert.That(transactionFilterMetadataResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionFilterMetadataResponse.Success, Is.True);
            Assert.That(transactionFilterMetadataResponse.Cards.Any(cardFilterOption => cardFilterOption.Label == "Mastercard **** 1111")); // LINQ
            Assert.That(transactionFilterMetadataResponse.Cards.Any(cardFilterOption => cardFilterOption.Label == "Visa **** 1111"));
        }
    }
    [Test]
    public void GetFilterMetadata_TransactionsByUserExists_ReturnsAvailableStatuses()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());
        mockTransactionHistoryRepository.GetAccountsByUserId(userId).Returns(new List<Account>
        {
            new () { Id = 2, AccountName = "Savings", IBAN = "RO49AAAA1B31007593840000" },
            new () { Id = 1, AccountName = "Everyday", IBAN = "RO49AAAA1B31007593840001" }
        });
        mockTransactionHistoryRepository.GetCardsByUserId(10)
            .Returns(new List<Card>
            {
                new () { Id = 8, CardBrand = "Visa", CardType = "Debit", CardNumber = "4111111111111111" },
                new () { Id = 7, CardBrand = "Mastercard", CardType = "Debit", CardNumber = "5555444433331111" }
            });

        TransactionFilterMetadataResponse transactionFilterMetadataResponse = transactionHistoryService.GetFilterMetadata(userId);

        Assert.That(transactionFilterMetadataResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionFilterMetadataResponse.Success, Is.True);
            Assert.That(transactionFilterMetadataResponse.AvailableStatuses.ToArray(), Is.EqualTo(new[] { "Completed", "Reversed" }));
        }
    }
    [Test]
    public void GetFilterMetadata_TransactionsByUserExists_ReturnsAvailableDirections()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());
        mockTransactionHistoryRepository.GetAccountsByUserId(userId).Returns(new List<Account>
        {
            new () { Id = 2, AccountName = "Savings", IBAN = "RO49AAAA1B31007593840000" },
            new () { Id = 1, AccountName = "Everyday", IBAN = "RO49AAAA1B31007593840001" }
        });
        mockTransactionHistoryRepository.GetCardsByUserId(10)
            .Returns(new List<Card>
            {
                new () { Id = 8, CardBrand = "Visa", CardType = "Debit", CardNumber = "4111111111111111" },
                new () { Id = 7, CardBrand = "Mastercard", CardType = "Debit", CardNumber = "5555444433331111" }
            });

        TransactionFilterMetadataResponse transactionFilterMetadataResponse = transactionHistoryService.GetFilterMetadata(userId);

        Assert.That(transactionFilterMetadataResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionFilterMetadataResponse.Success, Is.True);
            Assert.That(transactionFilterMetadataResponse.AvailableDirections.ToArray(), Is.EqualTo(new[] { "Credit", "Debit" }));
        }
    }
    [Test]
    public void GetFilterMetadata_TransactionsByUserExists_ReturnsAccountTypes()
    {
        int userId = 10;
        mockTransactionHistoryRepository.GetTransactionsByUserId(userId).Returns(CreateTransactions());
        mockTransactionHistoryRepository.GetAccountsByUserId(userId).Returns(new List<Account>
        {
            new () { Id = 2, AccountName = "Savings", IBAN = "RO49AAAA1B31007593840000" },
            new () { Id = 1, AccountName = "Everyday", IBAN = "RO49AAAA1B31007593840001" }
        });
        mockTransactionHistoryRepository.GetCardsByUserId(10)
            .Returns(new List<Card>
            {
                new () { Id = 8, CardBrand = "Visa", CardType = "Debit", CardNumber = "4111111111111111" },
                new () { Id = 7, CardBrand = "Mastercard", CardType = "Debit", CardNumber = "5555444433331111" }
            });

        TransactionFilterMetadataResponse transactionFilterMetadataResponse = transactionHistoryService.GetFilterMetadata(userId);

        Assert.That(transactionFilterMetadataResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transactionFilterMetadataResponse.Success, Is.True);
            Assert.That(transactionFilterMetadataResponse.Accounts.Select(account => account.Name).ToArray(), Is.EqualTo(new[] { "Everyday", "Savings" }));
        }
    }

    private static List<TransactionHistoryItemDto> CreateTransactions()
    {
        return new List<TransactionHistoryItemDto>
        {
            new ()
            {
                Id = 1,
                AccountId = 1,
                ReferenceNumber = "REF-001",
                Timestamp = new DateTime(2026, 3, 20, 10, 0, 0),
                TransactionType = "CardPayment",
                CounterpartyOrMerchant = "Central Market",
                Amount = 35m,
                Currency = "EUR",
                Direction = "Debit",
                RunningBalanceAfterTransaction = 465m,
                Status = "Completed",
                CategoryName = "Groceries"
            },
            new ()
            {
                Id = 2,
                AccountId = 1,
                ReferenceNumber = "REF-002",
                Timestamp = new DateTime(2026, 3, 19, 12, 0, 0),
                TransactionType = "CardPayment",
                CounterpartyOrMerchant = "Weekend Market",
                Amount = 15m,
                Currency = "EUR",
                Direction = "Debit",
                RunningBalanceAfterTransaction = 500m,
                Status = "Completed",
                CategoryName = "Groceries"
            },
            new ()
            {
                Id = 3,
                AccountId = 1,
                ReferenceNumber = "REF-003",
                Timestamp = new DateTime(2026, 3, 18, 8, 0, 0),
                TransactionType = "Salary",
                CounterpartyOrMerchant = "Employer Inc",
                Amount = 100m,
                Currency = "EUR",
                Direction = "Credit",
                RunningBalanceAfterTransaction = 515m,
                Status = "Completed",
                CategoryName = "Income"
            },
            new ()
            {
                Id = 4,
                AccountId = 1,
                ReferenceNumber = "REF-004",
                Timestamp = new DateTime(2026, 3, 17, 8, 0, 0),
                TransactionType = "Transfer",
                CounterpartyOrMerchant = "Refunded Merchant",
                Amount = 25m,
                Currency = "EUR",
                Direction = "Debit",
                RunningBalanceAfterTransaction = 415m,
                Status = "Reversed",
                CategoryName = "Shopping"
            }
        };
    }
}