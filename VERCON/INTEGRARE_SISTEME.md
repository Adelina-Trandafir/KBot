# VERCON — integrarea cu sistemele comune (SLICE-VC01)

Stare: DOCUMENTAT + IMPLEMENTAT_LOCAL (cod nerulat). Nimic validat pe server.

## Matrice sistem → import → initializare → utilizare → curatare

| Sistem comun | Unde este in VERCON | Initializare | Utilizare | Curatare |
|---|---|---|---|---|
| Sesiune portal | `static/js/vercon/gate.js`, `api.js`; server: `require_portal_session` | token din `sessionStorage` | fiecare apel trimite `X-Portal-Token`, `X-Vercon-Unit`, `X-Request-Id` | `pagehide` anuleaza cererile (`AbortController`) |
| Cronometru sesiune | `adechit/session.js` (reutilizat) | pornit daca exista token | ultimele 5 minute vizibile; 401 → iesire | intervalele traiesc cat pagina; nu exista alte timere in VERCON |
| Meniu aplicatii | `adechit/menu.js` (reutilizat); intrari VERCON in `portal/apps-menu.js`, `adechit/app.js`, `portal.js` | la click | — | meniul se inchide singur (Escape, click in afara) |
| Mesaje | `portal/messages.js` | — | cod diagnostic = `X-Request-Id` | — |
| Log / diagnostic | `routes/vercon/diagnostics.py` | — | linie `VR op= outcome= unit= user=(mascat) rid=` | — |
| Timing | `utils.timing.timed('vercon.<op>')` | — | automat la fiecare operatie | — |
| Conexiuni MariaDB | `routes/vercon/connection.py` | fabrica `utils.database.get_kbot_connection` | `with connection(db):` | inchidere garantata; fara pool propriu; fara reincercare automata la scriere |
| Drepturi | `routes/portal/drepturi.py`, sectiunea `VR` | citite la fiecare apel | operatie obligatorie per ruta | — |
| Documente (print/PDF/xlsx) | `static/js/vercon/documents.js` peste `adechit/reports.js` | — | context DC/subunitate/gestiune/an in antete | URL-urile blob sunt revocate dupa 60 s |
| Preferinte | `localStorage`, prefix `vercon.` | — | `readPreference`/`writePreference` | cheile ADECHIT (`ade.*`) nu sunt citite sau scrise |

Garda serverului refuza: unitate diferita de cea a sesiunii (`409 CONTEXT_CHANGED`), lipsa dreptului (`403 FORBIDDEN`), eroare interna (`500 SERVER`, fara detalii).

## Publicare

- `AvacontPush` trimite arborele `PYTHON/` (`LocalRoot`); `PYTHON/.pushignore` exclude jurnale, copii de siguranta si unealta de push. Fisierele noi VERCON sunt toate sub `PYTHON/`: `routes/vercon/`, `static/vercon.html`, `static/css/vercon.css`, `static/js/vercon/`.
- Baza Access, exporturile si documentatia (`VERCON/`) sunt in afara `PYTHON/`, deci nu se trimit. Fixture-urile si launcherul de preview sunt tot in `VERCON/tools/`.
- Nu s-a rulat nicio publicare; manifestul efectiv (lista fisierelor trimise) se verifica la prima publicare reala.

## Schema (VC03 va aplica)

- Schimbarile de schema VC_ intra in `AVACONT_SURSA`, sablonul din care se clonează unitatile; `schema_sync` le propaga in fiecare DC. `AVACONT_COMUN` nu este tinta automata a sincronizarii (este registrul unitatilor); rolurile VR se adauga acolo printr-un script separat (VC04-01).
- ADECHIT: regresia pe ADE se verifica prin faptul ca VERCON nu schimba din ADE decat intrarea de meniu din `adechit/app.js`.

## Limite

Cod nerulat; nu exista test de regresie ADE/portal executat. Singurele modificari in fisiere comune: intrarea VERCON in `portal/apps-menu.js` si `adechit/app.js`, iar in `portal/portal.js` lista `NEXT_PAGES` si selectorul de aplicatii.
