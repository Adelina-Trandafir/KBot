# SLICE 0081-06 — the director's K-BOT (G0b)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-06.

## Read first: how a user spans several units

`Contabil` does NOT span units inside one session. `AVACONT_COMUN.Unitati_Utilizatori` has one row per
(user, unit, role); `/api/auth/units` lists them, the login picks ONE, and the session (token) is bound
to that unit's database (`g.session.db_name`). Every data route reads the session's database. The
service account (`get_kbot_connection(dc)`) can open any unit database.

## What was built

- **Server** `PYTHON/routes/forexe/ddf_director.py`, `GET /api/forexe/ddf/director/de-semnat`:
  only for a session whose role is `Director` (else 403). Reads the director's units
  (`Unitati_Utilizatori.Rol = 'Director'`) and, in each database, the revisions that have a PDF on the
  server, are past the send (`StareTrimitere = 3`, or `Incarcat` / `Preluat` for those sent before 0081)
  and carry signatures A and B but not Ordonator. A unit that cannot be read is named in
  `unitati_necitite` and does not stop the list. A database without the 0081 column is still read
  (only `Incarcat` / `Preluat`).
- **`Program`**: after login, `Role = "Director"` → `DirectorForm` instead of `KbotForm`.
- **`DirectorForm`** (`KBotShellForm`, all controls in the Designer except the DDF view, which takes
  the API client in its constructor — same as the shell's views): «Aveți N documente de fundamentare de
  semnat», the list (unit, angajament, object, revision, date, total), and on the right the existing
  `DdfView` on the selected revision. Its signing session (0078) signs and uploads; the upload writes
  `A,B,Ordonator` → S4. The write commands of the DDF menu answer «here the document is only signed».
- **`LoginForm.PreferredDc`**: the unit pre-selected at phase 2 (wins over the remembered one).
- **`IDdfSendApi.GetDdfDeSemnatDirectorAsync`** now returns `DdfDeSemnatLista` (revisions + units not read).

## Assumptions (details, decided and written down)
1. **A document of another unit is opened after a login on that unit** (login window, unit pre-selected,
   password again); the old session is then logged out. No new authentication route — a «switch unit
   without password» route would be a new security surface the plan did not ask for.
2. **Waiting = A and B and no Ordonator**, compared as a comma list with spaces removed; a PDF must exist
   on the server (without one there is nothing to sign).
3. **The list is not refreshed by itself after a signature**: «Reîncarcă lista» does it (the signing
   session lives inside `DdfView`, which does not tell its host).
4. **The DDF view's own menu stays** (the director sees «Modifică», «Șterge»…); each answers with a
   refusal message instead of acting. Hiding them would need a read-only mode in `DdfView`.
5. The list is a plain `ListView` (a standard control, themed by the generic rules), not a new custom
   control.

## Files touched
- NEW `PYTHON/routes/forexe/ddf_director.py`; `PYTHON/routes/forexe/__init__.py` (import).
- NEW `src/KBot.App/Director/DirectorForm.vb` + `DirectorForm.Designer.vb`.
- `src/KBot.App/Program.vb` (role → window, DI), `src/KBot.App/LoginForm.vb` (`PreferredDc`).
- `src/KBot.Api/IDdfSendApi.vb`, `src/KBot.Api/ApiClient.DdfSend.vb` (`DdfDeSemnatLista`).
- NEW test `PYTHON/tests/test_forexe_ddf_director.py` (written, NOT run).

## State
Build App **0 / 0**; `py_compile` green. Nothing run, nothing seen on screen.
**Done when:** a login with a `Director` row sees only its waiting list and signs one document end to end.
The operator must add the `Director` rows to `AVACONT_COMUN.Unitati_Utilizatori` (one per unit).
