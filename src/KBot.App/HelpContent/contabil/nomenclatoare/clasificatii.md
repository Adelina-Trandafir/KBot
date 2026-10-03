---
id: contabil.nomenclatoare.clasificatii
title: Clasificații bugetare
part: contabil
order: 10
parent: contabil.nomenclatoare
screens: ClasificatiiForm, ClasificatiiAddForm, BudgetCheckForm
keywords: clasificatii, clasificatie bugetara, indicatori, buget, trimestre, rectificari, capitol, subcapitol, articol, alineat, verificare buget, credit bugetar, diferenta, trimite in access
open: menu:clasificatii
---
<!-- slice: 0087, 0075-00, 0102, 0103-04, 0103-06, 0105, 0107-02 -->
<!-- capture: clasificatii | caption: Fereastra «Clasificații bugetare» | goto: menu:clasificatii | prepare: Alegeți în arbore un alineat care are buget și rectificări. | redo: 2026-10-02 18:00 | why: 0103-04 / 0103-06: două butoane noi în subsol, «Verifică bugetul» și «Trimite în Access» -->

- **Arborele** din stânga: Capitol › Subcapitol › Articol › Alineat, cu denumirile alături. Implicit arată doar clasificațiile cu **mișcare** în anul de lucru: cele care au vreun trimestru de buget sau de rectificare diferit de zero. Bifează **«Arată toate clasificațiile»**, de sub arbore, ca să le vezi pe toate cele configurate.
- În dreapta, pentru clasificația aleasă:
  - **bugetul** anului, pe **versiuni**: fiecare rând este bugetul de la data din coloana
    **«Început»** încolo, pe trimestre (Trim. 1–4), cu **totalul** rândului în ultima coloană. Sub grilă nu
    există un total general, pentru că versiunile unui buget nu se adună între ele. Semnul **«+»** din josul grilei adaugă o versiune, **«✕»** o șterge;
  - **rectificările**: numărul și data documentului, plus trimestrul (sau trimestrele) pe care le
    schimbă. Ele nu sunt buget, dar îl influențează. Semnul **«+»** din josul grilei adaugă o
    rectificare, completată direct în tabel. Rândul de jos însumează rectificările clasificației alese.
- Dacă alegi un nod **mai sus de alineat** (capitol, subcapitol sau articol), tabelele din dreapta arată doar un sumar, fără editare: **Clsf** (coloana care se întinde), **ultimul buget** al fiecărei clasificații de sub nod și, la rectificări, **totalul** pe fiecare clasificație. Nu apar «Început», «Nr. doc.», «Data», «✕» și «+». În aceste sumare, ca și când nu e ales nimic, nu există rând de total jos: rectificările tuturor clasificațiilor la un loc nu înseamnă nimic.
- **Salvează** scrie versiunile de buget și rectificările. Dacă treci pe alt nod sau închizi
  fereastra cu modificări nesalvate, K-BOT te întreabă ce faci cu ele.
- **Verifică bugetul** compară bugetul din K-BOT cu cel din FOREXE (vezi mai jos).
- **Trimite în Access** scrie în baza Access a unității bugetul și rectificările clasificației alese
  (vezi mai jos).

> Data de început a unei versiuni și data unei rectificări trebuie să fie în anul de lucru. Două
> versiuni ale aceleiași clasificații nu pot începe în aceeași zi.

## Cum se folosește bugetul în documentul de fundamentare
<!-- slice: 0102, 0108 -->

Documentul de fundamentare arată la «Buget» ce buget avea clasificația **la data reviziei**, nu cel
de azi: **totalul** versiunii cu cea mai mare dată de început până la acea zi (cele patru trimestre
adunate), plus totalul rectificărilor de la data ei de început până la data reviziei. Trimestrul
zilei nu mai contează: o versiune care începe la 01.04 cu 100, 200, 300 și 400 dă 1.000 în orice zi
de la 01.04 încolo.

Ca să iasă corect o revizie din ianuarie – martie, adaugă pe clasificație o versiune care începe
la **01.01** a anului. Dacă la data reviziei nu există nicio versiune, documentul folosește creditul
bugetar de azi din FOREXE și te anunță pe ecran care clasificații sunt în cazul acesta.

## Clasificații noi
<!-- slice: 0087 -->

Semnul **«+»** din josul arborelui deschide fereastra de adăugare: bifezi sursele / sectoarele,
apoi clasificațiile funcționale și economice. Se pot alege doar surse / sectoare pe care unitatea
le are deja. Clasificațiile care există deja sunt sărite și numărate.

## Verificarea bugetului față de FOREXE
<!-- slice: 0103-04, 0105, 0108 -->

Butonul **«Verifică bugetul»** din josul ferestrei deschide o fereastră cu fiecare clasificație și
trei valori:

- **Buget K-BOT** – totalul versiunii în vigoare azi plus totalul rectificărilor ei (nu se mai oprește la trimestrul curent);
- **Credit FOREXE** – creditul bugetar al clasificației, așa cum l-a raportat FOREXE la ultima descărcare. Este o singură valoare pe clasificație, la fel pentru toate angajamentele care o folosesc;
- **Diferență** – FOREXE minus K-BOT.

Se văd doar clasificațiile la care diferența nu este zero. Un câmp gol înseamnă că nu există buget K-BOT în vigoare azi,
respectiv că FOREXE nu a raportat încă un credit pentru clasificația respectivă.

La sfârșitul unei descărcări de angajament K-BOT face singur aceeași verificare pentru clasificațiile
angajamentului și deschide fereastra **doar dacă** găsește diferențe. Dacă totul se potrivește, nu
apare nimic.

## Trimite în Access
<!-- slice: 0103-06 -->

Butonul **«Trimite în Access»** (activ doar după ce ai salvat, cu o clasificație aleasă) scrie în
baza Access a unității:

- bugetul versiunii în vigoare azi (trimestrele 1–4);
- rectificările anului: cele care există deja în Access (același document și aceeași dată) sunt
  actualizate, celelalte sunt adăugate.

K-BOT te întreabă înainte. O rectificare ștearsă în K-BOT **nu** se șterge din Access. Dacă fișierul
Access al unității nu se găsește sau este deschis exclusiv, primești un mesaj cu motivul.
