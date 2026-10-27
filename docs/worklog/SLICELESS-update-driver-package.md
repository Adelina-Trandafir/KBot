# SLICELESS - Playwright driver as its own update package (12.10.2026)

## What changed and why
`.playwright\` (node.exe + cli, ~100 MB) sat inside every update zip although it only changes with the Playwright version (pinned 1.49.0). It is now a separate package, fetched only when the version changes.

- `publish-release.ps1`: the update zip no longer carries `.playwright/` (guard added); step 7a writes `artifacts\KBot_Driver_<playwright version>.zip` (top folder `KBot_Driver_<v>/`, `.playwright/` inside). The staging folder is untouched, so the Inno installer still ships the driver.
- `push-update.ps1`: uploads the driver zip only when the server's driver version differs or is missing; always writes the shared top-level `driver` block into `latest.json` (the file is rewritten whole); verifies the block through the API afterwards.
- `PYTHON/routes/update.py`: optional `driver` block in `latest.json`; `GET /api/update/latest` adds it; new `GET /api/update/driver` streams the zip.
- `KBot.Domain` `UpdateInfo.Driver` / `DriverInfo`; `KBot.Api` `IUpdateApi.DownloadDriverAsync`.
- `KBot.App` `AppUpdateService`: `DriverNeeded` compares `.playwright\package\package.json` version (and a node.exe under `.playwright\node`) with the server's; downloads the driver after the app zip (`UpdateProgressForm` driver mode) and passes `--driver-zip/--driver-sha256` to the updater. The offer shows the summed size.
- `KBot.Updater`: `UpdaterArgs` + `UpdaterForm` verify and apply the driver zip after the app zip.

## Files touched
publish-release.ps1, push-update.ps1, PYTHON/routes/update.py, src/KBot.Domain/Update/UpdateInfo.vb, src/KBot.Api/IUpdateApi.vb, src/KBot.Api/UpdateApi.vb, src/KBot.App/Update/AppUpdateService.vb, src/KBot.App/Update/UpdateProgressForm.vb, src/KBot.Updater/UpdaterArgs.vb, src/KBot.Updater/UpdaterForm.vb.

## Test results
`dotnet build` of KBot.Updater and KBot.App: 0 errors, 0 warnings. Both .ps1 files parse clean; update.py compiles. No tests written or run (project rule); nothing run against a real server or a real update.

## Unverified / deferred
- **Deploy order**: the new `update.py` must be on the VPS (and the service restarted) BEFORE the first `push-update.ps1`, or its final check throws "does not report the driver block".
- First push after this change uploads the driver zip (~100 MB) once; later pushes skip it.
- Existing clients keep their `.playwright\`: the updater never deletes, and their installed version (1.49.0) equals the server's, so nothing is downloaded. A client with a missing/broken driver gets it on its next update.
- A driver change alone (no new app version) is not offered: the check is triggered by the app version.
- Fresh installs are unaffected (installer still contains the driver).
- Help/NOUTATI: nothing the operator sees changes except the update window's size line and, on a driver change only, a second download window ("Se descarca componenta Playwright"). Help not updated.
