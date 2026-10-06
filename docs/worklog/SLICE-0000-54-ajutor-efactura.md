# SLICE-0000-54 - Help for 00EF-05: the «E-Factura» menu row and the token window (text; not seen on screen)

Help sub-slice (operator rule 30.09.2026: any visual change goes into the help with its slice number).

## What changed and why
- New topic `contabil.efactura` (`HelpContent/contabil/efactura/index.md`, order 75 under «Despre K-BOT»): what the window shows, how to get or renew the token
  (connect the token, «Autorizează» / «Reînnoiește tokenul», pick the certificate, PIN, result), and the messages the operator can meet. `screens:` `TokenForm`,
  `CertificatePickerForm`; `open: menu:efactura`. Quotes are the exact captions / messages of the code (`TokenForm.vb`, `TokenForm.Designer.vb`,
  `CertificatePickerForm.Designer.vb`, `TokenAuthorizer.vb`, `AnafAuthorizeClient.vb`).
- `contabil.fereastra`, section MENIU: new row «E-Factura» with a link; `tur-fereastra`, step «Butonul MENIU»: the sentence names the new window.
- `HelpContent/README.md`: `efactura` added to the `menu:` keys. No change to `KbotForm.HelpCapture.vb`: `menu:` already finds any row by key.
- Left out on purpose (users only): how the server keeps and renews the token, the code exchange, the certificate rules' internals.

## Files touched
New: `HelpContent/contabil/efactura/index.md`, this file. Edited: `HelpContent/contabil/fereastra.md`, `HelpContent/tours/tur-fereastra.md`, `HelpContent/README.md`,
`docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the last run in the 00EF-05 report; the only remaining errors are the pre-existing ones in
`tutorials\ordonantare-din-plata.md` (missing slice tags, not touched here). The `dotnet build` of `KBot.App` is clean.

## Unverified / deferred
- **Capturi noi, lipsă:** `efactura-token` (window with a unit that already has a token; prepare text in the tag). The certificate picker has no capture
  (it needs a real token).
- Nothing was run: the text is written from the code, not from the window on screen.
- The help watermark (`help-version.txt`) stays `2026-10-06` (same day as 0000-53).
