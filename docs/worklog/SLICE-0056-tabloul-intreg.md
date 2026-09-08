# SLICE 0056 — tabloul întreg al propunerii: ancora recepțiilor noi și contextul care lipsea

## De ce

Prima salvare adevărată a contractului în două faze, pe calculatorul cu FOREXE, pe 08.09.2026.
Descărcarea a mers, propunerea a venit, formularul s-a deschis, operatorul a așezat — și
salvarea a fost refuzată:

```
2026-09-08 06:37:26,801 - WARNING - PRELUCRARE dc=000_DEMO cod=AAB2MAACHXB
    decizii respinse: Recepția 188 nu există pe acest angajament.
```

Operatorul a renunțat («am apăsat pe renunță»), a deschis editorul de oricând ca să se uite —
ultimele trei rânduri din jurnal, `sumar` / `receptii` / `asociere` — și a spus ce a văzut:

> receptiile vechi NU mai vin in macheta de asociere! ele trebuie sa vina TOATE impreuna cu
> cele noi - nu doar cele noi. si se vor salva DOAR cele modificate sau noi - NU TOATE din nou!

**Sunt două defecte, și amândouă sunt ale tabloului pe care îl trimite faza întâi.** Al doilea
îl explică pe primul: dacă pe ecran nu se vede decât ce a adus descărcarea asta, atunci fiecare
tragere a operatorului a căzut, prin construcție, pe o recepție nou-nouță — adică exact pe
categoria de recepții al cărei număr nu supraviețuiește până la salvare.

---

## Defectul 1 — `IDRR`-ul din propunere nu e un nume

### Ce se întâmplă

`step4b_receptii_prelucrare` inserează în `FX_Receptii_R` recepțiile din `ListaReceptii` care nu
se potrivesc cu niciun rând existent, și ia `cursor.lastrowid` ca `IDRR`. Faza «propunere» face
asta **înăuntrul tranzacției pe care o derulează apoi înapoi, necondiționat** — asta e chiar
contractul. Dar:

> **Contorul `AUTO_INCREMENT` al InnoDB NU se derulează înapoi cu tranzacția.**

Deci recepția care în propunere s-a numit 188 se naște, la salvare, cu alt număr. Decizia care o
numea prin `idrr` cade la `_tinta`, cu mesajul de mai sus. Și cade **de fiecare dată** când
descărcarea aduce o recepție nouă — nu e un caz-limită, e drumul obișnuit al unei prime
descărcări.

Fișierul își spunea singur regula, la instantanee, și nu o aplicase și la recepții:

> `citeste_instantanee`: «id-urile atribuite in timpul propunerii dispar la rollback si nu se
> intorc identice, dar indicele e stabil prin constructie, fiindca AMBELE faze poarta acelasi
> payload.»

Instantaneele erau ancorate pe `rand_istoric`, indicele rândului în `TabelIstoric` (F24).
Recepțiile — nu. Recepțiile *reconstituite* aveau deja tratamentul corect (eticheta
`receptie_noua`, fiindcă nu există încă); recepțiile *descărcate* au căzut între cele două
cazuri.

### Ce s-a schimbat

**O a treia țintă a unei decizii: `rand_receptie`** — indicele rândului în `ListaReceptii`.
Același raționament ca F24, aplicat celeilalte jumătăți a problemei.

| recepția | numele ei pe fir | de ce |
|---|---|---|
| exista dinaintea rulării | `idrr` | cheie reală, nu se mișcă între faze |
| **născută de rularea asta** | **`rand_receptie`** | `IDRR`-ul ei moare la derularea înapoi |
| reconstituită (F26) | `receptie_noua` | nu există nicăieri încă |

Serverul cere **exact una** dintre cele trei pentru `asociat` / `stergere`, niciuna pentru
`ignorat`, doar eticheta pentru `reconstituire` — 400 la orice altă combinație, ca înainte.

`step4b_receptii_prelucrare` întoarce acum și **ancorele**: `{indice în ListaReceptii → IDRR}`,
doar pentru rândurile a căror recepție s-a născut în rularea curentă. «Născută acum» se măsoară
**pe IDRR, nu pe ramura care a rulat**: două rânduri de payload din aceeași zi se potrivesc pe
același rând, deci al doilea poate ajunge, prin potrivire, pe o recepție pe care primul tocmai a
inserat-o — și `IDRR`-ul ei e la fel de trecător. E scris ca test.

Clientul nu alege: `AsociereForm.DeciziiDin` primește acum **și recepțiile** (parametru
OBLIGATORIU, nu opțional — aici s-a greșit o dată, iar un implicit tăcut ar reînvia greșeala) și
pune `RandReceptie` dacă recepția are unul, `Idrr` altfel. Niciodată amândouă.

---

## Defectul 2 — propunerea arăta doar jumătate din tablou

### Ce se întâmplă

`_INSTANTANEE_SQL` din calea de ingestie selectează `IDRR IS NULL AND Sters = 0`: **doar
rândurile de așezat**. Recepțiile veneau toate (`FROM FX_Receptii_R WHERE CodAngajament = %s`,
fără filtru) — dar goale, fiindcă lanțul lor era deja legat și deci absent din tablou.

Ce vede operatorul dintr-o recepție cu lanț de lungime zero:

* în **grafic** — nimic: `ConstruiesteGraficAngajament` are `If lant.Count = 0 Then Continue For`,
  iar graficul e **vederea implicită** a formularului;
* pe **bandă** — o bandă goală, fără marcaje;
* în **arbore** — un rând fără nimic sub el.

Adică exact «recepțiile vechi nu mai vin». Erau acolo; povestea lor nu era.

**Și nu e doar aspect.** Serverul își dă deja vetourile F15 / F16 **pe lanțul întreg** —
`aplica_decizii` citește din bază instantaneele deja asociate ale acelorași recepții înainte de
`valideaza_plasarile`, tocmai fiindcă «lanțul» înseamnă tot lanțul. Până la felia asta,
formularul era singurul care nu vedea ce vede vetoul care îl refuză.

### Ce s-a schimbat

Propunerea poartă acum, pe lângă rândurile de decis:

* **`instantanee_asezate`** — restul instantaneelor angajamentului, în aceeași formă ca cele ale
  editorului de oricând (aceleași DTO-uri, ca să nu existe două conversii care alunecă una față
  de alta);
* **`plati`** — plățile angajamentului. §1.3 din fundament: fiecare ordonanțare citește totalul
  recepției **așa cum stătea la data plății**, deci partea pe care cade un instantaneu față de o
  plată e diferența dintre o cifră corectă și una greșită, tăcut și pentru totdeauna (F12).
  Reperele trebuie să fie pe ecran **când** se așază, nu abia după.

**Complement, nu al doilea filtru.** `citeste_instantanee_context` întoarce tot ce NU e în
mulțimea de decis, calculat pe `IDRH` în Python. Așa nu poate exista un rând pe care să nu-l ia
niciuna dintre cele două liste — nici măcar cel lăsat afară fiindcă rândul lui de istoric nu e în
descărcarea asta (până acum dispărea de pe ecran cu totul, rămânând doar într-un avertisment).

**Se arată, nu se mișcă.** Toate ies cu `blocat = True`. Nu e o alegere de aspect: acoperirea
cerută de `verifica_acoperirea` e exact mulțimea de decis, iar o decizie pentru un rând din afara
ei e respinsă cu 400 — și pe drept, altfel fiecare descărcare ar rescrie tăcut legături vechi.
Asta e chiar a doua jumătate a cerinței operatorului: **«se vor salva DOAR cele modificate sau noi
— NU TOATE din nou!»** Corectarea unei legături vechi rămâne treaba editorului de oricând, care e
la un clic distanță și există exact pentru asta.

Motivul afișat spune care dintre cele două cazuri e:

| rândul | motivul, dacă nu e blocat de o ordonanțare sau de plăți |
|---|---|
| are legătură (`IDRR`) | «Legătura este deja scrisă. Se corectează în editorul de asociere…» |
| n-are (`IDRR` gol) | «Rândul lui de istoric nu este în această descărcare…» |

**Formularul n-a avut nevoie de cod nou ca să le arate cum trebuie.** `Blocat` era deja onorat în
toate cele șase locuri: lacătul din arbore, culoarea stinsă, stilul marcajului de pe bandă,
pornirea tragerii din arbore, pornirea tragerii de pe bandă și meniul contextual.

**Chei negative, ca să nu se ciocnească două numerotări.** Un rând de decis e ancorat pe indicele
lui (0, 1, 2…), unul de context pe `IDRH`, cheia reală. Amândouă ajung în `_pozitie` și în
dicționarele de rânduri ale formularului, unde `IDRH = 5` și indicele `5` ar fi același lucru.
`AsociereStare.DinPropunere` păstrează deci contextul ca `-IDRH`: negarea e reversibilă (`IDRH`
pornește de la 1, e cheie primară Access), nu poate atinge un indice, și spune dintr-o privire că
rândul nu poartă o hotărâre.

Două locuri au trebuit să învețe să sară peste ele:

* `DeciziiDin` — altfel ar fi trimis decizii respinse cu 400;
* `NehotarateleCount` — altfel un singur rând de context neașezat ar fi stins butonul de salvare
  **pentru totdeauna**, fiindcă nimic nu-l poate așeza.

---

## Fișiere atinse

| fișier | ce |
|---|---|
| `PYTHON/routes/forexe/prelucrare_pasi.py` | pasul 4b întoarce ancorele |
| `PYTHON/routes/forexe/prelucrare_asociere.py` | `rand_receptie` în decizii și în recepții; `_tinta` îl rezolvă; `citeste_instantanee_context` |
| `PYTHON/routes/forexe/prelucrare.py` | firul ancorelor; contextul și plățile în corpul propunerii |
| `PYTHON/routes/forexe/asociere.py` | `citeste_plati` scoasă din rută ca funcție publică (o folosesc amândouă căile) |
| `src/KBot.Domain/AsociereInfo.vb` | `ReceptiePropusa.RandReceptie`, `DecizieAsociere.RandReceptie`, `PrelucrarePropunere.InstantaneeAsezate` / `.Plati` |
| `src/KBot.Domain/AsociereStare.vb` | `DinPropunere` aduce contextul cu chei negative și plățile |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | câmpurile noi de pe fir |
| `src/KBot.Api/ApiClient.vb` | `CitesteReceptie` / `CitesteInstantaneuLegat` / `CitestePlata` — cititori comuni celor două rute; `CatreFir` pune a treia țintă |
| `src/KBot.App/Forexe/AsociereForm.vb` | `DeciziiDin` primește recepțiile și sare peste context; `NehotarateleCount` la fel |

## Verde

* `PYTHON`: **485 trecute / 15 sărite** (linia de plecare a feliei: 472 / 15). Cele **13 teste
  noi** acoperă ancorele pasului 4b (inclusiv rândul care aterizează pe recepția născută de
  rândul dinaintea lui), forma cu trei ținte a unei decizii, rezolvarea prin ancoră în
  `aplica_decizii`, refuzul zgomotos al unui indice care n-a creat nimic, faptul că un `idrr`
  inexistent cade în continuare la fel, și complementul din `citeste_instantanee_context` cu
  ambele motive.
* `dotnet build src\KBot.App\KBot.App.vbproj` — **0 erori**. Cele 7 avertismente sunt `MSB3825`
  pe `.resx`-uri neatinse de felia asta (`DdfEditDescrierePage`, `AsociereForm`, `DdfView`,
  `OrdView`, `PlatiView`, `ReceptiiView`, `RezervariView`), preexistente și raportate ca atare și
  în feliile 0049 / 0054.
* `KBot.App.Tests` — **222 trecute / 13 picate / 235**. Linia de plecare era 217 / 13 / 230:
  aceleași 13 roșii, aceleași nume (DDF / Istoric / XFA / MainFormNavItems), plus **5 teste noi**
  verzi.
* `KBot.Api.Tests` **94 / 95** și `KBot.Domain.Tests` **28 / 31** — **identice** cu linia de
  plecare măsurată în felia 0055, aceleași roșii de `EtichetaRevizie` (DDF), pe care felia asta
  nu le atinge.

## Ce NU e verificat

**Nimic din felia asta n-a rulat pe calculatorul cu FOREXE.** Aici nu există nici aplicația
FOREXE, nici baza MariaDB — dovada rămâne că data viitoare salvarea trece, nu că testele sunt
verzi.

Anume:

* niciun `POST /api/forexe/prelucrare` real n-a purtat `rand_receptie`;
* contextul n-a fost văzut pe ecran: lacătele, benzile pline, liniile vechi din grafic și
  reperele de plată în timpul ingestiei sunt deduse din cod, nu privite;
* `AUTO_INCREMENT` care nu se derulează înapoi este **comportamentul documentat al InnoDB** și e
  singura explicație care se potrivește cu toate cele trei dovezi (mesajul rutei, faptul că
  recepția 188 nu se afla în `citeste_receptii` la salvare, și faptul că propunerea o listase) —
  dar n-a fost observat direct pe baza operatorului.

## Ce rămâne deschis

* **Reconstituirea (`receptie_noua`) tot nu se poate cere din formular.** Un lanț F26 rămâne
  neasezabil în modul propunere. Nimic din datele operatorului nu pare să ceară asta acum (niciun
  lanț nu se termină cu o recepție dispărută din `ListaReceptii`), dar rămâne o gaură cunoscută.
* **Mutarea unei legături vechi în timpul ingestiei** — deliberat nu se poate; e treaba editorului
  de oricând. Dacă operatorul o cere acolo, e o felie separată, fiindcă schimbă contractul de
  acoperire al rutei.
* Antetul benzii «Neașezate (N)» numără și rândurile de context neașezate, în timp ce notificarea
  numără doar pe cele de hotărât. Cele două cifre pot să difere; lacătul explică de ce, dar nu e
  aceeași cifră.
