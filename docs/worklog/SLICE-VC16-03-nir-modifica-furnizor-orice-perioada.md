# SLICE-VC16-03 — «Modifică furnizor» în arborele NIR, în orice perioadă

13.11.2026 — slice ales de Adelina: `VCff-mm` cu ff = ultimul slice VC care a atins NIR-ul (VC16-01), mm = primul sub-slice liber (03).

În meniul **⋮** al unui NIR din arbore apare **Modifică furnizor**. Operatorul alege alt furnizor, fără să deschidă documentul pentru editare. Merge și când exercițiul (anul) este închis: ruta nu apelează `protocol.ensure_open`.

## Ce se schimbă împreună (aceeași tranzacție)

- `VC_NIR.CodPartener` + `FurnizorV` ale recepției (versiunea documentului se verifică și crește).
- Aceleași două coloane la facturile storno (FCS) ale recepției (`IdStorno = NIR`, neanulate): FCS nu are furnizor propriu, îl copiază din recepție.
- `VC_FACTURI.CodPartener` al facturii comune la care e legat NIR-ul (și FCS-urile lui), **numai dacă** nicio altă recepție vie nu e legată de ea.
- `VC_Audit`: operația `nir.supplier` cu furnizorul vechi și nou (eticheta «Modificare furnizor NIR» în fereastra Istoric).
- Contorul de numere și restul documentului nu se ating.

## Ce se refuză (mesajul spune de ce, fereastra rămâne deschisă)

- document anulat (`CANCELLED`, din `_load_for_write`);
- NIR asociat cu o eFactură (`EF_NIR_LOCKED`, regula existentă a editării) — furnizorul vine din eFactură;
- factura comună are și alte recepții vii (`INVOICE_SHARED`) — ar rămâne cu furnizorul vechi;
- furnizor negăsit sau inactiv (`INVALID`); același furnizor (`NO_CHANGE`); versiune veche (`CONFLICT`).
- Un număr de factură dublu la noul furnizor este doar **avertizare** (ca VC14-05), nu blocare.

## Ce rămâne neschimbat

Dreptul operației (`receptie`) și blocarea gestiunii se cer la fel ca la orice scriere; doar perioada închisă nu mai blochează. `build(..., closed_ok=True)` este singurul loc nou cu această excepție.

## Fișiere

- `PYTHON/routes/vercon/supplier_change.py` (nou): `change`.
- `PYTHON/routes/vercon/doc_routes.py`: ruta `POST /api/vercon/doc/nir/<id>/supplier`, parametrul `closed_ok` la `build`.
- `PYTHON/static/js/vercon/supplier-change.js` (nou): fereastra cu lista de furnizori; `docs.js` (meniul, `changeSupplier`, `state.canChangeSupplier` = dreptul listei, fără condiția „perioadă deschisă”); `document-history.js` (eticheta operației).
- `PYTHON/static/js/utils/confirm-box.js` (`ensureStyle` exportat), `PYTHON/static/css/confirm-box.css` (`.confirm-box--supplier`).
- `PYTHON/static/help/vercon.json` (secțiunea «Modificarea furnizorului unui NIR», eticheta VC16-03).
- `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`.

## Presupuneri (de confirmat)

- „Luna închisă” = exercițiul închis (`VC_Exercitii.Closed`), singura închidere pe care o cunoaște VERCON; nu există închidere pe lună.
- NIR asociat cu eFactură și NIR anulat rămân blocate; factura comună partajată cu alte recepții blochează schimbarea (se schimbă pe toate sau se separă întâi).

## Neverificat

Doar sintaxa (`py_compile`, `node --check`) și JSON-ul ajutorului. Nu s-a rulat nimic în aplicație: nici meniul, nici fereastra (lista de furnizori în `<dialog>`, deschiderea listei dedesubtul subsolului), nici scrierea în baza de date, nici într-un an închis. Fără teste. Fișierele nu sunt pe server; înregistrarea pentru publicare se face abia după proba Adelinei.
