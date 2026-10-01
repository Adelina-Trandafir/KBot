---
id: contabil.autentificare
title: Conectarea
part: contabil
order: 10
parent: contabil
screens: LoginForm
keywords: login, parola, utilizator, unitate, autentificare, sesiune expirata, reautentificare, tine minte parola
---
<!-- slice: 0001, 0063, 0072, 0081-06, 0097, 0097-02 -->
K-BOT pornește cu fereastra de conectare. Ea nu are butonul «?» în bara de titlu: pagina aceasta
se deschide cu **F1**.

<!-- capture: conectare-fereastra | caption: Fereastra de conectare | prepare: Porniți K-BOT; fereastra de conectare se fotografiază înainte de «Continuă». -->

1. Scrie **utilizatorul** (adresa de e-mail) și **parola**, apoi apasă **Continuă**.
2. Alege **unitatea** din listă. Apare doar dacă ai acces la mai multe unități.
3. Apasă **Autentificare**.

K-BOT ține minte ultimul utilizator și ultima unitate folosite pe acest calculator, așa că data
viitoare scrii doar parola. Ce se ține minte alegi în **Setări › Autentificare** —
[Setări](topic:contabil.setari).

> Dacă ai rolul **Director**, după conectare nu se deschide fereastra contabilului, ci lista
> documentelor care așteaptă semnătura ta (vezi partea a treia a ajutorului).

## După conectare: anul și sursa/sectorul
<!-- slice: 0001, 0086, 0097, 0097-03 -->

În bara de titlu a ferestrei principale alegi **An Date** (anul de lucru) și **Sursă/Sector**
(de exemplu 02A). Schimbarea lor reîncarcă lista de angajamente și toate ecranele. Ultima
alegere se ține minte pentru data viitoare.

Dacă ai acces la mai multe unități, poți trece la alta fără să te conectezi din nou, din bara de
titlu — [Unitatea de lucru](topic:contabil.fereastra).

## Când expiră sesiunea
<!-- slice: 0097 -->

Sesiunea cu serverul K-BOT expiră după **20 de minute fără activitate**. La următoarea acțiune
K-BOT te conectează din nou, pe **aceeași unitate**, în unul din două feluri:

- **singur, fără nicio fereastră**, cu parola scrisă la intrare, dacă ultima conectare în
  fereastră e mai nouă decât intervalul ales în **Setări › Autentificare** (implicit 30 de minute);
- **cu fereastra de conectare**, altfel. Scrii din nou parola și continui de unde ai rămas.

Dacă serverul refuză reconectarea singură (de exemplu parola s-a schimbat între timp), apare
fereastra. La **pornirea** K-BOT fereastra de conectare apare întotdeauna.

## «Ține minte parola până la repornirea calculatorului»
<!-- slice: 0097 -->

Bifa aceasta, sub parolă, apare doar cu [opțiunile avansate](topic:avansat) pornite (și dacă nu a
fost ascunsă din Setări › Autentificare).

- Bifată: parola se păstrează **criptată, pentru contul tău Windows**, până la repornirea
  calculatorului sau ieșirea din Windows. Până atunci, la pornire, fereastra de conectare vine cu
  utilizatorul și parola deja completate: apeși doar «Continuă».
- Debifată la conectare: parola memorată se șterge.
- Fără această bifă, parola **nu se păstrează** niciodată pe calculator.
