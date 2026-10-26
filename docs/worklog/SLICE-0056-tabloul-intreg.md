# SLICE-0056 / 02 — Contextul propunerii și corectarea legăturilor vechi

Reunire documentară D001-27. Varianta inițială introduce rand_receptie și contextul blocat; 02 înlocuiește blocarea generală prin regula ordonanțării și admite corecții ale rândurilor vechi prin idrh, cu acoperire obligatorie doar pentru rândurile de așezat. F15 rămâne veto când se adaugă, avertisment când o recepție numai pierde instantanee. Ancorele, compatibilitatea, probele și limitele sunt păstrate pe etapă.

[0056-tabloul](#slice-0056-baza) · [0056-02](#slice-0056-02)

<a id="slice-0056-baza"></a>

## SLICE 0056 — tabloul întreg al propunerii: ancora recepțiilor noi și contextul care lipsea

### De ce

Prima salvare adevărată a contractului în două faze, pe calculatorul cu FOREXE, pe 08.09.2026. Descărcarea a mers, propunerea a venit, formularul s-a deschis, operatorul a așezat — și salvarea a fost refuzată:

```
2026-09-08 06:37:26,801 - WARNING - PRELUCRARE dc=000_DEMO cod=AAB2MAACHXB
    decizii respinse: Recepția 188 nu există pe acest angajament.
```

Operatorul a renunțat («am apăsat pe renunță»), a deschis editorul de oricând ca să se uite — ultimele trei rânduri din jurnal, `sumar` / `receptii` / `asociere` — și a spus ce a văzut:

> receptiile vechi NU mai vin in macheta de asociere! ele trebuie sa vina TOATE impreuna cu
> cele noi - nu doar cele noi. si se vor salva DOAR cele modificate sau noi - NU TOATE din nou!

**Sunt două defecte, și amândouă sunt ale tabloului pe care îl trimite faza întâi.** Al doilea îl explică pe primul: dacă pe ecran nu se vede decât ce a adus descărcarea asta, atunci fiecare tragere a operatorului a căzut, prin construcție, pe o recepție nou-nouță — adică exact pe categoria de recepții al cărei număr nu supraviețuiește până la salvare.

---

### Defectul 1 — `IDRR`-ul din propunere nu e un nume

#### Ce se întâmplă

`step4b_receptii_prelucrare` inserează în `FX_Receptii_R` recepțiile din `ListaReceptii` care nu se potrivesc cu niciun rând existent, și ia `cursor.lastrowid` ca `IDRR`. Faza «propunere» face asta **înăuntrul tranzacției pe care o derulează apoi înapoi, necondiționat** — asta e chiar contractul. Dar:

> **Contorul `AUTO_INCREMENT` al InnoDB NU se derulează înapoi cu tranzacția.**

Deci recepția care în propunere s-a numit 188 se naște, la salvare, cu alt număr. Decizia care o numea prin `idrr` cade la `_tinta`, cu mesajul de mai sus. Și cade **de fiecare dată** când descărcarea aduce o recepție nouă — nu e un caz-limită, e drumul obișnuit al unei prime descărcări.

Fișierul își spunea singur regula, la instantanee, și nu o aplicase și la recepții:

> `citeste_instantanee`: «id-urile atribuite in timpul propunerii dispar la rollback si nu se
> intorc identice, dar indicele e stabil prin constructie, fiindca AMBELE faze poarta acelasi
> payload.»

Instantaneele erau ancorate pe `rand_istoric`, indicele rândului în `TabelIstoric` (F24). Recepțiile — nu. Recepțiile *reconstituite* aveau deja tratamentul corect (eticheta `receptie_noua`, fiindcă nu există încă); recepțiile *descărcate* au căzut între cele două cazuri.

#### Ce s-a schimbat

**O a treia țintă a unei decizii: `rand_receptie`** — indicele rândului în `ListaReceptii`. Același raționament ca F24, aplicat celeilalte jumătăți a problemei.

| recepția | numele ei pe fir | de ce |
|---|---|---|
| exista dinaintea rulării | `idrr` | cheie reală, nu se mișcă între faze |
| **născută de rularea asta** | **`rand_receptie`** | `IDRR`-ul ei moare la derularea înapoi |
| reconstituită (F26) | `receptie_noua` | nu există nicăieri încă |

Serverul cere **exact una** dintre cele trei pentru `asociat` / `stergere`, niciuna pentru `ignorat`, doar eticheta pentru `reconstituire` — 400 la orice altă combinație, ca înainte.

`step4b_receptii_prelucrare` întoarce acum și **ancorele**: `{indice în ListaReceptii → IDRR}`, doar pentru rândurile a căror recepție s-a născut în rularea curentă. «Născută acum» se măsoară **pe IDRR, nu pe ramura care a rulat**: două rânduri de payload din aceeași zi se potrivesc pe același rând, deci al doilea poate ajunge, prin potrivire, pe o recepție pe care primul tocmai a inserat-o — și `IDRR`-ul ei e la fel de trecător. E scris ca test.

Clientul nu alege: `AsociereForm.DeciziiDin` primește acum **și recepțiile** (parametru OBLIGATORIU, nu opțional — aici s-a greșit o dată, iar un implicit tăcut ar reînvia greșeala) și pune `RandReceptie` dacă recepția are unul, `Idrr` altfel. Niciodată amândouă.

---

### Defectul 2 — propunerea arăta doar jumătate din tablou

#### Ce se întâmplă

`_INSTANTANEE_SQL` din calea de ingestie selectează `IDRR IS NULL AND Sters = 0`: **doar rândurile de așezat**. Recepțiile veneau toate (`FROM FX_Receptii_R WHERE CodAngajament = %s`, fără filtru) — dar goale, fiindcă lanțul lor era deja legat și deci absent din tablou.

Ce vede operatorul dintr-o recepție cu lanț de lungime zero:

* în **grafic** — nimic: `ConstruiesteGraficAngajament` are `If lant.Count = 0 Then Continue For`, iar graficul e **vederea implicită** a formularului;
* pe **bandă** — o bandă goală, fără marcaje;
* în **arbore** — un rând fără nimic sub el.

Adică exact «recepțiile vechi nu mai vin». Erau acolo; povestea lor nu era.

**Și nu e doar aspect.** Serverul își dă deja vetourile F15 / F16 **pe lanțul întreg** — `aplica_decizii` citește din bază instantaneele deja asociate ale acelorași recepții înainte de `valideaza_plasarile`, tocmai fiindcă «lanțul» înseamnă tot lanțul. Până la felia asta, formularul era singurul care nu vedea ce vede vetoul care îl refuză.

#### Ce s-a schimbat

Propunerea poartă acum, pe lângă rândurile de decis:

* **`instantanee_asezate`** — restul instantaneelor angajamentului, în aceeași formă ca cele ale editorului de oricând (aceleași DTO-uri, ca să nu existe două conversii care alunecă una față de alta);
* **`plati`** — plățile angajamentului. §1.3 din fundament: fiecare ordonanțare citește totalul recepției **așa cum stătea la data plății**, deci partea pe care cade un instantaneu față de o plată e diferența dintre o cifră corectă și una greșită, tăcut și pentru totdeauna (F12). Reperele trebuie să fie pe ecran **când** se așază, nu abia după.

**Complement, nu al doilea filtru.** `citeste_instantanee_context` întoarce tot ce NU e în mulțimea de decis, calculat pe `IDRH` în Python. Așa nu poate exista un rând pe care să nu-l ia niciuna dintre cele două liste — nici măcar cel lăsat afară fiindcă rândul lui de istoric nu e în descărcarea asta (până acum dispărea de pe ecran cu totul, rămânând doar într-un avertisment).

**Se arată, nu se mișcă.** Toate ies cu `blocat = True`. Nu e o alegere de aspect: acoperirea cerută de `verifica_acoperirea` e exact mulțimea de decis, iar o decizie pentru un rând din afara ei e respinsă cu 400 — și pe drept, altfel fiecare descărcare ar rescrie tăcut legături vechi. Asta e chiar a doua jumătate a cerinței operatorului: **«se vor salva DOAR cele modificate sau noi — NU TOATE din nou!»** Corectarea unei legături vechi rămâne treaba editorului de oricând, care e la un clic distanță și există exact pentru asta.

Motivul afișat spune care dintre cele două cazuri e:

| rândul | motivul, dacă nu e blocat de o ordonanțare sau de plăți |
|---|---|
| are legătură (`IDRR`) | «Legătura este deja scrisă. Se corectează în editorul de asociere…» |
| n-are (`IDRR` gol) | «Rândul lui de istoric nu este în această descărcare…» |

**Formularul n-a avut nevoie de cod nou ca să le arate cum trebuie.** `Blocat` era deja onorat în toate cele șase locuri: lacătul din arbore, culoarea stinsă, stilul marcajului de pe bandă, pornirea tragerii din arbore, pornirea tragerii de pe bandă și meniul contextual.

**Chei negative, ca să nu se ciocnească două numerotări.** Un rând de decis e ancorat pe indicele lui (0, 1, 2…), unul de context pe `IDRH`, cheia reală. Amândouă ajung în `_pozitie` și în dicționarele de rânduri ale formularului, unde `IDRH = 5` și indicele `5` ar fi același lucru. `AsociereStare.DinPropunere` păstrează deci contextul ca `-IDRH`: negarea e reversibilă (`IDRH` pornește de la 1, e cheie primară Access), nu poate atinge un indice, și spune dintr-o privire că rândul nu poartă o hotărâre.

Două locuri au trebuit să învețe să sară peste ele:

* `DeciziiDin` — altfel ar fi trimis decizii respinse cu 400;
* `NehotarateleCount` — altfel un singur rând de context neașezat ar fi stins butonul de salvare **pentru totdeauna**, fiindcă nimic nu-l poate așeza.

---

### Fișiere atinse

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

### Verde

* `PYTHON`: **485 trecute / 15 sărite** (linia de plecare a feliei: 472 / 15). Cele **13 teste noi** acoperă ancorele pasului 4b (inclusiv rândul care aterizează pe recepția născută de rândul dinaintea lui), forma cu trei ținte a unei decizii, rezolvarea prin ancoră în `aplica_decizii`, refuzul zgomotos al unui indice care n-a creat nimic, faptul că un `idrr` inexistent cade în continuare la fel, și complementul din `citeste_instantanee_context` cu ambele motive.
* `dotnet build src\KBot.App\KBot.App.vbproj` — **0 erori**. Cele 7 avertismente sunt `MSB3825` pe `.resx`-uri neatinse de felia asta (`DdfEditDescrierePage`, `AsociereForm`, `DdfView`, `OrdView`, `PlatiView`, `ReceptiiView`, `RezervariView`), preexistente și raportate ca atare și în feliile 0049 / 0054.
* `KBot.App.Tests` — **222 trecute / 13 picate / 235**. Linia de plecare era 217 / 13 / 230: aceleași 13 roșii, aceleași nume (DDF / Istoric / XFA / MainFormNavItems), plus **5 teste noi** verzi.
* `KBot.Api.Tests` **94 / 95** și `KBot.Domain.Tests` **28 / 31** — **identice** cu linia de plecare măsurată în felia 0055, aceleași roșii de `EtichetaRevizie` (DDF), pe care felia asta nu le atinge.

### Ce NU e verificat

**Nimic din felia asta n-a rulat pe calculatorul cu FOREXE.** Aici nu există nici aplicația FOREXE, nici baza MariaDB — dovada rămâne că data viitoare salvarea trece, nu că testele sunt verzi.

Anume:

* niciun `POST /api/forexe/prelucrare` real n-a purtat `rand_receptie`;
* contextul n-a fost văzut pe ecran: lacătele, benzile pline, liniile vechi din grafic și reperele de plată în timpul ingestiei sunt deduse din cod, nu privite;
* `AUTO_INCREMENT` care nu se derulează înapoi este **comportamentul documentat al InnoDB** și e singura explicație care se potrivește cu toate cele trei dovezi (mesajul rutei, faptul că recepția 188 nu se afla în `citeste_receptii` la salvare, și faptul că propunerea o listase) — dar n-a fost observat direct pe baza operatorului.

### Ce rămâne deschis

* **Reconstituirea (`receptie_noua`) tot nu se poate cere din formular.** Un lanț F26 rămâne neasezabil în modul propunere. Nimic din datele operatorului nu pare să ceară asta acum (niciun lanț nu se termină cu o recepție dispărută din `ListaReceptii`), dar rămâne o gaură cunoscută.
* **Mutarea unei legături vechi în timpul ingestiei** — deliberat nu se poate; e treaba editorului de oricând. Dacă operatorul o cere acolo, e o felie separată, fiindcă schimbă contractul de acoperire al rutei.
* Antetul benzii «Neașezate (N)» numără și rândurile de context neașezate, în timp ce notificarea numără doar pe cele de hotărât. Cele două cifre pot să difere; lacătul explică de ce, dar nu e aceeași cifră.

---

<a id="slice-0056-02"></a>

## SLICE-0056-02 — old links can be corrected while placing a download

Operator, 01.10.2026: «I need to be able to edit ANY reception when new data is downloaded, as long as there is no ordonantare for it. Now, when I download new data, it does not let me change the old receptions — the ones that already exist — and I do not want that.»

Slice number: recorded as the second pass of 0056 («Tabloul întreg al propunerii»), the slice that brought the already written snapshots (the "context") into the proposal and froze all of them. **Assumed, not asked** — renumber if it belongs elsewhere.

### What was wrong (read from the code and from the operator's `asociere.log`)

In proposal mode every snapshot written before the download arrived with `blocat = True`, whatever the ordonantari said (`citeste_instantanee_context`, and again in `AsociereStare.DinPropunere`). The reason given at the time: the server asks for a decision on exactly the rows to place, and a decision for any other row was refused.

The pasted log (014_SCSV, AAB2HFBEEAF, run 9) shows what that costs. The download moved the value 1723.58 onto reception 33, where snapshot 63 (398.39, placed earlier) is the newest. The two snapshots to place (61, 62, 1723.58) belong on 33, but with 63 still on it the chain ends on 398.39, F15 refuses the save, and 63 could not be moved from this window. The download could not be saved at all without closing the window and going through the anytime editor first.

### What changed

**The rule is now the same in both modes: a link is frozen only by an ordonantare** (the existing rule of `asociere.citeste_blocaje`: an ordonantare line on the angajament dated on or after the snapshot's day).

Server (`PYTHON/routes/forexe/`):

- `prelucrare_asociere.citeste_instantanee_context`: `blocat` is no longer True for all. A placed row is blocked only by `blocaje`; an unplaced one is free; a row born in the current run that is not in the set to place (no history row, or a duplicate) stays blocked with `MOTIV_FARA_ISTORIC`, because its IDRH does not survive the rollback of phase one. `MOTIV_CONTEXT` is removed.
- `citeste_idrh_scrise` (new): the IDRHs of the angajament BEFORE the steps write anything, read in both phases. Only these can be named by IDRH.
- A third anchor, `("idrh", n)`, next to `rand_istoric` (F24) and `idh` (F34): a decision that carries `idrh` is a CORRECTION of a snapshot written before this run. `normalizeaza_decizii` asks for exactly one of the three and allows `desprins` only on a correction (`ACTIUNE_DESPRINS` moved here; `asociere.py` imports it).
- `verifica_corecturile` (new): the row must be a context row, once, with a matching `data_h`, and not blocked — checked again at save, since an ordonantare may appear after the proposal. No coverage: for an old link silence means «leave it as it is».
- `aplica_decizii`: coverage is asked only for the rows to place; labels, chains and writes treat both kinds alike. The reception a corrected row LEAVES is touched too (chain checked, `Final`/`Partial` redone, `Sters` taken off when it lost its deletion row). `desprins` and `ignorat` on a correction also clear `EsteStergere`. New counter `asocieri.desprins`.
- `valideaza_plasarile(f15_doar_semn=...)`: on a reception that only LOSES snapshots in this save F15 warns instead of refusing — the anytime editor's reason: a chain whose last snapshot was taken off does not close by definition. A reception that gains a snapshot keeps the veto.
- `prelucrare.py`: reads `idrh_scrise` before the steps; in the save phase reads the context only when a correction came.
- Journal (`asociere.log`): the context table has a `blocat` column and the reasons; the save writes «corecturi: N», and for each one where the row was.

Client:

- `KBot.Domain`: `InstantaneuLegat.Context` (row written before the download; key `-IDRH`); `DinPropunere` keeps the server's `Blocat`; `DecizieAsociere.Idrh` (the third anchor).
- `KBot.Api`: `PostPrelucrareDecizie.idrh`; `CatreFir` sends exactly one anchor and allows `Desprins` only with it. **`IApiClient` is unchanged** (the corrections travel in the same `decizii` list).
- `AsociereForm`: `DeciziiDin` sends a correction for every context row the operator changed and nothing for an untouched one; `NehotarateDin` skips context rows (no decision owed); «Golește așezările» puts moved old links back to the server's state and its question says so. Nothing else: every drag / menu / lock check already read `Blocat`.

Compatibility: an old client against the new server still marks all context rows blocked (its own `DinPropunere`); a new client against the old server gets them all blocked from the server.

### Files touched

- `PYTHON/routes/forexe/prelucrare_asociere.py`, `prelucrare.py`, `asociere.py`
- `src/KBot.Domain/AsociereStare.vb`, `AsociereInfo.vb`, `KBot.Domain.vbproj` (1.2.7.0 → 1.2.8.0)
- `src/KBot.Api/ApiClient.vb`, `UpsertAngajamenteRequest.vb`, `KBot.Api.vbproj` (1.0.15.0 → 1.0.16.0)
- `src/KBot.App/Forexe/AsociereForm.vb`
- help: see [0000-26](#slice-0000-26)

### Test results

- `dotnet build src/KBot.App/KBot.App.vbproj` (output to a scratch folder, K-BOT was running and locked `bin\Debug`): **0 warnings, 0 errors**.
- The three Python files parse (`ast.parse`); nothing was imported or run.
- **No tests were run and none were written** (operator: «nu testa»). The app was not started.

### Left unverified or deferred

- **Never run**: no proposal, no save, no window. The whole change is read from the code.
- **Tests that describe the old behaviour and will fail until updated** (not touched):
  - `PYTHON/tests/test_forexe_prelucrare_asociere.py` — the three `citeste_instantanee_context` calls (new required argument `idrh_scrise`; «all blocked» no longer true);
  - `PYTHON/tests/test_forexe_prelucrare_route.py` — the scripted cursor does not know the new `SELECT IDRH FROM FX_Receptii_H WHERE CodAngajament` read;
  - `tests/KBot.Domain.Tests/AsociereStareDinPropunereTests.vb` and the `Blocat` assertions in `tests/KBot.App.Tests/AsociereFormTests.vb` / `AsociereDeciziiTests.vb`, where they expect every context row blocked.
- **The server files must reach the VPS** together with the client; until then the new client behaves as before (all old links blocked).
- The fingerprint (`amprenta`) does not see a link MOVED by another session between the proposal and the save (it counts rows, not targets). Same limit as in the anytime editor; the blocking rule itself is re-checked at save.
- The two warnings of `citeste_instantanee` («... nu pot fi așezate din descărcare. Se rezolvă în editorul de asociere») are unchanged, although such rows written before the run can now be placed in this window. Wording left for the operator to decide.
- Ties on `DataH` between an old and a new snapshot on a reception started in the window are ordered by the form on `-IDRH`, by the server on `IDRH`. Not expected in real data.

---

Consolidare documentară D002-18: jurnalul ajutorului scris în aceeași sarcină cu 0056-02 este păstrat mai jos ca etapă distinctă. Textul ajutorului și verificarea sa nu certifică salvarea reală rămasă neprobată.

<a id="slice-0000-26"></a>

## SLICE-0000-26 — the help for slice 0056-02

Slice 0056-02 ([0056-02](SLICE-0056-tabloul-intreg.md#slice-0056-02), operator, 01.10.2026) lets the operator move old links while placing a download. This is the help for it, written in the same task, every changed section tagged `0056-02`.

### What changed and why

| Topic / tour | What |
|--------------|------|
| `contabil.asocieri.cazuri` | «Legături blocate» rewritten: only an ordonantare (dated on the snapshot's day or later) blocks, a payment does not, and the rule is the same after a download. «După o descărcare: salvare sau renunțare»: old links can be corrected in the same window, only the moved ones are written, why it matters («Lanțul nu se închide»), the warning instead of a refusal when a reception only loses a snapshot, what «Golește așezările» does with moved old links |
| `contabil.asocieri.fereastra` | the sentence about a blocked snapshot (ordonantare only; everything else can be moved, also after a download); the «Salvează legăturile» and «Golește așezările» rows of the buttons table |
| `tur-receptii` | step «Arbore › Asocieri»: the blocking sentence no longer mentions payments |

The «or a payment» wording was stale since 18.09.2026 (payments stopped blocking); corrected here because the same sentences had to be rewritten.

Left out on purpose (HELP_SYSTEM §5): anchors, how the corrections travel to the server, the journal.

### Files touched

- `src/KBot.App/HelpContent/contabil/asocieri/cazuri.md`, `fereastra.md`
- `src/KBot.App/HelpContent/tours/tur-receptii.md`
- `help-version.txt` stays `2026-10-01`

### Test results

- `tools\HelpCheck\Check-Help.ps1 -Coverage`: «No errors.», 48 topics, 17 tours, 57 capture tags. Coverage names only `RobotQueueForm` (0098, already in the open threads).
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- Nothing seen on screen.

### Capturi de refăcut / noi

- None. The window looks the same; after a download fewer rows carry the lock.

### De citit de operator

- `contabil.asocieri.cazuri`, «După o descărcare»: the sentence «K-BOT salvează și te avertizează după salvare; nu refuză» describes the server rule as written (a reception that only loses snapshots); it was not seen on a real save.
