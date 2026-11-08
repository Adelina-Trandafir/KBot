# SLICE-02EF-13 - Payments from the bank statement are attached to ordonantari made from a reception (07.11.2026)

## Request
Operator: the link between an ordonantare and its reception (`FX_ORD_TBL_REC.IDRP`) must work when payments come from the bank statements; payment orders (OP) made from the program will follow. Matching rule chosen by the operator: automatic «angajament + indicator + CUI + suma», plus a manual way when the automatic one misses.

## Final state
- **Automatic rule** (`PYTHON/routes/forexe/ord_receptie_plati.py`, `leaga_automat`): a free payment (`FX_Plati` + its `FX_Extrase` row, `Suma > 0`) goes to the oldest pending row (`FX_ORD_TBL_REC` with `IDRP`, `IdPlataFX` empty) with the same angajament, indicator and normalized tax code (`platitor_cui` = the line's beneficiary `CodFiscal`) whose remaining value is not below the payment. Equal = the row is filled; smaller = the row is split (it keeps the rest, a new row with the same `IDRP` takes the payment), so a line's total never changes; fitting nothing = left alone. Runs at the end of `step5_plati_incasari` (FOREXE ingest), at the end of `importa_extrase` (statement import; the payment and its statement may come in either order) and on request (`POST /api/forexe/ord/leaga-plati-automat`). The two ingest calls go through `leaga_automat_protejat` (lazy import; a failure is a warning, never stops the import).
- **By hand**: `GET /api/forexe/ord/plata/<id>/linii-candidate` and `POST /api/forexe/ord/leaga-plata` (same checks, refusals 404/409 in Romanian). Client: `IOrdReceptieApi.GetLiniiCandidateAsync/LeagaPlataAsync`, `OrdActiune.LeagaPlata`, `KbotForm.LeagaPlataDeOrdonantareAsync` (menu at the mouse with the candidate rows, then the views reload), `PlatiView` implements `IViewRibbon` (button `plati_leaga_ord`, lit on a selected payment no ordonantare covers), button in `AvacontMain`.
- A payment that is attached is `ordonantata` everywhere (`EXISTS FX_ORD_TBL_REC.IdPlataFX`), so `post_ord_genereaza` no longer offers it.

## Files touched
`PYTHON/routes/forexe/{ord_receptie_plati.py (new),prelucrare_pasi.py,extrase.py,__init__.py}`, `src/KBot.Api/{IOrdReceptieApi,ApiClient.OrdReceptie,OrdEditContract}.vb`, `src/KBot.App/KBotForm/Views/{PlatiView.vb,Ord/OrdComanda.vb}`, `src/KBot.App/KBotForm/Documents/KbotForm.Ord.vb`, `src/KBot.App/Avacont/AvacontMain{,.Designer}.vb`. Help 0000-169. Server files registered for deployment (SLICE-02EF-13).

## Test results
`py_compile` OK on every Python file touched, `import routes.forexe` OK; `dotnet build src/KBot.App`: 0 warnings, 0 errors. Tried by the operator (07.11.2026). No automated test.

## Deferred / traps
- The rule compares tax codes only (no IBAN); two suppliers with the same tax code are not told apart.
- The hooks run inside the transaction of the two imports; a SQL failure there is a warning, the import goes on.
- Payment orders (OP) made from the program: later, not started.
