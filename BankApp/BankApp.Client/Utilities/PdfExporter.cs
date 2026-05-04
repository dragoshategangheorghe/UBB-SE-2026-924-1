namespace BankApp.Client.Utilities
{
    using BankApp.Models.Features.Loans;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides utility methods to generate and export data to PDF documents.
    /// </summary>
    public class PdfExporter
    {
        private const float PageWidth = 595f;
        private const float PageHeight = 842f;
        private const float LeftMargin = 48f;
        private const float TopMargin = 54f;
        private const float RowHeight = 18f;
        private const float HeaderVerticalOffset = 34f;
        private const float FirstRowVerticalOffset = 58f;
        private const float BottomContentThreshold = 68f;
        private const float DueDateColumnOffset = 36f;
        private const float PrincipalColumnOffset = 130f;
        private const float InterestColumnOffset = 255f;
        private const float BalanceColumnOffset = 360f;

        private const int TitleFontSize = 18;
        private const int HeaderFontSize = 11;
        private const int RowFontSize = 10;
        private const int ObjectNumberStart = 1;
        private const int ObjectGenerationNumber = 0;
        private const int CatalogObjectId = 1;
        private const int PagesObjectId = 2;
        private const int FirstPageObjectId = 3;
        private const int FirstContentObjectId = 4;
        private const int ObjectsPerPage = 2;
        private const int CoordinateOrigin = 0;
        private const int FixedOffsetDigits = 10;
        private const int CrossReferenceFreeEntryGeneration = 65535;
        private const int CrossReferenceUsedEntryGeneration = 0;
        private const int ZeroBufferIndex = 0;
        private const long ZeroStreamOffset = 0L;

        private const string CurrencyDisplayFormat = "C2";
        private const string CoordinateDisplayFormat = "0.##";
        private const string DimensionDisplayFormat = "0";
        private const string PdfHeader = "%PDF-1.4\n";

        /// <summary>
        /// Exports a collection of amortization rows to a byte array representing a PDF document.
        /// </summary>
        /// <param name="rows">The amortization rows to export.</param>
        /// <returns>A byte array containing the generated PDF file.</returns>
        public byte[] ExportAmortization(IEnumerable<AmortizationRow> rows)
        {
            var amortizationRows = rows?.ToList() ?? [];
            var pageContents = BuildAmortizationPages(amortizationRows);

            return BuildPdfDocument(pageContents);
        }

        /// <summary>
        /// Exports a collection of transactions to a byte array representing a PDF document.
        /// </summary>
        /// <param name="rows">The transaction rows to export.</param>
        /// <returns>A byte array containing the generated PDF file.</returns>
        /// <exception cref="NotImplementedException">Thrown because transaction export is not yet implemented.</exception>
        public byte[] ExportTransactions(IEnumerable<object> rows)
        {
            throw new NotImplementedException("Transaction PDF export is not implemented yet.");
        }

        private static List<string> BuildAmortizationPages(IReadOnlyList<AmortizationRow> rows)
        {
            List<string> pages = [];
            var currentPage = new StringBuilder();

            WriteTitle(currentPage, "Amortisation schedule", PageHeight - TopMargin);
            WriteHeader(currentPage, PageHeight - TopMargin - HeaderVerticalOffset);

            var yPosition = PageHeight - TopMargin - FirstRowVerticalOffset;

            foreach (var row in rows)
            {
                if (yPosition < BottomContentThreshold)
                {
                    pages.Add(currentPage.ToString());
                    currentPage = new StringBuilder();
                    WriteTitle(currentPage, "Amortisation schedule", PageHeight - TopMargin);
                    WriteHeader(currentPage, PageHeight - TopMargin - HeaderVerticalOffset);
                    yPosition = PageHeight - TopMargin - FirstRowVerticalOffset;
                }

                WriteRow(currentPage, row, yPosition);
                yPosition -= RowHeight;
            }

            pages.Add(currentPage.ToString());
            return pages;
        }

        private static void WriteTitle(StringBuilder builder, string title, float verticalPosition)
        {
            AppendText(builder, TitleFontSize, LeftMargin, verticalPosition, title);
        }

        private static void WriteHeader(StringBuilder builder, float verticalPosition)
        {
            AppendText(builder, HeaderFontSize, LeftMargin, verticalPosition, "#");
            AppendText(builder, HeaderFontSize, LeftMargin + DueDateColumnOffset, verticalPosition, "Due date");
            AppendText(builder, HeaderFontSize, LeftMargin + PrincipalColumnOffset, verticalPosition, "Principal");
            AppendText(builder, HeaderFontSize, LeftMargin + InterestColumnOffset, verticalPosition, "Interest");
            AppendText(builder, HeaderFontSize, LeftMargin + BalanceColumnOffset, verticalPosition, "Balance");
        }

        private static void WriteRow(StringBuilder builder, AmortizationRow row, float verticalPosition)
        {
            AppendText(builder, RowFontSize, LeftMargin, verticalPosition, row.InstallmentNumber.ToString(CultureInfo.InvariantCulture));
            AppendText(builder, RowFontSize, LeftMargin + DueDateColumnOffset, verticalPosition, row.DueDate.ToString("MMM ''yy", CultureInfo.InvariantCulture));
            AppendText(builder, RowFontSize, LeftMargin + PrincipalColumnOffset, verticalPosition, FormatCurrency(row.PrincipalPortion));
            AppendText(builder, RowFontSize, LeftMargin + InterestColumnOffset, verticalPosition, FormatCurrency(row.InterestPortion));
            AppendText(builder, RowFontSize, LeftMargin + BalanceColumnOffset, verticalPosition, FormatCurrency(row.RemainingBalance));
        }

        private static string FormatCurrency(decimal value)
        {
            return value.ToString(CurrencyDisplayFormat, CultureInfo.CurrentCulture);
        }

        private static void AppendText(StringBuilder builder, int fontSize, float horizontalPosition, float verticalPosition, string text)
        {
            builder.AppendLine("BT");
            builder.AppendLine($"/F1 {fontSize} Tf");
            builder.AppendLine(
                $"{horizontalPosition.ToString(CoordinateDisplayFormat, CultureInfo.InvariantCulture)} {verticalPosition.ToString(CoordinateDisplayFormat, CultureInfo.InvariantCulture)} Td");
            builder.AppendLine($"({EscapePdfText(text)}) Tj");
            builder.AppendLine("ET");
        }

        private static string EscapePdfText(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private static byte[] BuildPdfDocument(IReadOnlyList<string> pageContents)
        {
            List<byte[]> objects = [];
            var pageCount = pageContents.Count;
            var fontObjectId = FirstPageObjectId + (pageCount * ObjectsPerPage);
            var pageReferences = new StringBuilder();

            for (var index = ZeroBufferIndex; index < pageCount; index++)
            {
                var pageObjectId = FirstPageObjectId + (index * ObjectsPerPage);
                pageReferences.Append($"{pageObjectId} {ObjectGenerationNumber} R ");
            }

            objects.Add(ToPdfBytes($"<< /Type /Catalog /Pages {PagesObjectId} {ObjectGenerationNumber} R >>"));
            objects.Add(ToPdfBytes($"<< /Type /Pages /Kids [{pageReferences.ToString().TrimEnd()}] /Count {pageCount} >>"));

            for (var index = ZeroBufferIndex; index < pageCount; index++)
            {
                var contentObjectId = FirstContentObjectId + (index * ObjectsPerPage);
                var pageObject =
                    $"<< /Type /Page /Parent {PagesObjectId} {ObjectGenerationNumber} R /MediaBox [{CoordinateOrigin} {CoordinateOrigin} {PageWidth.ToString(DimensionDisplayFormat, CultureInfo.InvariantCulture)} {PageHeight.ToString(DimensionDisplayFormat, CultureInfo.InvariantCulture)}] " +
                    $"/Resources << /Font << /F1 {fontObjectId} {ObjectGenerationNumber} R >> >> /Contents {contentObjectId} {ObjectGenerationNumber} R >>";

                var contentBytes = Encoding.ASCII.GetBytes(pageContents[index]);
                var streamHeader = $"<< /Length {contentBytes.Length} >>\nstream\n";
                var streamFooter = "endstream";

                objects.Add(ToPdfBytes(pageObject));
                objects.Add(
                    CombineBytes(
                        Encoding.ASCII.GetBytes(streamHeader),
                        contentBytes,
                        Encoding.ASCII.GetBytes("\n" + streamFooter)));
            }

            objects.Add(ToPdfBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"));

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.Write(PdfHeader);
            writer.Flush();

            List<long> offsets = [ZeroStreamOffset];

            for (var index = ZeroBufferIndex; index < objects.Count; index++)
            {
                offsets.Add(stream.Position);
                writer.Write($"{index + ObjectNumberStart} {ObjectGenerationNumber} obj\n");
                writer.Flush();
                stream.Write(objects[index], ZeroBufferIndex, objects[index].Length);
                writer.Write("\nendobj\n");
                writer.Flush();
            }

            var xrefPosition = stream.Position;
            writer.Write($"xref\n{ObjectGenerationNumber} {objects.Count + ObjectNumberStart}\n");
            writer.Write($"{ZeroStreamOffset:D10} {CrossReferenceFreeEntryGeneration:D5} f \n");

            for (var index = ObjectNumberStart; index < offsets.Count; index++)
            {
                writer.Write($"{offsets[index].ToString($"D{FixedOffsetDigits}", CultureInfo.InvariantCulture)} {CrossReferenceUsedEntryGeneration:D5} n \n");
            }

            writer.Write("trailer\n");
            writer.Write($"<< /Size {objects.Count + ObjectNumberStart} /Root {CatalogObjectId} {ObjectGenerationNumber} R >>\n");
            writer.Write("startxref\n");
            writer.Write($"{xrefPosition}\n");
            writer.Write("%%EOF");
            writer.Flush();

            return stream.ToArray();
        }

        private static byte[] ToPdfBytes(string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        private static byte[] CombineBytes(params byte[][] parts)
        {
            var totalLength = parts.Sum(part => part.Length);
            var result = new byte[totalLength];
            var offset = ZeroBufferIndex;

            foreach (var part in parts)
            {
                Buffer.BlockCopy(part, ZeroBufferIndex, result, offset, part.Length);
                offset += part.Length;
            }

            return result;
        }
    }
}