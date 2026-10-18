using RenewGuard.Infrastructure.Pdf;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace RenewGuard.UnitTests;

public class PdfPigTextExtractorTests
{
    private static byte[] BuildPdf(params string[] lines)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(PageSize.A4);

        double y = 780;
        foreach (var line in lines)
        {
            page.AddText(line, 11, new PdfPoint(40, y), font);
            y -= 16;
        }

        return builder.Build();
    }

    [Fact]
    public void ExtractsTextFromPdf()
    {
        var bytes = BuildPdf(
            "SOFTWARE SUBSCRIPTION AGREEMENT",
            "This Agreement renews automatically for twelve (12) months.");

        using var stream = new MemoryStream(bytes);
        var text = new PdfPigTextExtractor().ExtractText(stream);

        Assert.Contains("SOFTWARE SUBSCRIPTION AGREEMENT", text);
        Assert.Contains("renews automatically", text);
    }

    [Fact]
    public void Throws_OnInvalidPdf()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        Assert.ThrowsAny<Exception>(() => new PdfPigTextExtractor().ExtractText(stream));
    }
}
