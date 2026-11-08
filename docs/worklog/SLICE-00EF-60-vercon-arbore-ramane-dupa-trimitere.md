# SLICE-00EF-60 — «Trimite la Vercon»: după trimitere arborele se reface, dar rămâne unde era (06.11.2026)

Cererea operatorului: când se dă trimiterea facturii în Vercon, arborele să nu se închidă, ci să rămână unde era selecția (dar să facă refresh). Ajutor: 0000-154.

## What changed and why
- **Cauza:** `AfterWriteAsync` debifa facturile scrise (`SetTick(.., False)`) și refăcea arborele cu `BuildTree(TickedIds())`. `BuildTree` deschidea doar lunile și furnizorii care aveau o factură în lista de bife păstrate; facturile trimise nu mai erau bifate (după 00EF-59 sunt blocate, deci nu intră în `TickedIds()`), așa că nodurile lor se închideau, iar selecția și derularea se pierdeau.
- **Remediu** (`VerconForm.BuildTree`): parametru nou opțional `k_keepView` (implicit `False`). Când e `True`, înainte de `tree.Clear()` se rețin cheile nodurilor deschise, cheia nodului ales și `ScrollOffsetY`; după refacere se redeschid nodurile cu aceleași chei, se alege din nou nodul cu aceeași cheie (dacă mai există) și se pune la loc derularea. Ajutoare noi: `CollectOpenKeys`, `ReopenKeys`, `FindNodeByKey` (statice, private).
- `AfterWriteAsync` apelează `BuildTree(TickedIds(), True)`; deci și «Retrage de la Vercon» păstrează locul. Schimbarea grupării (`GroupingChosen`) rămâne cu `BuildTree(TickedIds())`, fără păstrarea locului (cheile se schimbă).
- **Ajutor:** o propoziție în tutorialul `efactura-trimite-la-vercon.md`, secțiunea «Trimite»; etichete `00EF-60, 0000-154`.
- FileVersion KBot.EFactura 1.14.1.0 → 1.14.2.0.

## Files touched
`src/KBot.EFactura/Forms/VerconForm.vb`, `src/KBot.EFactura/KBot.EFactura.vbproj`, `src/KBot.App/HelpContent/tutorials/efactura-trimite-la-vercon.md`.

## Test results
`dotnet build src\KBot.EFactura\KBot.EFactura.vbproj`: 0 errors (build făcut înainte de bump-ul de versiune și de textul de ajutor). Niciun test scris sau rulat (regula operatorului); nimic văzut pe ecran; `Check-Help.ps1` nerulat.

## Unverified / deferred
- Nu s-a văzut pe ecran: că arborele rămâne deschis, că rândul ales rămâne ales și că derularea nu sare.
- Nu am verificat dacă `tree.SelectedNode = ...` declanșează un eveniment de selecție care ar face altceva în pagină (în `VerconForm` nu există handler de selecție; verificat doar prin citirea fișierului).
- Capturile din tutorial nu au fost refăcute.
