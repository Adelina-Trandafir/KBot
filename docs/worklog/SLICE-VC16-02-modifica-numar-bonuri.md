# SLICE-VC16-02 — «Modifică număr» în arborii BC, BP, BT, BS

12.11.2026 — Plan: [PLAN_MODIFICA_NUMAR_BONURI.md](../PLANS/PLAN_MODIFICA_NUMAR_BONURI.md).

În meniul **⋮** al unui bon din arbore (bon de consum, bon de predare, de transfer, de casare) apare **Modifică număr**. Operatorul schimbă numai numărul, fără să deschidă documentul pentru editare.

## Reguli (verificate pe server la salvare; meniul doar sugerează)

- **Luna în curs**: data bonului este în luna calendaristică de azi (ora României). În alte luni opțiunea nu apare în meniu, iar ruta refuză.
- **Ordinea cronologică**: numărul trebuie să fie mai mare decât orice număr al unui document cu data mai veche și mai mic decât orice număr al unui document cu data mai nouă. Aceeași zi este permisă (ca `require_position` de la liste). Mesajul spune intervalul permis.
- **Număr liber** în serie.
- **Seria**: la BC, toată gestiunea/exercițiul, sau **numai contul bonului** dacă gestiunea are numerotarea pe cont (`consumption_numbering.per_account`). La BP/BT/BS, seria tipului în gestiune/exercițiu.
- BP: bonul de consum pe care îl deține (`VC_Iesiri_H.NumarBC`) primește același text; ambele rânduri își cresc `Version`.
- Contorul `VC_Numbering` doar crește (`raise_counter`), ca la numerele introduse de mână. Se scrie în `VC_Audit` operația `<familie>.number` cu numărul vechi și nou.

## Fișiere

- `PYTHON/routes/vercon/number_change.py` (nou): seria, intervalul permis, `annotate` (semnalul `number_change` din lista arborelui) și `change`.
- `PYTHON/routes/vercon/doc_routes.py`: ruta `POST /api/vercon/doc/<familie>/<id>/number` și anotarea listei.
- `PYTHON/static/js/vercon/number-change.js` (nou), `docs.js` (meniul și `changeNumber`), `PYTHON/static/js/utils/confirm-box.js` (`numberBox`, caseta cu câmp de număr care rămâne deschisă la refuz), `PYTHON/static/css/confirm-box.css`.
- `PYTHON/static/js/vercon/document-history.js` (etichetele operațiilor `bc|bp|bt|bs.number`).
- `PYTHON/static/help/vercon.json` (secțiunea «Modificarea numărului unui bon», eticheta VC16-02).
- `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.

## Presupuneri (propuse în plan, necontrazise)

«Luna în curs» = luna calendaristică, nu «luna activă» a bonurilor de consum. Rămân în afara acestui slice: BX (retur), BU și listele de alimente.

## Neverificat

Verificate doar sintaxa (`py_compile`, `node --check`) și JSON-ul ajutorului. Nu s-a rulat nimic în aplicație: nici meniul, nici caseta, nici scrierea în baza de date. Fereastra **Istoric** are acum etichete pentru operațiile noi (`document-history.js`), dar nu am văzut-o afișată. Nu am verificat vizual caseta (stilul câmpului de număr). Fișierele nu sunt pe server; înregistrarea pentru publicare se face abia după proba Adelinei.
