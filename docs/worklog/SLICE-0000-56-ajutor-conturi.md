# SLICE-0000-56 - Ajutor pentru 00EF-12 si pentru token: conturile unitatii emitente, obtinerea / reinnoirea tokenului

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026. Help only: no tutorial (operator's request).

## What changed and why
- **New topic `contabil.efactura.conturi`** (`contabil/efactura/conturi.md`): the window «E-Factura — conturile unității» (opened by «Conturile unității…» in the «Vânzător» tab): what the two columns show (the bank is filled by K-BOT from the bank code inside the IBAN, «— banca nu este în listă —» when unknown), «Cont nou», «✕», «Salvează», «Închide» with the unsaved-rows question, what is refused (empty row, wrong IBAN, repeated account, more than 50), the «tables missing» message. Tags `00EF-12, 0000-56`; `screens: ConturiForm, FacturiForm.btnVConturi`; capture tag `conturi-unitate`.
- **New topic `contabil.efactura.token`** (`contabil/efactura/token.md`): getting and renewing the ANAF token, moved out of `contabil.efactura` (text of 00EF-05 / 0000-54 kept, one sentence added: renew in good time, after full expiry the authorisation starts again): what the window shows, the steps with the certificate and the PIN, what can happen. `screens: TokenForm, CertificatePickerForm, FacturiForm.btnToken`; the capture tag `efactura-token` moved with the text.
- `contabil/efactura/index.md`: the token sections replaced by a short «Tokenul ANAF» pointer with a link; the accounts paragraph of «Fila Vânzător» shortened to a link; `screens:` reduced to `FacturiForm`; tags updated (`00EF-12`, `0000-56`).
- `help-version.txt` not changed (already `2026-10-07`).

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/{index,token,conturi}.md`, this file, `SLICE-00EF-12-conturi-unitate.md`, the status files.

## Test results
Nothing was run for this step (operator: no build, no tests, no git). `Check-Help.ps1` was NOT run after the split: the new topics, their `parent`, `screens`, tags and the two `topic:` links were written by hand and are unchecked. Earlier, before the split, it had reported only the missing worklog of `0000-56` for `contabil.efactura` (now written).

## Unverified / deferred
- Run `tools\HelpCheck\Check-Help.ps1 -Coverage -Map` once: the `order` values 76 / 77, the `FacturiForm.btnToken` / `FacturiForm.btnVConturi` screen keys and the moved capture tag are the likely places for a complaint.
- Captures to make by the operator: `conturi-unitate` (new); `efactura-token` (unchanged, now in the token topic).
- Text only; the windows were never seen on screen.
