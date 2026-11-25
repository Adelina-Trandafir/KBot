# SLICE-ANAF-01 — canalul „fiscolino-scheme” pe server (pachetul de scheme al programului FISCOLINO)

Cererea operatorului, 18.11.2026. Familia ANAF = lucrul din repo-ul **ANAF** (programul FISCOLINO) care are o parte pe serverul K-BOT/AVACONT. Numerotare ANAF-01, ANAF-02, … **Următorul liber: ANAF-02.**

## Contextul

FISCOLINO descarcă un pachet de scheme (XSD, formulare `.xdp`, datele SAF-T D406: `saft-schema.json`, `saft-nomenclatoare.json`, `saft-reguli.json`) de pe server, din Opțiuni > Scheme XSD. Programul cheamă:

- `GET /api/update/fiscolino-scheme/latest` → `{ version, file, size, sha256, published_utc }`, 404 dacă nu e publicat nimic;
- `GET /api/update/fiscolino-scheme` → zip-ul numit acolo.

Pe server exista doar `latest.json` (K-BOT), `tray.json` și `fiscolino.json`; canalul pachetului de scheme lipsea (programul primea 404).

Pachetul îl construiește și îl urcă `push-scheme.ps1` din repo-ul ANAF (sftp, ca `push-fiscolino.ps1`): `fiscolino-scheme-<versiune>.zip`, apoi, ultimul, `fiscolino-scheme.json` (în același `updates/`). În pachet intră tot ce e în `%LOCALAPPDATA%\FISCOLINO\Schemas`, fără `406\doc` (documentele ANAF descărcate de mână, ~145 MB), `incluse`, `*.xlsx`, `*.md`, `*.local.json`; ~36 MB zip.

## Ce s-a schimbat și de ce

**`PYTHON/routes/update.py`:**
- constanta `FISCOLINO_SCHEME_NAME = "fiscolino-scheme.json"`;
- `read_fiscolino_scheme()` — ca `read_fiscolino()`, dar blocul nu are `minimum`; `LatestInvalid` dacă fișierul e ilizibil sau blocul nu e de încredere (versiune, nume simplu de fișier, mărime, sha256);
- `GET /api/update/fiscolino-scheme/latest` (`fiscolino_scheme_latest`) și `GET /api/update/fiscolino-scheme` (`fiscolino_scheme_download`: `send_file`, `Range`, antetele `X-KBot-Version` și `X-KBot-Sha256`);
- `_validate_block(data, where, required=_REQUIRED_KEYS)`: lista cheilor cerute devine parametru (implicit ca înainte), ca blocul de scheme să nu ceară `minimum`; ceilalți apelanți nu se schimbă;
- paragraf nou în docstring-ul modulului (canalul, forma blocului, rutele).

Canal propriu, ca `tray.json` și `fiscolino.json`: o publicare a pachetului de scheme nu atinge `latest.json`, `tray.json` sau `fiscolino.json`, și invers.

**`PYTHON/tests/test_update_routes.py`:** 10 teste noi (404; blocul fără `minimum`; 5 blocuri invalide → 500 `FISCOLINO_SCHEME_INVALID`; independența de celelalte canale; 404/500 la descărcare; pachetul trimis cu antete și `Range`).

## Fișiere atinse

`PYTHON/routes/update.py`, `PYTHON/tests/test_update_routes.py`, `docs/worklog/SLICE-ANAF-01-canal-scheme-fiscolino.md` (nou), `docs/worklog/state/KBOT_STATUS_ANAF.md` (nou), `docs/worklog/KBOT_STATUS.md` (rând în index). Înregistrate pentru Git pe `SLICE-ANAF-01` (18.11.2026): `PYTHON/routes/update.py` și `PYTHON/tests/test_update_routes.py` (trimis și acesta ca debugging, la cererea operatorului); documentele din `docs/` nu sunt pe server.

## Rezultate de verificare

- `PYTHON\.venv\Scripts\python.exe -m pytest tests/test_update_routes.py`: **35 trec** (25 existente + 10 noi). Nimic nu a fost rulat pe server.
- **Neverificat:** încărcarea reală a pachetului (`push-scheme.ps1`, rulat doar cu `-NoUpload`), ruta pe serverul real, descărcarea din FISCOLINO. De făcut de operator: publicarea lui `update.py` din KBOTADMIN (Publicare, cu repornirea serviciului), apoi `push-scheme.ps1` din repo-ul ANAF; după test, confirmarea către agent pentru înregistrarea Git (`tools\Register-DeploymentTask.ps1 -Slice SLICE-ANAF-01`).
