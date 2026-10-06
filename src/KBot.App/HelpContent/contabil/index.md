---
id: contabil
title: Despre K-BOT
part: contabil
order: 0
screens: KbotForm
keywords: prezentare, fereastra principala, flux
---
<!-- slice: 0000-03, 0000-52 -->
K-BOT ține evidența angajamentelor bugetare ale unității și le leagă de **FOREXE** (sistemul de
control al angajamentelor al Ministerului Finanțelor): citește din FOREXE rezervările, recepțiile
și plățile, face documentele de fundamentare (DDF) și le trimite înapoi în FOREXE.

<!-- capture: fereastra-principala | caption: Fereastra principală K-BOT | goto: view:sumar | prepare: Selectați un angajament în lista din stânga, ca vederea «Sumar» să arate date. -->

## Lucrul de zi cu zi, pe scurt
<!-- slice: 0001, 0034, 0055, 0060, 0048-04, 0081, 0076, 0000-12 -->

1. **Te conectezi** la K-BOT cu utilizatorul tău și alegi unitatea — [Conectarea](topic:contabil.autentificare).
2. **Te conectezi la FOREXE** din banda de jos (butonul «Conectare») — [Legătura cu FOREXE](topic:contabil.forexe).
3. **Aduci lista de angajamente** și, pentru fiecare angajament care te interesează, **descarci**
   datele lui din FOREXE — [Descărcarea unui angajament](topic:contabil.forexe.descarcare).
   Înainte de robot alegi ce recepții se citesc din nou (fereastra «Ce recepții reîmprospătez?»).
   Dacă se deschide fereastra **Asocieri**, spui a cărei recepții e fiecare salvare din istoric —
   [Asocierile recepțiilor](topic:contabil.asocieri): de ea depind cifrele ordonanțărilor.
4. **Te uiți pe angajament** în vederile din stânga: Sumar, Istoric, Rezervări, Recepții, Plăți,
   Extrase — [Vederile angajamentului](topic:contabil.vederi).
5. **Faci documentul de fundamentare**, îl semnezi și îl trimiți în FOREXE —
   [Documentul de fundamentare](topic:contabil.ddf).

**Și din browser:** datele unității pot fi consultate, în regim doar de citire, și online, de oriunde — cu e-mail, parolă și cod de verificare sau, de pe calculator, cu tokenul tău digital. Accesul online la date îți oferă informațiile de care ai nevoie fără să fie necesară instalarea K-BOT pe dispozitivul de pe care le consulți. [Accesul online la date](topic:contabil.online).

> **Regula de aur:** un angajament pe care îl lucrezi prin K-BOT nu se mai modifică de mână în
> FOREXE. Tot ce se schimbă în FOREXE trebuie să treacă prin K-BOT — prin robot sau prin pagina
> din [vederea «Browser FOREXE»](topic:contabil.forexe.browser), pe care K-BOT o urmărește —,
> altfel documentele de fundamentare nu mai corespund cu ce e în FOREXE.
