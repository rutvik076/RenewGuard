namespace RenewGuard.Application.Abstractions;

public interface IPdfTextExtractor
{
    string ExtractText(Stream pdf);
}
