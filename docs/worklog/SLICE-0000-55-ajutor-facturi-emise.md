# SLICE-0000-55 - Ajutor pentru 00EF-08: fereastra «E-Factura — facturi emise»

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026.

## What changed and why
- `contabil/efactura/index.md` rewritten around the new window: «Lista facturilor» (columns, states, colours), «O factură nouă» (steps, what K-BOT
  refuses to save, Renunță, Modificare, Ștergere), one section per tab (Generale, Cumpărător, Vânzător, Atașamente, Conținut) and «Tokenul ANAF» (the old
  token sections, now reached from the «Token ANAF» button). Title `E-Factura: facturile emise și tokenul ANAF`; `screens:` now `FacturiForm, TokenForm,
  CertificatePickerForm`; keywords widened (factură, client, vânzător, cont emitent, IBAN, unitate de măsură, ciornă ...). Tags `00EF-08, 0000-55`.
- `contabil/fereastra.md` (row «E-Factura» of MENIU) and `tours/tur-fereastra.md` (step «Butonul MENIU»): the entry no longer opens the token window.
- `help-version.txt` = `2026-10-07`. The help says plainly that sending to ANAF, checking the state and correcting / cancelling an accepted invoice are
  not done from this window yet (a user-visible fact).

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `contabil/fereastra.md`, `tours/tur-fereastra.md`, `help-version.txt`; status files.

## Test results
`Check-Help.ps1 -Coverage`: no error about these files except the slice id `0000-55` before the status rows existed (written with this file); the coverage list
no longer names `FacturiForm` (it is in `screens:`). The old errors of `tutorials\ordonantare-din-plata.md` remain (not touched). The help was not opened in the app.

## Capturi
- **New, missing:** `efactura-facturi` (the new window; `goto: menu:efactura`, prepare: a unit with a few invoices, one chosen).
- **Changed:** `efactura-token` no longer has `goto:` (the menu now opens the invoice window): its `prepare:` says to press «Token ANAF». The picture never existed.

## Unverified / deferred (de citit de operator)
- All wording comes from the code of `FacturiForm` (captions, tooltips, messages), not from the window on screen: the window was never run. Read the
  sections against it. In particular «rândul are culoarea de avertisment / de eroare» (the colours come from the theme) and the sentence about what K-BOT
  asks before saving.
