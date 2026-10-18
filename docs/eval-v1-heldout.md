# Evaluation results

- Model: `claude-haiku-5-5`
- Date: 2026-10-18
- Contracts scored: 5 (errors: 0)
- Latency per contract: avg 4.7s, max 6.0s

## Overall

- Field accuracy: **100.0%** (40/40)
- Wrong values: 0
- Missed values (null when the key has a value): 0
- Hallucination rate (value returned where the key says null): **0.0%** (0/8)

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
| hard | 100.0% (40/40) |
