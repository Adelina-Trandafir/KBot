# Server-side unit test for POST /api/forexe/angajamente/upsert.
# Run on the Flask host, from the PYTHON folder:  python -m pytest tests/test_forexe_angajamente.py
#
# Preconditions (see handoff §9):
#   1) FX_Angajamente exists on the 000_DEMO MariaDB (gates the insert/update test);
#   2) config.py is present on the host (utils.database needs it);
#   3) the direct-verification step hits get_db_connection("000_DEMO") and always
#      cleans up its T1 row.
#
# Auth: the routes are guarded by @require_session (bearer token). The tests mint
# a session straight in the in-process STORE — the same object the guard reads via
# app.test_client() — so no operator credentials are needed here.
import json

import pytest

from main import app
from routes.auth.session_store import STORE
from utils.database import get_db_connection

DB_NAME = "000_DEMO"
URL = "/api/forexe/angajamente/upsert"
GET_URL = "/api/forexe/angajamente"


@pytest.fixture
def client():
    app.config["TESTING"] = True
    with app.test_client() as c:
        yield c


@pytest.fixture
def auth_headers():
    """Sesiune valida in STORE; token revocat la finalul testului."""
    token, _ = STORE.create(username="pytest-op", password="unused",
                            id_unitate=0, db_name=DB_NAME,
                            ctx={"DbName": DB_NAME}, pcname="PYTEST")
    yield {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    STORE.revoke(token)


def test_missing_token_is_rejected(client):
    r = client.post(URL, headers={"Content-Type": "application/json"},
                    data=json.dumps({"db_name": DB_NAME, "rows": []}))
    assert r.status_code == 401


def test_missing_rows_returns_400(client, auth_headers):
    r = client.post(URL, headers=auth_headers, data=json.dumps({"db_name": DB_NAME}))
    assert r.status_code == 400


def test_rows_not_list_returns_400(client, auth_headers):
    r = client.post(URL, headers=auth_headers,
                    data=json.dumps({"db_name": DB_NAME, "rows": "nope"}))
    assert r.status_code == 400


def test_empty_rows_is_success_zero_written(client, auth_headers):
    r = client.post(URL, headers=auth_headers,
                    data=json.dumps({"db_name": DB_NAME, "rows": []}))
    assert r.status_code == 200
    assert r.get_json().get("written") == 0


def test_empty_cod_is_skipped(client, auth_headers):
    body = {"db_name": DB_NAME, "rows": [{"Cod": "  ", "Descriere": "x", "Stare": "y"}]}
    r = client.post(URL, headers=auth_headers, data=json.dumps(body))
    assert r.status_code == 200
    assert r.get_json().get("written") == 0


def test_get_missing_db_name_returns_400(client, auth_headers):
    r = client.get(GET_URL, headers=auth_headers)
    assert r.status_code == 400


def test_get_missing_token_is_rejected(client):
    r = client.get(GET_URL + "?db_name=" + DB_NAME)
    assert r.status_code == 401


def test_get_returns_list_shape(client, auth_headers):
    r = client.get(GET_URL + "?db_name=" + DB_NAME, headers=auth_headers)
    assert r.status_code == 200
    body = r.get_json()
    assert body["db_name"] == DB_NAME
    assert isinstance(body["rows"], list)
    assert body["count"] == len(body["rows"])


def test_upsert_then_get_finds_the_row(client, auth_headers):
    """End-to-end (server side): a POSTed code is visible via GET, then cleaned up."""
    conn = get_db_connection(DB_NAME)
    try:
        body = {"db_name": DB_NAME, "rows": [{"Cod": "T2", "Descriere": "GD", "Stare": "GS"}]}
        assert client.post(URL, headers=auth_headers, data=json.dumps(body)).status_code == 200

        r = client.get(GET_URL + "?db_name=" + DB_NAME, headers=auth_headers)
        assert r.status_code == 200
        codes = {row["Cod"] for row in r.get_json()["rows"]}
        assert "T2" in codes
    finally:
        cleanup = conn.cursor()
        cleanup.execute("DELETE FROM FX_Angajamente WHERE CodAngajament = %s", ("T2",))
        conn.commit()
        conn.close()


def test_insert_then_update_refreshes_descriere_stare(client, auth_headers):
    """Upsert semantics: INSERT sets DC + Preluat=1; the duplicate refreshes ONLY
    Descriere/Stare. Verified directly in FX_Angajamente, then the T1 row is deleted."""
    conn = get_db_connection(DB_NAME)
    try:
        body1 = {"db_name": DB_NAME, "rows": [{"Cod": "T1", "Descriere": "D1", "Stare": "S1"}]}
        assert client.post(URL, headers=auth_headers, data=json.dumps(body1)).status_code == 200
        body2 = {"db_name": DB_NAME, "rows": [{"Cod": "T1", "Descriere": "D2", "Stare": "S2"}]}
        assert client.post(URL, headers=auth_headers, data=json.dumps(body2)).status_code == 200

        cursor = conn.cursor()
        cursor.execute(
            "SELECT Descriere, Stare, DC, Preluat FROM FX_Angajamente "
            "WHERE CodAngajament = %s",
            ("T1",),
        )
        row = cursor.fetchone()
        assert row is not None, "T1 row not found in FX_Angajamente after upsert"
        descriere, stare, dc, preluat = row
        assert descriere == "D2"
        assert stare == "S2"
        assert dc == DB_NAME
        assert preluat == 1
    finally:
        # Cleanup regardless of assertion outcome.
        try:
            cleanup = conn.cursor()
            cleanup.execute("DELETE FROM FX_Angajamente WHERE CodAngajament = %s", ("T1",))
            conn.commit()
        finally:
            conn.close()
