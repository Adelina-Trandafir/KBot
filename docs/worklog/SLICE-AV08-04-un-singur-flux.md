# SLICE-AV08-04 — Un singur flux de scriere (salvări și ștergeri), `IdEvidentiereAnterior`, note goale

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Decizia Adelinei (24.11.2026): «totul trebuie să folosească același flux; dacă schimb ceva, să fie într-un loc, nu în 10»; notele goale se șterg.

## What changed and why

- Ștergerile intră în `NoteC.salvare` (același `GET_LOCK` pe baza unității, aceeași tranzacție ca salvările): ștergerea operației / documentului din «Modificare operații» (`stergere._raspuns_sterge`), «Șterge plata» din Facturi și plăți (`facturi_plati.delete_facturi_plati_plata`) și din Contări (`primite_plata.sterge`, care folosea `facturi._unit`). Fiindcă toate scrierile sunt serializate de aceeași blocare, nu mai pot ajunge la blocaj reciproc între ele.
- O singură copie a ștergerii: `NoteC.sterge_operatie` (alocări ca plată, legătura cu e-factura, linii, operația) și `NoteC.curata_goale` (documentul fără operații, apoi nota fără documente); le folosesc `stergere.py`, `NoteC_plati.sterge_plata` și editorul.
- `NoteC_plati.sterge_plata` aplică acum și regulile de ștergere din «Modificare operații» (ordonanțare generată, perioadă închisă cu `NoteC.perioada_inchisa`), pentru toate cele trei butoane.
- `IdEvidentiereAnterior` = `IdOperatie` din fișierul Access al anului ANTERIOR (`AvcPlatiStep.vb:467`, `IdFacturaAA`), deci numerotare Access. Ștergerea golea coloana unde era egală cu un `IdOper` KBOT (chei amestecate): rândul scos. `NoteC.SQL_PLATI_PE_EVIDENTA` numără plățile pe evidențiere prin `IdEvidentiere` ȘI prin `IdEvidentiereAnterior = IdOperatieAccess` al evidențierii, pentru plățile aceleiași subunități din anul următor; o asemenea evidențiere e «plătită»: ștergerea o refuză, editorul o tratează ca plătită.
- Editorul: documentul mutat în altă notă lasă nota veche goală → ștearsă (`curata_goale`).

### Familia NoteC (a doua trecere, 24.11.2026)

Decizia Adelinei: «trebuie să fie totul într-un loc»; fișierele comune au același prefix, `NoteC_`; cuvântul «postare» dispare; niciun fișier peste ~500 de rânduri, împărțit pe logică (funcții întregi, nimic tăiat la mijloc); Operațiile diverse nu scriu în jurnalul `JI`.

Tot ce scrie note contabile stă în `PYTHON/routes/avacont/introducere_date/operatii/`, în fișierele `NoteC*`; `NoteC.py` le adună sub un singur nume (`NoteC.X`):

| Fișier | Ce conține | Rânduri |
|---|---|---|
| `NoteC.py` | adună numele familiei | 38 |
| `NoteC_tranzactie.py` | blocarea pe unitate și tranzacția (`salvare`), refuzul `NoteCEroare`, constantele (o singură copie: `CENT`, `TREZORERIE_BIC`, `FEL_INCHIDERE`, `JURNAL_INCHIDERE`, SQL-urile comune) | 80 |
| `NoteC_reguli.py` | subunitatea, perioada închisă / anul anterior / `JI`, conturile planului și protecția, partenerul sursei, IBAN-ul, regula de ștergere `motiv_stergere` (mutată din `citire.py`) | 171 |
| `NoteC_scriere.py` | notele și numerele lor, documente, operații, echilibrul, `sablonul` + `operatie_din_sablon` (linia de șablon devenită operație: O SINGURĂ copie, folosită de Operații diverse, Facturi și plăți, Contări și plata), legătura cu e-factura, plata, ștergerile (`sterge_operatie`, `curata_goale`, `sterge_randuri`) | 232 |
| `NoteC_plati.py` | plățile, OP-urile, alocările, restul de plată (fost `plati.py`) | 355 |
| `NoteC_editor.py` | «Notă manuală» / «Modifică operație» (fost `editor.py`) | 427 |
| `NoteC_diverse.py` | salvarea Operațiilor diverse (mutată din `op9.py`); refuză jurnalul `JI` | 47 |
| `NoteC_facturi_plati.py` | salvarea Facturilor și plăților (mutată din `facturi_plati.py`) | 121 |
| `NoteC_contari.py` | salvarea Contărilor, evidențiere și plată (mutată din `efactura/primite_contari.py` și `primite_plata.posteaza`) | 165 |
| `NoteC_contari_calcul.py` | previzualizarea Contărilor și ce are deja o factură (mutate din `primite_contari.py`) | 311 |
| `NoteC_contari_ajutatoare.py` | ajutătoarele Contărilor (mutate din `primite_contari.py`) | 186 |

Rutele ferestrelor (`op9.py`, `facturi_plati.py`, `note_manuale.py`, `efactura/primite_contari_routes.py`, `efactura/primite_plata.py`, `modificare_date/operatii/stergere.py`, `lista.py`, `citire.py`) doar citesc cererea, cheamă `NoteC` și răspund. Șterse: `postare.py`, `plati.py`, `editor.py`, `efactura/primite_contari.py`. Duplicate eliminate: linia de șablon → operație (4 copii), verificarea partenerului (3), `TREZORERIE_BIC` (2), `CENT` (2).

## Files touched

`PYTHON/routes/avacont/introducere_date/operatii/NoteC*.py` (11 fișiere, tabelul de mai sus), `op9.py`, `facturi_plati.py`, `note_manuale.py`, `__init__.py`; `PYTHON/routes/avacont/__init__.py`; `PYTHON/routes/avacont/modificare_date/operatii/stergere.py`, `citire.py`, `lista.py`; `PYTHON/routes/efactura/primite_plata.py`, `primite_contari_routes.py`, `primite_parteneri.py` (comentariu), `__init__.py` (comentariu); șterse: `postare.py`, `plati.py`, `editor.py`, `efactura/primite_contari.py`.

## Test results

`py_compile` fără erori. Nimic rulat.

## Unverified / deferred

- PRESUPUS: plata din anul următor e în aceeași subunitate ca evidențierea din anul anterior (Access = un fișier pe unitate și an). Dacă migrarea pune subunitatea altfel, potrivirea pe `IdOperatieAccess` nu găsește rândul.
- La trimiterea pe server: cele patru fișiere vechi (`postare.py`, `plati.py`, `editor.py` din `routes/avacont/introducere_date/operatii/` și `routes/efactura/primite_contari.py`) trebuie șterse și acolo; nimeni nu le mai importă.
- Verificat static (fișierele doar citite, nu rulate): `py_compile` și un analizor `symtable` al numelor nedefinite — nimic. Nerulat pe date.
