# SLICE-0000-71 - Ajutorul pentru 0115 / 0116 / 0117: pagina «Acasa» si panoul lateral (13.10.2026)

Sub-felie de ajutor a feliilor 0115 (Acasa + panou), 0116 (machete deschise) si 0117 (actiuni recente). Context,
capturi si navigare: `SLICE-0000-69-ajutor-machete-ca-pagini.md`.

## Ce s-a schimbat si de ce
- Subiect NOU `contabil.acasa` («Pagina «Acasa»», `contabil/acasa.md`, `screens: AcasaPage`): ce vezi la pornire, cum
  ajungi la lista angajamentelor si inapoi («Inchide»), cardul «Ce este nou» (ultimele trei versiuni,
  `ReleaseNotesText.RecentVersions = 3`; strangere tinuta minte; se desface singur o data dupa fiecare actualizare).
  Captura noua: `acasa`.
- Subiect NOU `contabil.panou` («Panoul lateral», `contabil/panou-lateral.md`, `screens: AvacontMain.sidebar`):
  strangerea sectiunilor si a panoului, derularea; **Machete deschise** (10, doar in sesiune, redeschiderea cu an /
  angajament / vedere, editoarele redeschise in vederea DDF / ORD, mesajele posibile, angajamentul nou nesalvat nu apare);
  **Actiuni recente** (10, ale tale, pe unitatea curenta, ora / ziua, actualizare singura, «Nicio actiune inca»);
  **Informatii** (tabel cu cele sapte randuri si culorile); **Erori din sesiune** (10, clic = detaliu).
  Capturi noi: `panou-lateral`, `panou-lateral-strans`.
- `contabil.efactura.token`: starea tokenului se vede si in panoul lateral; fraza «nu apare singur in alte parti ale
  K-BOT» inlocuita (eticheta `0115`).
- `contabil.actualizare`: legatura la cardul «Ce este nou» (in sectiunea «Istoric versiuni», 0000-72).

## Fisiere
`src/KBot.App/HelpContent/contabil/acasa.md` (nou), `contabil/panou-lateral.md` (nou), `contabil/efactura/token.md`.

## Teste / verificare
Vezi 0000-74. Nimic rulat in aplicatie.

## Capturi de refacut
Noi: `acasa`, `panou-lateral`, `panou-lateral-strans` (doar `prepare:`).

## Neverificat / de citit de operator
- «Actiuni recente»: ajutorul nu spune ca partea de server lipseste (nu e treaba utilizatorului). Pana se livreaza
  `routes/actiuni.py` (0117), lista arata «Nicio actiune inca» la toata lumea.
- Culorile randului «Token E-Factura» (verde / portocaliu / rosu) sunt cele ale temei Classic (`SuccessColor`,
  `WarningColor`, `ErrorColor`); in alte teme pot arata altfel.
