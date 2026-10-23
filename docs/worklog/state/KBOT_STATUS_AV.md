# K-BOT — STATUS, slice family AV (AVACONT, the Access accounting system, ported into K-BOT)

Plan: `C:\Users\Adelina Trandafir\.claude\plans\c-users-adelina-trandafir-source-repos-compiled-piglet.md` (7 stages) and
[`docs/access-map/avacont/README.md`](../../access-map/avacont/README.md) (reading sheets, decisions). Numbering AVnn-nn: AV01 = stage 1
(configuration with templates), AV02 = stage 2 (data entry), AV03 = salaries, AV04 = scholarships and statements, AV05 = off-balance,
AV06 = reports, AV07 = other. The main application BECOMES Avacont (no «AVACONT» section in the ribbon: buttons go into the existing tabs).
Operator request 23.10.2026. **Everything below is documents only: no code, no DDL run, nothing on the server, nothing on screen, no test.**

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| AV01-01 | Reading sheets for stage 1 (Conturi, Jurnale, Clasificatii Venituri, Sablon OCI, Sablon OCS, Setari + bife, Parteneri ca analitic) | GATA ca documente | `SLICE-AV01-01-fise-de-citire-etapa-1.md` | `docs/access-map/avacont/*.md`. The sheets of Clasificatii Venituri, Sabloane and Setari were written by sub-agents and only partly re-checked. Data read from a COPY of `baza2026.accdb` (read-only). |
| AV01-02 | Data design for stage 1 (tables, keys, subunit key, roles, migration) | GATA ca document, **PROPUNERE, neconfirmata** | `SLICE-AV01-02-proiect-de-date-etapa-1.md` | `docs/access-map/avacont/proiect-de-date-etapa-1.md`. |

## Locked decisions (operator, 23.10.2026)

- Accounting data: tables prefixed `TZ_` in the unit's database, `An` column only where data depends on the year. Only the current year + opening balances are migrated.
- Ported from Access ribbon stage 1: Conturi, Jurnale, Clasificatii Venituri, Sablon OCI, Sablon OCS. Setari -> page «Avacont» under Setari > Aplicatie, per user.
- NOT ported: add/load/transfer unit, online backup + download update, checkboxes «Stergere conturi» and «Mod Noapte», `Parteneri.Tip`, `ContPl`, TREZO import, Contracte 8067, Ajustari 8067, flags Ord/Ang/FX and numbers NrAng/NrOrd/NrProp (ALOP rules, manual `Surse/GHID UTILIZARE_ALOP_V2.pdf`).
- Deleting an account or a journal that has operations is BLOCKED. A salary-template article is never saved without at least one PC (VIRAMENT) row.
- Subunit = another subordinate unit with its own accounting; it decides the chapter (65.01=01A, 65.02=02A, 65.10=02E, 65.11=01A). `Zec2` becomes a subunit setting.
- Templates keep the base account only (rest composed at use); partner on a line: none / the operation's / fixed. `Chelt` stays: «effective expense», shown in a column of some reports.
- Chart of accounts per subunit; `TZ_Subunitati` separate from `VC_Subunitati` and unlinked.
- One posting service on the Flask server; the WinForms views only call the API.

## Open threads

- **Confirmed 23.10.2026:** the chart of accounts is per subunit; `TZ_Subunitati` is separate from `VC_Subunitati`, with NO link between them.
- **Not confirmed:** the rest of the data design (`proiect-de-date-etapa-1.md`), `Tip = M`, `DefaCont`/`DefaClase` in `AVACONT_COMUN`.
- **Not read:** `ParteneriForm` and slices 0121-0123 (do they require a fiscal code?).
- **Stage 2 needs** the ALOP guide read first, then sheets for Note, OP9, Evidentieri/Plati, the «Contari» view of E-Factura, MF2019, ModificareNC1280, MODNRNC.
- No help topic yet (`0000-NN`): nothing visible to the operator changed.
