---
id: contabil.fereastra
title: Fereastra principală
part: contabil
order: 20
parent: contabil
screens: KbotForm.tree, KbotForm.navViews, KbotForm.btnMeniu, KbotForm.cboAn, KbotForm.cboSs, KbotForm.btnInfo, InternalInfoForm
keywords: arbore, lista angajamente, meniu, vederi, bara de jos, cautare
---
Fereastra principală are patru zone.

<!-- capture: fereastra-zone | caption: Zonele ferestrei principale | goto: view:sumar | prepare: Selectați un angajament cu date, ca toate zonele să fie pline. -->

| Zona | Ce este |
|------|---------|
| **Sus** | butonul **MENIU**, anul, sursa/sectorul și utilizatorul conectat |
| **Stânga** | bara de **vederi** (Sumar, Istoric, Rezervări...) |
| **Mijloc** | **Lista angajamentelor** — alegi aici angajamentul pe care lucrezi |
| **Dreapta** | vederea aleasă, pentru angajamentul selectat |
| **Jos** | banda **FOREXE**: conectarea, progresul și ultimul mesaj al robotului |

## Lista angajamentelor

- Clic pe un angajament îl selectează; vederea din dreapta se umple cu datele lui.
- **Lupa** din capul listei deschide căutarea; **Esc** o golește și o închide.
- **Rotița** din capul listei alege sortarea și ce coloane se văd (cod, surse, dată).
- Iconița **din dreapta, jos** actualizează lista din FOREXE: angajamentele noi se adaugă, cele
  existente rămân cum sunt — [Lista de angajamente](topic:contabil.forexe.lista).
- Iconița **din stânga, jos** deschide fereastra **Extrase de cont** — [Extrase](topic:contabil.vederi.extrase).

## Vederile

O vedere e disponibilă doar dacă angajamentul are date de acel fel (de exemplu «Plăți» e gri
cât timp angajamentul nu are plăți). «Browser FOREXE» apare doar cât timp ești conectat la FOREXE.

## MENIU

- **Angajament nou** — [Angajament nou](topic:contabil.ddf.nou)
- **(!) Operațiuni necorelate** — apare doar când există — [Operațiuni necorelate](topic:contabil.notecab)
- **Extrase** — fereastra «Extrase de cont»
- **Nomenclatoare › Clasificații bugetare / Parteneri** — [Nomenclatoare](topic:contabil.nomenclatoare)

Când există operațiuni necorelate, butonul MENIU poartă semnul **(!)**.

## Butonul ⓘ

Butonul **ⓘ** din capul listei de angajamente deschide **«Informații interne»**: toate datele pe care
K-BOT le ține despre angajamentul selectat. Fereastra rămâne deschisă cât lucrezi și se schimbă
singură la fiecare angajament ales. E utilă mai ales când ceri ajutor: arată exact ce vede K-BOT.
