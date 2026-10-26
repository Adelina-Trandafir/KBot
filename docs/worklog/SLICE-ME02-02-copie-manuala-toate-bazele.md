# ME02-02 — Copia manuală pe toate bazele, cu motiv

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: nici o conexiune la server, nici un test.**

## Ce s-a cerut

Copia de siguranță din KBotAdmin se făcea pe o singură bază. Să meargă pe toate bazele, iar la copia manuală să se ceară și să se păstreze un motiv. Alegerile operatorului: doar bazele de unitate (nu `AVACONT_COMUN`, nu `AVACONT_SURSA`, **nici `000_DEMO`**, pe care îl copiază ea separat, pe nume); un singur motiv comun pentru toate.

## Ce s-a făcut

**Server** (`PYTHON/routes/admin_cli/admin_backup.py`, modul nou; verbele `backups`, `backup-text`, `backup` s-au mutat din `admin_cli.py`):

- `backup --db <baza>|ALL --reason <text>`: motivul e obligatoriu (5–2000 de caractere, spațiile strânse), refuzat și pe server, nu doar în fereastră.
- `ALL` = toate bazele de unitate **fără `000_DEMO`** (`_resolve_db` returna și `000_DEMO`, fiindcă `list_unit_databases` ia orice schemă care începe cu trei cifre; excluderea e explicită). `000_DEMO` se poate copia singur, alegându-l în arbore.
- O bază care eșuează nu oprește celelalte; răspunsul are un rând pe bază (`db`, `path`, `name`, `size`, `error`, `journal_error`).
- Fiecare copie intră în `AVACONT_COMUN.Jurnal`: acțiunea `ADMIN_BACKUP`, ținta = numele fișierului, detalii = motivul, rezultat `OK`/`EROARE`. Până acum `backup` nu scria nimic în jurnal. Dacă jurnalul nu poate fi scris după o copie reușită, se raportează în `journal_error`, nu se ascunde.
- `backups` pune motivul fiecărei copii din jurnal (coloana nouă `reason`).

**Client** (KBotAdmin):

- `BackupsView`: butonul «Copie nouă» merge și pe `<TOATE BAZELE>` (nu mai cere o singură bază). Cere motivul în `ReasonForm`, cu textul care spune câte baze se copiază și cât au ele în total (din coloana de mărime a arborelui). Rezultatul: linia din bara de stare; dacă ceva a eșuat, o fereastră cu fiecare bază care nu s-a copiat și de ce.
- `ReasonForm` a primit un constructor cu alte texte (titlu, buton, indicii); constructorul vechi, pentru respingerea cererilor, merge ca înainte.
- Grila are coloana nouă «Motiv»; DTO-uri noi `BackupItem` / `BackupRun` în `Dtos.vb`.

## Verificat

- `py_compile` pe `admin_cli.py` și `admin_backup.py`; parserul acceptă `backup --db ALL --reason …`; curățarea motivului (scurt → refuz, spații strânse) pe date de probă. Nu s-a atins serverul sau o bază.
- `dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 avertismente, 0 erori.

## Nevalidat / de știut

- Nimic nu a rulat pe server. Fișierul nou `admin_backup.py` și `admin_cli.py` modificat trebuie trimise de operator (publicare de mână). `admin_cli` pornește la fiecare apel SSH, deci serviciul Flask nu trebuie repornit.
- Fișierele de copie rămân pe serverul VPS, **în același loc cu baza**; ele nu sunt o protecție dacă se pierde mașina. Copiile locale criptate (ME02-03…) rezolvă asta.
- `dump_database` (neschimbat) nu folosește `--hex-blob`, spre deosebire de `copy_db_to_demo.sh`. Pentru tabelele cu PDF-uri/imagini, un dump text poate să nu se restaureze byte cu byte. **De verificat înainte să te bazezi pe aceste dump-uri la o restaurare.** Nu am schimbat comanda (e folosită și de `schema_sync`).
- Parola bazei apare în linia de comandă a `mysqldump` pe server (vizibilă în `ps` pentru un utilizator local); la fel ca înainte.

## Fișiere

`PYTHON/routes/admin_cli/admin_backup.py` (nou), `PYTHON/routes/admin_cli/admin_cli.py` (verbele mutate, antet, înregistrare), `KBotAdmin/Views/BackupsView.vb` + `.Designer.vb`, `KBotAdmin/ReasonForm.vb`, `KBotAdmin/Server/Dtos.vb`.
