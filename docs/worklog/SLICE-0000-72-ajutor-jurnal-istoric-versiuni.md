# SLICE-0000-72 - Ajutorul pentru 0118: «Jurnal activitate» si «Istoric versiuni» ca machete (13.10.2026)

Reper ulterior pentru pagini păstrate deschise: [0000-86](SLICE-0000-69-ajutor-machete-ca-pagini.md#slice-0000-86). Restanța turului și lipsa capturilor de mai jos rămân la checkpoint.

Sub-felie de ajutor a feliei 0118. Context, capturi si navigare: `SLICE-0000-69-ajutor-machete-ca-pagini.md`.

## Ce s-a schimbat si de ce
- Subiect NOU `contabil.jurnal` («Jurnal activitate», `contabil/jurnal.md`, `screens: JurnalPage, SetariJurnalView, LogViewerForm, LogClearDialog`, `open: menu:jurnal`): de unde se deschide (ribbon, doar cu comutatorul pornit), ce vezi (cautare, interval, niveluri, tabelul pe zile cu **ziua de azi desfacuta**, mesajul intreg sub **separatorul reglabil**, butoanele de jos), jurnalele serverului doar cu optiunile avansate. Sectiunile «Mesajele K-BOT» si «Golirea jurnalelor» au trecut aici din `contabil.setari`. Captura noua: `jurnal-activitate` (`goto: menu:jurnal`, care ajunge: deschide macheta).
- `contabil.setari`: intro rescris (Setari din ribbon, ca pagina; jurnalul si istoricul au machetele lor), randul «Jurnal» scos din tabelul paginilor, `screens:` fara vederile jurnalului (mutate), sectiunile de jurnal inlocuite de o trimitere; pornirea marita (bifata o data dupa actualizare, 0113). Captura `setari` rosie.
- `contabil.actualizare`: sectiune noua «Ce a adus fiecare versiune: «Istoric versiuni»» (`screens:` + `IstoricVersiuniPage`, `SetariIstoricView` — acopera si fereastra fara subiect raportata din 0000-32). Captura noua: `istoric-versiuni` (doar `prepare:`: nu exista `goto:` pentru ea).
- `avansat.jurnale`: `open: setari:jurnal` (nu mai ajunge) -> `open: menu:jurnal`; textul spune macheta, nu pagina din Setari.

## Fisiere
`src/KBot.App/HelpContent/contabil/jurnal.md` (nou), `contabil/setari.md`, `contabil/actualizare.md`, `avansat/jurnale.md`.

## Teste / verificare
Vezi 0000-74. Nimic rulat in aplicatie.

## Capturi de refacut
Rosie: `setari`. Noi: `jurnal-activitate`, `istoric-versiuni`.

## Neverificat
- Turul `tur-setari` are inca pasul «Jurnal» (`setari:jurnal`): eroare de verificare, lasat pentru decizia din 0000-69 (tururi).
