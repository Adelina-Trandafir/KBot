# SLICE-AD15-01 — O încasare pentru mai mulți copii și sume din soldul final

29.10.2026. Stare: **SCRIS LOCAL**, fără teste, verificări executabile, SQL, commit sau publicare.

## Ce s-a schimbat și de ce

Citirea codului existent a arătat că alegerea datei apela deja `fillSuggestedAmount`, dar cu SIC pentru restituiri și SID pentru chitanțe/alte plăți. Se utilizează acum SFC, respectiv SFD, din situația copilului selectat pentru luna curentă, când soldul este pozitiv. Valoarea rămâne editabilă.

Fluxul existent AD13 lega un extras de un singur copil; `IDFXE` era cheia primară unică. Pentru plata unui părinte care acoperă mai mulți copii s-a adăugat repartizarea explicită în modul manual:

- Alegerea copilului/plătitorului activ și a sumei, apoi «Adaugă copil la repartizare». Copiii pot fi din grupe diferite, în subunitatea curentă și aceeași lună.
- Listă alfabetică în română, ștergere/reintroducere, total repartizat/rest și propunerea restului pentru următorul copil. Explicația este păstrată separat pentru fiecare document când copilul este adăugat.
- Confirmarea enumeră fiecare copil, suma și explicația lui. Totalul trebuie să fie exact egal cu suma sursă; nu se modifică extrasul pentru repartizare. Un singur copil poate primi suma integrală fără adăugare în listă.
- Serverul validează sumele pozitive întregi, totalul, absența copiilor repetați și luna comună. Toate versiunile copiilor/plătitorilor sunt controlate înainte de prima reînnoire a plătitorilor. Se păstrează izolarea subunității, blocările comune, amprenta sursei, idempotenta, regulile temporale și actualizarea situațiilor închise.
- Fiecare copil primește propria plată/document și propria înregistrare de proveniență. Toate scrierile aparțin tranzacției existente; orice eroare provoacă rollback. Extrasul întreg rămâne rezervat la nivelul unității, inclusiv după anulare. Nu există repartizare parțială salvată în etape.
- Repartizarea nesalvată cere confirmare înainte de abandonare la schimbarea extrasului, lunii, modului, la reîncărcare și închidere. Căutarea păstrează repartizarea pregătită și amprenta sursei.

Previzualizarea automată rămâne cu un copil pentru fiecare încasare. Repartizările se pregătesc manual. Nu s-au extins calculele, anulările, raportarea, portalul sau importul extraselor. Sumele cu fracțiuni păstrează limita existentă `AD_Plati.Plata` INT și nu sunt rotunjite.

## Fișiere

- `PYTHON/static/js/adechit/app.js` — SFC/SFD.
- `PYTHON/static/js/adechit/bank-associations.js` — editorul repartizării și payload-ul cu alocări.
- `PYTHON/static/adechit.html`, `PYTHON/static/css/adechit-bank-associations.css` — controale și derulare.
- `PYTHON/routes/adechit/bank_associations.py` — validarea repartizării și reutilizarea scrierii unui document pentru fiecare copil; contractul anterior rămâne acceptat.
- `sql/AD_15_extrase_repartizare.sql` — înlocuiește cheia primară cu `(IDFXE, IDP)`, păstrând asocierile, cheile străine și unicitatea IDPL/IDA. Bazat pe exportul real `MariaDB_Schema/AVACONT_SURSA.sql` din 29.10.2026. Nu modifică AVACONT_COMUN.
- `PYTHON/static/help/adechit.json`, `ADECHIT/UTILIZARE_WEB.md` — pașii și limitele funcției.
- `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` — înregistrarea stării.

## Probe și limite

Conform AGENTS.md nu s-au rulat teste, compilări, verificări executabile, aplicația sau SQL. S-a făcut numai analiza codului, a contractelor și a exportului de schemă. Nu se afirmă validarea în browser sau pe MariaDB.

Rămân pentru operator: SFC pozitiv la restituire și SFD pozitiv la încasări, sold zero, sumă editată; extras repartizat la doi copii, inclusiv din grupe diferite; total incomplet/depășit, copil repetat, sursă/ținte schimbate, reîncercare după răspuns pierdut și concurență; luna închisă eligibilă; anularea unui document fără eliberarea extrasului; aspect PC/mobil și tema întunecată.

Ajutorul textual este actualizat. Captura `13-extrase-asociere-manuala.jpg` rămâne de refăcut pentru noile controale; nu s-au realizat capturi în această intervenție.

## Predare

1. Operatorul aplică manual `sql/AD_15_extrase_repartizare.sql` după AD_11, în fiecare bază a unității care folosește repartizarea. Scriptul nu șterge date. Fără migrare, asocierea unui singur copil continuă să fie acceptată; pentru mai mulți copii API-ul indică scriptul necesar.
2. Publică prin AvacontPush cele șase fișiere de aplicație/ajutor din lista de mai sus: `app.js`, `bank-associations.js`, `adechit.html`, `adechit-bank-associations.css`, `bank_associations.py`, `adechit.json`. Ghidul/statusul/worklogul rămân documentație în repo.
3. Operatorul probează scenariile de mai sus. Nu s-a executat publicare sau restart din chat.
