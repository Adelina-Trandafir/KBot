# SLICE-0121-03 - «Parteneri» window + `parteneri_edit.py` on the new link tables (19.10.2026)

Plan: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md). Needs 0121-01 run and synced.

## What changed and why
The window's «Coduri angajament» no longer read or write `Parteneri_Coduri`; they belong to the supplier's normalized CodFiscal (`Parteneri_Angajamente` / `Parteneri_Angajamente_Indicatori`).

Server:
- New `PYTHON/routes/forexe/partner_links.py`: `normalize_cui` (a copy of the DDF rule, on purpose - importing the efactura package would pull its blueprint), `read_links` (one query: link LEFT JOIN indicators LEFT JOIN `FX_Indicatori` LEFT JOIN `Clasificatii`, so the classification is only shown, by CodAI) and `save_codes`.
- `save_codes` in the caller's transaction: removes the deleted indicators / empty links (checked against the CodFiscal), validates every row (`Cod ang.` required, no repeated `CodAng`+`CodInd`), removes the old identity of rows that change it BEFORE writing (two rows may swap codes), then upserts link and indicator (`CodAI = CodAng-CodInd`; the window is the authority, so it overwrites `CodInd`/`ContBancar`). A link left with no indicator is deleted unless a row of the grid still names it. An id of another supplier is refused (rows) or ignored (deletes).
- `parteneri_edit.py`: reads/saves through it; wire adds `id_leg` per row and `legaturi_sterse`; `id_clsf` may be null; the old `Parteneri_Coduri` SQL and the refresh of its `CodPartener` are gone; the 1062 message names the new key. A save with codes for a partner without a CodFiscal is refused. Deleting a partner leaves the links (they belong to the CodFiscal).

Client:
- Domain `PartenerCod` (`IdLeg`, nullable `IdClsf`), `PartenerSaveRequest.LegaturiSterse`; `ApiClient.Nomenclatoare` wire.
- `ParteneriForm`: «Clasificație» is read-only text (no combo, `ClsfChoice` removed); hidden column `id_leg`; `ReadCodes` requires «Cod ang.» and refuses a repeated pair; a deleted row goes to `CoduriSterse` (indicator) or `LegaturiSterse` (angajament without indicator); the grid is disabled with a hint in the title while no fiscal code is typed, or while a saved partner's code was changed and not saved (`CodesAllowed` / `UpdateCodesState`); the delete-partner question says the codes stay.
- Help 0000-113 (separate worklog).

## Files touched
`PYTHON/routes/forexe/partner_links.py` (new), `PYTHON/routes/forexe/parteneri_edit.py`, `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`, `src/KBot.App/Views/Nomenclatoare/ParteneriForm.vb` + `.Designer.vb`, help files (see 0000-113), FileVersion: Domain 1.2.17.0, Api 1.0.25.0, App 1.1.2.5.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Both Python files pass `py_compile`. **No test written or run, the app not started, no database touched** (operator: no tests). 

## Left unverified or deferred
- Never run against a database: the SQL of `partner_links.py`, the transaction behaviour and the grid (read-only column, hidden column, disabled state) are unverified. The Designer was edited by hand, never opened in Visual Studio.
- The server still sends the `clasificatii` list (the window no longer uses it): left so the wire does not change more than needed.
- A partner whose CodFiscal is edited keeps no link: the old links stay on the old CUI (by design: they belong to the supplier, not the partner row).
- Old rows of `Parteneri_Coduri` are read by nobody now; they reach the new tables only through the Migrator (0121-02). Until that runs, the window shows empty codes.
- Server files go to the VPS by the operator's hand (never by Claude).

## Corrective 20.10.2026 - compatibility with the deployed client (App 1.1.2.4)
Operator: the version in use is 1.1.2.4 (before 0121); it must keep working against the new server files. Read from the code: that client reads each code's `id_clsf` into a plain `Integer`, so the `null` the new server sent (link with no indicator, or a CodAI unknown to `FX_Indicatori`) would make the whole partner list fail to load. `partner_links.read_links` now sends `id_clsf: 0` (= unknown); the new client does not use the value. Remaining difference for 1.1.2.4: a code row with no «Cod ang.» is refused with a message (the link is keyed by CodAng). Not run against a database; `py_compile` OK.
