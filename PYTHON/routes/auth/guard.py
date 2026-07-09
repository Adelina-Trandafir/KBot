# routes/auth/guard.py
"""
Decoratorul bearer pentru rutele autentificate ale aplicatiei K-BOT.

Cererea trebuie sa poarte `Authorization: Bearer <token opac>`. Un token absent,
necunoscut sau expirat -> 401 cu mesaj romanesc; clientul re-arata LoginForm.
Handler-ele din aval citesc g.session (NICIODATA claim-uri venite de la client).
SECURITY (R1): token-ul e credential bearer — nu se logheaza niciodata valoarea.
"""
from functools import wraps

from flask import request, jsonify, g

from routes.auth.session_store import STORE


def require_session(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        auth = request.headers.get("Authorization", "")
        token = auth[7:] if auth.startswith("Bearer ") else ""
        s = STORE.validate_and_touch(token)
        if s is None:
            return jsonify(
                error="Sesiune expirată sau invalidă. Autentificați-vă din nou."), 401
        g.session = s
        g.session_token = token
        return fn(*args, **kwargs)
    return wrapper


def require_session_or_api_key(fn):
    """
    Guard de TRANZITIE pentru rutele partajate intre K-BOT (bearer) si FOREXE
    legacy (X-Api-Key), ex. /api/tools/process_excel.

    Regula: daca cererea poarta un header Authorization, e judecata EXCLUSIV ca
    bearer (un token expirat primeste mesajul de re-login, nu cade pe cheia
    legacy). Fara Authorization, se accepta X-Api-Key-ul flotei vechi. Cand
    ultimul FOREXE legacy dispare, rutele trec pe require_session si acest
    decorator se sterge.

    Pe calea legacy g.session NU exista — handler-ele partajate nu trebuie sa
    depinda de el.
    """
    @wraps(fn)
    def wrapper(*args, **kwargs):
        auth = request.headers.get("Authorization", "")
        if auth:
            token = auth[7:] if auth.startswith("Bearer ") else ""
            s = STORE.validate_and_touch(token)
            if s is None:
                return jsonify(
                    error="Sesiune expirată sau invalidă. Autentificați-vă din nou."), 401
            g.session = s
            g.session_token = token
            return fn(*args, **kwargs)

        # Cale legacy (FOREXE vechi). Import lazy: guard-ul ramane importabil
        # fara config.py pe o masina de dev (vezi test_session_store).
        from config import API_KEY
        if request.headers.get("X-Api-Key") == API_KEY:
            return fn(*args, **kwargs)
        return jsonify(error="Unauthorized"), 401
    return wrapper
