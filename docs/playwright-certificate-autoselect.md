# Playwright pin & automatic certificate selection

**TL;DR — `Microsoft.Playwright` is pinned to `1.49.0` on purpose. Do not bump it
without re-testing automatic client-certificate selection on
`forexe.mfinante.gov.ro`.**

## Why 1.49.0 is pinned

FOREXE login needs the browser to auto-select the signed-in user's client
certificate without showing the Windows "Select a certificate" dialog. The
certificate's subject/issuer are read at runtime from the selected cert (they
differ per client), so nothing is hard-coded. This is done by writing a Chromium
policy at runtime — see `ConfigureAutoSelectCertificatePolicy()` in
[`src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb`](../src/KBot.Forexe/Executor/WorkflowExecutor.Browser.vb):

```
HKCU\Software\Policies\Chromium\AutoSelectCertificateForUrls
  "1" = {"pattern":"https://forexe.mfinante.gov.ro:443",
         "filter":{"ISSUER":{"CN":"..."},"SUBJECT":{"CN":"..."}}}
```

- **Playwright 1.49.0** ships **Chromium build 1148**, which **reads** this
  registry policy → the certificate is auto-selected. ✅
- **Playwright 1.61.x** ships **Chromium build 1228**, which **does NOT read**
  the policy from `HKCU\...\Policies\Chromium` *or* `...\Policies\Google\Chrome`.
  `chrome://policy` in the Playwright-launched browser is **empty**, so the
  certificate dialog reappears and the user must pick the cert manually. ❌

The certificate-handling code itself was verified **byte-identical** to the
original working `Surse\SURSA_FOREXE\` sources (`WorkflowExecutor.Browser.vb`,
`CertificateService.vb`, `WorkflowExecutor.Actions.AuthClick.vb`). The
regression is purely the bundled Chromium build, not our code.

## The pin

`Microsoft.Playwright` `Version="1.49.0"` in **both**:

- [`src/KBot.Forexe/KBot.Forexe.vbproj`](../src/KBot.Forexe/KBot.Forexe.vbproj) — the project that actually uses Playwright.
- [`src/KBot.App/KBot.App.vbproj`](../src/KBot.App/KBot.App.vbproj) — direct reference so `.playwright\` lands next to `KBot.App.exe` in build/publish output.

The two versions **must match**.

## If you ever change the version

1. Update the version in **both** `.vbproj` files (keep them identical).
2. **Clean build is mandatory** — `.playwright\` is `PreserveNewest` content, so
   an older package will *not* overwrite a newer stale copy, and the build will
   silently keep the wrong driver:
   ```sh
   rm -rf src/*/bin src/*/obj
   dotnet build src/KBot.App/KBot.App.vbproj -c Debug
   ```
3. Verify the driver actually changed:
   ```
   src\KBot.App\bin\Debug\net8.0-windows\.playwright\package\package.json   -> "version"
   src\KBot.App\bin\Debug\net8.0-windows\.playwright\package\browsers.json   -> chromium "revision"
   ```
4. Re-deploy and **re-test the live FOREXE connect**: the cert must be
   auto-selected with no dialog. If the dialog reappears, the new Chromium build
   doesn't honour the registry policy — revert to 1.49.0.

> Note: a smartcard certificate has a non-exportable private key, so Playwright's
> `BrowserNewContextOptions.ClientCertificates` API is **not** an alternative —
> the OS/registry-policy path is required. That's why staying on 1.49.0 is the
> right call.

## Deploying to a client PC (`C:\KBOT`)

1. Run the latest `KBot_Setup_<stamp>.exe` (silent install to `C:\KBOT`,
   overwrites everything).
2. Install the matching browser once per Windows user:
   ```powershell
   cd C:\KBOT
   .\playwright.ps1 install chromium    # downloads chromium-1148 for Playwright 1.49.0
   ```
3. The client also needs **.NET Desktop Runtime 8 (win-x64)** installed
   (the app and the installer are framework-dependent).
