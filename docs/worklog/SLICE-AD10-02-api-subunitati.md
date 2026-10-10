# ADE10-02 — API și operații cu context obligatoriu DC + subunitate

Implementează secțiunile 4, 5, 6 și 8 (partea web) din [planul subunităților](../../ADECHIT/plan_subunitati.md).

## Ce s-a schimbat și de ce

- **Context.** Antetul `X-Ade-Subunit` se validează pe server la fiecare cerere față de subunitățile
  active ale DC-ului (`Repository.select_subunit`). Un id necunoscut sau inactiv → 409 `SUBUNIT_CONTEXT`.
  Lipsa antetului este acceptată numai când DC-ul are **exact o** subunitate activă (pagini vechi, unități
  cu o singură evidență); altfel 409 `SUBUNIT_REQUIRED` / `SUBUNIT_NONE`. `/api/adechit/context` poate
  rula fără subunitate și întoarce lista (`subunits`) și subunitatea rezolvată (`subunit`).
- **Repository.** Toate accesările sunt limitate la subunitate: `rows`, `get`, `insert` (adaugă
  `SubunitId`), `update`, `delete`, `years`, `situation_data`, `setting`, `data`. Fără subunitate,
  `scope()` refuză. Un ID din altă subunitate nu se poate citi, modifica sau șterge (404 `NOT_FOUND`).
- **Blocare.** `AD_Lock` are un rând pe subunitate (ID = SubunitId, creat la nevoie); mutațiile unei
  subunități se serializează, subunitățile diferite nu. Ordinea rămâne: lock → drepturi → rânduri →
  configurația chitanțelor.
- **Chitanțe.** Seria și contorul se citesc și se incrementează din `AD_ReceiptConfig` al subunității,
  în aceeași tranzacție cu documentul; duplicatul se caută pe `(SubunitId, Serie, Numar)`. PDF-ul și
  listarea afișează „Subunitate: …".
- **Reîncercări.** `AD_Operations` este pe `(SubunitId, RequestKey)` și amprenta include subunitatea.
- **Regula temporală.** `temporal()` compară `Ordine + 1` cu cea mai nouă lună din Prezență (nu IDL);
  echivalentă cu sursa deoarece `Ordine` = IDL-ul din Access, inclusiv golurile. Calculele de solduri
  păstrează comparația pe `IDL`, valabilă pentru că importul inserează lunile în ordinea Access, deci
  id-urile serverului cresc în aceeași ordine (formulele de business nu s-au schimbat).
- **Import web** (`/api/adechit/import`, `/reconcile`). Intră în subunitatea curentă (trebuie să fie
  goală). ID-urile nu se mai păstrează: serverul le alocă și referințele se rescriu prin hartă, salvată
  în `AD_IdMap`. Reconcilierea compară prin hartă. Același extras într-o altă subunitate → 409
  `IMPORT_OTHER_SUBUNIT`. Seria se scrie în `AD_ReceiptConfig`.
- Câmp nou `Ordine` în `schema.json` (LunaD).

## Fișiere

`PYTHON/routes/adechit/`: `repository.py`, `__init__.py`, `service.py`, `importer.py`,
`receipt_output.py`, `schema.json`; `PYTHON/templates/adechit/receipt.html`. Unelte locale:
`ADECHIT/tools/preview.py` (schema SQLite cu `SubunitId`; opțiunea `--second-subunit` (acum implicit; `--single-subunit` o oprește) adaugă
„Evidenta B" cu serie proprie DEMOB), `ADECHIT/tools/verify_import.py`.

## Teste

Rulate: nimic (nici automate, nici manuale). Doar `ast.parse` pe modulele Python. Teste existente
aliniate la schema nouă: `PYTHON/tests/test_adechit.py` (lista tabelelor păstrate în
`test_import_repeat_and_reconcile`) și `PYTHON/tests/adechit_lazy.test.mjs` (contextul simulat întoarce
subunitatea). Nu s-au adăugat teste noi (regula proiectului).

## Rămâne neverificat / amânat

Toate fluxurile pe MariaDB; izolarea între subunități (scenariile 2–6 și 8 din plan) de probat de
utilizator. Subunitățile se creează numai prin migrator sau SQL (nu există ecran de administrare,
redenumire sau dezactivare). Restricții pe subunitate per utilizator — extensie viitoare (drepturile
rămân pe DC).

## Acces pe subunitate (completare ADE10-06)

`AD_SubunitAccess (SubunitId, Email)`: o subunitate fara randuri este deschisa tuturor utilizatorilor cu drepturi ADE in DC; una cu randuri este deschisa numai e-mailurilor listate. Selectorul arata doar subunitatile permise si apare numai daca utilizatorul are mai mult de una; una singura se alege automat. O subunitate ceruta fara acces -> 403 SUBUNIT_FORBIDDEN. Administrarea listei se face deocamdata prin SQL (INSERT INTO AD_SubunitAccess); nu exista ecran. Neprobat.

## Corecturi după review (ADE10-06)

- **Snapshot înainte de blocare:** `transaction()` citește subunitățile într-o citire scurtă pe care o încheie cu `commit`, apoi pornește
  tranzacția reală și blochează rândul din `AD_Lock`. Altfel, sub REPEATABLE READ prima citire fixa snapshot-ul și o cerere care aștepta
  blocarea continua cu date vechi (inclusiv la verificarea `AD_Operations`). Izolarea efectivă a serverului rămâne neverificată.
- **Reîncercări de dinainte de conversie:** `idempotent()` acceptă și amprenta veche, fără subunitate; căutarea rămâne limitată la subunitate.
- **Reconciliere:** harta `AD_IdMap` se caută după subunitate (o subunitate primește un singur import), nu după hash: migratorul VB salvează
  hash-ul fișierului MDB, importul web hash-ul extractului normalizat. Limitare: același MDB importat pe cele două căi, în subunități diferite,
  nu este detectat ca import repetat.
- **Acces pe utilizator:** `AD_SubunitAccess (SubunitId, Email)`. Subunitate fără rânduri = deschisă tuturor din DC; cu rânduri = doar e-mailurile listate.
  Contextul listează doar subunitățile permise; una interzisă cerută manual → 403 `SUBUNIT_FORBIDDEN`. Administrare prin SQL, fără ecran.
- **Sincronizarea schemei (`schema_sync`):** o schimbare de cheie primară pe o coloană încă lipsă din țintă rula (prioritate 3) înaintea `ADD COLUMN`
  (7) și eșua cu errno 1072 pe `AD_Operations`. În `schema_diff.py` o astfel de schimbare se emite acum ca `PK CREATE` (prioritate 9).
