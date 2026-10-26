# SLICE-ADE8-01…05 și AD11-01 — Încasări, chitanțe și anulări

01…04 păstrează contractele inițiale și taburile; 05 introduce listarea/PDF, excluse la checkpointul 02. AD11-01 păstrează separat anularea documentelor și vechea condiție AllowCancelDocuments; [ghidul actual](../../ADECHIT/UTILIZARE_WEB.md#anularea-unui-document) folosește ulterior dreptul AD_ANULARE și spune că setarea veche nu mai există. AD11 portal rămâne alt istoric, cu același număr reutilizat, fără renumerotare. Mailul chitanțelor rămâne inactiv. [Rapoartele](SLICE-ADE9-01-rapoarte.md) documentează ulterior viewerul comun. Nota Rezolvat din 04 și AD11-01: *Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- [ADE8-01](#slice-ADE8-01)
- [ADE8-02](#slice-ADE8-02)
- [ADE8-03](#slice-ADE8-03)
- [ADE8-04](#slice-ADE8-04)
- [ADE8-05](#slice-ADE8-05)
- [AD11-01 — anulare](#slice-AD11-01-anulare)

## SLICE-ADE8-01 — documente și anulări <a id="slice-ADE8-01"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Încasarea, alt document, restituirea, M04 și rescrierea M02 au teste. Anularea restituirii refuză explicit M03; transferul refuză explicit M06.

## SLICE-ADE8-02 — chitanțe <a id="slice-ADE8-02"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Seria/numărul/șablonul vin din configurația per unitate, numărul este incrementat tranzacțional și dublurile sunt refuzate. PDF și tipărire sunt excluse.

## SLICE-ADE8-03 — idempotency și tranzacții <a id="slice-ADE8-03"></a>

08.10.2026 — TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Repetarea aceleiași chei întoarce același rezultat, iar refolosirea cu alt payload este refuzată. Lock-ul local a fost exercitat; concurența MariaDB nu.

## SLICE-ADE8-04 — taburile de încasări și restituiri <a id="slice-ADE8-04"></a>

### Ce s-a schimbat și de ce

Footerul are taburile Chitanțe, Alte plăți și Restituiri legate de copilul selectat. Rândurile salvate sunt numai pentru citire; numai rândul nou permite Data și Valoarea. După alegerea datei se propune SID la Chitanțe/Alte plăți și SIC la Restituiri, iar suma poate fi modificată înainte de salvare. Salvarea reîncarcă situația lunară.

### Fișiere atinse

- `PYTHON/static/adechit.html`
- `PYTHON/static/css/adechit.css`
- `PYTHON/static/js/adechit/app.js`

### Verificări

- taburile, selecția copilului și rândul nou au fost probate vizual în preview.
- suită ADE: 9 teste trecute; sintaxa JavaScript și `git diff --check` au trecut.

### Neverificat sau amânat


Configurația și numerotarea reală a chitanțelor necesită proba pe server. Pentru Alte plăți, interfața trimite automat tipul „Altă plată” și numărul „Fără număr”, deoarece cerința curentă lasă editabile numai data și valoarea. Anularea restituirii rămâne blocată de M03.

## ADE8-05 — Meniu de listare și PDF pentru chitanțe <a id="slice-ADE8-05"></a>

Utilizatorul a confirmat că Listare și Descarcă PDF trebuie să fie funcționale. Pe PC, 📥 de lângă salvare deschide meniul ultimei chitanțe salvate a copilului; este inactiv înainte de prima salvare. Fiecare chitanță existentă are și buton propriu. Meniul are aspect Windows, pictograme în stânga, navigare din taste și închidere prin Escape/clic în afară. Este ascuns pe mobil.

Listare deschide documentul și dialogul browserului. PDF-ul se generează cu ReportLab. Ambele folosesc aceeași chitanță salvată, plata, plătitorul asociat și datele unității. Nu emit alt număr și nu salvează automat rândul nou. Documentul are două exemplare și marcaj ANULATĂ când este cazul. Structura câmpurilor se bazează pe raportul Access Chitanta; echivalența vizuală nu a fost verificată.

Trimitere pe mail apare numai când plătitorul asociat are email completat și rămâne inactivă. Nu se trimite niciun mesaj.

Endpointurile print/pdf reutilizează autorizarea și contextul unității. Cererile trimit tokenul/contextul în headere; acestea nu sunt introduse în URL. Răspunsurile nu se păstrează în cache. Șablonul HTML escapează datele, PDF-ul escapează textul pentru Paragraph, iar meniul curăță listenerii la închidere/navigare.

Fișiere principale: routes/adechit/receipt_output.py și __init__.py, static/js/adechit/app.js și receipt-print.js, static/css/adechit.css și adechit-receipt.css, templates/adechit/receipt.html, static/fonts/DejaVuSans.ttf și licența, requirements-adechit.txt, ajutorul și documentele de stare.

Verificare numai de sintaxă: py_compile pentru modulele Python și node --check pentru app.js și receipt-print.js, reușite. Nu s-au executat teste automate, generări PDF de probă sau verificări funcționale/vizuale, conform regulii ADECHIT.

Publicarea trebuie să includă fontul, licența, șablonul, CSS/JS și modulele API. Pe server se instalează `pip install -r requirements-adechit.txt` în mediul backendului, apoi se repornește backendul. Nu există DDL nou. Nu s-a efectuat publicare, instalare sau restart remote. Utilizatorul verifică meniul, emailul condițional, listarea, diacriticele, suma în litere și PDF-ul după publicare.

Următoarea subfelie liberă: ADE8-06.

## SLICE-AD11-01 — Buton de anulare document în taburile Chitanțe / Alte documente <a id="slice-AD11-01-anulare"></a>

### Ce s-a schimbat și de ce

La cererea operatorului (10.10.2026): în taburile de documente ADECHIT, pe rândurile deja salvate, apare un buton ❌ («Anulare document») în coloana de acțiuni din dreapta (lățimea ei exista deja; coloana Explicație nu se atinge).

- **Când apare:** luna selectată este deschisă sau este ULTIMA lună închisă. Pentru orice altă lună închisă butonul nu apare deloc. Nu apare pe documente deja anulate și nu apare la restituiri (serverul le refuză încă, regula M03). Ultima lună închisă se calculează într-un singur loc (`lastClosedMonthId`), folosit și de lacătul 🔒 de redeschidere.
- **La click:** caseta de confirmare a aplicației (roșie, cu fundalul estompat) întreabă «Ești sigur/ă că vrei să anulezi chitanța … în valoare de …?» și conține un câmp obligatoriu «Motivul anulării». Butonul «Anulează» (dreapta) rămâne dezactivat cât timp motivul e gol; «Renunță» (stânga), Escape și clicul în afară închid fără anulare.
- **Anularea:** `POST /api/adechit/cancel` (rută existentă) cu `kind`, `id`, `version` și motivul scris de operator; apoi situația copilului se reîncarcă. Salvarea și anularea folosesc aceeași reîncărcare (`reloadAfterDocument`).
- **Condiție în setări (10.10.2026):** butonul se vede doar dacă `AD_Settings.AllowCancelDocuments` este `true` (sau `1`/`yes`/`da`) pentru subunitatea curentă; lipsa valorii înseamnă `false`. Setarea o citește `service.cancel_allowed`; `/api/adechit/context` o trimite ca `settings.allowCancel`, iar `cancel_document` refuză cu `CANCEL_OFF` (403) dacă e oprită, deci regula nu se obține doar ascunzând butonul. Nu se seedează în schema de producție; se pornește cu `INSERT INTO AD_Settings (SubunitId, SettingKey, SettingValue) VALUES (<id>, 'AllowCancelDocuments', 'true');`. `preview.py` o pornește pe `true` (probe locale).
- Componentă nouă `reasonBox` în `js/utils/confirm-box.js` (stil în `confirm-box.css`).

### Fișiere atinse

`PYTHON/static/js/adechit/app.js` · `PYTHON/routes/adechit/{service,__init__}.py` · `ADECHIT/tools/preview.py` · `PYTHON/static/js/utils/confirm-box.js` · `PYTHON/static/css/confirm-box.css` · `ADECHIT/UTILIZARE_WEB.md` (ajutor) · `ADECHIT_STATUS.md` · `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`.

### Rezultate teste

Nu s-au rulat teste (regula proiectului). Verificat doar: `node --check` pe `app.js` și `confirm-box.js`. `py_compile` pe `service.py`, `__init__.py`, `preview.py`. Testele existente de anulare (`test_adechit.py`) nu s-au rulat; ele depind de setarea pornită în `preview.py`, iar testul restituirii primește acum `true` din preview, deci rămâne la M03.

### Nerezolvat / amânat


- Verificat în browser (aspectul casetei, dezactivarea butonului, reîncărcarea după anulare): TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
- Restituirile: decizia M03 (salvarea motivului) implementată.
- Ajutorul din aplicație desktop (HelpContent) nu acoperă ADECHIT web; ajutorul ADECHIT este `ADECHIT/UTILIZARE_WEB.md`, actualizat aici.
- TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
