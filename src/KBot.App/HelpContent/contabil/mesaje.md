---
id: contabil.mesaje
title: Mesajele K-BOT
part: contabil
order: 92
parent: contabil
screens: KBotMessageBoxForm
keywords: mesaj, caseta de mesaj, eroare, avertisment, intrebare, confirmare, trimite eroarea, raport eroare, raporteaza, buton, da, nu, anulare, ok
---
<!-- slice: 0112, 0112-03, 0112-04, 0000-59 -->
Când K-BOT are ceva de spus — o informare, un avertisment, o întrebare sau o eroare — deschide o
fereastră de mesaj. Ea arată ca restul aplicației: are bara de titlu proprie, butoane și culori
după tema aleasă. Fereastra se potrivește singură după lungimea mesajului; un mesaj poate avea și un
titlu aparte, scris îngroșat, deasupra textului.

## Ce este în fereastră
<!-- slice: 0112, 0112-03, 0000-59 -->
- **Pictograma** din stânga arată felul mesajului: informare, avertisment, întrebare sau eroare.
- **Butoanele** stau jos. Răspunsurile care înseamnă «da» — **«Da»**, **«OK»**, **«Reîncearcă»** — sunt
  mereu în dreapta; cele care înseamnă «nu» — **«Nu»**, **«Anulare»**, **«Renunță»** — sunt mereu în stânga.
  Butonul colorat este cel care se apasă și cu **Enter**.
- **Esc** răspunde ca **«Anulare»** (sau ca **«OK»**, când acesta e singurul buton).
- **X-ul** din colțul barei de titlu apare doar când mesajul se poate închide fără un răspuns anume.
  O întrebare la care trebuie să răspunzi (de exemplu «Da» / «Nu») nu are X și nu se închide altfel.

## Trimiterea unei erori
<!-- slice: 0112-04, 0000-59 -->
La un mesaj de **eroare**, în bara de titlu, lângă X, apare un buton cu o săgeată care iese dintr-o tavă:
**«Trimite eroarea»** (îl vezi și dacă ții cursorul deasupra). Dacă ai nevoie de ajutor, apasă-l:
K-BOT trimite mesajul, împreună cu detaliile care ajută la aflarea cauzei, la serverul K-BOT, ca să
poată fi analizat.

- Cât timp se trimite, butonul este estompat și nu poate fi apăsat din nou.
- Când trimiterea a reușit, butonul **dispare**: eroarea aceasta nu mai poate fi trimisă a doua oară.
- Nu apare niciun mesaj despre trimitere, nici dacă a reușit, nici dacă nu. Dacă butonul rămâne la
  loc, trimiterea nu a reușit: poți apăsa din nou.
- Trimiterea cere să fii conectat la K-BOT.
- Butonul apare doar la erori, nu și la informări, avertismente sau întrebări.
