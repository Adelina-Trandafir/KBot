# SLICE-ME01-20 - «Comenzi»: Stergere angajament, fara tabelele EF_ (04.11.2026)

Cererea operatorului: o comanda care sterge TOATE informatiile pentru un cod de angajament anume, nu si pe cele din e-factura. Owner's tool only (KBotAdmin, never published).

## What it is
- **Verb nou `angajament_stergere`** (`PYTHON/routes/admin_cli/angajament_stergere.py`, inregistrat in `admin_cli.py`: `COMMANDS`, `COMMAND_PARAMS` = «Cod angajament», `SINGLE_DB_COMMANDS`, `_c_angajament_stergere`). `writes=True`, o singura baza, un singur cod. Grupa FOREXE in client (langa «Fisa angajament»).
- **Ce sterge** (aceeasi imagine ca `fisa_angajament`, copii inaintea parintilor, fiecare pas numarat): `FX_Angajamente` (+ `_Grupe`), `FX_Indicatori`, `FX_Istoric`, `FX_DDF` (+ Parteneri, REV, REV_SA/SB/PRT/ATT/ATT_IMG, PDF), `FX_Rezervari` (+ IMG), `FX_Receptii_R/_H/_RHR`, `FX_Receptii` (+ IMG), `FX_Plati`, `FX_Operatiuni`, `FX_ORD` (+ ATT, ATT_IMG, DOC, PART, PDF, TBL, TBL_REC), `FX_PDF_SEMNATURI` ale acelor documente, `FX_NoteCAB_Corectii` (FK RESTRICT spre angajament, deci intai) si nota CAB (cu PDF si recipisa) **numai daca nu mai are alte corectii**, `FX_NumberLock`, plus orice alta tabela `FX_` cu coloana `CodAngajament` care nu e in plan (gasita din `information_schema`).
- **Ordinea conteaza**: `FX_ORD -> FX_DDF` e RESTRICT, `FX_NoteCAB_Corectii -> FX_Angajamente` e RESTRICT; de aceea nu ne bazam pe `DELETE FX_Angajamente` cu cascade.
- **Siguranta**: cere InnoDB pe toate tabelele din plan si pe Jurnal; `SELECT ... FOR UPDATE` pe randul angajamentului; o tranzactie (READ COMMITTED), commit abia dupa **garda finala** (nicio tabela din plan cu coloana `CodAngajament` nu mai are randuri cu codul, altfel rollback). Codul trebuie sa existe in `FX_Angajamente`. Intrare in `Jurnal` (actiunea `angajament_stergere`) cu randurile sterse pe tabela.
- **Client** (`KBotAdmin/Views/CommandsView.vb`): caseta «Cod angajament», confirmare care numeste codul si baza si spune ca nu se poate anula, text «Ce te asteapta»; KBotAdmin 1.2.0.9.

## Ce NU se atinge (si o exceptie)
- **Toate tabelele `EF_`** (facturi primite, linii, legaturi `EF_PrimiteReceptii`). Legaturile din `EF_PrimiteReceptii` pastreaza codul si id-urile receptiilor ca numere simple (fara FK), deci doar arata spre nimic pana la o noua descarcare; numarul lor e in rezultat.
- **Exceptie inevitabila**: `EF_PrimiteAsocieri` (factura <-> DDF, aleasa de mana) are FK `ON DELETE CASCADE` spre `FX_DDF` si `IDDF NOT NULL`, deci randurile pentru DDF-ul acestui angajament pleaca odata cu el. Sunt scrise **integral in Jurnal** inainte si numarate in rezultat.
- `Clasificatii`, `Parteneri`, `Parteneri_Angajamente*` (legaturi furnizor-cod, cheie = cod, fara FK), `FX_Extrase*` (extrase de trezorerie, nu sunt in `fisa_angajament`), `FX_PDF_BUCATI` (bucati comune).
- `TZ_Oper` (note contabile) ramane; FK-ul `SET NULL` ii pune `IdOrdTbl` pe NULL pentru liniile de ordonantare sterse; numarul e in rezultat.

## Files touched
`PYTHON/routes/admin_cli/angajament_stergere.py` (nou), `PYTHON/routes/admin_cli/admin_cli.py`; `KBotAdmin/Views/CommandsView.vb`, `KBotAdmin/KBotAdmin.vbproj`; `state/KBOT_STATUS_ME01.md`.

## Test results
`dotnet build KBotAdmin\KBotAdmin.vbproj`: 0 avertismente, 0 erori. `py_compile` curat pe ambele fisiere Python; fisierul nou e ASCII in afara ghilimelelor «» din mesajele pentru operator. Planul de stergere a fost construit din DDL-ul real din `MariaDB_Schema/000_DEMO.sql` (FK-uri si tipul lor de stergere) si din `sql/KE_01_02_fisa_angajament.sql`. Nimic rulat: nici un test, niciun SQL, nimic trimis pe server (regula operatorului).

## Unverified / deferred
- **Nerulata nicaieri.** Prima rulare: pe 000_DEMO, dupa «Fisa angajament» pe acelasi cod, ca sa se compare ce a spus fisa cu ce a sters comanda.
- Baza reala poate diferi de DDL-ul din `MariaDB_Schema` (dumpat 22.09.2026); coloanele din plan (`IDORDATTP`, `IdRevAtt`, `IDRZ`, `IdOrdTbl` etc.) sunt luate de acolo. O coloana lipsa da eroare SQL si **rollback complet**; lipsa unei tabele din plan opreste comanda inainte de orice stergere.
- Nu exista copie automata inainte: copia bazei (Copii / backup din KBotAdmin) o face operatorul.
- `FX_Extrase*` si `Parteneri_Angajamente*` raman; daca trebuie sterse si ele, se decide separat.
- Testata de Adelina; inregistrata pentru server 04.11.2026 (`Register-DeploymentTask.ps1`, `SLICE-ME01-20`: `angajament_stergere.py`, `admin_cli.py`). Ramane «Finalizeaza debugging» din KBOTADMIN, facut de ea.
- Ajutor: niciunul (KBotAdmin nu e pentru clienti).

## Correction 04.11.2026 - analysis first, a separate delete button, the NAME not only the code
Rule from Adelina: first an analysis, then ask whether to delete with another button; and always the angajament's NAME, not only its code.
- **Two commands now.** `angajament_analiza` (read only, in the list, FOREXE group, param «Cod angajament») counts per table what would go and says what stays; `angajament_stergere` (WRITES) is **hidden**, it exists only behind the new button **«Sterge angajamentul»** (`btnDeleteAng`, same cell as «Repara»/«Explica»; declared in `CommandsView.Designer.vb`). The button appears only after an analysis of ONE code in ONE database whose summary says `randuri de sters` > 0, and disappears if the code in the box changes, the command changes or the database changes. Its confirmation names the code AND the name and says it cannot be undone.
- **The name** (`_identity` in `angajament_stergere.py`): `FX_Angajamente.Alias` (the name the operator gave, slice 0008-02), else `Descriere`, else the `ObiectDDF` of the first DDF, else «(fara denumire)»; the detail line adds Alias, Descriere, Obiect DDF, Partener DDF (`FX_DDF.NumePartener`), Stare. It shows in the analysis summary (column `denumire`), in the first line of «ce se intampla», in the confirmation window, in the result of the deletion and in the Jurnal entry. Columns missing in an older database are skipped.
- Notes CAB are now decided before the deletion (a note stays when a correction of ANOTHER code uses it), the same function serves the analysis and the deletion, so the analysis shows exactly the notes that stay.
- KBotAdmin 1.2.0.10; `dotnet build` 0 warnings / 0 errors; `py_compile` clean; still nothing run, still not registered for the server.
