# Held-out first run (c026-c030), before answer-key audit

- Model: `claude-haiku-5-5`
- Field accuracy: 82.5% (33/40)
- Apparent hallucination rate: 44.4% (4/9), all four caused by inconsistent answer-key convention for computed end dates
- Missed: 3, all `termination_for_convenience` where the key said false but the contracts were silent

| Contract | Field | Expected | Got | Type |
|---|---|---|---|---|
| c026 | current_term_end_date | null | 2027-03-01 | key error |
| c027 | termination_for_convenience | false | null | key error |
| c028 | current_term_end_date | null | 2026-09-01 | key error |
| c028 | termination_for_convenience | false | null | key error |
| c029 | current_term_end_date | null | 2028-03-15 | key error |
| c030 | current_term_end_date | null | 2027-10-15 | key error |
| c030 | termination_for_convenience | false | null | key error |

Audit result: all 7 were errors in the answer keys, not in the model.
