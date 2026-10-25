# Plan ME02 - copii de siguranta locale, criptate, cu istoric (time-machine)

> **Stare 20.10.2026:** ME02-01...08 sunt scrise si compilate; nimic nu a rulat pe server. Abateri de la plan: restaurarea e un **export SQL** pentru o baza zz_*
> (fara incarcare automata pe server), iar "Istoric" este fereastra **Istoricul unui rand** din pagina "Copii locale", nu o pagina separata. Detalii in worklog-urile SLICE-ME02-*.

Reper de numerotare initial: ME02 fusese presupus, cu 01…08, dupa ME01-04; registrul pastreaza explicit aceasta provenienta. Consolidarea D001-30 nu atribuie un numar nou acestei lucrari.

**Reper ulterior, 21.10.2026:** [registrul](worklog/state/KBOT_STATUS_ME02.md) consemneaza confirmarea operatorului pentru primele doua copii locale, dupa corectia permisiunilor SFTP. Starea din 20.10 de mai sus precede acea confirmare; Verifica/export/istoric/curatare/stergere si mesajul de la deschidere nu sunt declarate incercate. [Istoricul 03…08](worklog/SLICE-ME02-03-format-si-motor.md) pastreaza reviziile; 01/02 sunt jurnale separate.

## Hotarari luate de operator
- Copiile se fac pe PC-ul operatorului, spatiu suficient, ~cativa GB in total.
- Doar bazele de unitate. NU `AVACONT_COMUN`, NU `AVACONT_SURSA`, NU `000_DEMO` (le face operatorul cand vrea).
- Format propriu, criptat. Backup manual: pe toate bazele deodata, cu un motiv comun, obligatoriu.
- Se adauga coloane de data creare / modificare la tabelele care nu le au.

## Ce am verificat (din fisiere citite) si ce presupun
Verificat: `cmd_backup` refuza `len(dbs) != 1` (admin_cli.py:659-669) si nu scrie in Jurnal; `_resolve_db` accepta `ALL` si nu da
niciodata COMUN/SURSA (admin_common.py:67); **`_unit_dbs` = orice schema care incepe cu 3 cifre, deci `000_DEMO` ar intra in ALL**
(schema_common.py:426) - trebuie exclus explicit; `ReasonForm` exista in KBotAdmin; in `AVACONT_SURSA`: 133 tabele, 48 cu data de
modificare automata (ON UPDATE), 5 cu `DataModif` pusa de cod (PDF/imagini), 21 doar cu data creare, 59 fara nicio data; server
MariaDB 10.3.32 (din STATUS); nimic nu cauta `INSERT INTO t VALUES` pozitional.
Presupus / neverificat: marimea reala a bazelor (nu o stiu); ca bazele reale au aceeasi forma ca sablonul; migratoarele VB; cum
serializeaza JSON rutele ADECHIT `SELECT *` o coloana `datetime` noua (vezi riscul din ME02-01).

## Reguli care se aplica (din CLAUDE.md / memorie)
RULE 0 fara diacritice in cod; `k_` la parametri/locali; try/catch + `GlobalErrorLog`; mesaje doar prin `KBotMessage`; ferestre in
`.Designer.vb`; `KBotToolTip`; **nu rulez teste** (nici xUnit, nici randari) decat daca ceri - verificarea e functia "Verifica" din
produs; **nu public nimic pe server, nu ating `config.py`, nu fac git add/commit/push** - spun doar ce fisiere s-au schimbat si le
trimiti tu; KBotAdmin nu are ajutor (nu e pentru clienti), nu intra in nicio livrare.

## ME02-01 - Date de creare/modificare pe tabelele care nu le au
- Un `.sql` nou in `sql/` (ME02_01_...), generat de un script node din `MariaDB_Schema/AVACONT_SURSA.sql`, repetabil:
  `ALTER TABLE ... ADD COLUMN IF NOT EXISTS ...`, coloanele la sfarsit.
- Familii (fara nume nou): `VC_` si `AD_` -> `CreatedAt` / `UpdatedAt`; restul -> `DataAdaugare` / `DataModificare`.
  Tip exact ca in schema: `datetime NULL DEFAULT current_timestamp()` si `datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP`.
- Se sar: cele 48 cu ON UPDATE, cele 5 PDF/imagini cu `DataModif`, `_snap_clsf` (fara cheie primara), tabelele de lacat/efemere
  (`AD_Lock`, `FX_NumberLock`, `VC_ContextLock`, `AD_PortalChallenges`) - lista finala o pun in worklog pentru confirmare.
  Cele 21 doar-cu-creare primesc doar coloana de modificare.
- Operatorul ruleaza fisierul pe `AVACONT_SURSA`; `schema_sync` il duce in cele 22 de baze. Eu nu rulez nimic.
- **Risc de cercetat inainte de predare:** rute care fac `SELECT *` si trimit rezultatul ca JSON (ADECHIT `repository.py:79,111,134`,
  `parent_portal.py`, `parent_identity.py`, VERCON). Un `datetime` nou poate cadea la serializare. Auditez fiecare consumator;
  la nevoie tabela respectiva ramane fara coloana noua sau ruta isi alege coloanele.
- Rezultat: scriptul generator + fisierul `.sql` + lista exceptiilor + worklog. Dupa ce operatorul reimprospateaza
  `MariaDB_Schema/`, schema noua devine sursa pentru motorul de backup.

## ME02-02 - Copia manuala pe toate bazele, cu motiv
Server (`admin_cli.py`, verbul `backup`): accepta `--db ALL` si `--reason`; exclude `000_DEMO` din ALL (ramane alegibil singur);
refuza motivul gol (si pe server, nu doar in fereastra); o baza care esueaza nu opreste restul - raport pe baza
(`results: [{db, path, size, error}]`), nimic inghitit; fiecare copie intra in `AVACONT_COMUN.Jurnal` (actiune BACKUP, tinta = fisierul,
detalii = motivul). `backups` arata motivul din Jurnal pe fiecare dump.
Client (`BackupsView`): scot `RequireSingleDb`; `ReasonForm` obligatoriu (un motiv comun); confirmarea spune cate baze; rezultatul
pe baze intr-o lista; coloana "Motiv" in grila. DTO-uri noi in `Dtos.vb`.
Nota: dump-ul text de pe server ramane (e nevoie de el inaintea operatiilor distructive); noul format e separat.

## ME02-03 - Formatul si motorul (fara server)

[ME02_FORMAT](ME02_FORMAT.md) este specificatia scrisa inaintea codului din KBotAdmin/Backup: criptografie .NET standard, pagini pe intervale de chei (inclusiv compuse), obiecte deduplicate si instantanee complete cu DDL, metadate si blob-uri. PDF-urile semnate se pastreaza o singura data. Scrierea/citirea, verificarea integritatii si implementarea sunt in [03](worklog/SLICE-ME02-03-format-si-motor.md#slice-me02-03). Propunerea initiala DPAPI era de confirmat; hotararea ulterioara din 20.10 pastreaza cheia pe PC (cont Windows), iar parola din manager ramane recuperarea unica. Comportamentul la restart/schimbarea parolei Windows ramanea neprobat.

## ME02-04 - Verbele de server, doar citire
Fisier nou `routes/admin_cli/admin_snapshot.py` + inregistrare in `admin_cli.py`. **O singura comanda pe baza**, cu o tranzactie
`START TRANSACTION WITH CONSISTENT SNAPSHOT` (ca `--single-transaction`), ca sa nu prinda jumatate dintr-o operatie:
clientul trimite starea precedenta (pagini + sume de control), serverul calculeaza sumele noi pe aceleasi intervale, trimite inapoi
schema, starea noua si DOAR paginile schimbate (NDJSON comprimat, blob-uri base64). Fara SQL liber; identificatorii verificati si ghilimelati
ca in restul `admin_cli`. Coloana de modificare, daca exista (cautata dupa `ON UPDATE` in `information_schema`), ocoleste calculul
sumelor pe tabelele cu blob-uri; **fereastra de siguranta** inapoi de cateva minute si **verificare completa pe amprente o data pe
saptamana**, fiindca data modificarii e un indiciu, nu o dovada. Stergerile se vad din sumele/numarul de pe interval.
Eu nu public fisierul pe server - il trimiti tu.

## ME02-05 - Rulare periodica

Propunerea initiala de mai jos este inlocuita de [corectiile operatorului din 20.10](worklog/SLICE-ME02-03-format-si-motor.md#slice-me02-05): fara copie automata de noapte in interfata, reamintire la deschiderea KBotAdmin; codul headless ramane neexpus.
`KBotAdmin.exe --backup` (fara fereastra, iese cu cod de eroare), pornit de Windows Task Scheduler; un buton in KBotAdmin inregistreaza
sarcina (tu apesi). Daca PC-ul era oprit, ruleaza la prima ocazie. Lacat pe rulare (nu doua deodata), jurnal in `Logs\`, motiv "periodic".
Avertisment in KBotAdmin daca ultima copie reusita e mai veche de N zile. Cheia SSH fara parola, ca azi pentru publicare.

## ME02-06 - Restaurare si verificare
"Verifica depozitul" (integritate) si "Restaureaza in baza de proba" (`zz_*`, niciodata peste o baza vie; verb nou de scriere, cere confirmare
si e jurnalizat), apoi comparatie cu baza vie: randuri si sume pe tabel. Restaurarea unui rand / tabel produce un script SQL pe care il aplici
tu - nu scrie singura in baza vie.

## ME02-07 - Pagina "Istoric" (time-machine)
Pagina noua in KBotAdmin: baza -> axa timpului cu copii (data, manual/periodic, motiv, spor de spatiu) -> rasfoire tabele la acel moment
(din depozit, fara server) -> istoricul unei inregistrari (toate versiunile) -> diferenta intre doua momente (adaugate / modificate / sterse)
-> script de restaurare. Limita onesta: istoricul e la fel de fin ca frecventa copiilor (zilnic = nu vezi ce s-a schimbat intre doua rulari).

## ME02-08 - Retentie si curatare

[Implementarea si hotararea ulterioara](worklog/SLICE-ME02-03-format-si-motor.md#slice-me02-08) pastreaza manualele fara limita, sterse doar de mana; regulile periodice initiale raman pentru codul periodic neexpus.
Pastrare de ex. 14 zilnice / 8 saptamanale / 12 lunare (reglabil), apoi stergerea obiectelor la care nu mai trimite niciun instantaneu.
Cu avertisment si previzualizare ("se elibereaza X MB") inainte de curatare.

## Ordine si paralelism
01, 02 si 03 sunt independente (fisiere diferite); 04 atinge `admin_cli.py` ca si 02, deci vine dupa 02. 05-08 urmeaza in ordine.
Fiecare sub-felie: worklog `SLICE-ME02-0N-...md`, `state/KBOT_STATUS_ME02.md` + rand in `KBOT_STATUS.md`, build curat (`KBotAdmin`),
"nimic rulat pe server" spus in clar acolo unde e cazul.

## Ce nu fac
Nu public pe server, nu ating `config.py`, nu rulez teste, nu fac comenzi git care scriu, nu ating `AVACONT_COMUN` / `AVACONT_SURSA` /
`000_DEMO` in copii, nu restaurez niciodata peste o baza vie.
