using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RenewGuard.Application.Abstractions;
using RenewGuard.Domain.Models;
using RenewGuard.Infrastructure;
using RenewGuard.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["ANTHROPIC_API_KEY"]
    ?? throw new InvalidOperationException("Set the ANTHROPIC_API_KEY environment variable.");
var model = builder.Configuration["RENEWGUARD_MODEL"] ?? "claude-haiku-5-5";
var connection = builder.Configuration.GetConnectionString("Default") ?? "Data Source=renewguard.db";

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddRenewGuardInfrastructure(connection, apiKey, model);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<RenewGuardDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");

app.MapPost("/contracts", async (
    IFormFile file,
    IPdfTextExtractor pdf,
    IContractExtractor extractor,
    RenewGuardDbContext db,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    if (file is null || file.Length == 0)
        return Results.BadRequest(new { error = "Upload a PDF in the 'file' form field." });

    if (file.Length > 10 * 1024 * 1024)
        return Results.BadRequest(new { error = "File is larger than 10 MB." });

    if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { error = "Only .pdf files are supported." });

    string text;
    try
    {
        await using var stream = file.OpenReadStream();
        text = pdf.ExtractText(stream);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not read uploaded PDF {FileName}", file.FileName);
        return Results.BadRequest(new { error = "Could not read this file as a PDF." });
    }

    if (text.Length < 200)
        return Results.UnprocessableEntity(new
        {
            error = "No readable text found. Scanned (image-only) PDFs are not supported yet."
        });

    ContractExtraction result;
    try
    {
        result = await extractor.ExtractAsync(text, ct);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Extraction failed for {FileName}", file.FileName);
        return Results.Problem("Extraction failed. Please try again.", statusCode: 502);
    }

    var record = new ContractRecord
    {
        Id = Guid.NewGuid(),
        FileName = Path.GetFileName(file.FileName),
        UploadedAtUtc = DateTime.UtcNow,
        VendorName = result.VendorName.Value,
        TermEndDate = result.ResolvedTermEndDate,
        NoticePeriodDays = result.NoticePeriodDays.Value,
        LastCancelDate = result.LastCancelDate,
        AutoRenews = result.AutoRenews.Value,
        ExtractionJson = JsonSerializer.Serialize(result)
    };

    db.Contracts.Add(record);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/contracts/{record.Id}", new { record.Id, record.FileName, Extraction = result });
})
.DisableAntiforgery();

app.MapGet("/contracts/{id:guid}", async (Guid id, RenewGuardDbContext db, CancellationToken ct) =>
{
    var record = await db.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    return record is null
        ? Results.NotFound()
        : Results.Content(record.ExtractionJson, "application/json");
});

app.MapGet("/contracts/upcoming", async (int? days, RenewGuardDbContext db, CancellationToken ct) =>
{
    var window = Math.Clamp(days ?? 90, 1, 3650);
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var limit = today.AddDays(window);

    var rows = await db.Contracts
        .AsNoTracking()
        .Where(c => c.LastCancelDate != null && c.LastCancelDate >= today && c.LastCancelDate <= limit)
        .OrderBy(c => c.LastCancelDate)
        .Select(c => new { c.Id, c.FileName, c.VendorName, c.LastCancelDate, c.NoticePeriodDays, c.AutoRenews })
        .ToListAsync(ct);

    return rows.Select(c => new
    {
        c.Id,
        c.FileName,
        c.VendorName,
        c.LastCancelDate,
        DaysLeft = c.LastCancelDate!.Value.DayNumber - today.DayNumber,
        c.NoticePeriodDays,
        c.AutoRenews
    });
});

app.Run();

public partial class Program { }
