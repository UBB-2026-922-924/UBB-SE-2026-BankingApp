namespace BankingApp.Desktop.ViewModels;

using System.Globalization;
using System.Text;
using Domain.Aggregates.LoanAggregate.Entities;

internal sealed class PdfExporter
{
    private const int MaxRowsPerPage = 45;

    public byte[] ExportAmortization(IReadOnlyCollection<AmortizationRow> rows)
    {
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 10 Tf");
        content.AppendLine("40 780 Td");
        content.AppendLine("(Amortization schedule) Tj");
        content.AppendLine("0 -18 Td");
        content.AppendLine("(No.   Due date      Principal     Interest      Remaining) Tj");

        foreach (AmortizationRow row in rows.Take(MaxRowsPerPage))
        {
            string line = string.Create(
                CultureInfo.InvariantCulture,
                $"{row.InstallmentNumber,-5} {row.DueDate:yyyy-MM-dd}   {row.PrincipalPortion,10:0.00}   {row.InterestPortion,10:0.00}   {row.RemainingBalance,10:0.00}");
            content.AppendLine("0 -14 Td");
            content.Append('(').Append(EscapePdfText(line)).AppendLine(") Tj");
        }

        content.AppendLine("ET");
        return BuildPdf(content.ToString());
    }

    private static byte[] BuildPdf(string pageContent)
    {
        byte[] contentBytes = Encoding.ASCII.GetBytes(pageContent);
        var builder = new StringBuilder();
        var offsets = new List<int> { 0 };

        builder.AppendLine("%PDF-1.4");
        AppendObject(builder, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
        AppendObject(builder, offsets, 2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        AppendObject(
            builder,
            offsets,
            3,
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>");
        AppendObject(builder, offsets, 4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
        builder.AppendLine("5 0 obj");
        builder.AppendLine(CultureInfo.InvariantCulture, $"<< /Length {contentBytes.Length} >>");
        builder.AppendLine("stream");
        builder.Append(pageContent);
        builder.AppendLine("endstream");
        builder.AppendLine("endobj");

        int xrefOffset = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.AppendLine("xref");
        builder.AppendLine("0 6");
        builder.AppendLine("0000000000 65535 f ");
        foreach (int offset in offsets.Skip(1))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"{offset:0000000000} 00000 n ");
        }

        builder.AppendLine("trailer");
        builder.AppendLine("<< /Size 6 /Root 1 0 R >>");
        builder.AppendLine("startxref");
        builder.AppendLine(CultureInfo.InvariantCulture, $"{xrefOffset}");
        builder.AppendLine("%%EOF");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static void AppendObject(StringBuilder builder, List<int> offsets, int objectNumber, string body)
    {
        offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
        builder.AppendLine(CultureInfo.InvariantCulture, $"{objectNumber} 0 obj");
        builder.AppendLine(body);
        builder.AppendLine("endobj");
    }

    private static string EscapePdfText(string text)
    {
        return text.Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("(", @"\(", StringComparison.Ordinal)
            .Replace(")", @"\)", StringComparison.Ordinal);
    }
}
