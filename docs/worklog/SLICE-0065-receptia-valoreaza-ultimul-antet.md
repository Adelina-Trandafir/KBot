# SLICE-0065 — Recepția valorează ultimul ei antet; rădăcina «Toate recepțiile»; DIF după salvarea din editor; perechile unice se așază singure

**Data:** 17.09.2026
**Cerere (operator, aceeași zi), pe vederea Recepții și pe editorul de legături:**

> 0. să aibă un rând-rădăcină «Toate Recepțiile»;
> 1. la o recepție selectată să arate DOAR ultimii indicatori — nu totalul;
> 2. să arate totalul DOAR al ultimului H al fiecărui R;
> 3. tooltip-ul fiecărui nod (R) să arate și câmpul Descriere;
> 4. totalul recepțiilor pe ultimul R din JSON iese greșit: ar trebui 29.645, tooltip-ul și
>    OrdonantareForm arată 25.410 (lipsește una);
> 5. la deschiderea AsociereForm, dacă se găsesc valori unice pentru R și H, să le asocieze
>    singur — «ia-i din muncă».
>
> Apoi: **fără teste, fără git.**

Numărul feliei: 0065 = «Next free» din STATUS. Presupunere, nu confirmare — se poate renumerota.

## 1. Cauza lui (4): pasul 4d nu rula după salvarea din editor

În JSON-ul operatorului (`AAB2KRCC64T`, `027_SCGM`), antetul `IDRH 242` (recepția `IDRR 187`)
are `DIFH NULL`, iar liniile lui 476/477 au `DIF NULL`. Celelalte șase recepții au `DIFH =
4.235`. Tooltip-ul din Recepții însuma `DIFH` pe anteturi (`qFX_MAIN_REC_TT_DIFH`), iar
`qFX_ORD_REC_ANT` (ordonanțarea) însumează `DIF` pe linii: `NULL` -> 0 pe amândouă drumurile, deci
6 × 4.235 = 25.410.

De ce e NULL: `DIFH`/`DIF` nu vin de pe site, le calculăm noi (pasul 4d,
`step4d_calculeaza_dif`) — F20 retras spune exact asta. Ingestia îl rulează după 4c
(`_pas4d_pe_receptiile_atinse`) și refacerea din 0062 după ce a scris. **Editorul de oricând
(`routes/forexe/asociere.py`, `aplica_comenzi`) nu îl rula deloc**: recalcula `Final`/`Partial`
și F28, dar nu diferențele. Orice instantaneu așezat de acolo (sau o recepție pornită de acolo,
0059 — `IDRR 187` are `Incarcat NULL`, semnul unui rând scris de editor) rămânea invizibil pentru
ambele totaluri.

**Fix:** `aplica_comenzi` cheamă `step4d_calculeaza_dif(cursor, cod, idrr)` pentru fiecare lanț
atins (`de_recalculat`), după `recalculeaza_final`. Datele deja stricate se repară de la sine la
următoarea salvare din editor pe acel angajament, la orice «Reîmprospătare» (ingestia rulează 4d
pe TOATE recepțiile angajamentului) sau la «Refacere din istoric» pe lanțurile atinse.

## 2. Vederea Recepții — o recepție VALOREAZĂ ultimul ei instantaneu

Lanțul unei recepții = instantaneele ei în ordinea `DataH`, fiecare cu valoarea ÎNTREGII
recepții la acel moment (F3). Ce e adevărat acum e ULTIMUL antet. Vederea le calcula altfel:
totalul nodului R era `SumaAntet` al rândului R, grila aduna `Valoare` peste TOATE anteturile
lanțului (deci `AAB` = 4.235 + 0 pe `IDRR 180`, când ultimul antet spune 0), iar tooltip-ul aduna
`DIFH`-uri.

`ReceptiiView.vb` acum:

| Cerere | Ce s-a făcut |
|---|---|
| 0 | Rădăcina `ROOT_KEY = "all"`, «Toate recepțiile ~~~ total», bold, deschisă; lunile sub ea; dosarul «Instantanee neașezate» rămâne la nivelul de sus, după ea. Tooltip de reconciliere pe rădăcină: numărul recepțiilor, cumul întreg vs. TOATE plățile. |
| 1 | `LiniileDeAratat`: pe orice nod, liniile ultimului antet CU linii al fiecărei recepții (`LiniileUltimuluiAntet`), apoi gruparea pe clasificație. Pe un nod R = exact indicatorii ultimului antet. Rândul-total sintetic rămâne scos (2026-08-13). |
| 2 | `ValoareaReceptiei` = `Total` al ultimului antet (`UltimulAntet`: `DataH` desc, `IDRH` desc, fără `StersH`). Eticheta R, iconița ▲/▼, totalul lunii și al rădăcinii (`TotalReceptii`) pornesc toate de aici; `SumaAntet` nu mai e citit de arbore. |
| 3 | Rândul «Descriere» în tooltip-ul de recepție: `R.Descriere` (nou pe fir, `descriere_r`), altfel `Descriere` al ultimului antet. |
| 4 | Cumulul «Recepții cumulate» = suma rulantă a `ValoareaReceptiei`, nu a `DIFH` (`SumDistinctAntetDifh` scos). Pe lanț complet cele două coincid; pe `DIFH NULL` doar a doua e adevărată — 29.645 pe JSON-ul operatorului. `Difh` rămâne pe `ReceptieRow`, necitit de vedere. |

Rândul de ștergere (F21) e ultimul antet al unei recepții șterse și nu are linii: totalul lui e
valoarea recepției la ștergere (deci cumulul nu se schimbă față de înainte), liniile se iau din
ultimul antet care are, iar eticheta R primește «[ștearsă]» (`este_stergere` nou pe fir). F22
(«ascunsă din arbore») rămâne neimplementată ca înainte — nu s-a cerut aici.

Pe fir (`receptii.py`): `descriere_r` (`R.Descriere`) și `este_stergere`
(`COALESCE(H.EsteStergere,0)`); `GetReceptieRow`, `ApiClient.GetReceptiiAsync`, `ReceptieRow`
(`DescriereR`, `EsteStergere`) le poartă.

## 3. AsociereForm — perechile cu valoare unică se așază singure

`AsazaPerechileUnice`, chemată în `ReincarcaAsync` după citirea tabloului și înainte de
`Reconstruieste`, DOAR în `ModAsociere.Oricand` (în propunere sugestiile le face serverul, 4c):

* candidații din coș: neașezate, neblocate, nemarcate «fără schimbare», nu rânduri de ștergere;
* recepțiile: cele neșterse pe site;
* valoarea rotunjită la ban; se ia o pereche NUMAI când valoarea apare pe un singur instantaneu
  din coș ȘI pe o singură recepție (`SumaAntet`). Valoarea nu e cheie (F5) — de aceea unicitatea
  pe amândouă laturile, nu potrivirea: șapte recepții de 4.235 cu două instantanee de 4.235 nu
  se ating;
* vetourile F14/F16 se aplică prin `MotivulRefuzului`, ca la o tragere;
* rezultatul e o MUTARE LOCALĂ (`_pozitie`), nesalvată: intră în `Comenzi()`, se vede în arbore,
  se poate trage înapoi. Banda spune câte s-au așezat (`TextAsezareAutomata`) și cere verificarea.

## 4. Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/asociere.py` | import `step4d_calculeaza_dif`; 4d pe fiecare lanț atins în `aplica_comenzi` |
| `PYTHON/routes/forexe/receptii.py` | `descriere_r`, `este_stergere`; docstring |
| `PYTHON/tests/test_forexe_asociere.py` | falsul răspunde la `_DIF_H_SQL`/`_DIF_R_SQL`; `test_a_save_recomputes_dif_on_every_touched_chain` (pică fără fix — scris ÎNAINTE de «fără teste», lăsat) |
| `PYTHON/tests/test_forexe_receptii.py` | cheile noi în `ROW_KEYS` + două asserturi (host-only, sare aici) |
| `src/KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb` | DTO + mapare |
| `src/KBot.Domain/ReceptiiInfo.vb` | `DescriereR`, `EsteStergere`; antetul fișierului |
| `src/KBot.App/Views/ReceptiiView.vb` | rădăcina, ultimul antet, grila, tooltip-urile, `[ștearsă]` |
| `src/KBot.App/Forexe/AsociereForm.vb` | `AsazaPerechileUnice`, `TextAsezareAutomata`, apelul din `ReincarcaAsync` |

Neatinse deliberat: `ord_edit._SQL_REC_ANT` (rămâne `SUM(DIF)`, ca în Access — cu 4d rulat
corect cifrele lui sunt bune); `Difh` pe `ReceptieRow` (pe fir, necitit); `KBotDataView`.

## 5. Verificări

* `dotnet build src\KBot.App\KBot.App.vbproj`: **0 erori, 0 avertismente** (cele MSB3825 din
  `.resx` sunt de dinainte). `tests\KBot.App.Tests` **compilează**.
* PYTHON (`.venv`, `KBOT_ASOCIERE=0`): `test_forexe_asociere.py`, `test_forexe_receptii.py`,
  `test_forexe_prelucrare_pasi.py` — **69 trecute / 1 sărit**. Suita întreagă NU a rulat
  («fără teste»).

## 6. Neverificat / amânat

* **Nimic pe ecran, nimic pe MariaDB.** Arborele pe trei niveluri, rândul «Descriere» din
  tooltip și banda «așezate automat» nu s-au văzut nici prin `DrawToBitmap`.
* **Testele VB din `ReceptiiViewTests.vb` NU s-au actualizat** (cerere «fără teste»): compilează,
  dar cele care numără `t.Items` (acum 1 rădăcină + dosar) sau așteaptă cumulul din `DIFH`
  (Februarie: 3.480,43 -> 6.344,55 cu regula ultimului antet) vor pica la rulare. Tot așa
  `AsociereFormTests.vb`: `StandardStare` are H31 = 200 unic față de R2 = 200 unic, deci se așază
  singur la încărcare — `FaraNiciOSchimbare_NuPleacaNicioComanda` și
  `Incarcarea_AsazaFiecareInstantaneuUndeIlAreServerul` vor pica; `StareCuNeasezatInaintePlatii`
  la fel (H31 = 250 unic, R2 = 250 unic). Fixturile trebuie mutate pe valori nepotrivite, cu
  teste noi pentru așezarea automată — de făcut într-o rundă cu teste.
* Datele operatorului: `IDRH 242` rămâne cu `DIFH NULL` până la prima salvare din editor pe
  angajament, o reîmprospătare sau o refacere; vederea Recepții e corectă și așa (nu mai citește
  `DIFH`), ordonanțarea NU (citește `DIF`).
* Nimic comis («fără git»).
