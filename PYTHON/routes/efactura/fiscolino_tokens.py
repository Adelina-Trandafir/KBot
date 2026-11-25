"""FISCOLINO's ANAF registration and token operations (SLICE-FS02-01)."""

import base64
import os
from datetime import timedelta
from . import cripto, fiscolino_store as store, oauth
from .ef_config import DEFAULT_REFRESH_DAYS, EfConfig
from .tokens import CODE_MAX, MAX_LABEL, EfEroare, TokenNecesar, TokenIndisponibil, _crypto, _crypto_token, _iso, _short

CALLBACK_URL = "https://kbot.avatarsoft.ro/api/fiscolino/efactura/callback"
AUTH_URL = "https://logincert.anaf.ro/anaf-oauth2/v1/authorize"
TOKEN_URL = "https://logincert.anaf.ro/anaf-oauth2/v1/token"
REFRESH_WINDOW = timedelta(days=3)


def config():
    names = ("EF_CLIENT_ID", "EF_CLIENT_SECRET", "EF_REDIRECT_URI", "EF_TOKEN_KEY")
    values = {name: os.environ.get("FISCOLINO_" + name, "").strip() for name in names}
    invalid = [name for name in names if not values[name] or values[name] == os.environ.get(name, "").strip()]
    if values["EF_REDIRECT_URI"] != CALLBACK_URL and "EF_REDIRECT_URI" not in invalid:
        invalid.append("EF_REDIRECT_URI")
    if "EF_TOKEN_KEY" not in invalid:
        try:
            key = values["EF_TOKEN_KEY"].encode("ascii")
            decoded = base64.urlsafe_b64decode(key)
            if len(decoded) != 32 or base64.urlsafe_b64encode(decoded) != key:
                invalid.append("EF_TOKEN_KEY")
        except (ValueError, UnicodeError):
            invalid.append("EF_TOKEN_KEY")
    if invalid:
        raise EfEroare(
            "Datele OAuth FISCOLINO lipsesc sau sunt invalide: " + ", ".join("FISCOLINO_" + name for name in invalid) + ".",
            "EF_NECONFIGURAT", 503,
        )
    return EfConfig(
        client_id=values["EF_CLIENT_ID"], client_secret=values["EF_CLIENT_SECRET"], redirect_uri=values["EF_REDIRECT_URI"],
        auth_url=AUTH_URL, token_url=TOKEN_URL, token_key=values["EF_TOKEN_KEY"], refresh_days=DEFAULT_REFRESH_DAYS,
    )


def start(conn, cur, username, cif, fingerprint):
    cfg = config()
    state = store.start(cur, username, cif, fingerprint)
    conn.commit()
    return {"state": state, "authorize_url": oauth.authorize_url(cfg),
            "expira_in_secunde": store.START_LIFETIME_MINUTES * 60}


def finish(conn, cur, username, cif, fingerprint, state, code, label):
    cfg = config()
    if not isinstance(code, str) or not code.strip() or len(code) > CODE_MAX:
        raise EfEroare("Codul de autorizare lipsește sau este invalid.", "COD_INVALID", 400)
    _crypto(lambda: cripto._fernet(cfg.token_key))
    if not store.spend_start(cur, state, username, cif, fingerprint):
        raise EfEroare("Cererea de autorizare a expirat sau a fost utilizată. Reluați pasul cu certificatul.", "STATE_INVALID", 400)
    conn.commit()

    store.lock_unit(cur, cif)
    try:
        issued = oauth.exchange_code(cfg, code.strip(), basic_auth=True)
    except oauth.OauthError as err:
        store.save_error(cur, cif, str(err))
        conn.commit()
        raise EfEroare(str(err), "ANAF_REFUZ" if err.definitive else "ANAF_INDISPONIBIL", 502) from err

    now = store.utc_now()
    access = _crypto(lambda: cripto.encrypt(issued.access_token, cfg.token_key))
    refresh = _crypto(lambda: cripto.encrypt(issued.refresh_token, cfg.token_key))
    store.save_authorised(cur, cif, username, fingerprint, _short(label, MAX_LABEL), access, refresh,
                          now + timedelta(seconds=issued.expires_in), now + timedelta(days=cfg.refresh_days))
    conn.commit()
    return _describe(cif, store.token_row(cur, cif), configured=True)


def _for_cif(row, cif):
    return row if row is not None and row["CUI"] == cif else None


def _can_refresh(row, now):
    return bool(row and row["RefreshTokenCrypt"] and row["RefreshExpiraLa"] and row["RefreshExpiraLa"] > now)


def _is_access_valid(row, now):
    return bool(row and row["AccessTokenCrypt"] and row["AccessExpiraLa"] and row["AccessExpiraLa"] > now)


def _refresh_due(row, now):
    return not _is_access_valid(row, now) or row["AccessExpiraLa"] - now <= REFRESH_WINDOW


def refresh(conn, cur, cif, cfg, *, force):
    store.lock_unit(cur, cif)
    row = _for_cif(store.token_row(cur, cif, for_update=True), cif)
    now = store.utc_now()
    if not _can_refresh(row, now):
        raise TokenNecesar("Firma are nevoie de o nouă autorizare ANAF cu certificatul.")
    if not force and not _refresh_due(row, now):
        return _describe(cif, row, configured=True)

    try:
        saved = _crypto_token(lambda: cripto.decrypt(row["RefreshTokenCrypt"], cfg.token_key))
    except TokenNecesar as err:
        store.save_error(cur, cif, str(err), needs_authorization=True)
        conn.commit()
        raise

    try:
        issued = oauth.refresh(cfg, saved, basic_auth=True)
    except oauth.OauthError as err:
        store.save_error(cur, cif, str(err), needs_authorization=err.definitive)
        conn.commit()
        if err.definitive:
            raise TokenNecesar("ANAF cere o nouă autorizare cu certificatul.") from err
        raise TokenIndisponibil(str(err)) from err

    access = _crypto(lambda: cripto.encrypt(issued.access_token, cfg.token_key))
    next_refresh = _crypto(lambda: cripto.encrypt(issued.refresh_token, cfg.token_key)) if issued.refresh_token else row["RefreshTokenCrypt"]
    store.save_refreshed(cur, cif, access, next_refresh, store.utc_now() + timedelta(seconds=issued.expires_in))
    conn.commit()
    return _describe(cif, store.token_row(cur, cif), configured=True)


def status(conn, cur, cif):
    row = _for_cif(store.token_row(cur, cif), cif)
    try:
        cfg = config()
    except EfEroare:
        return _describe(cif, row, configured=False)

    now = store.utc_now()
    if _can_refresh(row, now) and _refresh_due(row, now):
        try:
            return refresh(conn, cur, cif, cfg, force=False)
        except (TokenNecesar, TokenIndisponibil) as err:
            # Preserve any usable access token and return the updated state.
            conn.rollback()
            answer = _describe(cif, store.token_row(cur, cif), configured=True)
            answer["ultima_eroare"] = str(err)
            return answer
    return _describe(cif, row, configured=True)


def _describe(cif, row, configured):
    row = _for_cif(row, cif)
    now = store.utc_now()
    can_refresh = _can_refresh(row, now)
    fields = row or {}
    refresh_end = fields.get("RefreshExpiraLa")
    return {
        "cui": cif,
        "configurat": configured,
        "exista": row is not None,
        "trebuie_reinnoit": not can_refresh,
        "acces_valid": _is_access_valid(row, now),
        "avertizeaza": can_refresh and refresh_end - now <= REFRESH_WINDOW,
        "certificat": fields.get("CertEticheta"),
        "autorizat_la": _iso(fields.get("DataAutorizare")),
        "expira_acces_la": _iso(fields.get("AccessExpiraLa")),
        "valabil_pana": _iso(refresh_end),
        "ultimul_refresh_la": _iso(fields.get("DataReinnoire")),
        "ultima_eroare": fields.get("UltimaEroare"),
    }
