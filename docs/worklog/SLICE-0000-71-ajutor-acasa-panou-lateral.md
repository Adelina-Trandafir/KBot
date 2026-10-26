# SLICE-0000-71 / 87 / 106 — Ajutorul Acasă și panoul lateral

Etapele 87/106 modifică descrierea inițială: acțiuni doar azi și buton jos, apoi panou în spatele machetelor. Secțiunea Machete deschise din 71 precede [0000-86](SLICE-0000-69-ajutor-machete-ca-pagini.md#slice-0000-86), care descrie stiva și exclude modalele/vederile. Listele de capturi și limitele pe temă se păstrează.

- [0000-71](#slice-0000-71)
- [0000-87](#slice-0000-87)
- [0000-106](#slice-0000-106)

## SLICE-0000-71 - Ajutorul pentru 0115 / 0116 / 0117: pagina «Acasa» si panoul lateral (13.10.2026) <a id="slice-0000-71"></a>

Sub-felie de ajutor a feliilor 0115 (Acasa + panou), 0116 (machete deschise) si 0117 (actiuni recente). Context, capturi si navigare: `SLICE-0000-69-ajutor-machete-ca-pagini.md`.

### Ce s-a schimbat si de ce
- Subiect NOU `contabil.acasa` («Pagina «Acasa»», `contabil/acasa.md`, `screens: AcasaPage`): ce vezi la pornire, cum ajungi la lista angajamentelor si inapoi («Inchide»), cardul «Ce este nou» (ultimele trei versiuni, `ReleaseNotesText.RecentVersions = 3`; strangere tinuta minte; se desface singur o data dupa fiecare actualizare). Captura noua: `acasa`.
- Subiect NOU `contabil.panou` («Panoul lateral», `contabil/panou-lateral.md`, `screens: AvacontMain.sidebar`): strangerea sectiunilor si a panoului, derularea; **Machete deschise** (10, doar in sesiune, redeschiderea cu an / angajament / vedere, editoarele redeschise in vederea DDF / ORD, mesajele posibile, angajamentul nou nesalvat nu apare); **Actiuni recente** (10, ale tale, pe unitatea curenta, ora / ziua, actualizare singura, «Nicio actiune inca»); **Informatii** (tabel cu cele sapte randuri si culorile); **Erori din sesiune** (10, clic = detaliu). Capturi noi: `panou-lateral`, `panou-lateral-strans`.
- `contabil.efactura.token`: starea tokenului se vede si in panoul lateral; fraza «nu apare singur in alte parti ale K-BOT» inlocuita (eticheta `0115`).
- `contabil.actualizare`: legatura la cardul «Ce este nou» (in sectiunea «Istoric versiuni», 0000-72).

### Fisiere
`src/KBot.App/HelpContent/contabil/acasa.md` (nou), `contabil/panou-lateral.md` (nou), `contabil/efactura/token.md`.

### Teste / verificare
Vezi 0000-74. Nimic rulat in aplicatie.

### Capturi de refacut
Noi: `acasa`, `panou-lateral`, `panou-lateral-strans` (doar `prepare:`).

### Neverificat / de citit de operator
- «Actiuni recente»: ajutorul nu spune ca partea de server lipseste (nu e treaba utilizatorului). Pana se livreaza `routes/actiuni.py` (0117), lista arata «Nicio actiune inca» la toata lumea.
- Culorile randului «Token E-Factura» (verde / portocaliu / rosu) sunt cele ale temei Classic (`SuccessColor`, `WarningColor`, `ErrorColor`); in alte teme pot arata altfel.

## SLICE-0000-87 - Help for 0115-01: side panel (17.10.2026) <a id="slice-0000-87"></a>

### What changed and why
`contabil/panou-lateral.md` (tag `0115-01` on the page and on «Machete deschise» / «Actiuni recente»): the fold button is at the bottom of the panel, the state is kept between runs, a tooltip lists the section when the panel is folded, an open macheta shows a picture of itself on hover (a closed one shows nothing), «Actiuni recente» shows only today and empties at midnight. Captures `panou-lateral` and `panou-lateral-strans` marked `redo:`. `help-version.txt` = `2026-10-17`.

### Files touched
`src/KBot.App/HelpContent/contabil/panou-lateral.md`, `src/KBot.App/HelpContent/help-version.txt`.

### Test results
`Check-Help.ps1 -Coverage` run 17.10.2026: no error from `panou-lateral.md` (the 20 remaining ones are old: tutorials, `tur-setari`).

### Unverified / deferred
Nothing seen on screen. The two captures above must be redone.

## SLICE-0000-106 - Help for 0115-02 (18.10.2026) <a id="slice-0000-106"></a>

### What changed and why
`contabil/panou-lateral.md`: the opening no longer says the panel is beside every page; it is behind the machete (shown only with none on screen) unless the tick is turned off. `contabil/setari.md`: the new tick in the table of the «Fereastra principala» group. Slice tags `0115-02, 0000-106`; `help-version.txt` set to 2026-10-18.

### Files touched
`src/KBot.App/HelpContent/contabil/panou-lateral.md`, `src/KBot.App/HelpContent/contabil/setari.md`, `src/KBot.App/HelpContent/help-version.txt`.

### Test results
See the check line in the final report of the task (`Check-Help.ps1`); build 0/0.

### Unverified / deferred
Nothing seen on screen. Captures `panou-lateral` / `panou-lateral-strans` are unchanged (they are taken on «Acasa», where the panel is shown).
