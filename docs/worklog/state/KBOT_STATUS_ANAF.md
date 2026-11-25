# K-BOT — STATUS, slice family ANAF (serverul pentru programul FISCOLINO din repo-ul ANAF)

Cererea operatorului 18.11.2026. ANAF este o familie în K-BOT, ca TRAY, KE și 00EF: partea de pe serverul AVACONT a lucrului din repo-ul **ANAF** (programul FISCOLINO). Numerotare ANAF-01, ANAF-02, … **Next free: ANAF-02.** Deployment: `SLICE-ANAF-01` (formatul cerut de `tools\Register-DeploymentTask.ps1`).

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| ANAF-01 | **Canalul `fiscolino-scheme` pe server** (18.11.2026): `GET /api/update/fiscolino-scheme/latest` și `GET /api/update/fiscolino-scheme` în `update.py`, din `fiscolino-scheme.json` (fără `minimum`), canal propriu, separat de `latest.json`, `tray.json` și `fiscolino.json`; pachetul îl urcă `push-scheme.ps1` din repo-ul ANAF | GATA pe cod; 35 de teste trec (10 noi); **nepublicat pe server, nerulat pe server, încărcarea reală a pachetului neîncercată**; înregistrarea pentru Git: după testul și confirmarea Adelinei | `SLICE-ANAF-01-canal-scheme-fiscolino.md` | Server: `PYTHON/routes/update.py` (de publicat din KBOTADMIN, cu repornirea serviciului). Client: `Services/SchemaPackageService.vb` din repo-ul ANAF. |

## Current focus

- **ANAF-01 (18.11.2026).** `update.py`: `read_fiscolino_scheme`, două rute, `_validate_block(required=…)`; testele în `tests/test_update_routes.py`. Pachetul: `push-scheme.ps1` (repo ANAF) → `<RemoteRoot>/updates/fiscolino-scheme-<versiune>.zip` + `fiscolino-scheme.json`.

## Open threads

- **Neprobat pe server:** publicarea lui `update.py` (KBOTADMIN > Publicare, cu repornirea), apoi `push-scheme.ps1` (încărcarea reală și verificarea prin API) și descărcarea din FISCOLINO (Opțiuni > Scheme XSD).
- **Git pe server:** după testul Adelinei, înregistrate 18.11.2026 `PYTHON/routes/update.py` și `PYTHON/tests/test_update_routes.py` (`Register-DeploymentTask.ps1 -Slice SLICE-ANAF-01`, din repo-ul KBOT); urmează «Finalizează debugging» / publicarea cu Git din KBOTADMIN. Dacă unul din fișiere se mai editează, se înregistrează din nou.
- `update.py` are în arborele de lucru și modificări mai vechi (canalul `fiscolino`, FS01-01), tot necomise: verifică la «Trimite» că fișierul intră cu slice-urile potrivite.
