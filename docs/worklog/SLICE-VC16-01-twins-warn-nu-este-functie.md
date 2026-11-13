# SLICE-VC16-01 — Reparat «twins.warn is not a function» la salvarea NIR / FCS

12.11.2026 — La salvarea unui NIR (și la completarea numărului de factură sau a furnizorului) fereastra de eroare afișa «twins.warn is not a function» și salvarea se oprea.

**Cauza.** [VC14-05](SLICE-VC14-05-numar-factura-dublu-si-asociere-storno.md) a adăugat avertizarea «același număr de factură la același furnizor». `docs.js` păstrează rezultatul lui `invoiceTwins(...)` în `twins` și apelează `twins.warn()` (la modificarea `NrFact` / `CodFiscal` și chiar înainte de salvare). Varianta pentru celelalte familii este un obiect `{ warn: async () => {} }`, dar `invoiceTwins` returna direct funcția `warn`, nu un obiect: `twins.warn` era `undefined`.

**Reparația.** În `invoice-twins.js`, `invoiceTwins` returnează acum `{ warn }`. Fără alte schimbări de comportament.

**Ajutor.** Neafectat: nu se schimbă nimic din ce vede operatorul, ci se repune în funcțiune avertizarea deja descrisă la VC14-05.

## Fișiere

- `PYTHON/static/js/vercon/invoice-twins.js`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
- `docs/worklog/SLICE-VC16-01-twins-warn-nu-este-functie.md`

## Neverificat

Verificat doar că `invoice-twins.js` trece de `node --check`. Nu s-a salvat un NIR în aplicație. Fișierul nu e încă pe server; proba de către Adelina este în așteptare, iar înregistrarea pentru publicare se face abia după ea.
