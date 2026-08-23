# SLICE-0045-06 — schema_sync, bara de progres, bifele legate de obiect, panoul de constatari

Data: 23.08.2026 · Slice **0045**, pasul 06 (pas de raspuns la revizia operatorului).

## De ce exista pasul asta

Pasul 05 se incheiase. Operatorul a **redesenat el formularul** si a trimis sase observatii.
Pasul asta le implementeaza pe toate, fara sa atinga aranjarea pe care si-a facut-o.

## 1. Formularul e al operatorului — codul se muleaza pe el, nu invers

`MigratorForm.Designer.vb` a fost modificat de operator si **nu a fost atins aici**. Ce s-a
schimbat acolo si ce a insemnat pentru codul din spate:

- `txtParolaForexe` / `lblParolaForexe` au fost **sterse**. Erau doua parole in pasul 05, una
  pentru fisierele de unitate si una pentru cele FOREXE. Acum e una singura,
  `txtParolaUnitati`, reetichetata **«Parolă fișiere»**. Codul din spate are deci o singura
  metoda `AccessPassword()` si o da amandurora — ceea ce se potriveste oricum cu descoperirea
  din 0045-01, unde **toate trei fisierele s-au deschis fara nicio parola**.
- A aparut `rtbInfoRowConstatari`, un panou de detaliu sub grila de constatari (punctul 6).
- A aparut `tlpButoane`, care aranjeaza butoanele.

**Consecinta de proiectare, luata deliberat:** optiunea `schema_sync` (punctul 4) **nu a primit
un buton**. Un buton nou ar fi insemnat sa modific aranjarea operatorului. E o **intrebare care
apare exact cand se aplica** — dupa o verificare care a gasit o baza fara tabele — si nu se
vede niciodata altfel. E si mai corect ca flux: nu e un pas al migrarii, e leacul unei singure
constatari.

## 2. schema_sync pentru o baza existenta dar goala

`Schema/SchemaSyncRunner.vb` — nou. Ruleaza scriptul Python care construieste structura.

Drumul intreg: `Verifier` gaseste ca baza-tinta exista dar are **zero tabele** → constatare
noua **`BAZA_FARA_TABELE`** (Blocant) → `VerifyAsync` o vede si cheama `OfferSchemaSyncAsync`
→ intrebare in romana → se ruleaza scriptul cu iesirea lui **linie cu linie in jurnalul
rularii** (`   | ` pentru stdout, `   ! ` pentru stderr) → **verificarea se reia singura**.

Constatarea e deliberat **distincta de `TABEL_LIPSA`**: un tabel lipsa e o schema care a
derivat, dar ZERO tabele e o baza pe care cineva a creat-o goala, si are alt leac.

Reluarea se face cu `offerSchemaSync:=False`. Daca scriptul raporteaza succes dar nu a creat
nimic, a doua trecere **spune o data si se opreste**, in loc sa ofere la nesfarsit aceeasi
rulare.

### Doua abateri de la comanda primita, amandoua consemnate

Comanda din observatia operatorului era
`python3 -m routes.schema_sync.schema_sync --mode FORCE --targets [chosen_Dc]`.
Sablonul implicit are **doua diferente**, si amandoua sunt necesare:

1. **`--run` e adaugat si trebuie sa ramana.** Fara el `schema_sync.py` ajunge la
   `_ask("Executați acum?")` si **citeste de la stdin**. Pornit dintr-un formular cu fluxurile
   redirectate, citirea aia da EOF — scriptul ori abandoneaza, ori sta acolo la infinit in timp
   ce formularul asteapta un proces care asteapta un om. Cu `--run` e neinteractiv, singura
   forma care poate merge de aici. (In plus stdin se inchide imediat dupa `Start`, ca o
   versiune viitoare care totusi intreaba sa pice repede, nu sa blocheze formularul.)
2. **`python3` e o grafie de Linux.** Pe parcul asta interpretorul care are dependintele e venv-ul
   depozitului, `PYTHON\.venv\Scripts\python.exe`, iar modulul `routes.schema_sync.schema_sync`
   se rezolva doar cu dosarul de lucru pe `PYTHON\`.

Amandoua sunt **setari, nu constante** (`pythonExecutable`, `pythonWorkingFolder`,
`schemaSyncArguments` in `migrator-settings.json`), deci un operator cu alta asezare le poate
muta fara recompilare. Implicitul se **ghiceste**: `GuessPythonFolder` urca pana la sapte
nivele de la executabil cautand `PYTHON\routes\schema_sync` (urca in loc sa presupuna o
adancime fixa, fiindca aplicatia ruleaza si din `bin\Debug\net8.0-windows`, si din dosarul
instalat), iar `GuessPythonExecutable` prefera venv-ul.

`Validate` verifica interpretorul si dosarul **inainte** de pornire, ca o configuratie gresita
sa fie spusa in romana, nu ca eroare Win32.

`PYTHONIOENCODING=utf-8` e pus pe proces: partea Python scrie romana cu diacritice, si fara el
ar ajunge mojibake in jurnal pe o masina a carei pagina de cod a consolei nu e UTF-8.

Oprirea omoara **arborele de procese**, nu doar procesul lansat.

**Nicio parola nu ajunge la script si niciuna nu apare pe linia de comanda** — scriptul isi ia
singur configurarea de pe latura Python. Linia de comanda **se scrie in jurnal**, tocmai fiindca
nu poarta nicio acreditare.

## 3. Bara de progres se misca si la verificare

Pana acum bara se misca doar la transfer; verificarea o lasa moarta desi are zece porti si
poate dura. `Verifier` a capatat `StepCount` (11) si un apel `Step1(eticheta)` la fiecare
poarta, plus o functie de progres in constructor. Formularul are `BeginProgress` / `ShowStep` /
`EndProgress` / `ResetProgress`, si `StepFromWorker` face trecerea pe firul de interfata.
`BeginProgress(0)` inseamna **marquee** — asa merge bara in timpul lui `schema_sync`, unde nu
exista pasi de numarat.

## 4. Bifele se citesc din obiect, nu din indicele randului

Cuplajul pe care il consemnasem in 05 — «bifele grilei de unitati se citesc dupa indicele
randului» — a fost **scos**, nu documentat. Fiecare rand isi poarta acum obiectul in
`KBotDataRow.Tag`:

```vb
For Each row In dgvUnitati.Rows
    If Not IsTicked(row) Then Continue For
    Dim unit = TryCast(row.Tag, CaiUnit)
    If unit IsNot Nothing Then request.Units.Add(unit)
Next
```

Motivul e ca modul de dinainte **presupunea tacut ca randul _n_ al grilei e unitatea _n_ a
listei**. Presupunerea tine pana cand cineva filtreaza, sorteaza sau insereaza un rand — si
atunci nu pica, ci **migreaza ALTA UNITATE**. Cu `Tag` intrebarea nu se mai pune. Acelasi tipar
pentru grila de tabele (`TableMap`) si pentru cea de constatari (`Finding`).

## 5. Panoul de detaliu al constatarilor

Clic pe un rand din `dgvConstatari` umple `rtbInfoRowConstatari`, in forma ceruta:

```
COLOANĂ - nume_coloana
MESAJ   - mesajul constatarii
```

cu eticheta ingrosata si valoarea normala. Cele doua fonturi sunt **pastrate in campuri si
eliberate in `FormClosed`** — construite la fiecare clic ar scurge cate doua descriptoare GDI
de fiecare data. Se reconstruiesc doar daca fontul grilei s-a schimbat (schimbare de tema).

Randul se ia tot din `Tag`, nu din indice.

## Ce ramane nedovedit

Nimic din pasul asta nu a fost vazut pe ecran si niciun `schema_sync` nu a fost rulat din
formular. `dotnet build KBot.sln` — **0 erori**. Fara teste automate si fara pas de regresie,
la cererea explicita a operatorului (si conform §9 al planului).
