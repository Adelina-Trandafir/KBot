# SLICE-VC06-08 - NIR from an invoice K-BOT sent: returning it and closing the sending (EV05, 21.10.2026)

Plan: [`docs/EV_PLAN.md`](../EV_PLAN.md) (slice EV05). Reading and prefilling: VC06-07.

## What changed and why
- **Return** (`efactura_queue.send_back`, `POST /api/vercon/doc/nir/efactura/<id>/return`, right `receptie`): the sending goes to `RETURNATA` with the reason (3-500 characters, required), who and when; audited (`efactura.return`). K-BOT shows «Returnată: motivul» in the column «Vercon» and the invoice can be sent again.
- **Closing** (`efactura_queue.close`, called by `receipt_distribution.save` when the body has `efactura`): in the SAME transaction as the NIR(s) — after `write`, before the answer is remembered — the sending (it must still wait, row locked `FOR UPDATE`) becomes `PRELUATA` (`UnPreluat`, `DataPreluat`) and one row per NIR created goes to `EF_PrimiteVerconNir` (`INSERT IGNORE`; the editor creates several NIRs in one save when the accounts belong to different gestiuni). A sending that no longer waits (taken back, taken over by another session, returned) stops the whole save with a 409: no NIR is written. `receipt_distribution.write` now adds `exercitiu` to each created NIR entry for this.
- **Cancelled NIR** (decision D5): not written anywhere. `PRELUATA` with no valid NIR counts as waiting again (both in the list here and in K-BOT's view of the state), so the invoice reappears by itself.
- The editor replays an unconfirmed save with the same request key; a replay answers the stored result and does not close again.

## Files touched
`PYTHON/routes/vercon/efactura_queue.py`, `receipt_distribution.py` (import, `exercitiu` in the result, the call to `close`), `implementare_vercon_v2.md`.

## Test results
`py_compile`, import: ok. No test written or run, no SQL.

## Unverified / deferred
- Nothing run in a transaction: the `FOR UPDATE` on the sending, the primary key of `EF_PrimiteVerconNir`, the audit row (`VC_Audit.TableName` for a table that is not a `VC_` one) are as written, not exercised.
- **Open case (plan, section 4):** one of SEVERAL NIRs of a sending is cancelled while the others stay valid: the sending stays `PRELUATA` (valid NIRs exist) and does not come back. The plan proposed bringing it back and taking over only the lines left without a NIR; not built.
- A NIR edited later (PUT) does not change the sending. A NIR cancelled in VERCON writes nothing in K-BOT's tables.
- Taking over with the editor's «Stoc inițial» type is not blocked (the sending would close with a NIR of that type); the list is only offered from «Adăugare NIR».
