# SLICE-VC06-34 — NIR din eFactura: cantitate primită cu preț recalculat; Disponibil imediat la bonurile PTC

Data: 03.11.2026.

## Cerere

1. La preluarea unei facturi din eFactura în NIR, cantitatea primită trebuie să poată fi modificată, iar prețul să se recalculeze.
2. La Bonul de predare, «Disponibil» apărea abia după ce focusul pleca din «Cantitate»; trebuie să apară imediat după alegerea produsului cu mouse-ul. Verificat la toate bonurile PTC (BP, BT, BS).

## Ce s-a schimbat și de ce

- `PYTHON/static/js/vercon/efactura-intake.js`: fiecare linie preluată păstrează valoarea netă a liniei facturii (`ef_net`).
- `PYTHON/static/js/vercon/docs.js`, `efacturaPrice`: la schimbarea «Cantitate» pe o linie cu `ef_net`, `Pret = ef_net / Cant` (4 zecimale, ca în `VC_Intrari.Pret`), deci linia rămâne la valoarea facturii; mesaj în bara de stare. Dacă operatorul a scris el prețul (`ef_price_manual`), o nouă schimbare a cantității nu îl mai atinge. Liniile fără `ef_net` (NIR manual, factura fără valoare pe linie) nu sunt afectate. Serverul nu a fost modificat: nu impune cantitatea facturii, iar `Valoare`/`TVA`/`TOTAL` se recalculează ca până acum.
- `docs.js`, BP: cauza era `availability()`, care aduce stocul după 300 ms și rescrie rândurile doar dacă grila nu e în editare; după alegerea produsului editarea trece imediat pe «Cantitate», deci rescrierea se sărea. Acum `onCellSave` pentru `variant` completează `avail` din stocul deja încărcat (`state.available`, încărcat la adăugare/editare), cu aceeași formulă (`bpAvailable`, scade cantitățile celorlalte linii cu același produs).
- BT/BS: «Disponibil» este `qty`, deja pus pe rând din lotul ales în `onCellSave` (`_choice`), deci apare imediat; neschimbat. BC nu a fost atins.
- Ajutor: `PYTHON/static/help/vercon.json` (secțiunea `nir-efactura`, tag `VC06-34`, și pasul «Disponibil» la Bon de predare).

## Fișiere

`PYTHON/static/js/vercon/efactura-intake.js`, `PYTHON/static/js/vercon/docs.js`, `PYTHON/static/help/vercon.json`.

## Teste

Testat de Adelina pe 03.11.2026: merge corect. Fișierele pentru server (inclusiv `api.js`, de care `docs.js` depinde prin `anySignal`) sunt înregistrate (SLICE-VC06-34). Niciun test automat.

## De știut

- `ef_net` = valoarea netă a liniei din `EF_PrimiteLinii.Valoare` (din codul `efactura_queue.py`).
- BT/BS aveau deja «Disponibil» imediat (din lotul ales); neschimbate.
