# SLICE-0123-01 - Filele VERCON si ADECHIT in ribbon, podul bearer, curatarea snapshoturilor si pagina «ADECHIT Rapoarte» (21.10.2026)

Operator request: bring the VERCON and ADECHIT reports of the site into K-BOT.app, as native pages (not a web control), in two new ribbon tabs (VERCON and ADECHIT, each with a «Rapoarte» button; VERCON also gets «Trimite la Vercon»). Decisions taken with the operator: ADECHIT first, VERCON afterwards with full parity (slice 0123-02); no state left on the server (the site's snapshot system is kept, but made to delete what is expired).

## What changed and why

### Server (PYTHON) - NOT deployed, the operator publishes it
- **`reporting/snapshots.py`** (shared by ADECHIT and VERCON): the saved report models used to be deleted only when the SAME user generated another report, and the owner folders were never deleted, so files stayed on the disk for good. Now `put` sweeps the expired files (and `.tmp`) of ALL owners and the folders left empty (once a minute per process), `put` survives a sweep of another worker removing its folder (second attempt), and the new `release(owner, token)` removes one snapshot at once. The first sweep also clears everything that piled up before, because all of it is expired.
- **`routes/adechit/kbot_bridge.py`** (new) + one branch in `routes/adechit/__init__.py`: the SAME routes accept a K-BOT bearer token, but only for GET context / years / `rows/LunaD` / catalog-data / child-search / `reports/<kind>/generate` / `reports/export/<token>/(pdf|xlsx)` and DELETE `reports/export/<token>`. The identity (`username` = e-mail, `db_name`) goes into `g.portal`, so the AD rights (`drepturi.operations_of`), the subunit access list and the snapshot owner work as for a site user. Anything else with a bearer token is refused by the portal check as before.
- **`routes/adechit/report_routes.py`**: `DELETE /api/adechit/reports/export/<token>` (204) releases the snapshot.

### K-BOT
- **KBot.Domain/`Adechit.vb`**: plain data (subunit, context, month, group, child, search result, report handle, report kinds).
- **KBot.Api/`IAdechitApi.vb`, `ApiClient.Adechit.vb`**: context, years, months, groups, child search, generate, download (pdf / xlsx), release. Sends `X-Ade-Unit` (the session's unit) and `X-Ade-Subunit`.
- **KBot.App/Views/Adechit/`AdechitRapoarteForm`** (+ Designer, all controls declared there): subunit / year / month / group / interval / child search (Enter) and the six reports; the PDF is shown in the embedded Adobe surface (`ReaderHostPreview`, files in the PDF work area); «Salveaza Excel» saves the report on screen (all columns); replacing the report or closing the page releases the server's copy (`ReleaseQuietly`, best effort). Rights / errors come back as the server's Romanian text on the notice line.
- **`KbotForm.Adechit.vb`**, registry entry `adechit_rapoarte`, ribbon dispatch in `AvacontMain.vb`.
- **Ribbon (`AvacontMain.Designer.vb`)**: tab **VERCON** (group «Vercon»: «Trimite la Vercon», moved from the E-Factura tab, same key `efactura_vercon`) and tab **ADECHIT** (group «ADECHIT»: «Rapoarte»). The VERCON «Rapoarte» button is NOT there yet (slice 0123-02): a visible button that does nothing was refused.
- **Help** 0000-119: new topic `contabil/adechit.md`, `ribbon.md`, `efactura/index.md`, tutorial `efactura-trimite-la-vercon.md` (the first step now points to the VERCON tab).

## Files to publish together (from `PYTHON`, through AvacontPush) - the operator does it
`reporting/snapshots.py`, `routes/adechit/__init__.py`, `routes/adechit/kbot_bridge.py` (new), `routes/adechit/report_routes.py`. Then restart the service. Optional hard guarantee (a cron on the server, one line): delete `instance/adechit-reports/snapshots/*/*.json` older than 10 minutes, e.g. `find /root/AVACONT/instance/adechit-reports/snapshots /root/AVACONT/instance/vercon-reports/snapshots -type f -mmin +10 -delete` (check the two folders' real paths on the server first; I did not read them there).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. `py_compile` of the four Python files: ok. `Check-Help.ps1`: see below. No test written or run, the application was not started, nothing was called on the server (not deployed).

## Unverified / deferred
- Nothing ran: not the page on screen (layout of the rows, DPI, theme), not the bearer bridge against the server, not the Adobe surface with a report.
- The bearer bridge assumes `Session.username` is the e-mail used in `Utilizatori_Roluri.UN` (so the user needs an AD role for the unit). If K-BOT user names are not e-mails the first call answers 403 «Nu aveti dreptul...» - then the mapping must be decided.
- Every ADECHIT call takes the unit's `AD_Lock` (existing behaviour of the guard), so the page makes a request per choice, not per key.
- «Salveaza Excel» exports ALL columns (the site lets the user pick columns); a column picker is a later addition if wanted.
- The Adobe surface (`UserChoosesSave = True`) was reused as on the received invoices; printing from it was not tried.
- VERCON reports (slice 0123-02) not started; the snapshot cleanup already covers VERCON's snapshots (same class) but VERCON has no release route yet.
- Help: no capture taken for the two new tabs / the new page (the ribbon captions `ribbon-file`, `ribbon-efactura` are marked for redo in the 0000-119 worklog).
