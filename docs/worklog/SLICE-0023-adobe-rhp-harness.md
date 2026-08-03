# SLICE-0023 — Adobe right-hand pane (RHP): every removal option, in the harness only

Slice number confirmed against `KBOT_STATUS.md` ("Next free slice number: 0023" — unchanged
when this pass started). Plan: pasted in-session ("PLAN — Adobe right-hand pane (RHP)").

## What changed and why

The operator cannot remove Adobe's right-hand Tools pane ("Export PDF" / "Start Free Trial")
from the embedded Reader window. The `/A` open parameters already in the harness address
*document* chrome; the RHP is *application* chrome and has no open parameter. Which of the four
candidate levers works depends on the Adobe build and cannot be settled from the repo — so this
pass builds **all four** into the existing `AdobeReaderHarnessForm` (DevHarness → categoria
"Adobe/PDF") for the operator to judge on screen, on both the classic Reader DC UI and the
modern ("new Acrobat") UI:

- **§3.A Diagnostic** — `btnProbe` walks the hosted window's children (GW_CHILD/GW_HWNDNEXT
  recursion, depth ≤ 4) and logs per node: depth, HWND, class, text (truncated 40), rect in
  `pnlHost` client coordinates, size, visibility, style/exstyle. Output goes to the harness log
  AND `AppDir\Logs\test_adobe_rhp.log` (timestamped, appended). The status line names the RHP
  *candidate* — visible, flush against the host's right edge (±8 px) and narrower than half the
  host width, widest wins — explicitly labelled a HEURISTIC in the log. This probe decides
  everything else: discrete HWND ⇒ §3.C wins; no HWND ⇒ §3.B is the only deterministic option.
- **§3.B Decupare** — geometry clipping, live (no relaunch, no kill): with `chkClip` on, the
  hosted window is positioned at `(0, -sus)` sized `(lățime gazdă + dreapta, înălțime + sus)`,
  so the clipped bands fall outside `pnlHost`. `btnClipAuto` copies the probe candidate's width
  into `numClipRight`. Implemented as a single `HostedBounds()` used by both
  `LayoutHostedWindow` and `NudgeRedraw` (the existing redraw-nudge sequence is reused — Adobe
  lays out late and will not repaint on its own).
- **§3.C Ferestre copil** — `lstChildren` (populated by the probe; class + size, `Tag`=HWND) +
  hide/show/restore-all via `ShowWindow`. Hidden handles tracked for exact restore; every call
  guarded with `IsWindow`, dead handles dropped with a log line, `NudgeRedraw` after each change.
- **§3.D Scurtături** — Shift+F4 / F4 sent to the hosted window (`SetFocus` + `SendKeys`),
  logged as EXPERIMENTAL and version-dependent; "nothing happened" is a valid recorded result
  (no retries, per plan).
- **§3.E Preferințe Adobe (HKCU)** — the three community-sourced RHP values
  (`bExpandRHPInViewer=0`, `bRHPSticky=1`, `aDefaultRHPViewMode_L="Collapsed"`) plus the
  Adobe-documented viewer-generation switch (`bEnableAv2=0`), written to the auto-detected
  AVGeneral hive (Reader DC vs Adobe Acrobat; `cboHive` can override). Apply order per plan:
  once-per-session snapshot → kill our Adobe by PID + ask (Romanian, never silent) before
  killing foreign Adobe → write with old→new logging → relaunch via the existing
  `RelaunchAsync`. Restore (button and on-close, `chkRestoreOnClose` default checked) replays
  the snapshot exactly: **absent restores to DELETION, never to 0**; wrong-type originals are
  written back verbatim.
- **§3.F Politici Adobe (HKLM)** — `bAcroSuppressUpsell=1` +
  `cServices\bToggleAdobeDocumentServices=1` under
  `HKLM\SOFTWARE\Policies\Adobe\<product>\DC\FeatureLockDown`, applied by generating
  `adobe_policy_apply.reg` / `adobe_policy_revert.reg` into `AppDir\Logs\` (UTF-16; revert built
  from pre-apply reads, with `"name"=-` deletion lines for absent values) and importing via
  `reg.exe` with `Verb=runas` — K-BOT itself is never elevated. UAC cancel (Win32Exception
  1223) shows "Operația a fost anulată de utilizator." with full detail only in the log.
- **§3.G** — `lblStatus` always shows one state block: hive in use, clip values, hidden-child
  count, HKCU values applied, HKLM applied this session, plus the last action message.

Pure logic extracted behind an I/O seam so tests never touch the real registry
(`Internal/Adobe/`): `AdobeRegistryConstants` (the §2 names, transcribed — never from memory),
`RegistryValueSnapshot` + `RegistrySnapshotSet` (three-state snapshot: absent /
present-with-value / wrong-type-as-present-verbatim; capture idempotent per session),
`RegFileBuilder` (header, `dword:` hex, deletion lines, escaping), `AdobeHiveResolver`
(hive/product from key existence + exe name), `IRegistryAccess` (seam) and `WinRegistryAccess`
(the real HKCU/HKLM-read boundary; logs + rethrows per house rule).

## Files touched

- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.vb` — all §3 sections (see above)
- `src/KBot.DevHarness/Internal/AdobeReaderHarnessForm.Designer.vb` — ~30 new controls, all
  declared here (house rule); flowLeft widened 320→360
- `src/KBot.DevHarness/Internal/Adobe/AdobeRegistryConstants.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/RegistryValueSnapshot.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/RegistrySnapshotSet.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/RegFileBuilder.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/AdobeHiveResolver.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/IRegistryAccess.vb` — new
- `src/KBot.DevHarness/Internal/Adobe/WinRegistryAccess.vb` — new
- `tests/KBot.DevHarness.Tests/` — **new test project** (`KBot.DevHarness` had none; the pure
  helpers live in `KBot.DevHarness/Internal/Adobe/` and are covered from here):
  `RegistrySnapshotSetTests` (6), `RegFileBuilderTests` (8), `AdobeHiveResolverTests` (8),
  `FakeRegistryAccess` (in-memory seam)
- `KBot.sln` — KBot.DevHarness.Tests added under the Tests folder
- `src/KBot.DevHarness/KBot.DevHarness.vbproj` — FileVersion 1.0.2.0 → 1.0.3.0

Out-of-scope files NOT touched, per plan: `ReaderHostPreview.vb`, `IDdfPreview.vb` /
`DdfPreviewFactory` (mode constant stays `XfaXml`), installer/publish/startup. No registry
write happens outside an explicit button press in this harness.

## Test results

- `dotnet build KBot.sln` — 0 errors, 0 BC warnings (the 16 NU1701 iTextSharp/BouncyCastle
  warnings pre-date this slice).
- `dotnet test KBot.sln` — **458 passed / 0 failed / 0 skipped** across 9 projects
  (was 436; +22 new: DevHarness.Tests 22).

## What the operator will record (empty — fill in on screen)

For each of: classic Reader DC, and the modern ("new Acrobat") UI —

| # | Question | Classic DC | Modern UI |
|--:|----------|------------|-----------|
| 1 | Is the RHP a discrete child HWND in the probe output? Class name and width. | | |
| 2 | Does hiding that child remove the pane, and does it stay hidden when the document changes? | | |
| 3 | Does clipping work, and at what `numClipRight` value? | | |
| 4 | Does Shift+F4 do anything? | | |
| 5 | Do the three HKCU RHP values collapse the pane, and does the collapse survive opening a document? | | |
| 6 | Does `bEnableAv2 = 0` change the answer to 5? | | |
| 7 | Does the HKLM policy pair remove the pane, or only its contents (Export PDF, trial upsell)? | | |

## Anything left unverified or deferred

- **Nothing in plan §2 has been tested on a real machine.** Every registry key/value name is
  transcribed from the plan (Adobe ETK + community threads). If a name is absent on the
  operator's build, that absence is the result to record here — no alternative names were
  invented (plan §8). The known community-reported failure (Adobe overwriting
  `aDefaultRHPViewMode_L` with a binary zero on document open; no effect on Acrobat Pro
  2023.003) is exactly what the bench exists to observe.
- **No visual verdict** — the new sections have never been rendered on screen (like the rest of
  the harness form). The whole slice is a human-verdict bench; code green only.
- **`AdobeUtils.GetAdobeReaderPath` / `IsAdobeViewer` (KBot.Xfa) were NOT reused** although the
  plan suggested it: both are `Private` in `AdobeUtils`, and DevHarness does not reference
  KBot.Xfa — making them reusable means touching KBot.Xfa, which this pass does not do. The
  harness keeps its own App-Paths-based resolver (documented in code). If reuse is wanted,
  a later pass should make them `Public Shared` in KBot.Xfa and swap the harness over.
- **Deliberate deviation (on-close restore):** foreign Adobe processes are NOT killed during
  the on-close restore — there is no consent dialog at that point and the plan's own rule
  ("never kill the operator's own open documents silently") wins; instead their presence is
  logged with the warning that the restore may be overwritten when they exit. On restore
  failure the operator gets a MessageBox naming the exact keys (lblStatus alone would never be
  seen on a closing form) — the plan asked for lblStatus; both are done.
- **Exotic HKLM originals:** a pre-apply HKLM value that is neither DWORD nor REG_SZ cannot be
  represented in the generated revert `.reg`; it is logged as needing manual revert instead of
  being guessed at.
- The keyboard shortcuts (§3.D) are labelled experimental in code and log; no retries by
  design.
