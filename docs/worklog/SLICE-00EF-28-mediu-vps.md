# SLICE-00EF-28 — E-Factura: environment confirmed (13.10.2026)

Not a code change. The operator ran the checks below and reported the results in chat; this file
records what is now CONFIRMED so the older "not run / not deployed" lines are read in that light.

## What changed and why

Nothing in the repo code. Status notes only: the prerequisites listed as open in
`PLAN_00EF_EFactura.md` («Open questions now», point 1) and in the 00EF rows of the state files are done.

## Confirmed by the operator (13.10.2026)

- **DDL run.** A UNION ALL query over `information_schema` (plus a row count on `EF_UM`) found every
  object of the 00EF scripts present: `AVACONT_COMUN.EF_Token`, `EF_TokenStart`, `EF_UM` (rows present),
  `Unitati_Date`, `Unitati_Conturi`; `AVACONT_SURSA.EF_Clienti`, `EF_Facturi` (+ `EroareAnaf`),
  `EF_FacturiLinii`, `EF_Mesaje` (+ `ZipContinut`), `EF_Primite`, `EF_PrimiteLinii`, `EF_PrimiteNote`,
  `EF_PrimiteMesaje`, `EF_PrimiteAsocieri`, `EF_PrimiteTVA`; the old `EF_Furnizor` is absent, as it
  should be. `00EF_06` and `00EF_12` hold only comments (replaced by 00EF-13) — nothing to run.
  Scope of that check: `AVACONT_SURSA` and `AVACONT_COMUN` only; other unit databases (e.g. `000_DEMO`) not checked.
- **Server code is on the VPS.** The VPS keeps the code directly in `/root/AVACONT` (there is NO `PYTHON/`
  folder there; `routes/` is at its root), venv `/root/AVACONT/.venv`, service `avacont`.
- **`cryptography` 50.0.2** is installed in that venv.
- **`/etc/avacont/efactura.env`** exists and is complete; checked with the server's own code, secrets not printed:
  `ef_config.missing_names()` empty, `is_configured()` True, `cripto.encrypt`/`decrypt` round trip OK.
- **systemd** shows `EnvironmentFiles=/etc/avacont/efactura.env (ignore_errors=no)`; `avacont` is `active`.
- **K-BOT side.** The operator wrote that the «E-Factura» window and the certificate step "work"
  («merg»). No detail was given (no screen text, no log), so this is the operator's word, not something checked here.

## Still NOT verified

- Anything called at ANAF with a real invoice (upload with the PDF attachment, status, download of the zip).
- The other open threads in
  `state/KBOT_STATUS_0000-0009.md` → Open threads.
- That the service account can WRITE in `AVACONT_COMUN` from the unit connection («Date Unitate», conturi).
- The 7-day expiry note on the main window (never built).

## Files touched

`docs/worklog/SLICE-00EF-28-mediu-vps.md` (new), `PLAN_00EF_EFactura.md`,
`state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`. No code, no tests. (First logged as sliceless; moved to 00EF-28 on 15.10.2026, operator: all E-Factura work sits in an EF slice.)

## Delimitarea probei de mesaje din 00EF-17

[00EF-17](SLICE-00EF-17-primite-server.md#slice-00EF-17) consemnează separat citirea a patru mesaje primite reale la 09.10, cu un token de probă și fără bază de date. Acea probă nu confirmă fluxul aplicației pe MariaDB sau emiterea unei facturi reale (încărcare, stare, zip) menționate mai sus ca neîncercate. Cele două consemnări își păstrează aria.
