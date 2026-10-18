# Evaluation results

- Model: `claude-haiku-5-5`
- Date: 2026-10-18
- Contracts scored: 25 (errors: 0)
- Latency per contract: avg 4.8s, max 6.3s

## Overall

- Field accuracy: **90.5%** (181/200)
- Wrong values: 1
- Missed values (null when the key has a value): 18
- Hallucination rate (value returned where the key says null): **0.0%** (0/30)

## Per field

| Field | Accuracy | Wrong | Hallucinated | Missed |
|---|---|---|---|---|
| vendor_name | 100.0% | 0 | 0 | 0 |
| effective_date | 100.0% | 0 | 0 | 0 |
| current_term_end_date | 60.0% | 1 | 0 | 9 |
| auto_renews | 96.0% | 0 | 0 | 1 |
| renewal_term_months | 100.0% | 0 | 0 | 0 |
| notice_period_days | 100.0% | 0 | 0 | 0 |
| price_escalation_percent | 100.0% | 0 | 0 | 0 |
| termination_for_convenience | 68.0% | 0 | 0 | 8 |

## By difficulty

| Difficulty | Accuracy |
|---|---|
| easy | 100.0% (40/40) |
| hard | 86.3% (69/80) |
| medium | 90.0% (72/80) |

## Mistakes

| Contract | Field | Expected | Got | Type |
|---|---|---|---|---|
| c011 | current_term_end_date | 2027-04-15 | null | MISSED |
| c011 | termination_for_convenience | false | null | MISSED |
| c013 | current_term_end_date | 2026-02-10 | null | MISSED |
| c013 | termination_for_convenience | false | null | MISSED |
| c014 | current_term_end_date | 2027-06-01 | null | MISSED |
| c015 | termination_for_convenience | false | null | MISSED |
| c016 | current_term_end_date | 2027-03-12 | null | MISSED |
| c016 | termination_for_convenience | false | null | MISSED |
| c017 | termination_for_convenience | false | null | MISSED |
| c019 | current_term_end_date | 2026-05-01 | null | MISSED |
| c020 | current_term_end_date | 2026-10-10 | null | MISSED |
| c021 | current_term_end_date | 2027-02-14 | null | MISSED |
| c021 | auto_renews | false | null | MISSED |
| c021 | termination_for_convenience | false | null | MISSED |
| c022 | current_term_end_date | 2026-01-01 | 2025-12-31 | WRONG |
| c023 | current_term_end_date | 2027-03-30 | null | MISSED |
| c023 | termination_for_convenience | false | null | MISSED |
| c024 | current_term_end_date | 2027-04-22 | null | MISSED |
| c024 | termination_for_convenience | false | null | MISSED |
