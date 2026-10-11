# SLICE-AD12-01 — ajutorul ADECHIT (buton «?», cuprins, imprimare, PDF)

11.10.2026. SCRIS LOCAL. Doar verificări de sintaxă (`node --check`, `py_compile`) și o singură
construire de probă a PDF-ului (a produs un fișier PDF valid; conținutul și aspectul nu au fost
privite). Fereastra nu a fost deschisă în browser.

## Ce s-a făcut și de ce

Butonul **?** în bara de sus, în stânga butonului de meniu, deschide o fereastră de ajutor:
cuprinsul pe care se poate apăsa în stânga (secțiuni și subtitluri), textul formatat în dreapta
(titluri, liste, tabele, casete de notă), cu linkuri între secțiuni; bara ferestrei are
**Imprimă**, **Export PDF** și **Închide**.

- **O singură sursă:** `PYTHON/static/help/adechit.json` (secțiuni cu blocuri `p`, `h`, `ul`, `ol`,
  `note`, `table`; în text `**aldin**` și `[text](#id)`). Din ea se hrănesc fereastra, imprimarea și PDF-ul.
  Textul este scris pentru utilizator (cum se folosește), nu descrie funcționarea internă și nu
  menționează Access. Fiecare afirmație vine din codul paginii sau din `UTILIZARE_WEB.md`.
- **Fereastra:** `js/adechit/help.js` + `css/adechit-help.css` + `<dialog id="ade-help-dialog">`.
  Secțiunea curentă este evidențiată în cuprins la derulare. Pe ecrane înguste cuprinsul trece
  deasupra textului. Culorile vin din temă (luminoasă/întunecată).
- **Imprimă:** o fereastră nouă cu tot ajutorul (titlu, cuprins, secțiuni) pe alb, apoi dialogul de
  imprimare al browserului; linkurile din copie merg la secțiunea lor.
- **Export PDF:** `GET /api/adechit/help/pdf` (`routes/adechit/help_pdf.py`, ReportLab, fontul
  DejaVuSans ca la rapoarte): copertă, cuprins cu linkuri, secțiuni, tabele, număr de pagină.
  Fontul are o singură grosime, deci textul aldin apare în albastru închis; emoji-urile din text
  sunt înlocuite cu simboluri pe care fontul le are (↓ ✕ + → ↩).

## Fișiere atinse

`PYTHON/static/help/adechit.json` (nou), `PYTHON/static/js/adechit/{help.js (nou),app.js}`,
`PYTHON/static/css/adechit-help.css` (nou), `PYTHON/static/adechit.html`,
`PYTHON/routes/adechit/{help_pdf.py (nou),__init__.py}`, statusuri.

## Nerezolvat / neverificat

- Aspectul ferestrei (desktop și mobil), imprimarea și PDF-ul nu au fost văzute.
- Textul ajutorului trebuie citit și corectat de operator: poate conține formulări de ajustat;
  nu a fost parcurs cu utilizatorul.
- Conținutul trebuie ținut la zi când se schimbă interfața (punctul 4 din CODE_WORKFLOW):
  se editează doar `adechit.json`; imprimarea și PDF-ul îl urmează automat.
- PDF-ul cere `reportlab` și `static/fonts/DejaVuSans.ttf` pe server (deja prezente acum).
