# SLICE-VC15-01 — Reparat «changeEfactura is not defined» la asocierea eFactura–NIR

10.11.2026 — La alegerea din meniul unui NIR salvat a «Asociază eFactura…», «Asociază cu altă eFactură» sau «Elimină asocierea cu eFactura», fereastra de eroare afișa «changeEfactura is not defined» și nu se întâmpla nimic.

**Cauza.** [VC06-42](SLICE-VC06-42-inlocuire-eliminare-asociere-nir-efactura.md) a mutat acțiunile eFactura în `efactura-actions.js`. `docs.js` importa `receiptEfacturaChanges` și apela `changeEfactura(...)` în `changeSavedEfactura`, dar nu construia nicăieri funcția `changeEfactura`.

**Reparația.** În `docs.js`, lângă `efLineDialog`: `const changeEfactura = receiptEfacturaChanges(opts.api);`. O singură linie, fără alte schimbări de comportament.

**Ajutor.** Neafectat: nu se schimbă nimic din ce vede operatorul, ci se repune în funcțiune acțiunea deja descrisă la VC06-42.

## Fișiere

- `PYTHON/static/js/vercon/docs.js`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
- `docs/worklog/SLICE-VC15-01-changeefactura-nedefinit.md`

## Neverificat

Verificat doar că `docs.js` trece de `node --check`. Nu s-a rulat asocierea eFactura–NIR în aplicație. Fișierul nu e încă pe server; proba de către Adelina este în așteptare. `docs.js` e înregistrat pentru publicare sub acest slice.
