# SLICE-AD11-01 — Buton de anulare document în taburile Chitanțe / Alte documente

## Ce s-a schimbat și de ce

La cererea operatorului (10.10.2026): în taburile de documente ADECHIT, pe rândurile deja salvate, apare un buton
❌ («Anulare document») în coloana de acțiuni din dreapta (lățimea ei exista deja; coloana Explicație nu se atinge).

- **Când apare:** luna selectată este deschisă sau este ULTIMA lună închisă. Pentru orice altă lună închisă butonul nu
  apare deloc. Nu apare pe documente deja anulate și nu apare la restituiri (serverul le refuză încă, regula M03).
  Ultima lună închisă se calculează într-un singur loc (`lastClosedMonthId`), folosit și de lacătul 🔒 de redeschidere.
- **La click:** caseta de confirmare a aplicației (roșie, cu fundalul estompat) întreabă «Ești sigur/ă că vrei să anulezi
  chitanța … în valoare de …?» și conține un câmp obligatoriu «Motivul anulării». Butonul «Anulează» (dreapta) rămâne
  dezactivat cât timp motivul e gol; «Renunță» (stânga), Escape și clicul în afară închid fără anulare.
- **Anularea:** `POST /api/adechit/cancel` (rută existentă) cu `kind`, `id`, `version` și motivul scris de operator;
  apoi situația copilului se reîncarcă. Salvarea și anularea folosesc aceeași reîncărcare (`reloadAfterDocument`).
- **Condiție în setări (10.10.2026):** butonul se vede doar dacă `AD_Settings.AllowCancelDocuments` este `true` (sau `1`/`yes`/`da`) pentru subunitatea curentă; lipsa valorii înseamnă `false`. Setarea o citește `service.cancel_allowed`; `/api/adechit/context` o trimite ca `settings.allowCancel`, iar `cancel_document` refuză cu `CANCEL_OFF` (403) dacă e oprită, deci regula nu se obține doar ascunzând butonul. Nu se seedează în schema de producție; se pornește cu `INSERT INTO AD_Settings (SubunitId, SettingKey, SettingValue) VALUES (<id>, 'AllowCancelDocuments', 'true');`. `preview.py` o pornește pe `true` (probe locale).
- Componentă nouă `reasonBox` în `js/utils/confirm-box.js` (stil în `confirm-box.css`).

## Fișiere atinse

`PYTHON/static/js/adechit/app.js` · `PYTHON/routes/adechit/{service,__init__}.py` · `ADECHIT/tools/preview.py` · `PYTHON/static/js/utils/confirm-box.js` · `PYTHON/static/css/confirm-box.css` ·
`ADECHIT/UTILIZARE_WEB.md` (ajutor) · `ADECHIT_STATUS.md` · `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`.

## Rezultate teste

Nu s-au rulat teste (regula proiectului). Verificat doar: `node --check` pe `app.js` și `confirm-box.js`.
`py_compile` pe `service.py`, `__init__.py`, `preview.py`. Testele existente de anulare (`test_adechit.py`) nu s-au rulat; ele depind de setarea pornită în `preview.py`, iar testul restituirii primește acum `true` din preview, deci rămâne la M03.

## Nerezolvat / amânat

- Nu s-a verificat în browser (aspectul casetei, dezactivarea butonului, reîncărcarea după anulare).
- Restituirile nu pot fi anulate până la decizia M03 (salvarea motivului).
- Ajutorul din aplicație desktop (HelpContent) nu acoperă ADECHIT web; ajutorul ADECHIT este `ADECHIT/UTILIZARE_WEB.md`, actualizat aici.
- Necomis și nepublicat, conform cererii (fără commit, fără push).
