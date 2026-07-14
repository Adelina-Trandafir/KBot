# K-BOT session store — Redis backend (runbook)

The K-BOT login session store (`routes/auth/session_store.py`) has two backends with
one identical contract:

| Backend | Class | State lives in | Survives a process restart? | Multi-worker safe? |
|---|---|---|---|---|
| `memory` (default) | `SessionStore` | this Gunicorn process | no | no (single worker only) |
| `redis` | `RedisSessionStore` | shared Redis server | yes | yes (for sessions) |

Login (`/api/auth/login`) mints tokens into `STORE`, and the data-route guard
(`routes/auth/guard.py`) validates against the **same** `STORE`. (Historically login
wrote to a private dict while the guards read a different store — a real login token
was invisible to the data routes and every first authenticated call returned 401.
That is fixed: one `STORE`, either backend.)

The code ships with `SESSION_BACKEND` defaulting to **memory**, so a deploy can never
fail because Redis is unreachable. Switching to Redis is a deliberate, one-time config
change on the VPS, described below.

> Note: the Gunicorn single-worker guard in `gunicorn.conf.py` stays even on Redis,
> because `_upload_sessions` is **not** on Redis yet. Multi-worker requires migrating
> that too — a separate, planned step.

---

## 1. Find the Redis connection details on the VPS

Redis is already installed on the server (shared with another project). SSH in and:

```bash
# Is redis running, and on which port?
systemctl status redis-server 2>/dev/null || systemctl status redis 2>/dev/null
ss -ltnp | grep -i redis            # shows the bind address:port, usually 127.0.0.1:6379

# The config file (bind, port, requirepass):
grep -E '^\s*(bind|port|requirepass|unixsocket)\b' /etc/redis/redis.conf

# Confirm you can talk to it (add -a <password> if requirepass is set):
redis-cli ping                      # -> PONG
redis-cli -n 3 dbsize               # how many keys already in logical DB 3
```

What you need out of this:

- **host / port** — almost always `127.0.0.1` / `6379` for a local Redis.
- **password** — the value of `requirepass` if present; otherwise none.
- **a free logical DB number** — Redis has DBs `0..15`. Pick one the other project is
  **not** using (e.g. `3`) so K-BOT keys can't collide. Check candidates with
  `redis-cli -n <N> dbsize` (0 = empty = safe to take). K-BOT also namespaces every key
  as `kbot:sess:<token>`, so even sharing a DB is safe, but a dedicated DB is cleaner.

## 2. Install the Python client on the host

```bash
# In the same venv/interpreter Gunicorn uses for AVACONT:
pip install redis
```

(`fakeredis` is only needed for the test suite, not in production.)

## 3. Point K-BOT at Redis in `config.py`

`config.py` is host-only (not in the repo). Add, using the values from step 1:

```python
SESSION_BACKEND = "redis"

# Simplest: one URL. DB number is the trailing /3. Add a password as
# redis://:PASSWORD@127.0.0.1:6379/3 if requirepass is set.
REDIS_URL = "redis://127.0.0.1:6379/3"

# Optional — defaults to "kbot:sess:" if omitted.
# SESSION_KEY_PREFIX = "kbot:sess:"
```

Discrete keys are also supported instead of `REDIS_URL`:
`REDIS_HOST`, `REDIS_PORT`, `REDIS_DB`, `REDIS_PASSWORD`.

## 4. Restart and verify

```bash
systemctl restart avacont            # or however the AVACONT gunicorn unit is named
# The app pings Redis at startup. If Redis is unreachable, it refuses to start with a
# clear error (no silent fallback) — check the journal / error log:
journalctl -u avacont --since "2 min ago" | tail -30
```

Then exercise the real flow from the client (Continuă → Autentificare → MainForm loads).
On success you can watch sessions appear:

```bash
redis-cli -n 3 --scan --pattern 'kbot:sess:*'   # one key per live login
```

To roll back instantly: set `SESSION_BACKEND = "memory"` (or remove it) in `config.py`
and restart. No data migration is involved — sessions are ephemeral.

---

## Security notes

- **The operator's MariaDB password is never written to Redis.** Login uses the
  password only to prove identity, then discards it (`password=""`), and
  `RedisSessionStore` additionally refuses to serialize the password field.
- **Tokens are bearer credentials** — only the first 8 characters are ever logged.
- If `requirepass` is set on the shared Redis, keep the password only in host-only
  `config.py`, never in the repo.

## Expiry semantics (unchanged across backends)

- 20-minute sliding idle window; 30-minute absolute cap from issue time.
- Under Redis, idle/absolute expiry is enforced in-app from timestamps in the value,
  and the native key TTL is set to the absolute cap as a backstop reaper. Because Redis
  reaps idle-expired keys itself, an idle expiry may surface to the client as
  `TOKEN_UNKNOWN` rather than `EXPIRED_IDLE`; the operator message ("authenticate
  again") is the same either way.
