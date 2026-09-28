# K-BOT — STATUS, slices 0000–0009

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0001

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0001 | Auth — bearer tokens, session store (Felia 1) | DONE | (pre-worklog-rule) | Static API key eliminated for K-BOT; legacy FOREXE still uses X-Api-Key |

---

## Slice 0002

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0002 | Split-brain 401 fix + reason codes | DONE | (pre-worklog-rule) | login mints via STORE; every 401 carries a reason code |

---

## Slice 0003

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0003 | Redis session backend | DONE (code) / config PENDING on VPS | — | `SESSION_BACKEND="redis"`, `SESSION_KEY_PREFIX`, `REDIS_DB=2` must be set in host config.py; verify DB 2 is free |

---

## Slice 0004

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0004 | Tier 1 hardening | IN PROGRESS | — | rate limiter DONE+pushed; remaining: ApiOptions address, retire AppConfig, verify gunicorn guard. Plan: `KBOT_Tier1_Plan.md` |

---

## Slice 0005

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0005 | Phase A cleanup (lying tests, https guard) | DONE | — | Python 75 passed / 7 skipped, 0 fail/error; .NET 80 green |

---

## Slice 0006

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0006 | MainForm scaffolding | DONE | (pre-worklog-rule) | Plan: `PLAN_MainForm_Scaffolding.md`. Built against its own 11-item checklist; `WithReauth(Of T)` + ListaAngajamente vertical preserved. Real DI signature is 5 params (`forexeRunner, session, apiClient, authApi, loginFactory`), not the 4 the plan expected |

---

## Slice 0007

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0007 | AngajamentTreeInfo POCO correction | SUPERSEDED by 0008 | `SLICE-0008-tree-data-api.md` | Was done WRONG: built against `qFX_MAIN_TREE` alone, so `Salarii` was dropped and `IDORD` kept. 0008 rewrote the POCO against the real contract (row-source `_DESCRIERE` + flags `qFX_MAIN_TREE`): `Salarii` restored, `IDORD` dropped |

---

## Slice 0008

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0008 | Tree data API + `MainForm.LoadTree` | DONE (code) / UNVERIFIED on a live DB | `SLICE-0008-tree-data-api.md`, `SLICE-0009-maintree-loadtree.md` (Part A amendment) | Plan: `PLAN_TreeDataApi.md`. `GET /api/forexe/tree` (an/ss/include_hidden, base from session), nine `EXISTS` flags, POCO rewrite, tree load + nav gating. **Amended by 0009:** the SS filter now has an orphan escape (`EXISTS SS OR NOT EXISTS any indicators`) so zero-indicator angajamente stay visible. **No part of it has touched a real database** — all route tests are host-only and skip off-host |

### Current focus

- **Now:** run Slices 0008 + 0009 on the host. The endpoint (incl. 0009's orphan escape)
  and the client are written and green offline, but nothing has hit a real database:
  `PYTHON/tests/test_forexe_tree.py` skips off-host and is the fastest way to answer
  verification items 1–5 and 8, plus the two new orphan tests (`TREEO`/`TREEX`).

### Open threads

- ~~`KBotNavList` has no `SetItemVisible` — 0008 gates views with `SetItemEnabled`
  (grey-out) instead of hiding.~~ RESOLVED in slice 0018: `SetItemVisible` added and
  `ApplyViewGating` now hides (not greys) a view whose `Are*` flag is FALSE.

---

## Slice 0009

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0009 | `MainForm.LoadTree` (client half) + tree orphan escape | DONE (code) / UNVERIFIED on a live DB | `SLICE-0009-maintree-loadtree.md` | The brief's Parts B/C/D (DTOs, `GetTreeAsync`, `LoadTreeAsync` + gating) were **already shipped by 0008**; the real deltas are Part A (orphan escape on the server, see 0008 row) + its 2 host-only tests, and the 4 `GetTreeAsync` client tests 0008 never added (Api 26 → 30). Kept 0008's choices: mapping in the client (no `BuildTreeInfo`), token from session (no param), `IDDF As Long?` throughout. LoadTree is period-driven (runs on load + every An/SS change = the `SetPeriod` precondition) |
