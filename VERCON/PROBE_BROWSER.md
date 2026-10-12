# VERCON — matricea de probe in browser (SLICE-VC02-05)

Stare: **PLANIFICAT / NEEXECUTAT**. Nicio probă de mai jos nu a fost rulată; nu există măsurători. Coloana „Rezultat" se completează doar după o rulare reală pe `VERCON/tools/preview.py` (port 5053). Nu declar performanța din implementare.

Pornire: `PYTHON\.venv\Scripts\python.exe VERCON\tools\preview.py --port 5053`, apoi `http://127.0.0.1:5053/vercon`. DC-uri: `/vercon/preview/unit/preview` și `preview2`. Simulare răspuns pierdut la salvare: adaugă `?simulate=lose-response` în adresa paginii.

## Context (VC02-01)

| # | Scenariu | Așteptat | Rezultat |
|---|---|---|---|
| C1 | DC `preview2` (o subunitate, o gestiune) | subunitate, gestiune și an alese singure | — |
| C2 | DC `preview`, subunitatea A1: „Magazie materiale" și „Magazie materiale 2" | două gestiuni distincte, ani proprii | — |
| C3 | schimbi anul pe OI/MF | rămâne aceeași gestiune; bara spune „filtru an" | — |
| C4 | schimbi anul pe Materiale/Alimente | bara spune „exercițiu" | — |
| C5 | schimbi subunitatea cu draft nemodificat / modificat | fără întrebare / întrebare „Rămân aici / Renunț" | — |
| C6 | alegi rapid două gestiuni la rând | rămâne contextul ultimei alegeri | — |

## Meniu și capabilități (VC02-02)

| # | Scenariu | Așteptat | Rezultat |
|---|---|---|---|
| M1 | OI și MF | fără Bon de consum, Retur, Listă alimente, Închidere an; cu predare/transfer/casare | — |
| M2 | Alimente („Bucătărie", ocupată de alt utilizator) | formularele de editare dezactivate cu motiv; Rapoarte active | — |
| M3 | apel direct `/api/vercon/forms/bc/open?...` pe gestiune OI | 403 `FORM_NOT_AVAILABLE` | — |
| M4 | NIR pe MF | coloane nr. inventar, data intrării, durată; pe Materiale nu | — |
| M5 | „Gestiune nouă (goală)" | selectorul de tip activ; după schimbare rămâne activ (tot goală) | — |
| M6 | gestiune cu date | selectorul de tip dezactivat; `POST /api/vercon/gestiune/type` → 409 `TYPE_LOCKED` | — |

## Editare (VC02-03) și salvare (VC02-04)

| # | Scenariu | Așteptat | Rezultat |
|---|---|---|---|
| E1 | text cu apostrof și diacritice | păstrat exact | — |
| E2 | `1.234,56`, `12,5`, `0`, gol | `1234.56`, `12.5`, `0`, NULL (obligatoriu → eroare) | — |
| E3 | dată `5.3.2026` | devine `05.03.2026` | — |
| E4 | preț `-1` | „cel puțin 0" | — |
| E5 | multe linii, scroll, ștergere linie din mijloc | valorile celorlalte rămân | — |
| S1 | dublu Enter | o singură cerere, o singură revizie | — |
| S2 | `?simulate=lose-response` | stare nesigură; „Verifică salvarea" → „fusese aplicată"; nicio dublură | — |
| S3 | salvare cu revizie veche | conflict; datele rămân în formular | — |
| S4 | sesiune expirată | mesaj; datele rămân | — |
| S5 | schimbi contextul în timpul salvării | răspunsul e ignorat („stale") | — |

## Generale

| # | Scenariu | Rezultat |
|---|---|---|
| G1 | desktop 1280, mobil 375 (fără scroll orizontal pe pagină) | — |
| G2 | tastatură: Tab prin selectoare, meniu, câmpuri; Enter salvează | — |
| G3 | temă luminoasă / întunecată | — |
| G4 | 10 deschideri/închideri de formular: număr de listeners/timere constant | — |
| G5 | 2.000 de linii: timp de desenare și memorie măsurate | — |
| G6 | consolă fără erori; nicio resursă rămasă după `pagehide` | — |

## Ajutor

Ajutorul explicit pe flux și gestiune **nu este scris**: mecanismul existent al ADECHIT (`js/adechit/help.js`) nu a fost încă legat de VERCON. Rămâne de făcut după ce fluxurile sunt stabile (VC06+).
