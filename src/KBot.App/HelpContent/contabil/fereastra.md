---
id: contabil.fereastra
title: Fereastra principală
part: contabil
order: 20
parent: contabil
screens: KbotForm.tree, KbotForm.capBar, KbotForm.navViews, KbotForm.btnMeniu, KbotForm.cboAn, KbotForm.cboSs, KbotForm.btnInfo, InternalInfoForm
keywords: arbore, lista angajamente, meniu, vederi, bara de jos, cautare, unitate, schimba unitatea, alta unitate
---
<!-- slice: 0006, 0086 -->
Fereastra principală are cinci zone.

<!-- capture: fereastra-zone | caption: Zonele ferestrei principale | goto: view:sumar | prepare: Selectați un angajament cu date, ca toate zonele să fie pline. -->

| Zona | Ce este |
|------|---------|
| **Sus** | bara de titlu cu **unitatea** (sau alegerea ei), butonul **MENIU**, anul, sursa/sectorul și utilizatorul conectat |
| **Stânga** | bara de **vederi** (Sumar, Istoric, Rezervări...) |
| **Mijloc** | **Lista angajamentelor** — alegi aici angajamentul pe care lucrezi |
| **Dreapta** | vederea aleasă, pentru angajamentul selectat |
| **Jos** | banda **FOREXE**: conectarea, progresul și ultimul mesaj al robotului |

## Lista angajamentelor
<!-- slice: 0009, 0777, 0034, 0080-03, 0095-02 -->

- Clic pe un angajament îl selectează; vederea din dreapta se umple cu datele lui.
- **Lupa** din capul listei deschide căutarea; **Esc** o golește și o închide. Toate butoanele comune
  ale arborilor și tabelelor: [Arborii și tabelele](topic:contabil.liste).
- **Rotița** din capul listei alege sortarea și ce coloane se văd (cod, surse, dată).
- Iconița **din dreapta, jos** actualizează lista din FOREXE: angajamentele noi se adaugă, cele
  existente rămân cum sunt — [Lista de angajamente](topic:contabil.forexe.lista).
- Iconița **din stânga, jos** deschide fereastra **Extrase de cont** — [Extrase](topic:contabil.vederi.extrase).

## Vederile
<!-- slice: 0018, 0074, 0088, 0097 -->

O vedere e disponibilă doar dacă angajamentul are date de acel fel (de exemplu «Plăți» e gri
cât timp angajamentul nu are plăți, iar «Note corecție» cât timp nu s-a făcut nicio notă pe el). «Browser FOREXE» apare doar cât timp ești conectat la FOREXE.

## Unitatea de lucru
<!-- slice: 0097 -->

Bara de titlu arată unitatea pe care lucrezi: «K-BOT — Numele unității».

Dacă ai acces la **două sau mai multe unități**, numele unității devine o listă: un clic pe el
arată unitățile tale, cu o bifă pe cea curentă. Alegi alta și K-BOT o deschide **fără să-ți
ceară din nou parola**, ca după o conectare: lista de angajamente, anul, sursa/sectorul și vederile
se reîncarcă pentru unitatea nouă, iar ea se ține minte pentru conectarea următoare (dacă în
Setări › Autentificare e bifată «Ține minte și unitatea aleasă»).

<!-- capture: unitate-selector | caption: Alegerea unității din bara de titlu | goto: view:sumar | prepare: Conectați-vă cu un utilizator care are acces la cel puțin două unități, apoi faceți clic pe numele unității din bara de titlu. -->

Schimbarea **nu se poate face**:

- cât timp ești **conectat la FOREXE** — conexiunea aparține unității curente, iar ce descarcă
  robotul se scrie în baza ei. Schimbi unitatea înainte de «Conectare» sau după o repornire a
  K-BOT;
- spre o unitate pe care ai rolul **«Director»** — ea se deschide în fereastra de semnare, după o
  conectare nouă.

> Ferestrele deschise separat (Extrase de cont, Nomenclatoare…) nu se schimbă singure: închide-le
> și deschide-le din nou după ce ai trecut pe altă unitate.

## MENIU
<!-- slice: 0087, 0084, 0088, 0095-02 -->

- **Angajament nou** — [Angajament nou](topic:contabil.ddf.nou)
- **(!) Operațiuni necorelate** — apare doar când există — [Operațiuni necorelate](topic:contabil.notecab)
- **Extrase** — fereastra «Extrase de cont»
- **Nomenclatoare › Clasificații bugetare / Parteneri** — [Nomenclatoare](topic:contabil.nomenclatoare)

Când există operațiuni necorelate, butonul MENIU poartă semnul **(!)**.

## Butonul ⓘ
<!-- slice: fara-felie -->

Butonul **ⓘ** din capul listei de angajamente deschide **«Informații interne»**: toate datele pe care
K-BOT le ține despre angajamentul selectat. Fereastra rămâne deschisă cât lucrezi și se schimbă
singură la fiecare angajament ales. E utilă mai ales când ceri ajutor: arată exact ce vede K-BOT.
