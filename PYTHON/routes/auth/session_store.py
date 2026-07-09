# routes/auth/session_store.py
"""
Registrul in-memory token -> sesiune (login-ul aplicatiei K-BOT).

SAFE ONLY sub gunicorn single-process (1 worker / 4 threads) — aceeasi
constrangere care fixeaza _upload_sessions. Cand AVACONT trece pe multi-worker,
ACEST registru migreaza pe Redis impreuna cu _upload_sessions, in acelasi pas.

SECURITY (R2): Session.password este cel mai sensibil camp din sistem — parola
MariaDB a operatorului, tinuta DOAR in memoria procesului pe durata sesiunii.
Nu se logheaza, nu se serializeaza, nu se scrie pe disc; e evacuata la logout
si la expirare. Token-ul insusi e credential bearer — nu se logheaza niciodata.
"""
import secrets
import threading
import time
from dataclasses import dataclass

_TTL_SECONDS = 20 * 60        # fereastra glisanta de inactivitate
_ABS_TTL_SECONDS = 30 * 60    # plafon absolut de la emitere (R6) — re-auth fortat
_TOKEN_NBYTES = 32            # token opac de 256 biti (CSPRNG)


@dataclass
class Session:
    username: str
    password: str          # parola MariaDB a operatorului (DOAR in memorie)
    id_unitate: int
    db_name: str           # baza unitatii rezolvata, ex. "000_DEMO"
    ctx: dict              # payload-ul SessionContext intors clientului
    issued_at: float
    expires_at: float
    pcname: str = ""


class SessionStore:
    def __init__(self):
        self._lock = threading.Lock()          # 4 thread-uri impart procesul
        self._by_token: dict[str, Session] = {}

    def create(self, username, password, id_unitate, db_name, ctx, pcname):
        token = secrets.token_urlsafe(_TOKEN_NBYTES)
        now = time.time()
        s = Session(username=username, password=password,
                    id_unitate=id_unitate, db_name=db_name, ctx=ctx,
                    issued_at=now,
                    expires_at=min(now + _TTL_SECONDS, now + _ABS_TTL_SECONDS),
                    pcname=pcname)
        with self._lock:
            self._by_token[token] = s
        return token, s

    def validate_and_touch(self, token):
        """Intoarce sesiunea vie si ii gliseaza expirarea, sau None daca token-ul
        lipseste / a expirat (inactivitate sau plafon absolut)."""
        if not token:
            return None
        now = time.time()
        with self._lock:
            s = self._by_token.get(token)
            if s is None:
                return None
            if s.expires_at <= now or now >= s.issued_at + _ABS_TTL_SECONDS:
                del self._by_token[token]          # maturare lenesa la acces
                return None
            # Fereastra glisanta, dar niciodata dincolo de plafonul absolut.
            s.expires_at = min(now + _TTL_SECONDS, s.issued_at + _ABS_TTL_SECONDS)
            return s

    def revoke(self, token):
        with self._lock:
            self._by_token.pop(token, None)

    def sweep(self):
        """Curatare periodica optionala a sesiunilor expirate."""
        now = time.time()
        with self._lock:
            dead = [t for t, s in self._by_token.items()
                    if s.expires_at <= now or now >= s.issued_at + _ABS_TTL_SECONDS]
            for t in dead:
                del self._by_token[t]


STORE = SessionStore()   # singleton la nivel de modul, ca _upload_sessions
