---
id: avansat.pagina
title: Pagina FOREXE: regulile de stil
part: avansat
order: 20
parent: avansat
screens: SetariPaginaView, RegulaPaginaForm
keywords: pagina forexe, css, stil, reguli, selector, meniu lateral, latime pagina, implicite
---
<!-- slice: 0073-01 -->
K-BOT poate schimba felul în care arată pagina FOREXE din browserul lui: ascunde meniul lateral
cât e deschis un angajament, lărgește pagina, readuce textul la mărimea normală. Aceste schimbări
sunt **reguli de stil (CSS)** și se țin în pagina **Pagina FOREXE** din Setări.

<!-- capture: avansat-pagina | caption: Setări › Pagina FOREXE | goto: setari:pagina -->

## Lista regulilor
<!-- slice: 0073-01 -->

Fiecare rând e o regulă:

| Câmp | Ce este |
|------|---------|
| **Activ** | bifat = regula se pune în pagină; debifat = rămâne în listă, dar nu se aplică |
| **Ce face** | descrierea regulii, pentru tine |
| **Selector** | ce element din pagină schimbă |
| **Pagina** | gol = orice pagină; altfel doar pagina cu adresa scrisă (întreagă, doar calea sau doar ultimul cuvânt, de exemplu `contract`) |
| **Stil** | ce se schimbă, de exemplu `display: none` sau `max-width: 90%` |

Rândul ales se editează în dreapta. **Nimic nu se aplică până la «Salvează și aplică»**, care
scrie regulile și le trimite în pagina FOREXE deschisă.

## Butoanele
<!-- slice: 0073-01 -->

- **Regulă nouă...** — deschide arborele elementelor paginii FOREXE deschise. Un clic pe un element
  îi pune selectorul și stilul în câmpuri și îl **încadrează în pagina FOREXE**, ca să vezi ce ai
  ales; dublu clic îl adaugă. Fără browser pornit, regula se scrie de mână.
- **Șterge** — scoate rândul selectat.
- **Implicite** — înlocuiește lista cu regulile K-BOT de la început (meniul lateral ascuns cât e
  un angajament deschis, conținutul pe toată lățimea, pagina la 90 %, textul la 100 %, butonul
  «Înapoi» din bara de file ascuns pe pagina angajamentului).

<!-- capture: avansat-regula-noua | caption: Fereastra «Regulă nouă pentru pagina FOREXE» | goto: setari:pagina | prepare: Conectați-vă la FOREXE, deschideți un angajament în browser, apoi apăsați «Regulă nouă...». -->

> Regulile rămân pornite și cât lucrează robotul. Dacă o regulă ascunde ceva pe care robotul
> trebuie să-l apese, robotul o ridică doar pentru acel clic. Totuși, o regulă care ascunde
> butoane importante poate încurca operatorul: verifică pagina după «Salvează și aplică».

## Capturile pentru documente
<!-- slice: 0081-05 -->

Robotul face singur capturile de ecran cerute de ghidul Ministerului Finanțelor pentru
documentele de fundamentare și ordonanțări. În pagina **FOREXE** din Setări alegi dacă aceste
capturi arată **pagina originală** (fără regulile tale de stil, ca exemplele din ghid — implicit)
sau **pagina așa cum o vezi**. Meniul K-BOT din pagină și modul întunecat nu apar niciodată în poză.
