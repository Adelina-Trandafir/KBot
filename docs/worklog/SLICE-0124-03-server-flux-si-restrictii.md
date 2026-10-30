# SLICE-0124-03 — Server: the flow's routes, `DeSemnat` moving on, and the restricted roles

**Date:** 30.10.2026. Plan: [SLICE-0124-01](SLICE-0124-01-flux-ddf-kbot-plan-si-banca.md). SQL: [0124-02](SLICE-0124-02-sql-desemnat-comp-rol.md).
The operator confirmed the two defaults («confirm și fă tot într-o bucată») and asked for the whole flow in one piece.

## What changed and why

- **`PYTHON/routes/forexe/ddf_flux_core.py` (new, no routes).** The rules, imported by the guard, by `pdf.py` and by the routes:
  roles (`Administrator`, `Secretar`, `Imputernicit`, `Contabil`, `Director`); `roluri_vizibile` (a proxy also sees what is the accountant's);
  `urmatorul` / `avanseaza` (A → `Contabil`, B → `Director`, Ordonator → NULL, run in the PDF upload's own transaction); the probes for the column and
  the table (a unit where sql/0124 was not carried is left alone); the allow-list of the restricted roles; `refuza_pdf_nevizibil`.
- **Flow start (operator's step 1):** `ddf_edit.py` `_scrie_graf` — a revision created in K-BOT (not from FOREXE) gets `DeSemnat` from the compartment **when it is created**; a compartment with no role stops the save with a message naming it. The route that started the flow on «Generează» was removed.
- **`PYTHON/routes/forexe/ddf_flux.py` (new).** `GET /api/forexe/ddf/de-semnat` (every role; spans the user's units, each read with the role the user
  has there; a unit that cannot be read is named in `unitati_necitite`), `GET` / `PUT /api/forexe/ddf/flux/comp-rol` (the compartments in use + the table + the roles; the PUT replaces the table, all or nothing).
- **CRITICAL rule (operator): a `Secretar` / `Administrator` must not see any DDF other than the ones addressed to them — and everything else works for them as it did before** (corrected the same day: the first version closed every route
  of the server to them except the signing ones, which the operator did not ask for). `guard.py` calls `refuza_restrans` for those roles: inside the DDF area (`/api/forexe/ddf*`) a route that names a revision (`pdf`, `draft`,
  `rev`, `trimitere`, `att`) is allowed only for a revision whose `DeSemnat` is the role's (else 404 / 403); a PDF upload by them must carry exactly the signature `A`; a DDF route nobody listed (save, delete, numbers…) stays closed;
  the angajament dump (it dumps the DDF tables) is closed; the lookup lists of the DDF editor stay open; **every route outside the DDF area is untouched**. `ddf.py` (`GET /api/forexe/ddf`) answers a restricted role with the addressed revisions only
  (`filtreaza_ddf`).
- **`pdf.py`**: in `_incarca`, after the `Semnatura` update, `avanseaza` moves `DeSemnat` (same transaction). The access check is the guard's.
- **`ddf.py`**: the revision list carries `de_semnat` (NULL where the column does not exist). **`__init__.py`**: registers `ddf_flux`.

## Differences from the plan of 0124-01

- The signer (also a Secretar / Administrator) **generates the document locally** from the revision's data (as the accountant does) and uploads it signed; **nothing is stored on the server before the signature** (the first version stored an unsigned
  A-only PDF; the operator rejected it). So the list of what is addressed (`ddf_flux.py`) does not need a PDF row (`LEFT JOIN FX_DDF_PDF`), and a restricted role reads the DDF through the filtered `GET /api/forexe/ddf`.

## Files touched

`PYTHON/routes/forexe/ddf_flux_core.py` (new), `ddf_flux.py` (new), `pdf.py`, `ddf.py`, `__init__.py`; `PYTHON/routes/auth/guard.py`.

## Test results

`py_compile` of the six files: OK. `import routes.forexe` with the venv: OK; `refuza_restrans` checked by hand for a Secretar without a database (the DDF read, the list and the lookup lists pass; the dump, `ddf/save`, `ddf/numar/rezerva` are refused; `/api/forexe/tree` and `/api/efactura/x` are not touched; a Contabil is never refused); `urmatorul`: A → Contabil, A,B → Director, A,B,Ordonator → none). **Nothing ran against a database or a running
server; the Python files are published by the operator, not by me.**

## Left unverified / deferred

- The restriction covers the DDF area and the angajament dump only. Other places that might show DDF content (sumar / istoric / tree show angajament-level figures and flags, not documents) were left as they were; to be reviewed if the operator wants them closed too.
- The director's narrowing is still only in the window (DIR1 «restanțe»); the same mechanism could close it on the server if asked.
- sql/0124 must have reached each unit database (the sync); a unit without it saves a new revision the old way (outside the flow) and shows no list.
- Not tried: two different signers on two PCs, 409 on a concurrent upload, the real upload of an unsigned document through `_incarca`.
