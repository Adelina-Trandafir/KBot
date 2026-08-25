# SLICE-0048-01 — premisele portării FOREXE: AUTO_INCREMENT, scutirea, ajutoarele

Prima transă din felia 0048 (portarea conductei de ingestie FOREXE din Access VBA în API-ul
Flask, plan: `docs/PLAN_ForexeIngest.md`). Numărul feliei a fost **confirmat** față de
`KBOT_STATUS.md`, care spunea *Next free slice number: 0048*.

Transa aceasta livrează **premisele** (§3 și §3.2 din plan), **corectura `.wfl`** (D11) și
**modulul de ajutoare pure** pe care se sprijină toți cei șapte pași. Ruta propriu-zisă
(`prelucrare.py`) și jumătatea VB.NET **nu** sunt în transa aceasta — vezi «rămas de făcut».

---

## 1. Ce s-a schimbat și de ce

### 1.1 Planul a fost scris în depozit, apoi verificat rând cu rând față de sursa reală

`docs/PLAN_ForexeIngest.md` a fost scris la calea pe care o numește el însuși. Apoi, conform
§0 din plan și `CODE_WORKFLOW.md` §1.3, **fiecare afirmație «VBA-ul face X» a fost recitită în
fișierul real**, nu preluată. Cinci nepotriviri au ieșit la iveală, toate în pasul 2, și patru
dintre ele **schimbă ce se scrie în bază**. Au fost puse operatorului, care a ales:

| # | Constatare | Decizia operatorului |
|---|---|---|
| **D17** | **D16 era cheiat pe coloana greșită.** Planul blochează căutarea unității pe `(SS, ClsfSal)`; `Obtine_IdUnitate_Din` o rezolvă în realitate pe **`(SS, ClsfE)`** — `Right(ClsfE, 8)` fără puncte, adică **ultimele 6 cifre** (`Articol` + `Alineat`) — cu `INNER JOIN Cai ... WHERE Cai.DC = DC()`. `ClsfSal` e folosit doar la a doua căutare, cea a lui `IdClsf`. | **`(SS, ClsfE)` — fidel Access-ului.** Două căutări: `ClsfE` ▸ `IdUnitate`, apoi `ClsfSal` în interiorul acelei unități ▸ `IdClsfAcc`. |
| **D18** | **`BugetIndicator` nu e citit niciodată.** `If dInd.Exists("BugetIndicatori")` (cu *i* la coadă) păzește `dInd("BugetIndicator")` (fără *i*). Cheia din sarcina utilă e `BugetIndicator`, deci testul e mereu fals ▸ `vRec = vPlati = -1` ▸ `FX_Indicatori.Receptii` și `.Plati` **nu se scriu niciodată** pe calea asta. | **Se portează defectul**, consemnat. Coloanele rămân nescrise, exact ca azi. |
| **D19** | **O clasificație negăsită NU e eroare în Access:** `If Not IsNull(IdClsf) Then RC!IdClsf = IdClsf` lasă coloana nescrisă și merge mai departe. Planul §7 pasul 2 cerea `raise`. | **NULL + avertisment** în `avertismente`. Toleranța Access-ului, dar nu tăcută. |
| **D20** | **`FX_Indicatori_Actualizare_Extrase` e VIU**, nu mort (§13.3 din plan era întrebare deschisă): două `UPDATE`-uri care umplu `FX_Extrase.CodAI` din `FX_Plati` unde e null. Dar §12 pune `FX_Extrase` în afara scopului. | **Se include**, în interiorul tranzacției. |
| — | **Fără completare cu zerouri.** Planul §5.2 dedusese `Format(x, "00")` dintr-o funcție-soră (`Angajament_Incarcat_Prelucrare_Initiala`). `Prelucrare_Indicatori` **nu** face asta: doar scoate spațiile și punctele. | Se portează funcția reală. Planul o spunea el însuși: «use this only as a cross-check». |

Două întrebări deschise din §13 s-au **închis** prin citire, fără să fie nevoie de întrebare:

- **§13.2 — `DataInceputDerulare` ARE coloană.** `Prelucrare_Angajament` scrie
  `DataAngajament` ▸ `FX_Angajamente.DataCreare` și `DataInceputDerulare` ▸
  `FX_Angajamente.DataDefinitivare`. Planul bănuia că nu are unde să stea.
- **§13.3 — vezi D20 mai sus.**

Și defectul pe care planul îl anunțase la §5.1 s-a **confirmat** litera cu literă:

```vb
If cTask.ExtraObject.Exists("StareAngajament") Then
    Stare = Nz(cTask.ExtraObject("Stare"), "")     ' citește "Stare", nu "StareAngajament"
ElseIf cTask.ExtraObject.Exists("Stare") Then
    Stare = Nz(cTask.ExtraObject("Stare"), "")
End If
```

Se portează **intenția** (`StareAngajament`), nu defectul — cum cerea planul.

### 1.2 §3 — pasul AUTO_INCREMENT, în `KBot.Migrator`

`src/KBot.Migrator/MariaDb/AutoIncrementStep.vb` (nou). Cele **șapte** chei primare devin
`AUTO_INCREMENT` ca **ultim** lucru care i se întâmplă unei baze de unitate.

De ce trebuie să rămână un pas separat și final, scris în clasă ca să nu se piardă: o bază nouă
se creează din `AVACONT_SURSA` și se migrează **după aceea**, iar migrarea scrie id-urile Access
**verbatim**. Pe o coloană `INT NOT NULL` simplă, un rând care sosește fără id (lipsă, NULL sau
zero) **ridică** și rularea se oprește. Pe o coloană `AUTO_INCREMENT`, exact același rând e
primit, iar MariaDB **inventează tăcut o cheie** — într-un tabel ale cărui id-uri sunt referite
de alte cinci. Paza asta e chiar motivul pentru care coloanele sunt simple, și trebuie să
supraviețuiască pentru fiecare bază încă nemigrată.

Ordinea e deci mereu: **creare din `AVACONT_SURSA` ▸ migrare ▸ verificare ▸ ALTER.**

Ce refuză clasa, în `RefusalReason`:

- **transfer neîncheiat cu COMMIT** — singurul lucru care autorizează pasul este
  `result.Committed`;
- **`AVACONT_SURSA`** (§3.1) — comparat *neinsensibil la caz*, fiindcă un server cu
  `lower_case_table_names=1` pliază numele, iar o potrivire exactă ar lăsa `avacont_sursa` să
  treacă exact prin poarta care există ca s-o oprească;
- **`AVACONT_COMUN`** — nu e bază de unitate.

Ce **raportează** per tabel (§3.3, măsurat, nu presupus): rânduri, `MAX(cheie)` înainte,
`AUTO_INCREMENT` după. Tipul coloanei e luat din **coloana vie**, nu scris fix `INT(11)`: dacă
referința se lărgește vreodată la `BIGINT`, `MODIFY` nu trebuie s-o îngusteze la loc.

Pasul e **re-rulabil**: un tabel deja `AUTO_INCREMENT` e recunoscut și sărit. Contează, fiindcă
**DDL-ul nu se poate derula înapoi** — o oprire la jumătate lasă convertite tabelele făcute până
atunci, iar mesajul către operator spune asta pe față în loc să sugereze că baza e neatinsă.

În `MigratorForm.btnTransfera_Click`, pasul rulează **doar după COMMIT**, iar **eșecul lui nu e
eșecul transferului**: rândurile sunt deja scrise și rămân scrise. E prins separat, ca operatorul
să nu fie anunțat niciodată că un transfer reușit a fost derulat înapoi.

### 1.3 §3.2 — scutirea din `schema_sync`

`PYTHON/routes/schema_sync/schema_common.py`: `EXEMPT_COLUMNS` — **lista numită**, cele șapte
perechi `(tabel, coloană)` scrise în întregime, plus `is_exempt_column()`. Legată în
`schema_diff.py` `SchemaDiff._columns`, lângă săritura existentă peste coloanele generate.

Cele două lucruri pe care planul le cerea explicit, amândouă în cod:

1. **Listă numită, nu regulă.** O regulă de tipul «sari peste orice e auto_increment» ar înghiți
   tăcut o diferență reală în altă parte. Lista asta nu poate.
2. **Acoperă COLOANA ÎNTREAGĂ**, nu doar atributul `AUTO_INCREMENT`. Costul e scris în cod, pe
   față: dacă cineva schimbă vreodată deliberat una dintre cele șapte coloane în
   `AVACONT_SURSA` — tip, lățime — **nu se va propaga și divergența nu va fi raportată**. Lista
   e locul unde va trebui să se uite. Cost acceptat, nu scăpare.

Potrivirea e neinsensibilă la caz, din același motiv ca la §1.2.

### 1.4 D11 — corectura `.wfl`

`src/KBot.Forexe/Workflows/adlop - Prelucrare Completa.wfl`, rândul 165. Era:

```xml
collectFields="Tip,Data,Suma,DescriereReceptie,Detaliu"> <!-- TipReceptie,CodIndicator, -->
```

A devenit, identic cu fișierul Reverse:

```xml
collectFields="Tip,Data,Suma,TipReceptie,DescriereReceptie,CodIndicator,Detaliu">
```

Ambele câmpuri erau deja **citite** (`<Read ... saveTo="TipReceptie" />` la rândul 179 și
`saveTo="CodIndicator"` la 193) — doar nu erau *colectate*.

**Diferența cerută de §9 a fost făcută.** După normalizarea spațiilor (fișierul înainte fusese
reformatat cu tăiere de rânduri, ceea ce îneca un `diff` obișnuit), între cele două fluxuri
rămân **exact** lucrurile pe care planul le prevestise, și nimic altceva:

| diferență | înainte | Reverse |
|---|---|---|
| antet versiune | `V.5 - 13/08/2026` | `V.4 - 26/03/2026` |
| timeout-uri | `timeout="15"` | `timeout="5"` |
| paginare Istoric | `nextPageSelector="a[rel='next']"` | `lastPageSelector` / `prevPageSelector` / `startFromLast="true"` / `exitIfCellEquals="Timp:~:^{{DATA_IESIRE}}"` |
| `collectFields` (indicatori) | identic | identic |
| `collectFields` (recepții) | **identic acum** | identic |

### 1.5 Modulul de ajutoare pure

`PYTHON/routes/forexe/prelucrare_helpers.py` (nou, ~520 rânduri cu comentarii). Port rând cu
rând al ajutoarelor din `mdl_FX_Helpers`. **Nu atinge nici baza, nici starea Flask**, deci se
testează offline — de asta stă în modul separat, nu în `prelucrare.py`.

Comentat la nivelul cerut explicit de operator: fiecare idiom Python (indexare negativă, felii,
`set`/`tuple`, comprehensiuni, `%s`, `f-string`) explicat la prima apariție, în engleză, spunând
**ce face**, nu doar de ce.

Ce s-a **aflat** scriindu-l:

- **Hash-urile stocate sunt hex cu MAJUSCULE.** `BytesToHex` folosește `Hex$`, care în VBA
  întoarce majuscule, și pliază fiecare octet la două caractere prin
  `Right$("0" & Hex$(b), 2)`. Jumătate din necunoscuta de la §8 e închisă. Cealaltă jumătate —
  ce codificare de octeți aplică `BCrypt.HASH` peste șirul UTF-16 — **rămâne necunoscută**
  (modulul `BCrypt` nu e în export). Se presupune UTF-8, marcat în cod. Contează exact zero
  pentru deduplicare, fiindcă D9 dedupează pe cheia naturală.
- `BCrypt.Base64Bytes(BCrypt.HASH(x, bcSha256))` este, desfăcut, chiar `sha256(x)` —
  `Base64Bytes(base64(y)) = y`.
- **`ParseLooseNumber` decide separatorul după POZIȚIE**, nu după caracter: `Z` = al doilea
  caracter de la dreapta, iar dacă e cifră, al treilea. Așa nimerește pe separator și la
  `510,00` (două zecimale) și la `123.4` (una). Dacă `Z` e tot cifră, nu există separator
  zecimal deloc, iar orice `.` sau `,` e separator de mii — de unde **`"3.587"` ▸ `3587.0`**,
  nu `3.587`.
- **Două defecte fidele, portate cu tot cu comentariu:**
  - `ExtractNumberAfterLabel` are `If P2 < P1 Then Exit Function`; când nu există virgulă după
    etichetă, `P2 = 0 < P1` ▸ **iese cu 0**, iar ramura `IIf(P2 = 0, ...)` de sub ea e cod mort
    inaccesibil. `ExtractRezervareDefinitiva` însumează cinci apeluri din astea, deci ultima
    găleată fără virgulă la coadă **se pierde**. Totalurile deja din MariaDB au fost produse sub
    regula asta.
  - A treia ramură din `FX_ExtractCodIndicator` (`"Plata: Rand:"`) e **inaccesibilă**: a doua
    caută `"Rand:"`, care se potrivește și înăuntrul lui `"Plata: Rand:"`. Păstrată, cu test
    care fixează comportamentul.
- **`FX_Receptii_Istoric_GetIndent` e dependent de localizare**: adaugă `Valoare` ca `Double`
  brut, nu prin `FX_Receptii_NumKey`, iar `GetHashForRow_ReceptieH` îl trece prin `CStr`, care
  în VBA folosește separatorul zecimal al mașinii. Pentru valorile **întregi** (majoritatea)
  `CStr(510.0)` e `"510"`, fără separator, deci întrebarea nu se pune. Marcat, și nu afectează
  deduplicarea.
- **`ParseDataZZLLAAAA` nu există în export.** E *folosit* în `mdl_FX_Tasks_Receive_DWN` și
  `mdl_FX_Istoric`, dar *definit* în niciunul — stă într-un modul din afara
  `FX_System_Export`. Contractul a fost dedus din cele două locuri de apel, care amândouă îi dau
  un `dd/MM/yyyy` cu bare rescrise în puncte. Consemnat mai jos ca neverificat.

---

## 2. Fișiere atinse

**Noi**

| Fișier | Ce e |
|---|---|
| `docs/PLAN_ForexeIngest.md` | planul, scris la calea pe care o numește |
| `src/KBot.Migrator/MariaDb/AutoIncrementStep.vb` | pasul final AUTO_INCREMENT + raportul lui |
| `PYTHON/routes/forexe/prelucrare_helpers.py` | ajutoarele pure portate din `mdl_FX_Helpers` |
| `PYTHON/tests/test_forexe_prelucrare_helpers.py` | 83 de teste offline |
| `PYTHON/tests/test_schema_sync_exempt_columns.py` | 10 teste; fixează lista și acordul VB↔Python |
| `docs/worklog/SLICE-0048-01-premise-si-ajutoare.md` | fișierul acesta |

**Modificate**

| Fișier | Ce s-a schimbat |
|---|---|
| `PYTHON/routes/schema_sync/schema_common.py` | `EXEMPT_COLUMNS` + `is_exempt_column()` |
| `PYTHON/routes/schema_sync/schema_diff.py` | import + săritura din `SchemaDiff._columns` |
| `src/KBot.Migrator/MigratorForm.vb` | pasul rulează după COMMIT; `ShowResult` primește raportul; `AutoIncrementSummary` |
| `src/KBot.Forexe/Workflows/adlop - Prelucrare Completa.wfl` | `collectFields` (D11) |

---

## 3. Rezultatele testelor

Numere **reale**, din rulările de mai jos. Nimic copiat.

**Python** — `PYTHON/.venv/Scripts/python.exe -m pytest tests/ -q`

```
306 passed, 14 skipped in 13.70s
```

Zero eșecuri, zero erori. Cele 14 sărite sunt testele *host-only* dinainte (au nevoie de
`config.py` și de MariaDB), sărite curat, exact ca înainte de transa asta.

Din cele 306: **83** sunt noi în `test_forexe_prelucrare_helpers.py`, **10** noi în
`test_schema_sync_exempt_columns.py`. Cele 20 din `test_schema_diff_columns.py` (dinainte) trec
mai departe — scutirea nu le-a regresat.

**.NET** — `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Restul soluției **nu** a fost construit în transa asta: nimic din afara lui `KBot.Migrator` nu a
fost atins. Se construiește în transa care aduce clientul.

**Verificare empirică făcută pe date reale** (nu un test, o măsurătoare): singurele date de
clasificații de pe disc — jurnalul migrării `000_DEMO/20260824_092427/Clasificatii.sql`, **101
rânduri, unitățile 75 și 76** — au fost trecute prin regulile generate ale MariaDB:

| cheie | perechi distincte | perechi care duc la >1 `IdUnitate` |
|---|---|---|
| `(SS, ClsfSal)` | 101 | **0** |
| `(SS, ClsfE)` | 69 | **0** |

Deci `ClsfE` chiar pliază rânduri (101 ▸ 69), dar pe datele astea **niciuna** dintre plieri nu
traversează o unitate. Atenție la ce **nu** dovedește: sunt 2 unități din 13 pentru DC `000_DEMO`
și ~101 rânduri dintr-un tabel al cărui `AUTO_INCREMENT` e 1497. Testul-constatare cerut de §10.7
rămâne de scris, pe baza vie.

---

## 4. Rămas neverificat sau amânat

**Neverificat**

1. **`ParseDataZZLLAAAA` — definiția nu există în `FX_System_Export`.** Contractul (`zz.ll.aaaa`)
   e dedus din cele două locuri de apel. Dacă modulul care o definește apare vreodată, de
   comparat cu `parse_data_zzllaaaa`.
2. **Codificarea de octeți a hash-ului Access** (§8) — `BCrypt.HASH` nu e în export. Se presupune
   UTF-8. Verificarea cerută de §8 (rata de potrivire față de datele migrate) **nu s-a făcut**:
   are nevoie de baza vie. Nu blochează nimic, fiindcă D9 dedupează pe cheia naturală.
3. **`AutoIncrementStep` nu a rulat niciodată pe o bază reală.** Compilează, nu a fost executat.
   Ce trebuie citit la prima rulare, per §3.3: dacă InnoDB chiar a pornit contorul de la
   `MAX(cheie) + 1`. Pasul raportează exact cele trei numere de care e nevoie.
4. **`MigratorForm` nu a fost deschis pe ecran** în transa asta — la fel ca la 0045-05.
5. **Invariantul `(SS, ClsfE)` pe date vii** — vezi tabelul de mai sus. Măsurat pe 2 unități din
   13; testul-constatare §10.7 e de scris.

**Amânat în transele următoare ale feliei 0048**

- `routes/forexe/prelucrare.py` — cei șapte pași, tranzacția, contractul de la §6. **Nimic din
  ruta propriu-zisă nu e în transa asta.**
- Jumătatea VB.NET (§9): `PrelucrareRaspuns` în `KBot.Domain`, `TrimitePrelucrareAsync` în
  `IApiClient`/`ApiClient`, POST-ul din `ForexeController.DownloadNodeAsync`.
- Testele §10.2–§10.7 (idempotență, derulare înapoi, antet neasociat, rata hash-ului, `IDREV`
  inexistent, invariantul).
- `KBot.Migrator` **nu are proiect de teste**, deci lista celor șapte perechi din VB nu e fixată
  de un test .NET. Ținută în frâu, în schimb, de `TestTheVbListAgrees` din
  `test_schema_sync_exempt_columns.py`, care **citește fișierul `.vb`** și cere ca cele două
  liste să coincidă — nu are nevoie nici de compilator, nici de MariaDB.

**De reținut înainte de transa următoare:** până când pasul AUTO_INCREMENT nu rulează pe baza de
test, ingestia **nu poate insera**. Planul interzice explicit o rezervă MAX+1 în Python. Dacă
ruta trebuie exercitată mai devreme, cele șapte `ALTER` se aplică de mână și se scrie aici.
