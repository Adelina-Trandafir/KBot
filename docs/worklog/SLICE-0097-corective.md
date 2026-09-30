# SLICE-0097 — Corective updates (operator request, 30.09.2026)

Nine operator requests in one message. Answers given by the operator before the code
(30.09.2026): a signed DDF revision keeps only «Trimite / Reia trimiterea» in its menu; the new
tree roots are in ORD **and** DDF; a month / «toate» delete is **refused** (not offered) when any
document under it is signed; the re-login interval acts **at session expiry**, not on a timer.

## What changed and why

### ORD / DDF views
1. **No message after a delete** (ORD single / ORD group / DDF revision / document / month /
   «toate»). The counts that the box showed go to `mesaje_operator.log` only (`OperatorLog`).
   Errors are still shown.
2. **Root node** in both trees: «Toate ordonanțările» (`OrdView`) and «Toate reviziile»
   (`DdfView`), key `all`, months under it — the same shape as «Tot istoricul» (0095). Clicking it
   = what the view shows on load (every line).
3. **Delete on the month and on the root.**
   - ORD: «Șterge TOATE ordonanțările lunii» / «Șterge TOATE ordonanțările» → new
     `OrdActiune.StergeGrup`; the shell (`StergeOrdonantarileAsync`) confirms once, then one
     `DELETE /api/forexe/ord/{idordp}` per ordonanțare (same call as the single delete), stopping
     at the first failure and saying how many went. No new server route.
   - DDF: the month entry existed; the root adds «Șterge documentul (TOATE reviziile)» →
     `DdfActiune.StergeToate` → the existing `DeleteDdfAsync(iddf)`.
   - Both offered only when **no** document under the node is signed; the shell re-checks.
4. **Signed = at least one signature**: `Semnatura` non-empty OR a signed PDF on the server
   (`ArePdfSemnat`) — `OrdView.EsteSemnata` / `DdfView.EsteSemnata`.
   - ORD leaf signed → **no context menu at all** (so «Adaugă ordonanțare» and «Generare în lot»
     disappear with it).
   - DDF leaf signed → no «Modifică / Șterge revizia / Șterge documentul»; «Trimite / Reia
     trimiterea» stays when the state allows it; no entry left → no menu.

### KbotForm
5. **Collapsed nav flyout too narrow at scales other than 150%** (`KBotNavList` +
   `KBotNavFlyout`). The flyout width left the caption EXACTLY its measured width, measured on
   the screen DC with other flags than the ones used to draw it. Now: measured on this window's
   own device context, with the draw flags, with both fonts (regular + semibold, the wider wins),
   plus a scaled gap; the flyout also ignores `WM_DPICHANGED` (a per-monitor-aware form crossing
   to a monitor of another DPI was resized by the suggested rectangle). **Hypothesis, not a
   proven cause** — nothing was reproduced on another scale here.
6. **«Note corecție» only when the branch has notes**, like ORD/DDF. Server: `GET
   /api/forexe/tree` returns `AreNoteCab` = `EXISTS FX_NoteCAB_Corectii.CodAngajament`, or a
   literal 0 when that table does not exist on the unit database (checked in
   `information_schema` per request — `000_DEMO` does not have it). Client: `AngajamentTreeInfo
   .AreNoteCab`, gating in `ApplyViewGating` / `IsViewEnabled`; a note saved on the selected
   angajament raises the flag locally (`AprindePoartaNotelor`).

### Caption bar — unit selector
7. `KBotCaptionBar` gains a painted non-editable drop-down after the title
   (`KBotCaptionBar.UnitSelector.vb`: `SetSelectorItems`, `ClearSelector`, `SelectorSelectedKey`,
   event `SelectorChanged`). Shown only with two or more choices; otherwise the bar is unchanged.
   The list is a `CustomPopup` with a check on the current unit.
   - Server (gitignored `routes/auth/auth.py`): `GET /api/auth/my-units` (the operator's SELECT
     on `Unitati_Utilizatori ⋈ Unitati`, filtered by the session's UN) and `POST
     /api/auth/switch-unit` (bearer; same access check and answer as `/login`, new token, old token
     revoked, journal action `SWITCH_UNIT`). No password needed: the live token proves identity.
   - Client: `IAuthApi.GetMyUnitsAsync` / `SwitchUnitAsync`; `UnitInfo` gets `CF`, `Rol`;
     `KbotForm.Units.vb` loads the list after the tree, and on a pick: switch → session → the unit
     remembered (`LastLoginStore`) → combos, tree and views reset → periods + tree + «operațiuni
     necorelate» mark, as after login.
   - A live FOREXE session is **closed first, silently** (operator, 30.09.2026: «se deconectează
     și apoi se schimbă unitatea»): the browser session belongs to the unit being left, and
     downloads write into the session's database. New `IForexeDisconnect` (KBot.Forexe, apart from
     `IForexeRunner` so the test doubles stay as they are) → `ForexeRunner.DisconnectAsync` (the
     same teardown a new connection does) → `ForexeController.DisconnectAsync(forgetCertificate)`.
     The REMEMBERED certificate (the one «Conectare» uses without asking,
     `CertificateService.LastUsedCertificate`) is forgotten **only** when the operator ticks the new
     switch «Setări → FOREXE → Uită certificatul memorat când schimb unitatea din bara de titlu»
     (`AppSettings.ForexeForgetCertificateOnUnitSwitch`, **off by default**, operator 30.09.2026).
     **Refused** only while a FOREXE operation is running, and for a unit where the role is
     «Director» (that one is the signing window).

### Setări → Autentificare + login window
8. New section «În timpul lucrului (sesiunea expirată)»:
   - «Reafișează fereastra de autentificare când expiră sesiunea» (`ReloginPrompt`) — changeable
     only with the advanced options; without them it is always on.
   - «Reafișează cel mult o dată la» (`ReloginMinutes`): 10, 15, 20, 30, 45, 60 minutes; with the
     advanced options also 5, 90, 120, 240, 480. Without them the value in effect is clamped to
     10..60.
   - At startup the login window is always shown (unchanged).
   - Mechanism (`KbotForm.ReautentificaAsync`, called by `WithReauth` on a 401): if the window is
     off, or the last login typed in the window is younger than the interval, K-BOT logs back in by
     itself (`/api/auth/login` on the SAME unit) with the password kept in memory; otherwise, or if
     that fails, the window. One silent re-login at a time (semaphore); a server refusal drops the
     kept password.
9. Login window: «Ține minte parola până la repornirea calculatorului» under the password.
   Visible only with the advanced options AND the new switch «Arată în fereastra de autentificare
   bifa…» (`RememberPasswordOption`, default on, shown only with the advanced options) plus
   «Uită parola memorată». Storage (`KBot.Common/SessionCredentials.vb`): DPAPI (current Windows
   user, via crypt32 P/Invoke — no package) in a **volatile** HKCU key
   `Software\AVACONT\KBot\Session`, which Windows discards at sign-out / restart. Unticked at login
   → the stored pair is erased. The in-process copy for item 8 is DPAPI-encrypted too.

## Files touched

- `src/KBot.App/Views/OrdView.vb`, `Views/Ord/OrdComanda.vb`, `KbotForm.Ord.vb`
- `src/KBot.App/Views/DdfView.vb`, `Views/Ddf/DdfComanda.vb`, `KbotForm.Ddf.vb`, `KbotForm.DdfDelete.vb`
- `src/KBot.Controls/NavList/KBotNavList.vb`, `NavList/KBotNavFlyout.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.vb`, **new** `CaptionBar/KBotCaptionBar.UnitSelector.vb`
- `src/KBot.App/KbotForm.vb`, `KbotForm.Views.vb`, `KbotForm.CabNotes.vb`, **new** `KbotForm.Units.vb`
- `src/KBot.App/LoginForm.vb`, `LoginForm.Designer.vb`
- `src/KBot.App/Setari/SetariAutentificareView.vb`, `SetariAutentificareView.Designer.vb`
- `src/KBot.Common/AppSettings.vb`, **new** `KBot.Common/SessionCredentials.vb`
- `src/KBot.Api/IAuthApi.vb`, `AuthApi.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb`
- `src/KBot.Domain/AngajamentTreeInfo.vb`, `Auth/UnitInfo.vb`
- `src/KBot.Forexe/ForexeRunner.vb`, **new** `KBot.Forexe/IForexeDisconnect.vb`,
  `src/KBot.App/Forexe/ForexeController.vb`, `src/KBot.App/Setari/SetariForexeView.vb`,
  `SetariForexeView.Designer.vb`
- `PYTHON/routes/forexe/tree.py`; `PYTHON/routes/auth/auth.py` (**gitignored — deploy by hand**)

## Test results

- `dotnet build src/KBot.App/KBot.App.vbproj --no-incremental`: **0 errors, 0 warnings**.
  `KBot.DevHarness`: 0 errors.
- Python: syntax check (`ast.parse`) of `tree.py` and `auth.py` only.
- **No tests run, no test code written** (operator rule). Nothing seen on screen.

## Left unverified or deferred

- Nothing run, nothing on screen: menus, roots, caption selector, settings section, login box.
- **Flyout**: the fix removes two plausible causes; the real one was not reproduced.
- **Server not deployed**: `tree.py` (AreNoteCab) and `auth.py` (my-units / switch-unit) must be
  copied to the VPS. Until then: the notes view never shows (the flag is missing → false) and the
  unit list call fails silently (the title stays the plain unit name).
- `PYTHON/tests/test_forexe_tree.py` fakes the cursor with the old 22 columns and one `execute`;
  it will need the extra `information_schema` call and the 23rd column (not touched: no tests).
- ORD group delete leaves the «ord» nav entry on after every ordonanțare is gone (same as the
  single delete: corrected on the next tree reload).
- Unit switch: open secondary windows (Nomenclatoare, Extrase de cont, …) are not closed; they
  keep the previous unit's data until reopened. The FOREXE disconnect on a switch was never run:
  whether the docked «Browser FOREXE» view empties cleanly is unverified.
- The re-login window still lets the operator pick another unit (existing behaviour); the caption
  follows the session afterwards.
- FileVersion of KBot.App / KBot.Controls / KBot.Common / KBot.Api / KBot.Domain not bumped
  (`push-update.ps1` asks).
- **Help to update** (0000): `contabil.vederi.ord` (root, group delete, menu gone on signed),
  `contabil.ddf` / `contabil.ddf.semnare` (root, «toate», signed = no edit/delete), `contabil.fereastra`
  (unit selector in the title; «Note corecție» only with notes), `contabil.notecab`,
  `contabil.autentificare` (remember-password box; silent re-login), `contabil.setari` (the new
  Autentificare section); tours `tur-ord`, `tur-ddf`, `tur-setari`.
