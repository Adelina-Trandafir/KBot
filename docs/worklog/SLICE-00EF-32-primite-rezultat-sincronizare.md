# SLICE-00EF-32 - Received invoices: the end of a sync tells what was brought (15.10.2026)

## What changed and why
At the end of `PrimiteForm.SyncFromAnafAsync`:
- invoices were brought -> a window `SincronizareNoiForm` (own designer; caption bar, theme, one «OK») with a table of the NEW invoices
  (date, number, supplier, tax code, total, with a total in the footer), newest first; the same facts go to `mesaje_operator.log`
  through `OperatorLog` (the window is not a `KBotMessage`, so it logs by hand, as the house rule says);
- nothing brought and no unreadable message -> the info message «Nu există facturi noi pe serverul ANAF.» (`KBotMessage`, Information);
- brought but not visible in the list (another year than the one chosen in K-BOT, or the list could not be read first) -> an info
  message with the count.
«New» = not in the list before the sync (the list is read first when it never was: `PrimiteView.Items` / `ListLoaded`, new members);
no server change. The existing message system (`KBotMessageBox`) has no table, so the table is a small dialog of its own rather
than a change to that box.

## Files touched
`src/KBot.EFactura/Forms/SincronizareNoiForm.vb` and `.Designer.vb` (new), `PrimiteForm.vb`,
`src/KBot.EFactura/Views/Primite/PrimiteView.vb`, `contabil/efactura/index.md` (0000-78).

## Test results
`dotnet build src/KBot.App`: 0 warnings, 0 errors. Nothing run, nothing seen; no real sync.

## Unverified / deferred
- The look of the window and the footer total; no real sync against ANAF.
- The comparison is by the list of the chosen year: invoices of another year are only counted in the message.
- If someone else's sync added invoices between the baseline and the end, they appear as new too.

## Follow-up (15.10.2026): «no messages» is INFO, never a warning
ANAF answers an empty period as an error text («Nu exista mesaje in ultimele N zile pentru cif=...»), which the server turned into
`EfEroare` -> the client showed it as a warning and wrote it in the error log. Now:
- **Server** (`routes/efactura/anaf_api.py`, `lista_mesaje`): an `eroare` that starts with «nu exista ... mesaje» returns an empty list
  (no failure). `py_compile` ok; **not deployed**, not run against ANAF.
- **Client** (`PrimiteView.SyncFromAnafAsync`): an `ApiException` whose text says the same is shown as the info message
  «Nu există facturi noi pe serverul ANAF.» (Information, no entry in the error log) -- works before the server is redeployed.
- The footer menu's second caption is now only «Între 1 și 60 de zile».
