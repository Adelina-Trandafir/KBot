"""
Teste LIVE pentru login-ul aplicatiei K-BOT, lovind API-ul care ruleaza.
Se skip-uiesc cand variabilele de mediu lipsesc, deci CI fara DB ramane verde.

Preconditii: DDL-ul aplicat (sql/avacont_comun_login.sql), un rand CAI cu
DbName='000_DEMO' si un cont operator de test (ideal '000_DEMO_Contabil' ca
asertiunea de rol sa fie relevanta) cu grant pe 000_DEMO.

Env: KBOT_API_BASE_URL, KBOT_API_KEY, TEST_OP_USER, TEST_OP_PASS,
     optional TEST_OP_DENIED_UNIT (un IdUnitate pentru care operatorul nu are grant).
"""
import os

import pytest
import requests

BASE = os.getenv("KBOT_API_BASE_URL")
KEY = os.getenv("KBOT_API_KEY")
USER = os.getenv("TEST_OP_USER")
PWD = os.getenv("TEST_OP_PASS")

pytestmark = pytest.mark.skipif(
    not all([BASE, KEY, USER, PWD]),
    reason="Auth live env vars not set",
)

H = {"X-Api-Key": KEY}


def _post(path, body):
    return requests.post(f"{BASE}{path}", json=body, headers=H, timeout=30)


def test_units_bad_credentials_returns_401():
    r = _post("/api/auth/units", {"username": USER, "password": PWD + "_wrong"})
    assert r.status_code == 401


def test_units_good_credentials_lists_demo():
    r = _post("/api/auth/units", {"username": USER, "password": PWD})
    assert r.status_code == 200
    dbs = [u["DbName"] for u in r.json()["units"]]
    assert "000_DEMO" in dbs


def _demo_id_unitate():
    r = _post("/api/auth/units", {"username": USER, "password": PWD})
    return next(u["IdUnitate"] for u in r.json()["units"] if u["DbName"] == "000_DEMO")


def test_login_returns_session_context_and_role():
    idu = _demo_id_unitate()
    r = _post("/api/auth/login",
              {"username": USER, "password": PWD, "IdUnitate": idu, "pcname": "PYTEST"})
    assert r.status_code == 200
    body = r.json()
    assert body["session_id"] > 0
    ctx = body["SessionContext"]
    assert ctx["DbName"] == "000_DEMO"
    # rolul e derivat din sufixul username-ului (store-only)
    if USER.endswith("_Contabil"):
        assert ctx["Role"] == "Contabil"
    elif USER.endswith("_Administrator"):
        assert ctx["Role"] == "Administrator"


def test_logout_stamps_once():
    idu = _demo_id_unitate()
    login = _post("/api/auth/login",
                  {"username": USER, "password": PWD, "IdUnitate": idu, "pcname": "PYTEST"})
    sid = login.json()["session_id"]
    first = _post("/api/auth/logout", {"session_id": sid})
    assert first.status_code == 200 and first.json()["stamped"] == 1
    second = _post("/api/auth/logout", {"session_id": sid})
    assert second.json()["stamped"] == 0  # deja stampilat


@pytest.mark.skipif(not os.getenv("TEST_OP_DENIED_UNIT"),
                    reason="No denied-unit configured")
def test_login_rejects_unit_without_grant():
    denied = int(os.getenv("TEST_OP_DENIED_UNIT"))
    r = _post("/api/auth/login",
              {"username": USER, "password": PWD, "IdUnitate": denied, "pcname": "PYTEST"})
    assert r.status_code == 403
