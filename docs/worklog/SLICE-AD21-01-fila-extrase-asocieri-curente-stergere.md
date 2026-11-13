# SLICE-AD21-01 — Fila Extrase, Asocieri curente și ștergerea completă a unei asocieri

Data: 12.11.2026. Număr de felie dat de Adelina.

## Cerere

1. În secțiunea de jos (Chitanțe, Alte plăți, Restituiri) o filă nouă **Extrase**: extrasele asociate cu valoarea lor și valoarea asociată.
2. În asocierea extraselor, vizualizarea asocierilor curente și **ștergerea** unei asocieri dintr-o lună deschisă (ultima lună). La ștergere documentul din Alte plăți nu se doar anulează: se **șterge complet**, ca asocierea să poată fi refăcută.
3. Decizia Adelinei la întrebare: se șterge **o singură repartizare**; extrasul rămâne parțial asociat și **apare cu diferența** în lista de neasociate.

## Ce s-a schimbat

- `bank_associations.py`: `available()` listează extrasele ale căror plăți asociate nu acoperă încă suma (coloane noi `suma_asociata`, `suma_ramasa`); `associate()` blochează întâi rândul de extras, apoi citește repartizările existente; repartizarea nouă trebuie să acopere exact **suma rămasă**; un copil care are deja o repartizare din același extras este refuzat; calea veche cu un singur copil cere extras neasociat. Funcții noi: `current()` (lista lunii), `attendance_associations()` (fila Extrase), `delete_association()` (șterge legătura, apoi `AD_AlteDoc`, apoi `AD_Plati`; refuză luna închisă, versiune schimbată, plată cu chitanță).
- `__init__.py`: rute `GET /api/adechit/bank-extracts/current`, `POST /api/adechit/bank-extracts/delete-association` (dreptul `cancel`, cu cheie de idempotență), `kind=extract` pe `/api/adechit/ledger/<id>`.
- `bank_preview.py`: un extras parțial asociat nu primește copil propus (se asociază manual).
- Interfață: `adechit.html` (fila, modul și panoul), `bank-current.js` și `extract-ledger.js` (noi), `bank-associations.js` (trei moduri manual/automat/curente; suma rămasă), `app.js` (ramura fila Extrase), `adechit-ledger.css`, `adechit-bank-associations.css`.
- Ajutor `adechit.json` (secțiuni `incasari-extrase`, `banca-asocieri-curente`), `ADECHIT/UTILIZARE_WEB.md`.

## Decizii și limite

- Ștergerea se face numai în **luna deschisă**; în luna închisă lista e doar de consultat. Niciun instantaneu salvat nu se atinge.
- Plătitorul creat din extras **rămâne** (ca în AD20-15); nu se reactivează plătitorul anterior.
- Rândul de prezență creat cu zero zile la asociere rămâne.
- Repartizări multiple pe același extras cer în continuare AD_15 (cheia primară IDFXE+IDP); codul verifică. Fără SQL nou.
- Nu s-a atins politica de drepturi MariaDB (`privilege_policy.py`): API-ul folosește contul de serviciu.

## Verificări

Efectuate: `py_compile` pe cele trei fișiere Python; `node --check` pe cele patru fișiere JS; JSON-ul ajutorului se parsează. **Neverificat:** nicio rulare pe MariaDB sau în browser (interogările noi, aspectul modului «Asocieri curente» și al filei, ștergerea cu cheile străine); fără teste. Capturile din ajutor pentru fila nouă nu există.

## Fișiere

`PYTHON/routes/adechit/{bank_associations,__init__,bank_preview}.py`, `PYTHON/static/adechit.html`, `PYTHON/static/js/adechit/{bank-current,extract-ledger,bank-associations,app}.js`, `PYTHON/static/css/{adechit-ledger,adechit-bank-associations}.css`, `PYTHON/static/help/adechit.json`, `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE20-ADE29.md`, acest worklog.
