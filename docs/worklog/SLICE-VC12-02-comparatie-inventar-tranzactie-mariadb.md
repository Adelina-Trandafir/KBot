# SLICE-VC12-02 — Comparație inventar: pornirea tranzacției MariaDB

Data: 06.11.2026.

## Problema și corecția

Adelina a raportat în KBOTADMIN eroarea `ValueError: MySQL server version (5, 5, 5) does not support this feature` la utilizarea comparației inventarului implementate în VC12-01.

În `vercon_inventory.check`, apelul `conn.start_transaction(readonly=True, consistent_snapshot=True)` trecea prin verificarea versiunii MySQL a driverului Connector/Python. Prefixul de compatibilitate MariaDB poate fi interpretat ca versiunea MySQL 5.5.5, astfel încât parametrul `readonly=True` este respins înaintea executării interogărilor de comparație. Mesajul nu dovedește versiunea reală a serverului MariaDB.

Pornirea tranzacției este exprimată acum prin cursor, în sintaxa MariaDB:

```sql
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
START TRANSACTION WITH CONSISTENT SNAPSHOT, READ ONLY;
```

Izolarea se stabilește pentru tranzacția următoare. Citirile rămân într-un singur snapshot, cu protecția READ ONLY aplicată de server; rollback-ul existent se păstrează. Nu se modifică versiunea raportată de driver și nu se introduce fallback către o tranzacție care permite scrieri.

Corecția este limitată la pornirea tranzacției din comparația inventarului. Potrivirea, selectoarele, celelalte operații KBOTADMIN și conexiunile comune rămân în afara acestei intervenții.

Referințe de proiectare: [MariaDB — START TRANSACTION](https://mariadb.com/docs/server/reference/sql-statements/transactions/start-transaction), [MariaDB — SET TRANSACTION](https://mariadb.com/docs/server/reference/sql-statements/administrative-sql-statements/set-commands/set-transaction). În proiect, interogările numai-citire din `admin_query.py` folosesc deja pornirea tranzacției prin cursor.

## Stare

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Sunt înregistrate toate cele patru fișiere proprii, inclusiv documentația, cu motivul acestui slice. Au fost analizate codul și documentația pentru proiectare; agentul nu a executat teste, verificări de execuție, compilare, conexiuni la baze de date/SSH, SQL, publicare sau Git.

Corecția din `PYTHON/routes/admin_cli/vercon_inventory.py` nu necesită recompilarea KBOTADMIN. Înregistrarea prin `tools/Register-DeploymentTask.ps1` nu execută publicare sau Git pe server.

## Fișiere proprii create/modificate în acest task

- `PYTHON/routes/admin_cli/vercon_inventory.py`
- `docs/worklog/SLICE-VC12-02-comparatie-inventar-tranzactie-mariadb.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
