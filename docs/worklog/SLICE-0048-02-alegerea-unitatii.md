# SLICE-0048-02 — pașii 1–2 ai ingestiei și alegerea unității de către operator

A doua transă din felia 0048 (portarea conductei de ingestie FOREXE din Access VBA în API-ul
Flask, plan: `docs/PLAN_ForexeIngest.md`). Sub-numărul urmează pașii §7 ai planului, cum cere
planul însuși; nu s-a consumat un număr nou de felie.

Transa livrează **ruta de ingestie cu pașii 0, 1 și 2** și, peste ea, **drumul dus-întors prin
care operatorul spune cărei unități îi aparține o clasificație** atunci când perechea
`(SS, ClsfE)` se potrivește cu mai multe. Pașii 3–8 **nu** sunt portați — vezi §4.

---

## 0. De unde vine întrebarea (și de ce nu e o invenție)

Firul (2) al feliei 0048 din `KBOT_STATUS.md` spunea: invariantul `(SS, ClsfE)` e măsurat pe
prea puțin, iar dacă o pereche chiar dezacordă pe `IdUnitate`, **D17 se oprește și
raportează**. Citind sursa Access s-a văzut că oprirea nu e ce făcea sistemul vechi.
`Obtine_IdUnitate_Din`, în `mdl_FX_Tasks_Receive_DWN`:

```vb
If CLng(RC.RecordCount) <= 0 Then
    FX_IdUnitate = -1
    Obtine_IdUnitate_Din = False
ElseIf CLng(RC.RecordCount) = 1 Then
    FX_IdUnitate = RC!IdUnitate
    Obtine_IdUnitate_Din = True
Else
    DoCmd.OpenForm "FX_Unitate", acNormal, , , , acDialog, SS & "|" & ClsfE
    Obtine_IdUnitate_Din = (FX_IdUnitate <> -1)
End If
```

Deci ambiguitatea **nu e un defect și nu e o noutate**: Access avea un formular modal pentru
ea și întreba operatorul **de fiecare dată**. Ce adaugă K-BOT e că întrebarea trece prin HTTP
(serverul nu poate deschide o fereastră pe ecranul nimănui) și că operatorul poate bifa să nu
mai fie întrebat pentru acea combinație.

**Decizia operatorului pentru transa asta, textual:** «ask everytime, but add a checkbox to not
ask again for the same combo — if the user ticks it, then do it silently for the next ones. if
a new combo comes up — ask again for that combo.» Așa s-a construit: întrebarea e implicită,
memoria e **per pereche `(SS, ClsfE)`**, opt-in, și niciodată globală.

---

## 1. Ce s-a schimbat și de ce

### 1.1 Serverul — `routes/forexe/prelucrare.py` (pașii 0, 1, 2)

Ruta `POST /api/forexe/prelucrare`, `@require_session`, baza din sesiune (o bază = o unitate),
totul într-o singură tranzacție (D10).

- **Pasul 1** (`Prelucrare_Angajament`): port rând cu rând. `DescriereAngajament` ▸ `Descriere`,
  `StareAngajament` ▸ `Stare` (**intenția**, nu defectul citat la §5.1 din plan),
  `DataAngajament` ▸ `DataCreare`, `DataInceputDerulare` ▸ `DataDefinitivare`. La update, un
  câmp gol **nu** șterge ce e deja acolo — de-asta `UPDATE`-ul se compune din bucăți, nu e unul
  fix. `DC` și `Preluat` se pun doar la insert și nu se ating la update, exact ca în
  `angajamente.py`.
- **Pasul 2** (`Prelucrare_Indicatori`): rezolvarea unității și a clasificației, apoi upsert pe
  `(CodAngajament, CodIndicator)`. `NrCrt` e contorul **local** din VBA (pornește de la 0 la
  fiecare apel, crește doar pentru rânduri noi). D18 se respectă: `Receptii` și `Plati` nu se
  scriu niciodată pe calea asta.

### 1.2 Rezolvarea unității — `routes/forexe/prelucrare_unitate.py`

Traducerea interogării Access pe MariaDB:

| Access | MariaDB | de ce |
|---|---|---|
| `ClasificatiiG` | `Clasificatii` | e chiar tabelul de sub query |
| `INNER JOIN Cai ... WHERE Cai.DC = DC()` | `INNER JOIN Unitati` | `cai` era registrul de fișiere `.accdb`; filtrul pe `DC` e implicit, suntem deja conectați la baza aia |
| `Cai.AlteDetalii` | `Unitati.Detalii` | `AlteDetalii` **nu** s-a migrat (`MAPARE_NOMENCLATOARE.md` §2); `Detalii` = `cai.NumeUnitate`, eticheta din selectorul de unități al migratorului |

`INNER JOIN`-ul se păstrează intenționat: `Clasificatii.IdUnitate` e NULL-abil, iar o
clasificație fără unitate nu poate răspunde «care unitate» — cade la fel cum cădea și în Access.
**Niciun `LIMIT 1` în fișierul ăsta**: rutele de citire îl folosesc pe același nomenclator
fiindcă acolo alegerea greșită schimbă doar o etichetă; aici ar atașa un indicator la altă
subunitate și nu s-ar vedea luni de zile.

Trei rezultate, ca în VBA:

- **0 candidați** ▸ eroare care oprește rularea, numind indicatorul și perechea. (Access:
  `False` ▸ `GoTo Iesire`, adică pasul eșua fără să spună de ce.)
- **1 candidat** ▸ se folosește.
- **mai mulți** ▸ se caută un răspuns: întâi în `alegeri` din cerere, apoi în tabela de memorie;
  dacă nu e niciunul, se strânge întrebarea.

**Toate** perechile ambigue se rezolvă ÎNAINTE de orice scriere în `FX_Indicatori`, iar
întrebările se strâng **toate** înainte de a se ridica. Access rezolva unitatea în mijlocul
buclei de scriere; înăuntrul unei tranzacții diferența nu se vede (rollback-ul șterge oricum),
dar așa operatorul răspunde o dată pentru tot angajamentul, nu o dată per întrerupere.

A doua căutare, `ClsfSal` ▸ `IdClsfAcc` în interiorul unității alese (D17 + D7), rămâne
tolerantă ca în Access: negăsit = `NULL` + avertisment, nu eroare (D19). **Excepție conștientă:**
dacă aceeași `ClsfSal` are `IdClsfAcc` DIFERITE în aceeași unitate, se ridică. Dicționarul VBA
ar fi păstrat ultima citită, arbitrar — iar «arbitrar» acolo înseamnă indicatorul pe altă
clasificație.

### 1.3 Contractul de 409

```json
{ "error": "O clasificație se potrivește cu mai multe unități. Alegeți unitatea și trimiteți din nou.",
  "reason": "ALEGERE_UNITATE",
  "cod": "AAB37CNBK95",
  "alegeri_necesare": [
    { "ss": "02E", "clsfe": "200101", "clsf": "02E- 65. 04. 02. 20. 01. 01",
      "cod_indicator": "AAB", "indicatori": ["AAB", "AAC"],
      "unitati": [ { "id_unitate": 75, "detalii": "SC29 LOCAL",
                     "sursa_sector": "02A", "cod_program": "P75" }, … ] } ] }
```

**409, nu 400:** cererea nu e greșită; serverului îi lipsește o informație pe care doar un om o
are. `reason` e cod-motiv stabil, același tipar ca la 401-urile din `routes/auth/guard.py`, iar
`KBot.Api.ApiException` îl poartă deja ca `Reason`. **Tranzacția e derulată înapoi înainte de a
răspunde** — nici angajamentul scris la pasul 1 nu rămâne.

`indicatori` poartă TOȚI indicatorii care folosesc perechea, nu doar cel care a declanșat
întrebarea, fiindcă răspunsul li se aplică tuturor.

### 1.4 Memoria — `FX_Alegeri_Unitate` (`sql/0048_alegeri_unitate.sql`)

Un rând per pereche `(SS, ClsfE)`, cu `IdUnitate`, **`UN`** (cine a ales) și **`DataAlegere`**.
Cheia unică pe pereche face ca o re-bifare să ÎNLOCUIASCĂ răspunsul, nu să adauge un al doilea.

Urma (cine/când) e acolo tocmai pentru riscul pe care bifa îl introduce: o alegere greșită
altfel ar rămâne ascunsă. Cu tabela asta, «cine a hotărât asta și când» se răspunde cu un
`SELECT`, iar ștergerea rândului readuce întrebarea.

Trei decizii de scris pe față:

1. **Scrierea alegerii e ÎN tranzacție.** Dacă rularea pică mai târziu, alegerea se derulează
   înapoi împreună cu ea și operatorul e întrebat din nou. Alternativa — commit separat — ar
   lăsa în urmă un răspuns reținut pentru o salvare care nu s-a întâmplat.
2. **Tabela lipsă e zgomotoasă, nu tăcută.** `UnitChoiceTableMissing` numește fișierul `.sql` de
   rulat. Nu se degradează în «n-am alegeri reținute»: atunci bifa ar părea că merge, iar data
   viitoare s-ar întreba din nou fără nicio explicație. **Dar** tabela se atinge DOAR pe ramura
   ambiguă, deci o bază care nu întâlnește niciodată o ambiguitate merge mai departe fără ea.
3. **O alegere reținută care nu mai e printre candidați NU se folosește** — se avertizează și se
   întreabă din nou. Nomenclatorul se poate schimba sub ea, iar asta e exact modul în care bifa
   poate deveni periculoasă.

`sql/0048_alegeri_unitate.sql` se aplică **și pe `AVACONT_SURSA`**, spre deosebire de cele șapte
`ALTER` din transa 01: acolo scutirea exista fiindcă `AUTO_INCREMENT` dezarma o poartă de
siguranță a migrării; aici se adaugă un tabel nou, gol, pe care migrarea nu-l scrie niciodată.

### 1.5 Clientul — POCO-uri, `ApiClient`, dialogul, coordonatorul

- **`KBot.Domain`**: `PrelucrareRaspuns` (cu `Stare` = `Salvat` / `AlegereUnitate`),
  `AlegereNecesara`, `UnitateCandidat`, `AlegereUnitate`. Plus **`PrelucrareRezultat` s-a MUTAT**
  din `KBot.App\Forexe\WorkflowResultStore.vb` în `KBot.Domain`: `KBot.Api` are nevoie de el ca
  să compună cererea, și nu poate referi `KBot.App`. Tipul e neschimbat — doar proiectul.
- **`ApiClient.TrimitePrelucrareAsync`**: un 409 cu `reason = ALEGERE_UNITATE` NU aruncă, se
  întoarce ca **stare** — același tipar ca `PdfDownloadStatus.NotFound`, unde «nu există» e o
  cale normală. Un 409 cu alt cod-motiv, sau cu corp non-JSON, iese ca `ApiException`, ca orice
  alt conflict.
- **`AlegereUnitateForm`** (`src/KBot.App/Forexe/`): moștenește `KBotThemedForm`, **toate**
  controalele în `.Designer.vb`, controale K-BOT (`KBotDataView`, `KBotNotice`, `KBotCaptionBar`,
  `KBotToolTip`), etichete românești cu diacritice literale — la fel ca `LoginForm`. Antet cu
  angajamentul / indicatorul / clasificația, o grilă cu **numele** unităților (plus sursă-sector,
  program și cod), bifa «Nu mă mai întreba pentru această combinație», `Renunță` / `Alege
  unitatea`. Fără selecție nu se închide: a ghici pentru operator ar readuce exact defectul pe
  care dialogul îl repară.
- **`PrelucrareCoordinator`**: trimite ▸ dacă e 409, întreabă ▸ retrimite ACELEAȘI date cu
  alegerile atașate. Serverul nu ține nimic între încercări, deci a doua cerere poartă tot
  pachetul. Renunțarea la orice întrebare oprește tot și nu retrimite nimic. Bucla are un plafon
  (`MaxRunde = 5`) care ridică o eroare limpede în loc să se învârtă.
  Întrebătorul e **injectabil** (`Func(Of AlegereNecesara, String, Integer, Integer, AlegereUnitate)`);
  implicit deschide dialogul. Fără cuiul ăsta bucla nu s-ar putea verifica decât deschizând
  ferestre pe ecranul operatorului în timpul testelor. Același tipar ca `citesteIstoric` din
  `ForexeController.DownloadNodeAsync`.

### 1.6 O adăugire mică în `KBot.Controls`

`KBotNotice.Message` (doar-citire, `Browsable(False)`, serializare ascunsă). `Visible` nu
răspunde la «s-a arătat ceva?» — getterul lui urcă prin părinți, deci pe un formular neafișat
răspunde `False` chiar după `Show`. Proprietatea asta răspunde.

---

## 2. Fișiere atinse

**Noi — server**
- `PYTHON/routes/forexe/prelucrare.py` — ruta, pașii 0/1/2
- `PYTHON/routes/forexe/prelucrare_unitate.py` — rezolvarea unității + memoria
- `sql/0048_alegeri_unitate.sql` — `FX_Alegeri_Unitate`
- `PYTHON/tests/test_forexe_prelucrare_unitate.py` — 28 de teste
- `PYTHON/tests/test_forexe_prelucrare_route.py` — 11 teste

**Noi — client**
- `src/KBot.Domain/PrelucrareInfo.vb`, `src/KBot.Domain/PrelucrareRezultat.vb` (mutat)
- `src/KBot.App/Forexe/AlegereUnitateForm.vb` + `.Designer.vb`
- `src/KBot.App/Forexe/PrelucrareCoordinator.vb`
- `tests/KBot.Api.Tests/PrelucrareApiClientTests.vb` — 11 teste
- `tests/KBot.App.Tests/AlegereUnitateFormTests.vb` — 10 teste
- `tests/KBot.App.Tests/PrelucrareCoordinatorTests.vb` — 11 teste

**Modificate**
- `PYTHON/routes/forexe/__init__.py` — înregistrarea rutei
- `src/KBot.Api/IApiClient.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb` (DTO-urile de wire)
- `src/KBot.App/Forexe/WorkflowResultStore.vb` — `PrelucrareRezultat` a plecat de aici
- `src/KBot.Controls/Notice/KBotNotice.vb` — `Message`
- șapte fișiere de test din `KBot.App.Tests` — falsurile de `IApiClient` au primit noul membru

---

## 3. Rezultatele testelor

**Python** — `PYTHON/.venv/Scripts/python.exe -m pytest tests/ -q`

| | trecute | sărite |
|---|---:|---:|
| înainte (fără fișierele noi) | 307 | 14 |
| după | **346** | **14** |

**+39 de teste, zero sărituri noi.** Testele de rută rulează **offline**: în loc de
`from main import app` (care trage tot serverul, `pandas` inclus, absent aici) își fac un Flask
gol cu DOAR `forexe_bp` înregistrat — aceeași rută, aceeași gardă `@require_session`, aceleași
corpuri — iar baza de date e o conexiune falsă care răspunde după forma SQL-ului.

**.NET** — `dotnet build KBot.sln`: **0 erori**. Avertismentele sunt cele preexistente `MSB3825`
(BinaryFormatter în cinci `.resx` de vederi); niciunul nou.

| proiect | trecute | căzute | notă |
|---|---:|---:|---|
| `KBot.Controls.Tests` | 917 | 0 | |
| `KBot.App.Tests` | 169 | 13 | căderile sunt **preexistente** — vezi §4.2 |
| `KBot.Api.Tests` | 78 | 1 | idem (68 + 11 noi) |
| `KBot.Domain.Tests` | 14 | 3 | idem |

Cele 21 de teste noi de `KBot.App.Tests` și cele 11 de `KBot.Api.Tests` trec toate. Baza de
comparație pentru `KBot.App.Tests` s-a măsurat cu `git stash` pe arborele curat: **13 căzute /
148 trecute** înainte, **13 căzute / 169 trecute** după — aceleași 13, plus 21 noi.

---

## 4. Rămas neverificat sau amânat

### 4.1 Ce nu face transa asta

1. **Pașii 3–8 NU sunt portați** — istoric, rezervări, recepții, plăți/încasări, marcarea
   `Prelucrat`, `FX_Extrase`. Nu se scrie nimic în tabelele lor. Fiecare răspuns de 200 poartă
   asta scris în `avertismente`, în română, ca nicio rulare să nu pară completă fără să fie. Un
   test pinuiește avertismentul, ca să plece odată cu pașii.
2. **NIMIC nu cheamă încă `PrelucrareCoordinator`.** `ForexeController.DownloadNodeAsync` se
   oprește tot la salvarea JSON-ului local. Legarea (§9 din plan) e amânată **deliberat**: a
   posta automat o conductă pe jumătate la fiecare descărcare ar schimba în tăcere ce face
   sistemul pentru cine voia doar o citire. Se leagă odată cu pașii 3–8.
   *(`KBot.DevHarness` nu poate ține un declanșator: nu referă `KBot.App` — ar fi ciclu.)*
3. **Nimic nu a rulat pe MariaDB.** Nici ruta, nici `sql/0048_alegeri_unitate.sql`.
4. **Dialogul nu s-a văzut niciodată pe ecran.** Testele îl conduc headless (`Pregateste()`,
   `Confirma()`, `Renunta()` — exact metodele pe care le cheamă butoanele). Ce rămâne neacoperit
   e DOAR legarea `Handles`, scrisă în designer. `Button.PerformClick` nu face nimic cât timp
   lanțul de părinți e invizibil, deci nu se poate apăsa un buton fără a arăta o fereastră.

### 4.2 ⚠ Teste CĂZUTE care nu sunt din transa asta

Șapte căderi în trei proiecte au **o singură cauză**, deja pe branch înainte de transa asta:

`src/KBot.Domain/DdfInfo.vb:130`
```vb
Return $"{data}" '$"{numar} - {data}"
```

Prefixul cu numărul reviziei (`Format(NumarRev, "@@@")` din Access — aliniere la dreapta în trei
caractere cu SPAȚII) e comentat, iar `numar` a rămas o variabilă nefolosită. Testele care îl
pinuiau au rămas roșii: `DdfInfoTests.EtichetaRevizie_*` (3), `ApiClientTests.GetDdf_Formats
RevisionLabel_WithSpacePadding_Not_Zeroes` (1), `DdfViewTests.Leaf_CaptionPadsRevisionNumber
WithSpaces` (1). **Arată ca o schimbare voită de produs cu teste nereînnoite, nu ca un accident**
— de-asta nu s-a atins aici: dacă eticheta trebuie să fie doar data, testele se schimbă; dacă nu,
se scoate comentariul. E o decizie a operatorului.

Restul căderilor preexistente din `KBot.App.Tests` (11 din 13, cele două rămase fiind cele de mai sus): `DdfXfaParserTests` (2),
`XfaXmlPreviewTests` (1), `MainFormNavItemsTests.Designer_WroteLiteralDiacritics_NotEscapes` (1),
`IstoricViewTests` (6) — nemăsurate în transa asta, doar constatate.

### 4.3 Comportament portat fidel, dar contraintuitiv — **de decis**

`_step2_indicatori`, ramura de update: VBA-ul (`RC.Edit`) rescrie **doar** cele patru coloane de
bani. **NU** rescrie `IdUnitate`, `IdClsf`, `SS` sau `NrCrt`. Deci dacă indicatorul există deja,
alegerea de unitate pe care tocmai a făcut-o operatorul **nu ajunge în rând** — se aplică abia la
un insert. Access făcea exact la fel: deschidea `FX_Unitate`, întreba, și apoi arunca răspunsul
pe ramura asta.

S-a portat fidel și e comentat în cod la locul faptei. Dacă operatorul vrea altfel — ca alegerea
să rescrie `IdUnitate` și pe update — e o linie de schimbat, dar e o **abatere de la Access** și
trebuie hotărâtă, nu strecurată.

### 4.4 Neverificabil din depozit

- **Formularul `FX_Unitate` NU există în `FX_System_Export`** — nici sub numele ăsta, nici sub
  altul. Tot ce se știe despre el se știe din apelul care îl deschidea (modal, primea
  `SS & "|" & ClsfE`). Câte coloane arăta și cum arăta răspunsul **nu se poate ști**, deci
  aspectul lui `AlegereUnitateForm` e **proiectat, nu portat**.
- **Tipul lui `Unitati.IdUnitate`** (ținta cheii străine din `.sql`) e luat din
  `MariaDB_Schema/000_DEMO.sql`, care e din 22.08 și nu s-a confirmat pe serverul viu. Fișierul
  poartă proba de rulat înainte; un tip nepotrivit cade cu errno 150 — zgomotos, nu tăcut.
- **`AlegereUnitateForm.Designer.vb` e scris la 96 dpi** (`AutoScaleDimensions = (7, 15)`), pe
  când restul formularelor din depozit sunt autorizate la 150% (`(10, 25)`). `AutoScaleMode.Font`
  le face echivalente la rulare, dar cele două nu se pot amesteca în același fișier. Dacă
  operatorul deschide formularul în designerul Visual Studio la 150% și salvează, VS îl va
  rescrie — ceea ce e în regulă.
- **Invariantul `(SS, ClsfE)` rămâne măsurat pe prea puțin** (firul (2) al feliei 0048 din
  STATUS). Transa asta nu-l mai are ca blocaj — ambiguitatea are acum un răspuns —, dar
  testul-constatare §10.7 din plan tot trebuie rulat pe baza vie, ca să se știe **cât de des**
  va fi întrebat operatorul.
