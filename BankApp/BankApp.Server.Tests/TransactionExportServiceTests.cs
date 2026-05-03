using System.IO.Compression;
using System.Text;
using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Interfaces;
using NUnit.Framework;
using NSubstitute;

namespace BankApp.Server.Tests;

[TestFixture]
public class TransactionExportServiceTests
{
    private TransactionExportService exportService;

    private static List<TransactionHistoryItemDto> CreateTransactions()
    {
        return new List<TransactionHistoryItemDto>
        {
            new ()
            {
                Id = 100,
                ReferenceNumber = "REF-100",
                Timestamp = new DateTime(2026, 3, 25, 14, 0, 0),
                TransactionType = "CardPayment",
                CounterpartyOrMerchant = "Coffee Shop",
                Amount = 12.5m,
                Currency = "EUR",
                Direction = "Debit",
                RunningBalanceAfterTransaction = 320m,
                Status = "Completed",
                SourceAccountIban = "RO49AAAA1B31007593840000",
                DestinationAccountIban = "RO49BBBB1B31007593840001",
                Fee = 0m
            },
            new ()
            {
                Id = 101,
                ReferenceNumber = "REF-101",
                Timestamp = new DateTime(2026, 3, 25, 15, 0, 0),
                TransactionType = "CardPayment",
                CounterpartyOrMerchant = "Coffee Shop",
                Amount = 12.5m,
                Currency = "EUR",
                Direction = "Debit",
                RunningBalanceAfterTransaction = 320m,
                Status = "Completed",
                SourceAccountIban = "RO49AAAA1B31007593840000",
                DestinationAccountIban = "RO49BBBB1B31007593840001",
                ExchangeRate = 5.030602m,
                Fee = 0m
            },
        };
    }

    [SetUp]
    public void SetUp()
    {
        exportService = new TransactionExportService();
    }

    [Test]
    public void ExportStatement_TransactionsPresent_ReturnsCsvDocument()
    {
        TransactionExportService service = new ();

        var result = service.ExportStatement(CreateTransactions(), new TransactionHistoryRequest(), TransactionExportFormats.Csv);
        string content = Encoding.UTF8.GetString(result.Content);

        Assert.That(result.ContentType, Is.EqualTo("text/csv"));
    }

    [Test]
    public void ExportStatement_TransactionsPresent_ReturnsCsvDocumentContainingExpectedTransactionReferenceNumber()
    {
        TransactionExportService service = new ();

        var result = service.ExportStatement(CreateTransactions(), new TransactionHistoryRequest(), TransactionExportFormats.Csv);
        string content = Encoding.UTF8.GetString(result.Content);

        Assert.That(content.Contains("REF-100"), Is.True);
    }

    [Test]
    public void ExportStatement_TransactionsPresent_ReturnsPdfDocument()
    {
        TransactionExportService service = new ();

        var result = service.ExportStatement(CreateTransactions(), new TransactionHistoryRequest(), TransactionExportFormats.Pdf);

        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(Encoding.ASCII.GetString(result.Content).Contains("%PDF"), Is.True);
    }

    [Test]
    public void ExportStatement_TransactionsPresent_ReturnsXlsxArchive()
    {
        TransactionExportService service = new ();

        var result = service.ExportStatement(CreateTransactions(), new TransactionHistoryRequest(), TransactionExportFormats.Xlsx);

        using MemoryStream memoryStream = new (result.Content);
        using ZipArchive archive = new (memoryStream, ZipArchiveMode.Read);

        Assert.That(result.ContentType, Is.EqualTo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        Assert.That(archive.Entries.Where(entry => entry.FullName == "xl/worksheets/sheet1.xml").Any(), Is.True);
    }

    [Test]
    public void ExportStatement_RequestForTransactionsWithSpecifiedTimeInterval_ReturnsCsvDocumentWithTimePeriodSpecifiedInTitle()
    {
        TransactionExportService service = new ();
        TransactionHistoryRequest requestWithSpecificTimePeriod = new ()
        {
            FromDate = new DateTime(2026, 3, 24),
            ToDate = new DateTime(2026, 3, 26),
        };

        var result = service.ExportStatement(CreateTransactions(), requestWithSpecificTimePeriod, TransactionExportFormats.Csv);
        string content = Encoding.UTF8.GetString(result.Content);

        Assert.That(result.FileName, Is.EqualTo("transaction-history-2026-03-24-to-2026-03-26.csv"));
    }

    [Test]
    public void ExportStatement_RequestForTransactionsWithSpecifiedTimeInterval_ReturnsPdfDocumentWithTimePeriodSpecifiedInText()
    {
        TransactionExportService service = new ();
        TransactionHistoryRequest requestWithSpecificTimePeriod = new ()
        {
            FromDate = new DateTime(2026, 3, 24),
            ToDate = new DateTime(2026, 3, 26),
        };

        var result = service.ExportStatement(CreateTransactions(), requestWithSpecificTimePeriod, TransactionExportFormats.Pdf);

        Assert.That(Encoding.ASCII.GetString(result.Content).Contains("Period: 2026-03-24 to 2026-03-26"), Is.True);
    }

    [Test]
    public void ExportReceipt_OneTransactionSelected_ReturnsPdfReceiptNamedForTransaction()
    {
        TransactionExportService service = new ();

        TransactionExportResult result = service.ExportReceipt(CreateTransactions()[0]);

        Assert.That(result.FileName, Is.EqualTo("transaction-receipt-100.pdf"));
    }

    [Test]
    public void ExportReceipt_OneTransactionWithExchangeRateSpecifiedSelected_ReturnsPdfReceiptWithExchangeRate()
    {
        TransactionExportService service = new ();

        TransactionExportResult result = service.ExportReceipt(CreateTransactions()[1]);

        Assert.That(Encoding.ASCII.GetString(result.Content).Contains("Exchange Rate: 5.030602"), Is.True);
    }
}
