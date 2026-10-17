# RenewGuard

## Specification

### Problem
Companies lose money when vendor and SaaS contracts auto-renew silently. The cancellation deadline, often 30–90 days before the end date, is buried in legal text, and nobody notices until the invoice arrives.

### Users
Finance, procurement, and operations teams at small and mid-size companies that manage 20–200 vendor contracts without a legal department.

### What It Does
RenewGuard ingests contract PDFs, extracts 8 key terms with supporting evidence, computes the last day to cancel, and alerts the team in time.

### Success Metrics
- Field extraction accuracy of 85% or higher on the synthetic test set, reported per field.
- 100% of extracted values carry a source quote.
- Cost per contract and latency are measured and reported.
- The system answers "not found" instead of guessing.

### Out of Scope for v1
- E-signature
- Contract editing
- Multi-user permissions
- Non-English contracts
- Scanned image PDFs (OCR)

## The 8-Field Extraction Schema

| # | Field | Type | Notes |
|---|---|---|---|
| 1 | `vendor_name` | string | The counterparty |
| 2 | `effective_date` | date | When the contract starts |
| 3 | `current_term_end_date` | date | When the current term expires |
| 4 | `auto_renews` | bool | Does it renew without action? |
| 5 | `renewal_term_months` | int | Length of each renewal |
| 6 | `notice_period_days` | int | Days of notice required to cancel |
| 7 | `price_escalation` | string + percent | Annual increase clause, if any |
| 8 | `termination_for_convenience` | bool | Can you exit early without cause? |

Every field returns the same envelope:

```csharp
public record ExtractedField<T>(
    T? Value,            // null if the contract doesn't say
    string? Evidence,    // exact quote from the contract
    int? Page,
    double Confidence    // 0.0–1.0
);
```

Two rules matter most:

1. **If the contract doesn't say, return `null`.** Never guess. This is what makes the system trustworthy.
2. **`last_cancel_date` is computed in code, not by the LLM:** `current_term_end_date − notice_period_days`. Date math belongs in deterministic code, and saying so in an interview shows good engineering judgment.

## How We'll Score It (Step 6 Preview)

- Dates, booleans, and integers: exact match against ground truth.
- `price_escalation`: percentage matches within tolerance.
- `null` handling: a correct `null` counts as correct, and a hallucinated value counts as wrong. Track hallucination rate as its own metric.

## Repository Structure

```text
RenewGuard/
├── src/
│   ├── RenewGuard.Api/             # ASP.NET Core Web API
│   ├── RenewGuard.Application/     # Use cases, interfaces
│   ├── RenewGuard.Domain/          # Entities, last_cancel_date logic
│   ├── RenewGuard.Infrastructure/  # AI client, storage, queue
│   ├── RenewGuard.Worker/          # Extraction worker
│   └── RenewGuard.McpServer/       # MCP tools (later)
├── tests/
│   ├── RenewGuard.UnitTests/
│   └── RenewGuard.Eval/            # Accuracy runner + ground truth
├── data/
│   └── contracts/                  # Synthetic PDFs + expected JSON
├── docs/                           # Spec, architecture diagram
├── azure-pipelines.yml
├── Dockerfile
└── README.md
```
