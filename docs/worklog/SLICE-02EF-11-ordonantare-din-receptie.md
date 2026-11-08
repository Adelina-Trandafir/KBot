# SLICE-02EF-11 - «Genereaza ordonantare» from the reception (07.11.2026)

## Request
Operator: after a reception is saved from «Receptii <> Efactura», a ribbon-right button «Genereaza Ordonantare» appears, based on the RECEPTION (not on payments), with the partner data from Parteneri (account, tax code, whatever the ordonantare asks). Later: it must work with the payments that come from bank statements (done in 02EF-13).

## Final state
- **Server** `PYTHON/routes/forexe/ord_edit.py`: `POST /api/forexe/ord/genereaza-din-receptie` `{cod, idrh, data?}`, nothing written. Reads the snapshot's lines (`FX_Receptii` by `IDRH`, `SUM(DIF)` per indicator), the invoices linked to it (`EF_PrimiteReceptii` + `EF_Primite`), finds the supplier in `Parteneri` (same unit, normalized tax code, the row with an IBAN first), splits the lines per supplier by what each invoice takes and feeds `construieste_graf`. The reception's value above what the invoices take stays on the first line of that indicator, with a warning. The attachments are the pictures of THAT snapshot (`capturi_receptiilor(..., idrh)`); none = a warning. Each line keeps the reception line it covers: a `FX_ORD_TBL_REC` row with `IDRP` = lowest `FX_Receptii.IDR` of the indicator in the snapshot, `IdPlataFX` empty (the payment is attached later, 02EF-13). Refusals in Romanian: no DDF (409), reception not in K-BOT yet (404), no linked invoice (404).
- **Client**: `IOrdReceptieApi` + `ApiClient.OrdReceptie.vb` (own interface: nine test fakes implement `IApiClient`); `KbotForm.GenereazaOrdonantareDinReceptie` opens the same editor as «Adauga» with the same «reception chain» gate; the former reception window remembers the reception it saved (`OrdCod`, `OrdIdrh`, `OrdDisponibil`) and forgets it when the operator ticks other invoices; ribbon button `ef_receptie_ord` in `AvacontMain`, shown only on that page. `IDRP` travels through `OrdDraftRecDto.idrp`, `OrdDraftRec.Idrp` and the save / read-back.

## Files touched
`PYTHON/routes/forexe/ord_edit.py`, `src/KBot.Api/{IOrdReceptieApi,ApiClient.OrdReceptie,OrdEditContract,ApiClient}.vb`, `src/KBot.Domain/OrdDraft.vb`, `src/KBot.App/KBotForm/Documents/KbotForm.Ord.vb`, the former reception window, `src/KBot.App/Avacont/AvacontMain{,.Designer}.vb`. Help 0000-168. Server file registered for deployment (SLICE-02EF-11).

## Test results
`py_compile` OK; `dotnet build src/KBot.App`: 0 warnings, 0 errors. Tried by the operator (07.11.2026), who confirmed it and asked for the registration. No automated test written or run.

## Deferred
- The tax code is the only key between invoice and Parteneri (two partners with the same code are not told apart).
- Line explanation = «Factura nr din data»; nothing else was specified.
- If the page's reload after a save raises the tree's tick event, the button would disappear at once (not seen to happen).
