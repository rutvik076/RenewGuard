# Evaluation results

- Model: `claude-haiku-5-5`
- Date: 2026-10-18
- Contracts scored: 25 (errors: 0)
- Latency per contract: avg 4.4s, max 5.6s

## Overall

- Field accuracy: **100.0%** (200/200)
- Wrong values: 0
- Missed values (null when the key has a value): 0
- Hallucination rate (value returned where the key says null): **0.0%** (0/38)

## Per field

| Field | Accuracy | Wrong | Hallucinated | Missed |
|---|---|---|---|---|
| vendor_name | 100.0% | 0 | 0 | 0 |
| effective_date | 100.0% | 0 | 0 | 0 |
| current_term_end_date | 100.0% | 0 | 0 | 0 |
| auto_renews | 100.0% | 0 | 0 | 0 |
| renewal_term_months | 100.0% | 0 | 0 | 0 |
| notice_period_days | 100.0% | 0 | 0 | 0 |
| price_escalation_percent | 100.0% | 0 | 0 | 0 |
| termination_for_convenience | 100.0% | 0 | 0 | 0 |

## By difficulty

| Difficulty | Accuracy |
|---|---|
| easy | 100.0% (40/40) |
| hard | 100.0% (80/80) |
| medium | 100.0% (80/80) |
