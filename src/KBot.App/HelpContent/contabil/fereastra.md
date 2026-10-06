---
id: contabil.fereastra
title: Fereastra principală
part: contabil
order: 20
parent: contabil
screens: KbotForm.tree, KbotForm.capBar, KbotForm.navViews, KbotForm.btnMeniu, KbotForm.btnInfo, InternalInfoForm
keywords: arbore, lista angajamente, meniu, vederi, bara de jos, cautare, unitate, schimba unitatea, alta unitate, adaugare angajamente, fereastra marita, tot ecranul, maximizat, la pornire
---
<!-- slice: 0006, 0086, 0097-03 -->
Fereastra principală are cinci zone.

<!-- capture: fereastra-zone | caption: Zonele ferestrei principale | goto: view:sumar | prepare: Selectați un angajament cu date, ca toate zonele să fie pline. -->

| Zona | Ce este |
|------|---------|
| **Sus** | bara de titlu cu **unitatea** (sau alegerea ei), **anul** și **sursa/sectorul**, apoi butonul **MENIU** și utilizatorul conectat |
| **Stânga** | bara de **vederi** (Sumar, Istoric, Rezervări...) |
| **Mijloc** | **Lista angajamentelor** — alegi aici angajamentul pe care lucrezi |
| **Dreapta** | vederea aleasă, pentru angajamentul selectat |
| **Jos** | banda **FOREXE**: conectarea, progresul și ultimul mesaj al robotului |

## Lista angajamentelor
<!-- slice: 0009, 0777, 0777-02, 0034, 0080-03, 0095-02, 0078-08, 0098, 0000-31, 0101, 0109 -->

- Clic pe un angajament îl selectează; vederea din dreapta se umple cu datele lui. Cât Adobe încă
  deschide un document (în Fundamentare, Ordonanțare sau Note corecție), lista nu primește alt
  angajament — [Cât se deschide un document](topic:contabil.liste).
- **Lupa** din capul listei deschide căutarea; **Esc** o golește și o închide. Toate butoanele comune
  ale arborilor și tabelelor: [Arborii și tabelele](topic:contabil.liste).
- **Rotița** din capul listei alege sortarea și ce coloane se văd (cod, surse, dată). Meniul ei are
  și rândul **«Actualizează angajamente...»**: bifezi angajamentele de adus la zi din FOREXE. Cu
  descărcarea pe mai multe taburi pornită se descarcă deodată, altfel intră în coadă, unul după altul
  — [Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla).
- Când lista e sortată după **data creării**, ea cuprinde toate sursele anului, așa că alegerea **Sursă/Sector** din bara de titlu se ascunde; reapare la sortarea după nume.
- Iconița **din dreapta, jos** actualizează lista din FOREXE: angajamentele noi se adaugă, cele
  existente rămân cum sunt — [Lista de angajamente](topic:contabil.forexe.lista).
- Iconița **din stânga, jos** deschide fereastra **Extrase de cont** — [Extrase](topic:contabil.vederi.extrase).
- Un angajament scris cu **roșu** are recepții al căror lanț **nu se închide**: ultimul instantaneu al recepției nu are valoarea ei. Ținând mouse-ul pe rând vezi, pentru fiecare recepție, data, valoarea ultimului instantaneu și valoarea recepției. Se repară în fereastra de asociere — [Lanțul nu se închide](topic:contabil.asocieri.cazuri). Un lanț care se termină cu rândul de ștergere nu se socotește.
- Angajamentele descărcate sau actualizate de la pornirea programului sunt **subliniate** în listă. Sublinierea dispare când închideți programul.
- Dacă apăsați reîmprospătarea pe mai multe angajamente cât timp se descarcă deja unul, pentru ele nu se mai deschide fereastra de alegere a recepțiilor: se descarcă toate.
- Când mai multe acțiuni FOREXE așteaptă una după alta, se deschide fereastra **Coada robotului**; ea se închide singură după ce coada s-a golit. Butonul **«Coadă N»** din banda FOREXE o deschide oricând, dar se vede doar cât coada are ceva în ea — [Coada robotului](topic:contabil.forexe.coada).

## Vederile
<!-- slice: 0018, 0074, 0088, 0097, 0000-30, 0000-31 -->

O vedere apare doar dacă angajamentul selectat are date de acel fel; altfel butonul ei **nu se vede** în bara vederilor (nu e gri, lipsește). Când nu ai ales niciun angajament, se văd doar «Sumar» și, cât ești conectat la FOREXE, «Browser FOREXE». Ce trebuie să fie adevărat ca să apară fiecare:

| Vederea | Apare când |
|---------|------------|
| Sumar | mereu; lipsește doar cât lucrezi în formularul gol «Angajament nou» din pagina FOREXE |
| Istoric | angajamentul are rânduri de istoric |
| Rezervări | angajamentul are rezervări |
| Recepții | angajamentul are recepții |
| Plăți | angajamentul are plăți |
| Extrase | angajamentul are operațiuni în extrasele de cont |
| Browser FOREXE | ești conectat la FOREXE (butonul «Conectare» din banda de jos) |
| Fundamentare | angajamentul are document de fundamentare (se face din «Rezervări», cu semnul «+») |
| Ordonanțare | angajamentul are ordonanțări (se fac din «Plăți», cu semnul «+») |
| Note corecție | s-a făcut cel puțin o notă de corecție pe angajament |

Alege în listă alt angajament sau rezolvă condiția și butonul apare singur. Turul ghidat al ferestrei (din «?») îți arată și vederile care lipsesc acum, cu o notă care spune că le vezi doar pentru tur.

## Anul și sursa/sectorul
<!-- slice: 0097-03 -->

În **bara de titlu**, după unitate, sunt două liste: **An Date** (anul de lucru) și **Sursă/Sector**
(de exemplu 02A). Un clic pe ele arată alegerile; schimbarea oricăreia reîncarcă lista de
angajamente și toate ecranele. Sursa/sectorul se ține minte pentru data viitoare. Când lista de
angajamente e sortată după dată, **Sursă/Sector** nu se vede.

## Unitatea de lucru
<!-- slice: 0097 -->

Bara de titlu arată unitatea pe care lucrezi: «K-BOT — Numele unității».

Dacă ai acces la **două sau mai multe unități**, numele unității devine o listă: un clic pe el
arată unitățile tale, cu o bifă pe cea curentă. Alegi alta și K-BOT o deschide **fără să-ți
ceară din nou parola**, ca după o conectare: lista de angajamente, anul, sursa/sectorul și vederile
se reîncarcă pentru unitatea nouă, iar ea se ține minte pentru conectarea următoare (dacă în
Setări › Autentificare e bifată «Ține minte și unitatea aleasă»).

<!-- capture: unitate-selector | caption: Alegerea unității din bara de titlu | goto: view:sumar | prepare: Conectați-vă cu un utilizator care are acces la cel puțin două unități, apoi faceți clic pe numele unității din bara de titlu. -->

Dacă ești **conectat la FOREXE**, K-BOT închide singur conexiunea înainte să treacă pe unitatea
nouă: conexiunea aparține unității pe care o părăsești, iar ce descarcă robotul se scrie în baza
ei. Pe unitatea nouă apeși din nou «Conectare»; ea folosește certificatul memorat, afară de cazul
în care ai bifat în Setări › FOREXE «Uită certificatul memorat când schimb unitatea din bara de
titlu» — atunci ți-l cere din nou (vezi [Setări și aspect](topic:contabil.setari)).

Schimbarea **nu se poate face**:

- cât timp robotul **lucrează** (o descărcare sau o trimitere în curs) — aștepți să termine;
- spre o unitate pe care ai rolul **«Director»** — ea se deschide în fereastra de semnare, după o
  conectare nouă.

> Ferestrele deschise separat (Extrase de cont, Nomenclatoare…) nu se schimbă singure: închide-le
> și deschide-le din nou după ce ai trecut pe altă unitate.

## MENIU
<!-- slice: 0087, 0084, 0088, 0095-02, 0097-02, 0000-29, 0000-31, 00EF-05, 0000-54, fara-felie -->

- **Adăugare angajamente...** — un dosar cu două rânduri:
  - **Angajament nou** — îl faci în K-BOT și îl trimiți apoi în FOREXE — [Angajament nou](topic:contabil.ddf.nou);
  - **Creează angajament în FOREXE** — îl faci de mână, direct în pagina FOREXE — [Un angajament nou făcut direct în FOREXE](topic:contabil.forexe.browser).
- **(!) Operațiuni necorelate** — apare doar când există — [Operațiuni necorelate](topic:contabil.notecab)
- **Extrase** — fereastra «Extrase de cont»
- **E-Factura** — fereastra tokenului ANAF, pentru facturile electronice — [E-Factura și tokenul ANAF](topic:contabil.efactura)
- **Nomenclatoare › Clasificații bugetare / Parteneri** — [Nomenclatoare](topic:contabil.nomenclatoare)
- **Jurnal activitate** — jurnalele K-BOT; rândul lipsește dacă l-ai ascuns din Setări › Aplicație — [Setări și aspect](topic:contabil.setari)
- **Configurare K-BOT** — fereastra de setări — [Setări și aspect](topic:contabil.setari)

Când există operațiuni necorelate, butonul MENIU poartă semnul **(!)**.

## Cum pornește fereastra
<!-- slice: 0097-02 -->

Fereastra principală pornește la mărimea ei obișnuită, în mijlocul ecranului. Dacă o vrei
**mărită pe tot ecranul** de la pornire, bifează în **Setări › Aplicație** «Fereastra principală
pornește mărită (pe tot ecranul)» — [Setări și aspect](topic:contabil.setari).

La primele porniri, K-BOT îți arată singur **turul ferestrei principale** —
[Tururile ghidate](topic:contabil.ajutor).

## Butonul ⓘ
<!-- slice: fara-felie -->

Butonul **ⓘ** din capul listei de angajamente deschide **«Informații interne»**: toate datele pe care
K-BOT le ține despre angajamentul selectat. Fereastra rămâne deschisă cât lucrezi și se schimbă
singură la fiecare angajament ales. E utilă mai ales când ceri ajutor: arată exact ce vede K-BOT.
