# ME02-04 — Verbul de server `snap-run` (doar citire)

20.10.2026. **SCRIS; `py_compile` curat. NERULAT pe server, fără nicio conexiune la o bază.**

## Ce s-a făcut

`PYTHON/routes/admin_cli/admin_snapshot.py` (nou, ~380 de linii), înregistrat în `admin_cli.py`:

    snap-run --db D [--state-file P] [--mode fast|full] [--page-rows N] [--page-bytes N]

- O singură tranzacție `START TRANSACTION WITH CONSISTENT SNAPSHOT` (REPEATABLE READ), ca `--single-transaction`: copia nu prinde jumătate dintr-o operație. Nimic nu se scrie în baze.
- Fiecare tabelă e tăiată în **pagini pe intervale de cheie primară** care se ating și acoperă tot spațiul de chei (prima `lo = null`, ultima `hi = null`); chei compuse = tupluri.
  Tabelele fără cheie primară (sau cu cheie binară/generată) sunt o singură pagină.
- Valoarea de control a paginii o calculează **baza de date** (`COUNT(*)` + două `BIT_XOR` de MD5 pe rând). Clientul trimite paginile copiei precedente; cele cu aceeași valoare
  sunt răspunse doar prin descriere (`same`), fără să li se citească rândurile; cele schimbate se citesc din nou și se taie în pagini noi (500 de rânduri sau 4 MB).
  Un interval devenit gol rămâne ca pagină goală, ca intervalele să se atingă și data viitoare.
- Două valori de control: `sum` (rapidă: coloanele mari binare — PDF, imagini — contează prin LUNGIME când tabela are o coloană „modificat” de încredere: `ON UPDATE CURRENT_TIMESTAMP` sau `DataModif`)
  și `full` (MD5 pe conținut). Modul `full` compară `full`, ca să prindă un fișier înlocuit cu unul de aceeași lungime fără ca data să se schimbe.
- Coloanele generate (ex. `Clasificatii.Clsf*`, PERSISTENT) nu se salvează; DDL-ul se salvează fără `AUTO_INCREMENT=n` (se schimba la fiecare inserare).
- Valorile ies ca text (binarele în base64), pagina e comprimată cu zlib și pusă pe o linie JSON; views, triggere, rutine, evenimente ies ca înregistrări `extra`.
- `os.nice(10)`: copia nu trebuie să încetinească aplicația. Fișierul de stare trebuie să fie în `backup/` și e șters după citire.

Protocolul și forma înregistrărilor: antetul modulului și [ME02_FORMAT.md](../ME02_FORMAT.md).

## Verificat

- `py_compile` pe `admin_cli.py`, `admin_backup.py`, `admin_snapshot.py`. Revăzut de două ori pe hârtie (am găsit și corectat: golul de interval la ultima pagină, tabelele fără cheie
  care nu se mai recunoșteau, compararea blob-urilor între moduri, un `AUTO_INCREMENT` care schimba DDL-ul la fiecare rulare).

## Nevalidat / de știut înainte de prima rulare

- **Nu a rulat pe o bază reală.** Chestiuni care se văd doar acolo: sintaxa `(a,b) > (%s,%s)` pe chei compuse cu indexul folosit; performanța pe tabele mari (o interogare de control pe pagină);
  comparația unei chei numerice trimise ca text; `start_transaction(consistent_snapshot=True)` pe conexiunea cu `autocommit` pornit; `cursor(raw=True)` pe extensia C a conectorului.
- Un rând cu coloanele însumate mai mari decât `max_allowed_packet` face `CONCAT_WS` să dea NULL: rândul nu intră în valoarea de control (limită scrisă în antet).
- Chei `float`/`double` nu sunt potrivite (compararea ca text); nu există în șablon.
- Primul test recomandat: **o singură bază mică**, nu toate.
- Fișierele noi (`admin_backup.py`, `admin_snapshot.py`, `admin_cli.py` modificat) le trimiteți dumneavoastră pe server; `admin_cli` pornește la fiecare apel SSH, deci **nu e nevoie de repornirea serviciului**.

## Fișiere

`PYTHON/routes/admin_cli/admin_snapshot.py` (nou), `PYTHON/routes/admin_cli/admin_cli.py` (înregistrare + antet).
