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

## 2. schema_sync pentru o baza existenta dar goala — PE SERVER

`Schema/SchemaSyncRunner.vb` — nou. Ruleaza scriptul Python care construieste structura,
**pe VPS, prin SSH**. Niciodata pe masina operatorului.

> **Corectie fata de prima scriere a acestui pas.** Prima varianta pornea venv-ul local al
> depozitului. **Gresit, si operatorul a spus-o direct:** scriptul trebuie sa ruleze pe server.
> Motivul e mai adanc decat comoditatea — proiectul Python **desfasurat pe VPS**, cu `config.py`
> al lui si interpretorul lui, e cel care are voie sa modifice bazele. Un exemplar local pornit
> peste aceleasi baze ar fi **alt cod** indreptat spre acelasi server: alta versiune, alta
> configurare, alte dependinte. Diferenta nu se vede pana nu strica ceva.

Drumul intreg: `Verifier` gaseste ca baza-tinta exista dar are **zero tabele** → constatare
noua **`BAZA_FARA_TABELE`** (Blocant) → `VerifyAsync` o vede si cheama `OfferSchemaSyncAsync`
→ intrebare in romana, care **numeste serverul** → se lanseaza `ssh.exe` cu iesirea scriptului
**linie cu linie in jurnalul rularii** (`   | ` pentru stdout, `   ! ` pentru stderr) →
**verificarea se reia singura**.

Constatarea e deliberat **distincta de `TABEL_LIPSA`**: un tabel lipsa e o schema care a
derivat, dar ZERO tabele e o baza pe care cineva a creat-o goala, si are alt leac.

Reluarea se face cu `offerSchemaSync:=False`. Daca scriptul raporteaza succes dar nu a creat
nimic, a doua trecere **spune o data si se opreste**, in loc sa ofere la nesfarsit aceeasi
rulare.

### Comanda: ce s-a pastrat si ce s-a adaugat

Comanda din observatia operatorului era
`python3 -m routes.schema_sync.schema_sync --mode FORCE --targets [chosen_Dc]`.
**`python3` ramane exact asa** — e grafia corecta pe partea cealalta, care e Linux. Sablonul
implicit adauga doua lucruri, si amandoua sunt necesare:

1. **`--run`, care trebuie sa ramana.** Fara el `schema_sync.py` ajunge la
   `_ask("Executați acum?")` si **citeste de la stdin**. Pe un canal SSH neinteractiv citirea
   aia da EOF — scriptul ori abandoneaza, ori sta acolo la infinit in timp ce formularul
   asteapta un proces care asteapta un om. (In plus stdin se inchide imediat dupa `Start`, si
   clientul primeste `-n`, ca sa nu citeasca fluxul nostru deloc.)
2. **`cd` catre radacina proiectului de pe server.** Modulul `routes.schema_sync.schema_sync`
   se rezolva doar de acolo. Dosarul din implicit e un **substituent** — nimeni din depozit nu
   stie unde sta proiectul pe VPS.

### Cele patru reguli ale conexiunii

- **`BatchMode=yes`** — jumatatea SSH a aceleiasi reguli ca `--run`: clientul **pica** in loc
  sa ceara o parola sau o fraza de acces. O intrebare nu poate fi raspunsa dintr-un formular,
  deci singura alternativa la esec ar fi blocarea. Inseamna si ca **autentificarea e pe CHEIE**:
  unealta nu cere, nu tasteaza, nu pastreaza si nu trimite nicio parola.
- **`StrictHostKeyChecking` ramane pe implicit**, nu pe `accept-new`. O unealta de migrare nu
  are ce cauta in rolul de a decide tacut ca are incredere intr-un server pe care nu l-a mai
  vazut. Cu `BatchMode=yes` o gazda necunoscuta pica repede, iar `HostKeyHint()` ii spune
  operatorului sa se conecteze o data de mana si sa accepte el amprenta.
- **`IdentitiesOnly=yes`** cand e data o cheie — altfel un agent poate oferi intai altele si
  serverul poate rupe conexiunea pentru prea multe incercari.
- **Codul 255 e numit explicit** in jurnal SI in caseta: vine de la CLIENTUL SSH, nu de la
  script, deci scriptul nu a pornit. Fara asta operatorul ar cauta pe server urmele unei rulari
  care nu a existat.

### Doua lucruri de siguranta

- **Numele DC-ului e verificat cu o lista alba** (`^[A-Za-z0-9_]+$`) inainte sa intre in
  comanda de pe server. Asta e **singurul loc** in care o data citita dintr-un fisier Access
  trece intr-un interpretor de comenzi de pe alta masina; un DC vine din `cai.DC`, e data de
  operator, nu constanta, iar un nume cu accent grav, punct-si-virgula sau `$(` **ar rula ca
  o comanda pe server**. Verificat, nu escapat — orice DC real arata ca `000_DEMO`, deci nu se
  refuza nimic legitim.
- **Argumentele se dau prin `ArgumentList`, nu ca un sir concatenat**, ca sa plece comanda
  la distanta ca **un singur argument** desi are in ea spatii, punct-si-virgula si ghilimele.

`Validate` verifica clientul, tinta, cheia si portul **inainte** de pornire, ca o configurare
gresita sa fie spusa in romana, nu ca eroare Win32. Dosarul **de pe server** nu poate fi
verificat de aici — un `cd` gresit se vede ca «No such file or directory» in jurnal, imediat si
zgomotos, la prima rulare.

`PYTHONIOENCODING=utf-8` se pune **pe latura cealalta** (`export … ; comanda`), fiindca partea
Python scrie romana cu diacritice si fara el ar ajunge mojibake ori de cate ori locala
serverului nu e UTF-8. E pus de cod, nu lasat in setare, ca un operator care editeaza comanda
sa nu-l poata scapa din greseala.

Oprirea omoara **arborele de procese** al clientului. Mesajul spune «se inchide conexiunea
SSH», nu «se opreste schema_sync», fiindca **asta e adevarul**: ce face scriptul de pe server
dupa ce cade canalul e treaba serverului. O schema aplicata pe jumatate e exact ce cauta
verificarea care urmeaza.

**Setarile** (`sshExecutable`, `sshTarget`, `sshKeyFile`, `sshPort`, `schemaSyncRemoteCommand`)
stau in `migrator-settings.json`. **Se ghiceste doar CLIENTUL** — `ssh.exe` e parte fixa a
Windows-ului de la 10 incoace, deci acolo exista un raspuns corect. Serverul, cheia si dosarul
de pe server **nu se ghicesc deloc**: niciunul nu e scris nicaieri in depozit, iar o inventie
plauzibila e mai rea decat un camp gol, care macar spune ca e gol.

**Nicio parola nu ajunge la script si niciuna nu apare pe linia de comanda** — scriptul isi ia
singur configurarea din `config.py`-ul serverului. Tocmai de aceea linia de comanda **se scrie
in jurnal**: nu poarta nicio acreditare.

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

Nimic din pasul asta nu a fost vazut pe ecran. **Niciun `ssh` nu a fost lansat, nicio
conexiune la VPS nu a fost deschisa, `schema_sync` nu a fost rulat niciodata din formular** —
si nici nu putea fi: `sshTarget` e gol implicit, fiindca adresa serverului nu e scrisa nicaieri
in depozit. Prima rulare reala are deci de dovedit patru lucruri deodata: ca `sshTarget` si
cheia sunt bune, ca amprenta gazdei e deja acceptata (altfel cod 255), ca dosarul din
`schemaSyncRemoteCommand` chiar exista pe server, si ca `python3` de acolo vede modulul.
Esecurile sunt in ordinea asta si toate se citesc din jurnal.

`dotnet build KBot.sln` — **0 erori**. Fara teste automate si fara pas de regresie, la cererea
explicita a operatorului (si conform §9 al planului).
