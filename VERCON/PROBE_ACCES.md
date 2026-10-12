# VERCON — probe de acces și audit (SLICE-VC04-05)

Stare: **PARȚIAL EXECUTAT** (11.10.2026). Probele de apel direct au fost rulate pe launcherul de preview (port 5054, utilizatori `preview` și `altul`, ttl 20 s); celelalte rămân goale. Preview-ul nu validează conturi, roluri sau MariaDB reale. Conturile și drepturile reale se validează separat de preview (preview-ul are un utilizator care deține toate operațiile).

Preview: `PYTHON\.venv\Scripts\python.exe VERCON\tools\preview.py --port 5053`. Ocuparea din preview expiră în 20 s fără heartbeat. Al doilea utilizator: `/vercon/preview/user/altul` (cookie, deci un alt profil de browser); al doilea tab al aceluiași utilizator: deschizi pagina încă o dată.

## Ocupare exclusivă (VC04-04)

| # | Scenariu | Așteptat | Rezultat |
|---|---|---|---|
| L1 | utilizatorul A deschide NIR pe „Magazie materiale" | ocupare luată; meniul altui utilizator arată formularele de editare dezactivate | OK (apel direct, preview) |
| L2 | utilizatorul B (alt profil) deschide aceeași gestiune, **alt an** | tot „ocupată" (ocuparea nu depinde de an) | OK (apel direct, preview) |
| L3 | B deschide „Magazie materiale 2" | liber: gestiuni diferite, titulari diferiți | OK (apel direct, preview) |
| L4 | A închide fila / navighează | ocuparea se eliberează | eliberarea prin API OK; inchiderea efectiva a filei (pagehide) NEPROBATA |
| L5 | A pierde heartbeat > 20 s (tab în fundal oprit) | B poate prelua; A primește `LOCK_LOST`, datele rămân în formular, salvarea e refuzată | OK (apel direct, preview); expirarea a fost probata cu ttl 20 s, nu cu heartbeat din pagina |
| L6 | A, a doua filă, același utilizator | întrebare „Preiau"; după confirmare prima filă pierde accesul | OK (apel direct, preview) |
| L7 | B încearcă să preia cu `takeover=true` pe ocuparea lui A (apel direct) | refuzat `GESTIUNE_OCCUPIED` (preluarea e doar pentru același utilizator) | OK (apel direct, preview) |
| L8 | cerere veche a fostului titular după preluare | refuzată `LOCK_LOST`, fără efect | OK (apel direct, preview) |

## Operații, tip și context (VC04-01/02)

| # | Scenariu | Așteptat | Rezultat |
|---|---|---|---|
| A1 | `GET /api/vercon/forms/bc/open` pe gestiune OI | 403 `FORM_NOT_AVAILABLE` | OK (apel direct, preview) |
| A2 | `POST /api/vercon/gestiune/type` fără rolul `VR_SETARI` | 403 `FORBIDDEN` | — |
| A3 | `X-Vercon-Unit` diferit de unitatea sesiunii | 409 `CONTEXT_CHANGED` | OK (apel direct, preview) |
| A4 | `subunit`/`gestiune`/`year` din altă gestiune | 409 `SELECTION_CHANGED` | OK (apel direct, preview) |
| A5 | operație de scriere fără jeton de ocupare | 409 `LOCK_LOST` | OK (apel direct, preview) |
| A6 | `X-Vercon-Version` vechi | 409 `CONTEXT_CHANGED` | OK (apel direct, preview) |
| A7 | eroare la citirea drepturilor (tabel lipsă / baza oprită) | acces închis (403/500), niciodată deschis | — |
| A8 | utilizator fără niciun rol VR | `context` → 403; nicio gestiune vizibilă | — |

## Repository (VC04-02)

| # | Scenariu | Așteptat | Rezultat |
|---|---|---|---|
| R1 | `ScopedRepository` fără context verificat | `ScopeError` | — |
| R2 | UPDATE/DELETE fără condiție | `ScopeError` | — |
| R3 | `insert` cu `GestiuneId` în valori | `ScopeError` (contextul îl pune serverul) | — |
| R4 | tabel în afara `tables.py` | `ScopeError` | — |

## Audit (VC04-05)

Rândurile din jurnal au `outcome=` din: `success`, `refused` (o regulă a spus nu), `busy` (ocupat de altul), `rollback`, `commit`, `unknown` (răspuns pierdut). Doar `success`, `refused`, `busy` și `failed` sunt emise acum de garda comună; `rollback`/`commit`/`unknown` sunt funcții pregătite în `diagnostics.py` și le folosesc rutele de scriere din VC06.

## Server (după aplicare)

- `sql/VC04_01_roluri_vr.sql` pe `AVACONT_COMUN` (după AD10_01 și AD10_03), apoi acordarea manuală a rolurilor.
- `sql/VC04_03_privilegii_migrator.sql` (șablon) — contul migratorului, un DC pe rând; verificarea `SHOW GRANTS` din fișier.

## Rezultate rulate (11.10.2026, apeluri directe pe preview)

**Peste 40 de apeluri; toate au ieșit cum era așteptat, după o corectură.** Prima rundă a scos două defecte reale, reparate și reluate:

- Heartbeat și eliberare acceptau jetonul unui alt utilizator dacă jetonul era corect (un jeton scurs ar fi permis altui cont să-l reînnoiască sau să elibereze). Acum se verifică și e-mailul titularului: `LOCK_LOST` / `released:false`.
- Un tip de gestiune necunoscut dădea `SELECTION_CHANGED`; acum `400 TYPE_UNKNOWN`.

Verificate suplimentar: bon de consum pe OI, listă de alimente pe Materiale, bon de predare pe Materiale și închidere an pe MF sunt refuzate (`FORM_NOT_AVAILABLE`), iar bon de consum pe Materiale se deschide; NIR are coloanele de mijloc fix numai pe MF; gestiunea ocupată arată formularele de editare dezactivate și Rapoartele active; formular necunoscut → 404; tip schimbat pe gestiune goală → 200, pe gestiune cu date → `TYPE_LOCKED`; salvare cu jeton curent → salvat, aceeași cheie → repetată (revizie 2), revizie veche → `CONFLICT`, jeton vechi sau al altuia → `LOCK_LOST`.

**Neprobate încă:** A2/A7/A8 (roluri reale, eroarea citirii drepturilor), R1–R4 (repository, fără ecran), L4 la închiderea efectivă a filei, heartbeat-ul automat din pagină, tot ce ține de MariaDB (`SqlLockStore`).
