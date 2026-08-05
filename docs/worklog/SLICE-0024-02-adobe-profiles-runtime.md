# SLICE-0024 (pass 02) — the profiles reach the DDF «Document» tab, switchable while running

Continues slice **0024**. Pass 01 (`SLICE-0024-01-adobe-shared-host.md`) stands as written.

## §0 What this pass ships

`ReaderHostPreview` rewritten onto `AdobeReaderHost`, the two runtime settings on the PDF tab of
`DdfView`, and `docs\SETARI_UTILIZATOR.md`.

`DdfPreviewFactory` and its compile-time `Mode` constant are **untouched**. That switch chooses
which preview *implementation* the «Vizualizare» page uses; this slice's setting chooses how the
Adobe host *behaves*. They are not merged and are not related — note that the «Document» page has
mounted `ReaderHostPreview` unconditionally since 0020-04, whatever the factory constant says, so
this setting matters on every build.

## §1 `ReaderHostPreview` — what is left of it

Everything Win32 is gone (pass 01). What remains is UI: three states, the theme, the generate
button, and a fourth element — `lblNote`, a discreet bottom strip that appears **only** when the
Adobe version was not recognised, carrying `AdobeUiDetector.UnrecognisedNote` in Romanian.

Behaviour changes worth naming:

* **Embedding is asynchronous now.** It used to block the UI thread for up to 8 seconds polling
  `EnumWindows`. `ShowDocument` stays synchronous (the `IDdfPreview` contract), starts
  `EmbedAsync` fire-and-forget and guards against a stale response with `_requestedPath` — the same
  pattern as every view's `LoadAsync`.
* **The process is not always closed.** The old code called `CloseMainWindow()` on whatever
  `Process.Start` returned. Under the modern profile (`/n` off) Adobe may hand the document to the
  operator's own instance, and closing that would take their other documents with it.
  `AdobeReaderHost.Detach` now closes the process **only** when K-BOT started it *and* it owns the
  hosted window, and logs the refusal otherwise.
* **Every failure path produces a Romanian sentence on screen** (brief §6): Adobe not installed,
  launch failed, embed timeout, file missing, watcher failed to attach. Nothing is swallowed — the
  full detail goes to `<AppDir>\Logs\adobe_preview.log`, exceptions to `harness_errors.log`.
* **The popup watcher is on here and only here** (`PopupWatchEnabled = True`), so the bench's
  behaviour stays byte-identical to before the extraction.

## §2 The setting on screen

`pnlAdobe`, a `DockStyle.Top` band inside `pnlPdf`, declared in `DdfView.Designer.vb` like every
other control (house rule), carrying two labelled `DropDownList` combos:

| «Mod vizualizator Adobe:» | Automat · Modern · Clasic |
| «Instanță nouă Adobe:» | Automat · Da · Nu |

The band lives on the **«Document»** page rather than in a settings dialog because that is where the
effect is visible: changing the mode re-places the window hosted right there.

Both combos are backed by `AdobeModeItem` / `AdobeNewInstanceItem` wrappers instead of raw strings —
reading the selection must never mean comparing UI text. A change **persists both settings** (they
describe the same host; saving one and leaving the other is how a file ends up disagreeing with the
panel) and then calls `ReaderHostPreview.ReapplySettings`, which re-applies the geometry to the
document already on screen.

**A limit stated rather than hidden:** geometry re-applies immediately; the launch flags (`/n`,
`/s`, `/A`) cannot — you cannot change the command line of a process that is already running. They
take effect on the next document. That is in the log line, in the code comment and in §2.5 of the
operator document.

`_suppressAdobeComboEvent` guards the population: filling a ComboBox raises
`SelectedIndexChanged`, so without it merely **opening** the view would overwrite the stored setting
with whatever landed in the list first. There is a test for exactly that.

When `KBotPaths.Save` fails (read-only install folder), the setting still applies for the session and
the view says so — a silent failure would be discovered next launch, when the value is mysteriously
back.

## §3 The documentation (brief §7)

There was **no** user-facing settings document under `docs\` — checked: the six files there are
plans, a handoff, a Redis note, a Playwright note and the forms convention. So
**`docs\SETARI_UTILIZATOR.md` is new and this is its first entry**, in Romanian with real
diacritics, covering all six required points per setting plus the limitations section: the
measured geometry, the Acrobat build it was measured on (26.1.21771.0), the fact that an Adobe
update can invalidate it, and that `AdobeReaderHarnessForm` is where new values get measured.

It documents a third setting too — `DdfPdfRoot`, which already existed since 0020-04 and had no
operator-facing description anywhere.

## Files touched

* `src\KBot.App\Views\Ddf\ReaderHostPreview.vb` — rewritten (325 → 205 lines; all interop removed)
* `src\KBot.App\Views\Ddf\ReaderHostPreview.Designer.vb` — `lblNote`
* `src\KBot.App\Views\DdfView.vb` — `BuildAdobeCombos`, `AdobeSetting_Changed`, theme cascade,
  `AdobeModeItem` / `AdobeNewInstanceItem`
* `src\KBot.App\Views\DdfView.Designer.vb` — `pnlAdobe` + two labels + two combos
* `docs\SETARI_UTILIZATOR.md` — **new**
* `tests\KBot.App.Tests\DdfViewTests.vb` — 7 new headless STA tests
* `KBot.App` `FileVersion` 1.0.3.0 → 1.0.4.0

## Test results

* `dotnet build KBot.sln` — **0 errors, 0 BC warnings** (16 pre-existing NU1701).
* `dotnet test KBot.sln` — **663 passed / 0 failed / 0 skipped** (580 before slice 0024):
  Controls 201, DevHarness 153, App 143, Api 68, Xfa 39, Theming 27, Domain 17, Common 14,
  LocalStore 1.
* The 7 new App tests cover: the band is on `pnlPdf` and docked Top; both combos carry exactly the
  three documented Romanian labels in order; the combos start on the **stored** value; an invalid
  stored value starts on «Automat» without throwing; changing one combo persists both; and building
  the combos does not itself write the settings.

## Anything left unverified or deferred

* **NO VISUAL VERDICT, AND NO ADOBE.** Nothing in this slice has been seen on screen and no PDF has
  been hosted. The band's width, the combos' theming, whether the modern profile's −130/230/152
  actually removes the chrome *in the app's panel* (it was measured in the bench's panel, which is
  a different size) — all unobserved. This is the biggest open item and it can only be closed by
  running the app with a real DDF PDF.
* **The popup watcher has still never hidden a popup** (see pass 01). It is enabled here, so the
  first real run is also its first test.
* **The «relaunch once» path is untested at run time.** With `Auto` on the first document of a
  session, K-BOT launches with the classic flags, detects, and relaunches if the detected profile
  needs different ones. That means the first modern-Adobe document of a session can start Adobe
  **twice**. It is bounded (once per document, `_relaunchedForProfile`) and logged, but whether it
  looks acceptable to the operator is unknown.
* **`newInstance = false` on the modern profile remains a live risk** (brief §1). Implemented as
  measured; logged loudly; the process is not closed when it was not ours; the operator can force
  «Da». **A decision is still owed on whether the shipped default should be forcing `/n`.**
* The setting is per **installation**, not per user (pass 01 §4). If two Windows users share one
  K-BOT install and have different Adobe versions, they share one value.
* `docs\SETARI_UTILIZATOR.md` describes `DdfPdfRoot` as file-only; the configuration form that would
  edit these values in the UI is still a later slice.
