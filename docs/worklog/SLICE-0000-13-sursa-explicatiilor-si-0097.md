# SLICE-0000-13…15 — Proveniența explicațiilor și corecțiile confirmate de operator

Consolidare documentară D002-14. 13 introduce etichetele de sursă și lasă întrebări despre documentele semnate și comenzile listelor; 14 consemnează răspunsurile operatorului, refuzul regenerării și corecțiile explicațiilor; 15 rezolvă iconițele fără acțiune și eticheta PDF final. Întrebările din 13 și restanțele din 14 rămân la reperul lor, urmate de răspunsurile datate, fără a fi declarate lipsuri actuale.

Toate corpurile, inventarele, capturile de refăcut, numărătorile, probele și limitele sunt păstrate. Etichetele de proveniență erau «best effort», nu dovezi noi. Instrucțiunile istorice citate nu înlocuiesc AGENTS actual și nu implică schimbarea ajutorului încorporat.

[13](#slice-0000-13) · [14](#slice-0000-14) · [15](#slice-0000-15)

## SLICE-0000-13 — Where each explanation comes from + help for slice 0097 (30.09.2026) <a id="slice-0000-13"></a>

Operator request (30.09.2026):
1. every explanation in the help carries its slice, invisible to the user, so it is known where the decision behind it came from;
2. «Descărcarea unui angajament» must point to the window that picks the receptions to read again;
3. the new options in Setări › Autentificare and the unit selector in the KbotForm caption bar.

Mid-task, a new standing rule: **ANY change to what the operator sees must also be recorded in the help, and must reference its slice number.** Written into `CLAUDE.md` (section «Help (slice 0000)»), `CODE_WORKFLOW.md` (Definition of Done) and `docs/HELP_SYSTEM.md` §4 / §5.

### 1. Source tags

- Syntax: one line `<!-- slice: 0072, 0097 -->`, right after a topic's header block (covers the text before the first `##`) and right under **every** `## ` heading of topics and tour steps (`###` optional). Ids: a slice from the `KBOT_STATUS.md` index (`0048` or `0048-04`), a help sub-slice `0000-NN`, or `fara-felie` (work recorded only in `KBOT_STATUS_SLICELESS.md`).
- Engine: `HelpLibrary.SourceTagPattern` / `StripSourceTags`; the tags are removed when a topic (`HelpLibrary.Parse`) or a tour (`HelpTour.Parse`) is read, so no page, bubble, search hit or exported manual ever carries them.
- Checker: `Check-Help.ps1` fails on a topic without the header tag, a `##` section / tour step without a tag under it, an empty tag, or an id that is neither in the index nor a 0000-NN worklog.
- All 47 topics and 12 tours tagged (222 tags). **Best effort** (operator: «where the source isn't clear, don't bother too much»): taken from the code comments of each window («Slice 00NN») and from the worklogs that name the window / message. Least certain: the «Butonul ⓘ» section and «Mesajele K-BOT» (`fara-felie`), `contabil.ddf.mf` (0000-03 = the MF guide summary), the Asocieri concept sections (0048-03…09 + 0000-12). A later edit of a section replaces its tag with the slices that decided the new text.

### 2. Help for slice 0097

| Topic / tour | Change |
|--------------|--------|
| `contabil.forexe.descarcare` | new section «Înainte de descărcare: ce recepții se citesc din nou» (the «Ce recepții reîmprospătez?» window, columns, same-day rule, Descarcă / Renunță, the Setări switch); new «A doua apăsare, cât K-BOT e deschis» (reuse of the package, 0058); «Reîmprospătări parțiale» points up |
| `contabil` (index), `contabil.vederi.receptii`, `tur-forexe` | mention of the window |
| `contabil.autentificare` | «Când expiră sesiunea» (silent re-login vs window), «Ține minte parola până la repornirea calculatorului»; «Parola nu se salvează niciodată» removed (no longer true) |
| `contabil.fereastra` | new «Unitatea de lucru» (the caption selector, the two refusals, secondary windows); «Note corecție» gated; «cinci zone»; `KbotForm.capBar` in `screens:` |
| `contabil.setari` | new «Pagina «Autentificare»» (three groups, what needs the advanced options); table row |
| `contabil.vederi.ord`, `contabil.ord.generare`, `tur-ord` | root «Toate ordonanțările», month / root delete, signed = no menu, no box after a delete |
| `contabil.ddf`, `contabil.ddf.semnare`, `tur-ddf` | root «Toate reviziile», «Șterge documentul (TOATE reviziile)», signed = only the send; «Semnat A» no longer editable |
| `contabil.notecab` | the view appears only with a note |
| `tur-setari`, `tur-fereastra` | Autentificare step, unit selector in the title step |

Capture ids: **new** `unitate-selector`, `setari-autentificare`. **Moved** `selectie-receptii` (now in the new section; same window). **To re-shoot**: `ord`, `ddf-vedere` (the new roots), `conectare-fereastra` (only if the password box is shown on the shooting PC).

### Check + build

- `Check-Help.ps1 -Coverage`: 47 topics, 12 tours, 52 capture tags, **No errors**, coverage «(none)».
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**. Nothing run, nothing on screen.

### Files touched

engine: `src/KBot.App/Help/HelpLibrary.vb`, `Help/HelpTour.vb`; checker: `tools/HelpCheck/Check-Help.ps1`; all 47 topics and 12 tours under `src/KBot.App/HelpContent/` (tags); content changes as in §2; rule + syntax: `CLAUDE.md`, `docs/worklog/CODE_WORKFLOW.md`, `docs/HELP_SYSTEM.md`, `src/KBot.App/HelpContent/README.md`; status: `state/KBOT_STATUS_0000-0009.md` (row, watermark, Open threads), `KBOT_STATUS.md` index

### De citit de operator

- The source tags are a best effort — correct any you know better.
- ORD: with the menu gone on a signed ordonanțare, the help still says a broken signed document is «generated again from K-BOT» (the «Generează» button of the Document page). Is regenerating a signed ORD still intended?
- «Stare» in the reception window shows «încărcată» / «preluată» without explaining them — say what they mean to the accountant if it matters.
- The unit switch depends on the server routes of 0097 (`auth.py`, deploy by hand).

## SLICE-0000-14 — A signed document is never regenerated + the buttons of trees and grids (30.09.2026) <a id="slice-0000-14"></a>

Operator answers to 0000-13 (30.09.2026):
1. «A signed document CAN NEVER BE REGENERATED. That was there before the 0078 slice; now we know we can insert data into an already signed document, so it is not needed anymore.»
2. «încărcată» / «preluată» must be explained; «also we must document the buttons in the headers/footers of each tree/dgv».

### What changed and why

#### Code (visual change → help in the same task, per the new rule)
- `Views/SigningMessages.vb`: `ConfirmRegenerateSigned` (the Yes/No «generate an unsigned one over the signed version?») replaced by `ShowSignedNeverRegenerated` (information only).
- `Views/OrdView.vb` / `Views/DdfView.vb` `OnGenerateRequested`: a document with at least one signature (`EsteSemnata` = roles in `Semnatura` OR a signed PDF on the server) is refused with that message. Before, only `ArePdfSemnat` was checked, and the operator could say «Da».

#### Help
- **Regeneration**: `contabil.vederi.ord` (Document section, the «Semnătura 1» trap, validation errors, «Anulare Validare»), `tur-ord`.
- **DDF after sending — found stale while doing it** (text from 0000-03, before 0078-06): the help said the final PDF is a new document signed «de la capăt» on A and B. The code (0078-06, `DdfView.SectiuneaBInseratAsync`, `KbotForm.DdfSendMenu.GenereazaPdfFinalAsync`) puts Section B and the captures INTO the document signed on A; only B is signed. Rewritten: `contabil.ddf.semnare` («După trimitere — semnătura B»), `contabil.ddf` (the road, the «PDF final» state row), `contabil.ddf.trimitere`, `contabil.ddf.rezervare`, `tur-ddf`.
- **«încărcat» / «preluat»** (Access convention, the same flags on FX_Angajamente / FX_DDF / FX_ORD / FX_Receptii_R): încărcat = sent into FOREXE from K-BOT (or the old program), preluat = came in from a FOREXE download. `contabil.forexe.descarcare` («Stare» column), `contabil.vederi.sumar`.
- **Buttons of trees and grids**: new topic `contabil.liste` «Arborii și tabelele — butoanele din cap și din subsol» (tree header: folder = sign only, lupa = search band, right icon per view; tree footer: caption, left/right icons, collapse button + row flyout; grid: the funnel menu Sortare / Filtrare / Grupare with its buttons, filled funnel = filtered; TOTAL(URI) over the visible rows; «+» adds a row; check-all icon in a check column; a table pointing to each view's own buttons). Per view: `contabil.fereastra`, `contabil.vederi.istoric` (new section), `contabil.vederi.receptii`, `contabil.vederi.plati`, `contabil.vederi.sumar`, `contabil.vederi.ord`, `contabil.ddf`, `contabil.notecab`, `contabil.nomenclatoare.parteneri` (new section: the «Coduri angajament» grid, its «+» and ✕, the «Ascunde partenerii fără activitate» box, the window buttons).

#### Removed from the help (was false)
- «Iconița din subsolul arborelui, dreapta, descarcă plățile și încasările din CAB» (`contabil.vederi .plati` and the `tur-plati` step «Descărcarea din CAB»): the icon has a tooltip but **no handler**.

### Files touched

`src/KBot.App/Views/SigningMessages.vb`, `Views/OrdView.vb`, `Views/DdfView.vb`; HelpContent: new `contabil/liste.md`; `contabil/vederi/{ord,plati,sumar,istoric,receptii}.md`, `contabil/ddf/{index,semnare,trimitere,rezervare}.md`, `contabil/{fereastra,notecab}.md`, `contabil/forexe/descarcare.md`, `contabil/nomenclatoare/parteneri.md`, `tours/{tur-ord,tur-ddf,tur-plati}.md`; status: `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md` index

### Test results

- `Check-Help.ps1 -Coverage`: 48 topics, 12 tours, **No errors**, coverage «(none)».
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**. Nothing run, nothing on screen.

Capture ids **new**: `liste-subsol-arbore`, `liste-meniu-coloana`.

### Left unverified or deferred / de citit de operator

- **Three footer icons do nothing** (tooltip, no click handler anywhere): `PlatiView` right («Descarcă plățile/încăsările din CAB.»), `IstoricView` right («Reîncarcă istoricul angajamentului de la server.»), `DdfView` right «+» («Adaugă D.D.F. / Revizie»; the view's own comment says DDF adds are made from the Rezervări tree). Not in the help. Operator to decide: wire them or remove them.
- The state label «PDF final — de semnat A și B» (`DdfRevisionStates.Label`) still reads as if A were signed again; the help explains it as «semnezi ce lipsește (de obicei doar B)». Rename?
- The ORD «Semnătura 1 on an incomplete table» trap now has no way out in K-BOT (a signed ORD is neither regenerated nor deleted). K-BOT documents have the full table, so it should not happen.
- «încărcat» on receptions: K-BOT does not upload receptions yet; the meaning is Access's.

## SLICE-0000-15 — Dead footer icons removed + «PDF final — de semnat B» (30.09.2026) <a id="slice-0000-15"></a>

Operator answers to 0000-14: the three footer icons with no action are **taken out**; the state label is **renamed**.

### What changed and why

- `Views/PlatiView.Designer.vb`: tree footer right icon («Descarcă plățile/încăsările din CAB.») and its caption «Actualizează» removed. The footer band stays (empty), level with the grid's TOTALURI.
- `Views/IstoricView.Designer.vb`: tree footer right icon («Reîncarcă istoricul angajamentului de la server.») removed; the collapse button and «Perioade» stay.
- `Views/DdfView.Designer.vb`: tree footer «+» («Adaugă D.D.F. / Revizie») and its caption «Adaugă» removed; the collapse button stays. DDF additions are made from the Rezervări tree. None of the three had a click handler, so no code behind them changed.
- `KBot.Domain/DdfRevisionState.vb`: `Label(FinalToSign)` «PDF final — de semnat A și B» → **«PDF final — de semnat B»** (since 0078-06 section B goes into the document signed on A; only B is signed). XML doc of `FinalToSign` corrected.
- Help: `contabil.ddf` state table row, `tur-ddf` step «Reviziile documentului»; source tags on `contabil.vederi.istoric` / `contabil.vederi.plati` (the removed icons were already out of the help since 0000-14).

### Files touched

`src/KBot.App/Views/{PlatiView,IstoricView,DdfView}.Designer.vb`; `src/KBot.Domain/DdfRevisionState.vb`; `src/KBot.App/HelpContent/contabil/ddf/index.md`, `contabil/vederi/{istoric,plati}.md`, `tours/tur-ddf.md`; status: `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md` index

### Test results

- `Check-Help.ps1 -Coverage`: **No errors**, coverage «(none)».
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**. No tests, nothing on screen.

### Left unverified or deferred

- Designer files edited by hand (three / two / three property lines deleted); not opened in the VS designer.
- FileVersion of KBot.App / KBot.Domain not bumped (`push-update.ps1` asks).
- Capture to re-shoot if it shows the old footer or label: `plati`, `istoric`, `ddf-vedere`.
