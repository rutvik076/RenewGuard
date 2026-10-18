namespace RenewGuard.Domain.Models;

public class ContractRecord
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public DateTime UploadedAtUtc { get; set; }
    public string? VendorName { get; set; }
    public DateOnly? TermEndDate { get; set; }
    public int? NoticePeriodDays { get; set; }
    public DateOnly? LastCancelDate { get; set; }
    public bool? AutoRenews { get; set; }
    public int? RenewalTermMonths { get; set; }
    public string ExtractionJson { get; set; } = "";
}

