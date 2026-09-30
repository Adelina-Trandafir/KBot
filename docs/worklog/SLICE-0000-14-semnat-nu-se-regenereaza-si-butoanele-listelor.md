# SLICE-0000-14 — A signed document is never regenerated + the buttons of trees and grids (30.09.2026)

Operator answers to 0000-13 (30.09.2026):
1. «A signed document CAN NEVER BE REGENERATED. That was there before the 0078 slice; now we know we
   can insert data into an already signed document, so it is not needed anymore.»
2. «încărcată» / «preluată» must be explained; «also we must document the buttons in the
   headers/footers of each tree/dgv».

## What changed and why

### Code (visual change → help in the same task, per the new rule)
- `Views/SigningMessages.vb`: `ConfirmRegenerateSigned` (the Yes/No «generate an unsigned one over
  the signed version?») replaced by `ShowSignedNeverRegenerated` (information only).
- `Views/OrdView.vb` / `Views/DdfView.vb` `OnGenerateRequested`: a document with at least one
  signature (`EsteSemnata` = roles in `Semnatura` OR a signed PDF on the server) is refused with
  that message. Before, only `ArePdfSemnat` was checked, and the operator could say «Da».

### Help
- **Regeneration**: `contabil.vederi.ord` (Document section, the «Semnătura 1» trap, validation
  errors, «Anulare Validare»), `tur-ord`.
- **DDF after sending — found stale while doing it** (text from 0000-03, before 0078-06): the help
  said the final PDF is a new document signed «de la capăt» on A and B. The code (0078-06,
  `DdfView.SectiuneaBInseratAsync`, `KbotForm.DdfSendMenu.GenereazaPdfFinalAsync`) puts Section B and
  the captures INTO the document signed on A; only B is signed. Rewritten: `contabil.ddf.semnare`
  («După trimitere — semnătura B»), `contabil.ddf` (the road, the «PDF final» state row),
  `contabil.ddf.trimitere`, `contabil.ddf.rezervare`, `tur-ddf`.
- **«încărcat» / «preluat»** (Access convention, the same flags on FX_Angajamente / FX_DDF /
  FX_ORD / FX_Receptii_R): încărcat = sent into FOREXE from K-BOT (or the old program), preluat = came
  in from a FOREXE download. `contabil.forexe.descarcare` («Stare» column), `contabil.vederi.sumar`.
- **Buttons of trees and grids**: new topic `contabil.liste` «Arborii și tabelele — butoanele din cap
  și din subsol» (tree header: folder = sign only, lupa = search band, right icon per view; tree
  footer: caption, left/right icons, collapse button + row flyout; grid: the funnel menu Sortare /
  Filtrare / Grupare with its buttons, filled funnel = filtered; TOTAL(URI) over the visible rows;
  «+» adds a row; check-all icon in a check column; a table pointing to each view's own buttons).
  Per view: `contabil.fereastra`, `contabil.vederi.istoric` (new section), `contabil.vederi.receptii`,
  `contabil.vederi.plati`, `contabil.vederi.sumar`, `contabil.vederi.ord`, `contabil.ddf`,
  `contabil.notecab`, `contabil.nomenclatoare.parteneri` (new section: the «Coduri angajament» grid,
  its «+» and ✕, the «Ascunde partenerii fără activitate» box, the window buttons).

### Removed from the help (was false)
- «Iconița din subsolul arborelui, dreapta, descarcă plățile și încasările din CAB» (`contabil.vederi
  .plati` and the `tur-plati` step «Descărcarea din CAB»): the icon has a tooltip but **no handler**.

## Files touched

- `src/KBot.App/Views/SigningMessages.vb`, `Views/OrdView.vb`, `Views/DdfView.vb`
- HelpContent: new `contabil/liste.md`; `contabil/vederi/{ord,plati,sumar,istoric,receptii}.md`,
  `contabil/ddf/{index,semnare,trimitere,rezervare}.md`, `contabil/{fereastra,notecab}.md`,
  `contabil/forexe/descarcare.md`, `contabil/nomenclatoare/parteneri.md`, `tours/{tur-ord,tur-ddf,tur-plati}.md`
- status: `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md` index

## Test results

- `Check-Help.ps1 -Coverage`: 48 topics, 12 tours, **No errors**, coverage «(none)».
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**. Nothing run, nothing on screen.

Capture ids **new**: `liste-subsol-arbore`, `liste-meniu-coloana`.

## Left unverified or deferred / de citit de operator

- **Three footer icons do nothing** (tooltip, no click handler anywhere): `PlatiView` right
  («Descarcă plățile/încăsările din CAB.»), `IstoricView` right («Reîncarcă istoricul angajamentului de
  la server.»), `DdfView` right «+» («Adaugă D.D.F. / Revizie»; the view's own comment says DDF adds
  are made from the Rezervări tree). Not in the help. Operator to decide: wire them or remove them.
- The state label «PDF final — de semnat A și B» (`DdfRevisionStates.Label`) still reads as if A were
  signed again; the help explains it as «semnezi ce lipsește (de obicei doar B)». Rename?
- The ORD «Semnătura 1 on an incomplete table» trap now has no way out in K-BOT (a signed ORD is
  neither regenerated nor deleted). K-BOT documents have the full table, so it should not happen.
- «încărcat» on receptions: K-BOT does not upload receptions yet; the meaning is Access's.
