# SLICE-00EF-05 - KBot.EFactura project + the certificate step + menu entry «E-Factura» (code written, builds clean, NEVER RUN)

Slice 00EF (operator, 06.10.2026). Client side of the token: the project, the step with the hardware certificate, the first window,
the menu entry. The server half is 00EF-04 (`SLICE-00EF-04-server-token.md`).

## What changed and why
- **New project `src/KBot.EFactura`** (net8.0-windows, WinForms, in `KBot.sln` under the «UI» folder, referenced by `KBot.App`; it references
  Domain, Common, Api, Theming, Controls and nothing from App). FileVersion 1.0.0.0.
  - `Certificates/CertificateRules.vb` + `CertificateFinder.vb` + `AnafCertificate.vb`: the rules of the old `EF.EXE` (`LoadCertificates`) ported
    rule for rule: certificate with private key and not expired, key NOT exportable, not a Microsoft software store, a known hardware maker OR Client
    Authentication, CN without `test` / `localhost`. Only reads the store and the key's properties; never exports, copies or signs.
  - `Authorization/AnafAuthorizeClient.vb`: the call to ANAF's authorise address over TLS with the client certificate (`HttpClientHandler`, redirects
    followed, 3 attempts as in `EF.EXE`), `code` read from the FINAL address. Never logs the address or the code.
  - `Authorization/TokenAuthorizer.vb`: the order list certificates -> operator picks -> `POST token/start` -> call with certificate -> `POST token/cod`.
    Nothing reaches the server before a certificate is chosen. The PC never holds a token or a secret.
  - `Views/TokenForm` («E-Factura — tokenul ANAF»: state sentence, 7-day notice, unit / CUI / certificate / who / last error, buttons «Reîmprospătează»,
    «Autorizează» / «Reînnoiește tokenul», «Închide») and `Views/CertificatePickerForm` («E-Factura — alegerea certificatului»). Both `KBotThemedForm`,
    borderless, all controls in `.Designer.vb`, `KBotDataView` / `KBotNotice` / `KBotBusyBar` / `KBotCaptionBar` / `KBotToolTip`, `KBotMessage` for messages.
- **`KBot.Api`**: `IEFacturaApi` + `ApiClient.EFactura.vb` (`GetTokenStateAsync`, `StartTokenAuthorizationAsync`, `SubmitTokenCodeAsync`); wire names exactly the
  server's (ASCII). FileVersion 1.0.20.0.
- **`KBot.Domain`**: `EFacturaToken.vb` (`EFacturaTokenStart`, `EFacturaTokenState`). FileVersion 1.2.12.0.
- **`KBot.Common`**: `ReauthGate` MOVED here from `KBot.App\Views\Nomenclatoare\` (it depends on nothing in the shell; `KBot.EFactura` cannot reference App).
  The five existing users already `Imports KBot.Common`, so nothing else changed. FileVersion 1.5.13.0.
- **`KBot.App`**: reference to `KBot.EFactura`; header menu row «E-Factura» (key `efactura`, icon `binvoice`, after «Extrase», in `KbotForm.Designer.vb`);
  `Case "efactura"` in `MenuNou_ItemClicked`; `KbotForm.EFactura.vb` (`DeschideEFactura`: needs an open unit, modeless, one at a time). FileVersion of App NOT
  bumped here: `push-update.ps1` asks which part to bump.
- Help: topic `contabil.efactura` + menu row in `contabil.fereastra` + the main-window tour step (worklog `SLICE-0000-54-ajutor-efactura.md`).

## Files touched
New: `src/KBot.EFactura/**` (vbproj + 9 .vb + 2 .Designer.vb), `src/KBot.Api/{IEFacturaApi,ApiClient.EFactura}.vb`, `src/KBot.Domain/EFacturaToken.vb`,
`src/KBot.Common/ReauthGate.vb` (moved), `src/KBot.App/KbotForm.EFactura.vb`, `src/KBot.App/HelpContent/contabil/efactura/index.md`, this file.
Edited: `KBot.sln`, `src/KBot.App/{KBot.App.vbproj,KbotForm.Designer.vb,KbotForm.Nomenclatoare.vb}`, the three `.vbproj` FileVersions above, help files
(`contabil/fereastra.md`, `tours/tur-fereastra.md`, `HelpContent/README.md`). Deleted: `src/KBot.App/Views/Nomenclatoare/ReauthGate.vb` (moved).

## Decisions taken here (state them, change if wrong)
- **The server certificate of ANAF is CHECKED** in the authorise call. `EF.EXE` accepted any server certificate (`ServerCertificateCustomValidationCallback = True`);
  that was not copied. If a PC does not trust ANAF's server certificate the failure says the connection failed: first thing to look at on the first real try.
- **The address must be `https`; no host check.** The server hands the address over (the operator's `EF_AUTH_URL`).
- **Order: pick the certificate BEFORE asking the server to start** (the state lives 15 minutes; nothing is stored for a certificate nobody chose).
- **ReauthGate moved to KBot.Common** instead of copying it or giving the window one closure per response type.
- **No role check**: any user of the unit may open the window and authorise (same as the server, 00EF-04).
- The notice «expiră în N zile» is shown **in the E-Factura window when it is opened**. A notice on the main window at start-up is NOT built (open thread).
- Menu icon: `binvoice` (the invoice icon already used by «Extrase» and «Parteneri»); no new picture.
- Designer coordinates are written in 96 dpi (`AutoScaleDimensions = 96,96`); Visual Studio rewrites them with the first real save. The forms were never opened in the designer.

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura` and `KBot.App` (Debug): 0 errors, 0 warnings. **No test code written and no test run** (standing rule: no tests
unless asked). **Never run, never seen on screen** — no hardware certificate here.

## Unverified / deferred
- **Everything on a real certificate**: the store listing on a real token (RSACng vs CAPI providers, the export-policy test), the PIN prompt, the TLS
  handshake with `logincert.anaf.ro`, whether ANAF's redirect chain ends on the registered redirect address answering 2xx and carrying `code=` (as `EF.EXE` relied on),
  and the exchange on the server (00EF-04 caveats: form-encoded body, the real life of the refresh token).
- How the two forms look (sizes, wrapping of the long labels, the picker's grid); the theme / DPI behaviour; the designer was never opened.
- A start-up notice on the main window 7 days before the refresh token ends; `KBot.EFactura` has no tests project.
- The picture `efactura-token` (capture tag in the help) does not exist yet.
- The server (00EF-04) must be deployed with `cryptography`, the DDL and `/etc/avacont/efactura.env` before a real try.
