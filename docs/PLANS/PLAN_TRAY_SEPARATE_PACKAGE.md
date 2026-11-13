# PLAN — «Signer edition»: KBot.Tray as a separate, minimal program for people who only sign (slice family TRAY, proposed TRAY-02)

Operator request 10.11.2026, clarified the same day:
- Accountants (the full program) do NOT need a separate tray program.
- People who ONLY sign get a super-simplified variant: documents to sign + viewing a document + signing it. Nothing else.
- Its package and version are its own; a «tray only» push mode builds, signs and publishes ONLY that exe. The other exes (KBot.App.exe, Updater, Migrators) are never part of it, so those users cannot open them.
- The operator removes the tray already installed from the old zip by hand; no bridge version.
Progress is NOT kept here (see `docs/worklog/state/KBOT_STATUS_TRAY.md`).

## Today (read from the files, 10.11.2026)

- `publish-release.ps1` step 4c3 builds `KBot.Tray.exe` and puts it in the K-BOT zip; step 4d signs it (one of 6 SimplySign confirmations per build).
- The tray only LISTS documents (`TrayChecker`, `DocumentsToast`, own login `TrayLoginForm`). A click writes `tray_request.json` and starts `KBot.App.exe`, which opens `DirectorForm` (`src/KBot.App/Director/`, ~1300 lines with its Designer) -> `DdfView` (`src/KBot.App/Views/Comune/DdfView*.vb`, ~1900 lines + `Ddf/` folder) which hosts Adobe and does the signing (KBot.Xfa).
- So the viewing and signing code lives in `KBot.App`. A signer who has no `KBot.App.exe` cannot sign today.
- Server `PYTHON/routes/update.py`: `latest.json` has `access` / `non_access` blocks and one shared `driver` block with its own zip and version; the signer edition can follow the same pattern (a `tray` block).

## Target

### 1. Normal push no longer carries the tray
`publish-release.ps1`: remove step 4c3 and the tray sign line (5 confirmations per build). `publish-debug.ps1` keeps its copy for local tests.

### 2. The signer edition (the real work)
It must open `DirectorForm` / `DdfView` itself. Two ways, to be chosen:
- **A. Move the signing windows to a shared project** (new `KBot.Signing`, or into `KBot.Controls`/`KBot.Xfa` families as fits): `DirectorForm`, `DdfView` and what they use move out of `KBot.App`; `KBot.App` and `KBot.Tray` both reference them. Clean, nothing of the app ships to signers; but it touches ~3200+ lines of the app and everything `DdfView` calls inside `KBot.App` has to come along (to be measured file by file).
- **B. `KBot.Tray` references `KBot.App.dll`** and shows `DirectorForm` from it. Almost no code moves, but the signer package then carries the whole app as a DLL (no `KBot.App.exe`, so nothing to double-click, but the code is there and the package is large).

Either way the tray stops being a launcher: the click opens the document in-process; `tray_request.json`, `KBotLauncher` and `ShadowCopy` go away for the signer edition (the exe is installed where the signer puts it and updates itself, see 4).

### 3. «Tray only» push: `push-tray.ps1`
Builds ONLY `src\KBot.Tray` (+ its signing dependencies per 2), asks only «sign?» and «bump?» (FileVersion of `KBot.Tray.vbproj`), signs ONLY the tray exe, uploads `updates/KBot_Tray_<ver>...` as `*.part` + rename, rewrites `latest.json` whole with a new `tray` block (`access`, `non_access`, `driver` read back and kept). Same sftp transport as `push-update.ps1`.

### 4. Server (written locally; the operator publishes it by hand, never me)
`routes/update.py`: validated `tray` block, added to `GET /api/update/latest`, plus `GET /api/update/tray` (like `/driver`).

### 5. Self-update of the signer edition
At start the tray asks `/api/update/latest`, and if the `tray` version is newer it downloads, checks sha256, and replaces itself (a tiny swap-and-restart; it cannot overwrite the exe it runs from, so it runs from a shadow copy as today or uses a rename trick). Autostart (Run key) stays.

### 6. Help
Anything the signer sees (the window, the update) is recorded as `0000-NN` with the slice tag; no inner workings.

## Decisions (operator, 10.11.2026)
1. Option **B**: `KBot.Tray` references `KBot.App.dll` and opens `DirectorForm` / `DeSemnatRestransForm` from it. Package size does not matter; the point is that there is no `KBot.App.exe` to double-click.
2. Roles of the signer edition: **Director, Administrator, Secretar**.
3. The full program (accountants) carries no tray; the operator removes an already installed one by hand (no bridge version).
4. The server channel is `tray.json` + `/api/update/tray[/latest]` (not a block of `latest.json`, which `push-update.ps1` rewrites whole). `push-tray.ps1` is a row of KBotAdmin «Scripturi». The server file `update.py` is published by the operator by hand.
5. No Setari switch (it was an earlier idea, dropped).
6. First install: an **installer** (Inno Setup, per user, no administrator, `%LOCALAPPDATA%AVACONTKBotSemnare`), built by `push-tray.ps1` and handed to the signers; not uploaded to the server.

Done and still open: see `docs/worklog/SLICE-TRAY-02-editia-pentru-semnatari.md` and `state/KBOT_STATUS_TRAY.md`.
