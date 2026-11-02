# SLICE-AD12-01 — ajutorul ADECHIT (buton «?», cuprins, imprimare, PDF)

11.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Ce s-a făcut și de ce

Butonul **?** din bara de sus, la stânga meniului, deschide ajutorul: cuprins interactiv în stânga (secțiuni/subtitluri), text formatat în dreapta (titluri, liste, tabele, note), linkuri între secțiuni și bara **Imprimă**, **Export PDF**, **Închide**.

- **Sursă comună ferestrei/imprimării/PDF:** `PYTHON/static/help/adechit.json` (blocuri `p`, `h`, `ul`, `ol`, `note`, `table`; în text `**aldin**`, `[text](#id)`). Instrucțiuni de utilizare bazate pe codul paginii/`UTILIZARE_WEB.md`, fără funcționare internă sau mențiuni Access.
- **Fereastra:** `js/adechit/help.js` + `css/adechit-help.css` + `<dialog id="ade-help-dialog">`. Secțiunea curentă este evidențiată în cuprins la derulare. Pe ecrane înguste cuprinsul trece deasupra textului. Culorile vin din temă (luminoasă/întunecată).
- **Imprimă:** o fereastră nouă cu tot ajutorul (titlu, cuprins, secțiuni) pe alb, apoi dialogul de imprimare al browserului; linkurile din copie merg la secțiunea lor.
- **Export PDF:** `GET /api/adechit/help/pdf` (`routes/adechit/help_pdf.py`, ReportLab, fontul DejaVuSans ca la rapoarte): copertă, cuprins cu linkuri, secțiuni, tabele, număr de pagină. Fontul are o singură grosime, deci textul aldin apare în albastru închis; emoji-urile din text sunt înlocuite cu simboluri pe care fontul le are (↓ ✕ + → ↩).

## Fișiere atinse

`PYTHON/static/help/adechit.json` (nou), `PYTHON/static/js/adechit/{help.js (nou),app.js}`, `PYTHON/static/css/adechit-help.css` (nou), `PYTHON/static/adechit.html`, `PYTHON/routes/adechit/{help_pdf.py (nou),__init__.py}`, statusuri.

## Nerezolvat / neverificat

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- Aspectul ferestrei (desktop și mobil), imprimarea și PDF-ul nu au fost văzute.
- Textul ajutorului trebuie citit și corectat de operator: poate conține formulări de ajustat; nu a fost parcurs cu utilizatorul.
- Conținutul trebuie ținut la zi când se schimbă interfața (punctul 4 din CODE_WORKFLOW): se editează doar `adechit.json`; imprimarea și PDF-ul îl urmează automat.
- PDF-ul cere `reportlab` și `static/fonts/DejaVuSans.ttf` pe server (deja prezente acum).
