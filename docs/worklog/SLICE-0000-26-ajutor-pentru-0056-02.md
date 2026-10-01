# SLICE-0000-26 — the help for slice 0056-02

Slice 0056-02 (`SLICE-0056-02-corecturi-la-descarcare.md`, operator, 01.10.2026) lets the
operator move old links while placing a download. This is the help for it, written in the same
task, every changed section tagged `0056-02`.

## What changed and why

| Topic / tour | What |
|--------------|------|
| `contabil.asocieri.cazuri` | «Legături blocate» rewritten: only an ordonantare (dated on the snapshot's day or later) blocks, a payment does not, and the rule is the same after a download. «După o descărcare: salvare sau renunțare»: old links can be corrected in the same window, only the moved ones are written, why it matters («Lanțul nu se închide»), the warning instead of a refusal when a reception only loses a snapshot, what «Golește așezările» does with moved old links |
| `contabil.asocieri.fereastra` | the sentence about a blocked snapshot (ordonantare only; everything else can be moved, also after a download); the «Salvează legăturile» and «Golește așezările» rows of the buttons table |
| `tur-receptii` | step «Arbore › Asocieri»: the blocking sentence no longer mentions payments |

The «or a payment» wording was stale since 18.09.2026 (payments stopped blocking); corrected
here because the same sentences had to be rewritten.

Left out on purpose (HELP_SYSTEM §5): anchors, how the corrections travel to the server, the
journal.

## Files touched

- `src/KBot.App/HelpContent/contabil/asocieri/cazuri.md`, `fereastra.md`
- `src/KBot.App/HelpContent/tours/tur-receptii.md`
- `help-version.txt` stays `2026-10-01`

## Test results

- `tools\HelpCheck\Check-Help.ps1 -Coverage`: «No errors.», 48 topics, 17 tours, 57 capture tags.
  Coverage names only `RobotQueueForm` (0098, already in the open threads).
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**.
- Nothing seen on screen.

## Capturi de refăcut / noi

- None. The window looks the same; after a download fewer rows carry the lock.

## De citit de operator

- `contabil.asocieri.cazuri`, «După o descărcare»: the sentence «K-BOT salvează și te
  avertizează după salvare; nu refuză» describes the server rule as written (a reception that
  only loses snapshots); it was not seen on a real save.
