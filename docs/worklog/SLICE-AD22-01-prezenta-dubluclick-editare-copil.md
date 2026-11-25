# SLICE-AD22-01 — Editarea copilului prin dublu clic în Prezență

Data: 16.11.2026. Stare: SCRIS LOCAL, NEPROBAT, NEPUBLICAT, NEÎNREGISTRAT.

## Ce s-a schimbat și de ce

Cererea Adelinei: dublu clic pe un rând din Prezență să deschidă fereastra de editare a copilului.

- `app.js`: acțiunea rândului apelează noua intrare `editChildFromAttendance`, în locul deschiderii administrării plătitorilor.
- `payers.js`: se reutilizează formularul «Modifică copilul» și salvarea existentă. La deschidere se citesc catalogul grupelor și copilul după IDP prin API-urile existente, în subunitatea curentă. Se folosește grupa actuală a copilului, inclusiv pentru rândurile lunilor istorice. Grupele selectorului sunt ordonate alfabetic în limba română. Lipsa copilului/grupei este raportată prin tratarea existentă a erorilor. Deschiderile repetate sunt blocate în timpul încărcării.
- Se păstrează dreptul Catalog și prioritatea editării celulelor de prezență în controlul comun. Formularul se deschide direct, fără fereastra Plătitori dedesubt. Salvarea folosește versiunea actuală a copilului și reîmprospătarea existentă; Renunță închide fără salvare.
- Ajutorul comun ferestrei/imprimării/PDF (`copii-dublu`) și ghidul de utilizare descriu noul comportament, cu referință AD22-01. Fără capturi noi sau modificări ale celor existente.

## Fișiere proprii create/modificate

- `PYTHON/static/js/adechit/app.js`
- `PYTHON/static/js/adechit/payers.js`
- `PYTHON/static/help/adechit.json`
- `ADECHIT/UTILIZARE_WEB.md`
- `ADECHIT_STATUS.md`
- `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`
- `docs/worklog/SLICE-AD22-01-prezenta-dubluclick-editare-copil.md`

## Verificări și limite

Analiză de proiectare prin citirea implementării controlului DataGrid, a formularului și a API-urilor existente. Nu s-au executat teste, compilări, verificări sintactice, browser, SQL, publicare sau Git, conform instrucțiunilor proiectului. Nu se modifică API-uri, schema bazei, calcule sau controlul comun.

De probat de Adelina: dublu clic pe nume/celule neeligibile pentru editarea prezenței, salvare și Renunță, copil mutat în altă grupă față de luna consultată, cont fără Catalog, editarea zilelor și raportarea erorilor de încărcare.

Înregistrarea tuturor celor șapte fișiere de mai sus rămâne în așteptarea confirmării explicite a Adelinei după probă, conform AGENTS.md. Nu s-a executat `Register-DeploymentTask.ps1`.
