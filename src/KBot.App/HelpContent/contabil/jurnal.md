---
id: contabil.jurnal
title: Jurnal activitate
part: contabil
order: 85
parent: contabil
screens: JurnalPage, SetariJurnalView, LogViewerForm, LogClearDialog
keywords: jurnal, jurnale, log, jurnal activitate, erori, mesaje, ce mi-a spus k-bot, goleste jurnale, sterge jurnale, exporta jurnal, copiaza, detaliu, separator
open: menu:jurnal
---
<!-- slice: 0118 -->
**Jurnal activitate** arată ce a scris K-BOT în jurnalele lui: erorile, mesajele pe care ți le-a arătat și ce a
făcut. Se deschide din ribbon, **Configurări › Configurare › «Jurnal activitate»**, ca pagină a ferestrei principale
(înainte era o pagină din Setări). Butonul se vede doar cât e pornit, în **Setări › Aplicație**, comutatorul lui —
[Setări și aspect](topic:contabil.setari).

<!-- capture: jurnal-activitate | caption: Macheta «Jurnal activitate», cu ziua de azi desfăcută și detaliul sub separator | goto: menu:jurnal | prepare: Alegeți jurnalul de erori, lăsați zilele strânse și selectați un rând din ziua de azi, ca mesajul lui să apară dedesubt. -->

## Ce vezi
<!-- slice: 0118, 0072-01 -->

- Lista **fișierelor de jurnal**: alegi pe care îl citești.
- Sus: **Caută:** (textul se caută pe măsură ce scrii), intervalul **De la:** / **Până la:**, nivelurile afișate
  (eroare, avertisment, informație) și **«Reîmprospătează»**, care recitește fișierul.
- Tabelul cu intrările, **cele mai noi sus**, grupate pe zile. Când zilele sunt strânse, **ziua de azi rămâne
  desfăcută**: e cea pe care o citești de obicei.
- Sub tabel, **mesajul întreg** al rândului ales. Între tabel și mesaj e un **separator**: îl tragi cu mouse-ul în
  sus sau în jos ca să dai mai mult loc tabelului sau mesajului.
- Jos: **«Deschide dosarul»** (dosarul cu jurnale, în Explorer), **«Golește jurnale…»** (mai jos), **«Copiază»** (rândul
  ales sau, fără alegere, rândurile afișate) și **«Exportă»** (rândurile zilelor desfăcute, într-un fișier).

Jurnalele serverului se văd aici doar cu [opțiunile avansate](topic:avansat) pornite —
[Când ceva nu merge: jurnalele](topic:avansat.jurnale).

## Mesajele K-BOT
<!-- slice: fara-felie, 0118 -->

Orice mesaj pe care ți-l arată K-BOT se scrie și în jurnalul de mesaje, așa că îl poți regăsi după ce ai apăsat OK:
**Configurări › Configurare › «Jurnal activitate»**.

## Golirea jurnalelor
<!-- slice: 0031-04, 0000-07, 0118 -->

**«Golește jurnale…»** deschide lista fișierelor de jurnal de pe calculatorul tău, cu mărimea și numărul de intrări
ale fiecăruia. La deschidere nu e nimic bifat.

1. Bifează fișierele de șters.
2. Apasă **«Șterge»**. K-BOT îți spune exact ce dispare și cât ocupă, și cere confirmarea.

**Ștergerea nu se poate anula.** Un fișier folosit chiar acum de K-BOT nu se poate șterge; unul ținut deschis de alt
program se golește în loc să se șteargă. Jurnalele serverului nu se ating de aici.

> Înainte să golești jurnalele după o problemă, trimite-le întâi cui te ajută.
