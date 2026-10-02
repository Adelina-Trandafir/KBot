---
id: contabil.forexe.descarcare-multipla
title: Mai multe descărcări deodată
part: contabil
order: 35
parent: contabil.forexe
screens: ActualizareMultiplaForm
keywords: descarcare multipla, actualizare multipla, mai multe angajamente, deodata, in acelasi timp, taburi, fire, thread, multithread, actualizeaza angajamente, la conectare, neactualizate, angajamente vechi, angajamente noi, actualizeaza implicit toate receptiile, data actualizarii, pagina descarcari multiple, nu apare pagina, permis, limita de taburi
---
<!-- slice: 0100, 0100-02 -->
Implicit, K-BOT descarcă **un singur angajament odată**, în ordinea în care ai cerut. Dacă ai de adus
la zi mai multe, poți pune K-BOT să le descarce **deodată**: fiecare lucrează pe un tab al lui din
browserul FOREXE, în aceeași conectare, fără altă autentificare.

Această variantă e **oprită la început**. O pornești din **Setări › Descărcări multiple**, iar pagina aceea
există **doar dacă descărcarea pe mai multe taburi e permisă pentru unitatea ta**.

## Cum o pornești
<!-- slice: 0100, 0100-02 -->

Deschide **Setări** (butonul **MENIU** › «Configurare K-BOT») și alege pagina **Descărcări multiple**.

> **Nu vezi pagina în listă?** Atunci descărcarea pe mai multe taburi nu e permisă pentru unitatea la care
> ești conectat: toate descărcările merg una câte una. Cere celui care administrează K-BOT să o permită.
> Nu trebuie [opțiunile avansate](topic:avansat). Permisiunea se citește la conectare și când schimbi unitatea.

<!-- capture: setari-descarcari-multiple | caption: Pagina «Descărcări multiple» din Setări | goto: setari:multithread | prepare: Unitatea trebuie să aibă descărcarea pe mai multe taburi permisă (pagina apare în lista Setărilor), cu limita de taburi peste 1. Bifați prima opțiune și bifați și «La conectare, actualizează angajamentele vechi», ca să se vadă și câmpul cu zilele. | redo: 2026-10-01 23:30 | why: 0100-02: grupul nu mai e în «Aplicație»; e pagina separată «Descărcări multiple», vizibilă doar când unitatea o permite. -->

| Opțiunea | Ce face |
|----------|---------|
| **«Descarcă mai multe angajamente deodată»** — debifată la început | pornește descărcarea pe mai multe taburi și aduce în meniul arborelui rândul «Actualizează angajamente...». Debifată, totul merge una câte una, ca până acum. |
| **«Numărul de taburi deodată (1–N)»** — 3 la început, dar nu mai mult decât îngăduie unitatea | câte descărcări lucrează în același timp. **N**, scris în chiar numele opțiunii, e limita unității (cel mult 10). Un număr mai mare nu se primește. E o limită a lucrului deodată, nu a angajamentelor alese: cele în plus așteaptă la rând (vezi mai jos). |
| **«La conectare, actualizează angajamentele vechi»** — debifată la început | după conectarea la FOREXE, angajamentele neactualizate de cel puțin **N zile** se descarcă singure. |
| **«Neactualizate de (zile, cel mult 10)»** — 7 la început | N-ul de mai sus, de la 1 la 10. Se poate scrie doar cât e bifată opțiunea de dinainte. |
| **«Actualizează implicit toate recepțiile»** — debifată la început | nu se mai deschide fereastra de alegere a recepțiilor; se citesc toate (vezi mai jos). |

Opțiunile de sub prima se pot schimba doar cât prima e bifată. Se salvează pe măsură ce le schimbi.
Dacă la o conectare următoare unitatea nu mai permite descărcarea pe mai multe taburi, pagina dispare din
Setări, iar descărcările merg din nou una câte una; bifele tale rămân salvate pentru când se permite din nou.

## Alegi angajamentele de actualizat
<!-- slice: 0100 -->

Cu descărcarea pe mai multe taburi pornită, meniul iconiței din capul listei de angajamente (cea
de la sortare și coloane) are un rând în plus: **«Actualizează angajamente...»**.

<!-- capture: arbore-meniu-actualizare | caption: Meniul listei de angajamente, cu «Actualizează angajamente...» | prepare: Porniți descărcarea pe mai multe taburi în Setări › Descărcări multiple, apoi apăsați iconița din dreapta capului listei de angajamente și țineți meniul deschis. -->

Se deschide fereastra **«Actualizează angajamente»**, cu toate angajamentele din listă:

<!-- capture: actualizare-multipla | caption: Fereastra «Actualizează angajamente» | prepare: Porniți descărcarea pe mai multe taburi (Setări › Descărcări multiple), deschideți meniul listei și alegeți «Actualizează angajamente...». Bifați câteva rânduri. -->

| Coloana | Ce arată |
|---------|----------|
| **S** | bifa: angajamentul se actualizează. Un clic pe capul coloanei bifează sau debifează tot. |
| **Cod**, **Descriere**, **Stare** | angajamentul, așa cum e în K-BOT |
| **Actualizat** | când s-a salvat ultima descărcare; «—» dacă nu s-a salvat niciuna de când există această dată |

Rândurile sunt în **aceeași ordine ca în arbore**. Butonul **«Bifează cele neactualizate de N zile»**
bifează doar angajamentele descărcate deja, dar neactualizate de N zile sau mai mult (N e din
Setări). **«Actualizează»** pornește descărcarea celor bifate; **«Renunță»** închide fereastra fără
să descarce nimic. Rândul de jos spune câte sunt bifate.

Data ultimei actualizări se vede și în **descrierea de la trecerea mouse-ului** peste un angajament
(«Actualizat: ...»).

## Cum decurge descărcarea
<!-- slice: 0100 -->

1. K-BOT pornește cel mult atâtea descărcări câte taburi ai cerut. Dacă ai ales mai multe, **restul
   așteaptă la rând**, în ordinea alegerii: pe măsură ce un tab termină, ia următorul angajament din
   rând.
2. O descărcare care se blochează sau eșuează **nu le oprește pe celelalte**: pe ea o vei vedea în
   lista de la sfârșit, iar tabul ei se închide.
3. **Nimic nu se salvează cât încă se descarcă.** Abia după ce s-au terminat toate, K-BOT salvează
   rezultatele **una după alta**, fiecare ca la o descărcare obișnuită — dacă un angajament are
   instantanee de așezat, se deschide pentru el fereastra [Asocieri](topic:contabil.asocieri), apoi
   se trece la următorul.
4. La sfârșit rămâne deschis **un singur tab** FOREXE: cel al ultimei descărcări terminate fără
   eroare (dacă ultima a avut o eroare, cel dinainte ei, și tot așa).
5. O descărcare reușită nu spune nimic; dacă unele angajamente nu s-au actualizat, apare **o singură
   listă** cu ele și cu motivul.

Toată actualizarea e **o singură lucrare** în [Coada robotului](topic:contabil.forexe.coada).

Cât lucrează tabul **«Browser FOREXE»**, pagina e acoperită ca la orice lucrare a robotului: nu o
folosi până se termină.

## La conectare: angajamentele vechi
<!-- slice: 0100 -->

Cu **«La conectare, actualizează angajamentele vechi»** bifată, imediat după ce te-ai conectat la
FOREXE, K-BOT descarcă singur, deodată, angajamentele care:

- au fost descărcate deja (au indicatori sau istoric) și
- nu s-au actualizat de cel puțin N zile, sau au fost descărcate înainte să existe data actualizării.

**Nu se pune nicio întrebare**, iar recepțiile se citesc toate. Angajamentele care nu au fost
descărcate niciodată (doar cu codul și descrierea, aduse prin lista de angajamente) nu se iau.

## Angajamentele noi
<!-- slice: 0100 -->

Cu descărcarea pe mai multe taburi pornită, dacă [actualizarea listei de angajamente](topic:contabil.forexe.lista)
a adus angajamente **noi** în listă, K-BOT întreabă: **«Dorești actualizarea angajamentelor noi?»**

- **Da** — angajamentele noi se descarcă deodată, ca mai sus, cu toate recepțiile.
- **Nu** — rămân cu codul și descrierea, ca până acum; le descarci când vrei.

Cu descărcarea pe mai multe taburi oprită, întrebarea **nu apare**.

## Recepțiile: «Actualizează implicit toate recepțiile»
<!-- slice: 0100 -->

Înainte de descărcarea unui angajament, K-BOT deschide de obicei fereastra
[«Ce recepții reîmprospătez?»](topic:contabil.forexe.descarcare). Cu descărcarea pe mai multe
taburi pornită și **«Actualizează implicit toate recepțiile»** bifată:

- fereastra **nu se mai deschide** — se citesc **toate** recepțiile, pentru orice descărcare;
- nu se poate alege ce recepții se reîmprospătează.

Debifată, alegerea recepțiilor rămâne ca până acum: pentru angajamentele alese în fereastra
«Actualizează angajamente», fereastra se deschide **pe rând**, pentru fiecare, înainte să pornească
robotul. La actualizarea de la conectare și la cea a angajamentelor noi nu se întreabă niciodată.
