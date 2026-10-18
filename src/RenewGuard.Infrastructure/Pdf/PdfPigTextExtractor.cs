using System.Text;
using RenewGuard.Application.Abstractions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace RenewGuard.Infrastructure.Pdf;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    public string ExtractText(Stream pdf)
    {
        using var document = PdfDocument.Open(pdf);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(ContentOrderTextExtractor.GetText(page));
            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }
}
