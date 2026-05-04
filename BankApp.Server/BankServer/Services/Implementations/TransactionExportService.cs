using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using BankApp.Models.DTOs.Transactions;
using BankApp.Server.Services.Interfaces;

namespace BankApp.Server.Services.Implementations
{
    public class TransactionExportService : ITransactionExportService
    {
        private static readonly DateTimeOffset ZipEntryTimestamp = new (1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public TransactionExportResult ExportStatement(IReadOnlyCollection<TransactionHistoryItemDto> transactions, TransactionHistoryRequest request, string format)
        {
            string normalizedFormat = NormalizeFormat(format);
            return normalizedFormat switch
            {
                TransactionExportFormats.Pdf => new TransactionExportResult
                {
                    Content = BuildPdf(BuildStatementLines(transactions, request)),
                    ContentType = "application/pdf",
                    FileName = $"{BuildStatementBaseName(request)}.pdf"
                },
                TransactionExportFormats.Xlsx => new TransactionExportResult
                {
                    Content = BuildXlsx(CreateStatementRows(transactions)),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = $"{BuildStatementBaseName(request)}.xlsx"
                },
                _ => new TransactionExportResult
                {
                    Content = Encoding.UTF8.GetBytes(BuildCsv(CreateStatementRows(transactions))),
                    ContentType = "text/csv",
                    FileName = $"{BuildStatementBaseName(request)}.csv"
                }
            };
        }

        public TransactionExportResult ExportReceipt(TransactionHistoryItemDto transaction)
        {
            return new TransactionExportResult
            {
                Content = BuildPdf(BuildReceiptLines(transaction)),
                ContentType = "application/pdf",
                FileName = $"transaction-receipt-{transaction.Id}.pdf"
            };
        }

        private static string NormalizeFormat(string? format)
        {
            if (string.Equals(format, TransactionExportFormats.Pdf, StringComparison.OrdinalIgnoreCase))
            {
                return TransactionExportFormats.Pdf;
            }

            if (string.Equals(format, TransactionExportFormats.Xlsx, StringComparison.OrdinalIgnoreCase))
            {
                return TransactionExportFormats.Xlsx;
            }

            return TransactionExportFormats.Csv;
        }

        private static string BuildStatementBaseName(TransactionHistoryRequest request)
        {
            string fromPart = request.FromDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "all";
            string toPart = request.ToDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "latest";
            return $"transaction-history-{fromPart}-to-{toPart}";
        }

        private static IReadOnlyCollection<string[]> CreateStatementRows(IEnumerable<TransactionHistoryItemDto> transactions)
        {
            List<string[]> rows = new ()
            {
                new[]
                {
                    "Timestamp",
                    "Reference Number",
                    "Transaction Type",
                    "Counterparty or Merchant",
                    "Amount",
                    "Currency",
                    "Direction",
                    "Running Balance",
                    "Status",
                    "Source IBAN",
                    "Destination IBAN",
                    "Fee",
                    "Exchange Rate",
                    "Description"
                }
            };

            rows.AddRange(transactions.Select(transaction => new[]
            {
                transaction.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                transaction.ReferenceNumber,
                transaction.TransactionType,
                transaction.CounterpartyOrMerchant,
                transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                transaction.Currency,
                transaction.Direction,
                transaction.RunningBalanceAfterTransaction.ToString("0.00", CultureInfo.InvariantCulture),
                transaction.Status,
                transaction.SourceAccountIban ?? string.Empty,
                transaction.DestinationAccountIban ?? string.Empty,
                transaction.Fee.ToString("0.00", CultureInfo.InvariantCulture),
                transaction.ExchangeRate?.ToString("0.000000", CultureInfo.InvariantCulture) ?? string.Empty,
                transaction.Description ?? string.Empty
            }));

            return rows;
        }

        private static IEnumerable<string> BuildStatementLines(IEnumerable<TransactionHistoryItemDto> transactions, TransactionHistoryRequest request)
        {
            yield return "BankApp Transaction Statement";
            yield return $"Period: {request.FromDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "All"} to {request.ToDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "Latest"}";
            yield return string.Empty;

            foreach (TransactionHistoryItemDto transaction in transactions)
            {
                yield return $"{transaction.Timestamp:yyyy-MM-dd HH:mm} | {transaction.ReferenceNumber} | {transaction.TransactionType} | {transaction.CounterpartyOrMerchant} | {transaction.Direction} {transaction.Amount:0.00} {transaction.Currency} | {transaction.Status}";
            }
        }

        private static IEnumerable<string> BuildReceiptLines(TransactionHistoryItemDto transaction)
        {
            yield return "BankApp Transaction Receipt";
            yield return string.Empty;
            yield return $"Transaction ID: {transaction.Id}";
            yield return $"Reference Number: {transaction.ReferenceNumber}";
            yield return $"Timestamp: {transaction.Timestamp:yyyy-MM-dd HH:mm:ss}";
            yield return $"Transaction Type: {transaction.TransactionType}";
            yield return $"Counterparty or Merchant: {transaction.CounterpartyOrMerchant}";
            yield return $"Amount: {transaction.Amount:0.00} {transaction.Currency}";
            yield return $"Direction: {transaction.Direction}";
            yield return $"Status: {transaction.Status}";
            yield return $"Source IBAN: {transaction.SourceAccountIban}";
            yield return $"Destination IBAN: {transaction.DestinationAccountIban}";
            yield return $"Fee: {transaction.Fee:0.00}";
            yield return $"Exchange Rate: {(transaction.ExchangeRate?.ToString("0.000000", CultureInfo.InvariantCulture) ?? "N/A")}";
            yield return $"Description: {transaction.Description}";
        }

        private static string BuildCsv(IReadOnlyCollection<string[]> rows)
        {
            StringBuilder builder = new ();
            foreach (string[] row in rows)
            {
                builder.AppendLine(string.Join(",", row.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string EscapeCsv(string value)
        {
            string escapedValue = value.Replace("\"", "\"\"");
            return $"\"{escapedValue}\"";
        }

        private static byte[] BuildPdf(IEnumerable<string> rawLines)
        {
            List<string> lines = rawLines
                .SelectMany(line => WrapLine(line, 90))
                .ToList();

            StringBuilder content = new ();
            content.AppendLine("BT");
            content.AppendLine("/F1 10 Tf");
            content.AppendLine("72 760 Td");

            bool isFirstLine = true;
            foreach (string line in lines)
            {
                if (!isFirstLine)
                {
                    content.AppendLine("0 -14 Td");
                }

                content.AppendLine($"({EscapePdf(line)}) Tj");
                isFirstLine = false;
            }

            content.AppendLine("ET");

            string stream = content.ToString();
            List<string> objects = new ()
            {
                "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
                "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
                "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n",
                "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n",
                $"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream\nendobj\n"
            };

            StringBuilder pdf = new ();
            pdf.Append("%PDF-1.4\n");

            List<int> offsets = new () { 0 };
            foreach (string pdfObject in objects)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append(pdfObject);
            }

            int xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append($"xref\n0 {objects.Count + 1}\n");
            pdf.Append("0000000000 65535 f \n");
            for (int i = 1; i < offsets.Count; i++)
            {
                pdf.Append($"{offsets[i]:D10} 00000 n \n");
            }

            pdf.Append("trailer\n");
            pdf.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
            pdf.Append("startxref\n");
            pdf.Append($"{xrefOffset}\n");
            pdf.Append("%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private static IEnumerable<string> WrapLine(string line, int maxLength)
        {
            if (string.IsNullOrEmpty(line))
            {
                yield return string.Empty;
                yield break;
            }

            for (int index = 0; index < line.Length; index += maxLength)
            {
                yield return line.Substring(index, Math.Min(maxLength, line.Length - index));
            }
        }

        private static string EscapePdf(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);
        }

        private static byte[] BuildXlsx(IReadOnlyCollection<string[]> rows)
        {
            using MemoryStream stream = new ();
            using (ZipArchive archive = new (stream, ZipArchiveMode.Create, true))
            {
                WriteEntry(archive, "[Content_Types].xml", CreateContentTypesXml());
                WriteEntry(archive, "_rels/.rels", CreateRootRelsXml());
                WriteEntry(archive, "xl/workbook.xml", CreateWorkbookXml());
                WriteEntry(archive, "xl/_rels/workbook.xml.rels", CreateWorkbookRelsXml());
                WriteEntry(archive, "xl/styles.xml", CreateStylesXml());
                WriteEntry(archive, "xl/worksheets/sheet1.xml", CreateWorksheetXml(rows));
            }

            return stream.ToArray();
        }

        private static void WriteEntry(ZipArchive archive, string entryName, string content)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.NoCompression);
            entry.LastWriteTime = ZipEntryTimestamp;
            using StreamWriter writer = new (entry.Open(), new UTF8Encoding(false));
            writer.Write(content);
        }

        private static string CreateContentTypesXml()
        {
            XNamespace ns = "http://schemas.openxmlformats.org/package/2006/content-types";
            XDocument document = new (
                new XElement(ns + "Types",
                    new XElement(ns + "Default",
                        new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement(ns + "Default",
                        new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement(ns + "Override",
                        new XAttribute("PartName", "/xl/workbook.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                    new XElement(ns + "Override",
                        new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                    new XElement(ns + "Override",
                        new XAttribute("PartName", "/xl/styles.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"))));

            return document.Declaration + document.ToString(SaveOptions.DisableFormatting);
        }

        private static string CreateRootRelsXml()
        {
            XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";
            XDocument document = new (
                new XElement(ns + "Relationships",
                    new XElement(ns + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "xl/workbook.xml"))));

            return document.Declaration + document.ToString(SaveOptions.DisableFormatting);
        }

        private static string CreateWorkbookXml()
        {
            XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XNamespace relationshipNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            XDocument document = new (
                new XElement(spreadsheetNs + "workbook",
                    new XAttribute(XNamespace.Xmlns + "r", relationshipNs),
                    new XElement(spreadsheetNs + "sheets",
                        new XElement(spreadsheetNs + "sheet",
                            new XAttribute("name", "Transactions"),
                            new XAttribute("sheetId", "1"),
                            new XAttribute(relationshipNs + "id", "rId1")))));

            return document.Declaration + document.ToString(SaveOptions.DisableFormatting);
        }

        private static string CreateWorkbookRelsXml()
        {
            XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";
            XDocument document = new (
                new XElement(ns + "Relationships",
                    new XElement(ns + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                        new XAttribute("Target", "worksheets/sheet1.xml")),
                    new XElement(ns + "Relationship",
                        new XAttribute("Id", "rId2"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                        new XAttribute("Target", "styles.xml"))));

            return document.Declaration + document.ToString(SaveOptions.DisableFormatting);
        }

        private static string CreateStylesXml()
        {
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XDocument document = new (
                new XElement(ns + "styleSheet",
                    new XElement(ns + "fonts", new XAttribute("count", "1"),
                        new XElement(ns + "font",
                            new XElement(ns + "sz", new XAttribute("val", "11")),
                            new XElement(ns + "name", new XAttribute("val", "Calibri")))),
                    new XElement(ns + "fills", new XAttribute("count", "1"),
                        new XElement(ns + "fill", new XElement(ns + "patternFill", new XAttribute("patternType", "none")))),
                    new XElement(ns + "borders", new XAttribute("count", "1"),
                        new XElement(ns + "border",
                            new XElement(ns + "left"),
                            new XElement(ns + "right"),
                            new XElement(ns + "top"),
                            new XElement(ns + "bottom"),
                            new XElement(ns + "diagonal"))),
                    new XElement(ns + "cellStyleXfs", new XAttribute("count", "1"),
                        new XElement(ns + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"))),
                    new XElement(ns + "cellXfs", new XAttribute("count", "1"),
                        new XElement(ns + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"),
                            new XAttribute("xfId", "0")))));

            return document.Declaration + document.ToString(SaveOptions.DisableFormatting);
        }

        private static string CreateWorksheetXml(IReadOnlyCollection<string[]> rows)
        {
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XElement sheetData = new (ns + "sheetData");

            int rowIndex = 1;
            foreach (string[] rowValues in rows)
            {
                XElement row = new (ns + "row", new XAttribute("r", rowIndex));
                for (int columnIndex = 0; columnIndex < rowValues.Length; columnIndex++)
                {
                    string cellReference = $"{GetColumnName(columnIndex + 1)}{rowIndex}";
                    row.Add(new XElement(ns + "c",
                        new XAttribute("r", cellReference),
                        new XAttribute("t", "inlineStr"),
                        new XElement(ns + "is", new XElement(ns + "t", rowValues[columnIndex]))));
                }

                sheetData.Add(row);
                rowIndex++;
            }

            XDocument document = new (
                new XElement(ns + "worksheet", sheetData));

            return document.Declaration + document.ToString(SaveOptions.DisableFormatting);
        }

        private static string GetColumnName(int columnNumber)
        {
            StringBuilder builder = new ();
            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                builder.Insert(0, (char)('A' + remainder));
                columnNumber = (columnNumber - 1) / 26;
            }

            return builder.ToString();
        }
    }
}
