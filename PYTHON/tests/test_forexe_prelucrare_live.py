# Live-database tests for POST /api/forexe/prelucrare -- slice 0048-03.
# Run on the Flask host, from the PYTHON folder:
#     python -m pytest tests/test_forexe_prelucrare_live.py
#
# THE TEST THAT MATTERS MOST IN THIS SLICE is the first one: phase one must write
# NOTHING. It runs the whole pipeline against a populated database and then compares
# every affected table, row by row, against the snapshot taken beforehand. If the
# proposal ever leaks a write -- a forgotten commit, an autocommit connection, a step
# that opens its own transaction -- this is what catches it, and nothing else would.
#
# Preconditions:
#   1) config.py present (utils.database needs it);
#   2) 000_DEMO reachable, with sql/0049_receptii_stergere.sql already applied;
#   3) the seven AUTO_INCREMENT alters of slice 0048-01 section 3 already applied --
#      without them the inserts of steps 3-5 cannot allocate keys at all.
# Every fixture row is removed again, pass or fail.
import io
import json
import os

import pytest

try:
    from main import app
    from routes.auth.session_store import STORE
    from utils.database import get_kbot_connection
except Exception as e:                              # pragma: no cover - off-host
    pytest.skip(f"host-only test (config.py / app imports unavailable): {e}",
                allow_module_level=True)

DB_NAME = "000_DEMO"
URL = "/api/forexe/prelucrare"
FIXTURE = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                       "fixtures", "prelucrare_AAB37CNBK95.json")

# Tabelele pe care conducta le atinge, cu cheia lor. Ordinea e cea a cheilor straine,
# ca stergerea de la coada sa poata merge invers.
TABELE = (
    ("FX_Plati", "IdPlataFX"),
    ("FX_Rezervari", "IDRZ"),
    ("FX_Receptii", "IDR"),
    ("FX_Receptii_RHR", "IDRHR"),
    ("FX_Receptii_H", "IDRH"),
    ("FX_Receptii_R", "IDRR"),
    ("FX_Istoric", "ID"),
    ("FX_Indicatori", "CodAI"),
    ("FX_Angajamente", "CodAngajament"),
)


def _payload(**extra):
    body = json.load(io.open(FIXTURE, encoding="utf-8"))
    body.update(extra)
    return json.dumps(body, ensure_ascii=False)


def _cod():
    return json.load(io.open(FIXTURE, encoding="utf-8"))["cod"]


def _instantaneu_complet(cursor, cod):
    """
    Tot ce ar putea scrie conducta, citit ca liste de tupluri ordonate.

    `SELECT *` e deliberat: un instantaneu care numeste coloanele ar rata exact coloana
    noua pe care cineva o adauga si o scrie din greseala.
    """
    out = {}
    for tabel, cheie in TABELE:
        col = "CodAngajament"
        cursor.execute(
            f"SELECT * FROM {tabel} WHERE {col} = %s ORDER BY {cheie}", (cod,))
        out[tabel] = [tuple(str(v) for v in rand) for rand in cursor.fetchall()]
    return out


@pytest.fixture
def client():
    app.config["TESTING"] = True
    with app.test_client() as c:
        yield c


@pytest.fixture
def auth_headers():
    token, _ = STORE.create(username="pytest-op", password="unused",
                            id_unitate=0, db_name=DB_NAME,
                            ctx={"DbName": DB_NAME}, pcname="PYTEST")
    yield {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    STORE.revoke(token)


@pytest.fixture
def curat():
    """Sterge angajamentul de test inainte SI dupa. ON DELETE CASCADE face restul."""
    cod = _cod()

    def _sterge():
        conn = get_kbot_connection(DB_NAME)
        try:
            cur = conn.cursor()
            cur.execute("DELETE FROM FX_Angajamente WHERE CodAngajament = %s", (cod,))
            conn.commit()
            cur.close()
        finally:
            conn.close()

    _sterge()
    yield cod
    _sterge()


# ===========================================================================
# Faza unu nu scrie nimic
# ===========================================================================
def test_the_proposal_leaves_every_table_byte_identical(client, auth_headers, curat):
    """
    Cel mai important test al feliei.

    Se ruleaza INTAI o salvare, ca baza sa NU fie goala -- o propunere peste o baza goala
    ar trece si daca ar scrie, fiindca n-ar avea ce sa strice. Apoi se ia un instantaneu
    complet, se ruleaza propunerea, si se compara.
    """
    cod = curat

    # 1. Se populeaza: propunere, apoi salvare cu tot ce a propus ea.
    prop = client.post(URL, headers=auth_headers,
                       data=_payload(mod="propunere")).get_json()
    assert "amprenta" in prop, prop
    decizii = [{"rand_istoric": i["rand_istoric"], "data_h": i["data_h"],
                "actiune": "asociat", "idrr": i["sugestie_idrr"]}
               if i["sugestie_idrr"] else
               {"rand_istoric": i["rand_istoric"], "data_h": i["data_h"],
                "actiune": "ignorat"}
               for i in prop["instantanee"]]
    salv = client.post(URL, headers=auth_headers, data=_payload(
        mod="salvare", decizii=decizii, amprenta=prop["amprenta"]))
    assert salv.status_code == 200, salv.get_json()

    # 2. Instantaneul de referinta.
    conn = get_kbot_connection(DB_NAME)
    try:
        cur = conn.cursor()
        inainte = _instantaneu_complet(cur, cod)
        cur.close()
    finally:
        conn.close()
    assert inainte["FX_Istoric"], "baza trebuie sa fie populata inainte de test"

    # 3. Propunerea, peste o baza care ARE date.
    r = client.post(URL, headers=auth_headers, data=_payload(mod="propunere"))
    assert r.status_code == 200, r.get_json()
    assert r.get_json()["faza"] == "propunere"

    # 4. Nimic nu s-a miscat.
    conn = get_kbot_connection(DB_NAME)
    try:
        cur = conn.cursor()
        dupa = _instantaneu_complet(cur, cod)
        cur.close()
    finally:
        conn.close()

    for tabel, _ in TABELE:
        assert dupa[tabel] == inainte[tabel], (
            f"propunerea a modificat {tabel}: "
            f"{len(inainte[tabel])} randuri inainte, {len(dupa[tabel])} dupa")


def test_the_proposal_never_marks_history_as_processed(client, auth_headers, curat):
    """
    Pasul 7 se deruleaza inapoi cu tot restul. Daca ar supravietui, a doua propunere ar
    vedea istoricul ca prelucrat si n-ar mai construi niciun instantaneu -- exact ce face
    rularea NErepetabila.
    """
    p1 = client.post(URL, headers=auth_headers,
                     data=_payload(mod="propunere")).get_json()
    p2 = client.post(URL, headers=auth_headers,
                     data=_payload(mod="propunere")).get_json()
    assert p1["scrise"] == p2["scrise"]
    assert len(p1["instantanee"]) == len(p2["instantanee"])
    assert p1["amprenta"] == p2["amprenta"]


# ===========================================================================
# Amprenta
# ===========================================================================
def test_a_stale_fingerprint_is_409_and_writes_nothing(client, auth_headers, curat):
    cod = curat
    prop = client.post(URL, headers=auth_headers,
                       data=_payload(mod="propunere")).get_json()
    decizii = [{"rand_istoric": i["rand_istoric"], "data_h": i["data_h"],
                "actiune": "ignorat"} for i in prop["instantanee"]]
    r = client.post(URL, headers=auth_headers, data=_payload(
        mod="salvare", decizii=decizii, amprenta="nu-e-amprenta-buna"))
    assert r.status_code == 409
    assert r.get_json()["reason"] == "STARE_MODIFICATA"

    conn = get_kbot_connection(DB_NAME)
    try:
        cur = conn.cursor()
        cur.execute("SELECT COUNT(*) FROM FX_Istoric WHERE CodAngajament = %s", (cod,))
        assert cur.fetchone()[0] == 0
        cur.close()
    finally:
        conn.close()


# ===========================================================================
# Deciziile
# ===========================================================================
def test_a_missing_decision_is_400(client, auth_headers, curat):
    prop = client.post(URL, headers=auth_headers,
                       data=_payload(mod="propunere")).get_json()
    assert prop["instantanee"], "sarcina utila de test trebuie sa produca instantanee"
    decizii = [{"rand_istoric": i["rand_istoric"], "data_h": i["data_h"],
                "actiune": "ignorat"} for i in prop["instantanee"][:-1]]
    r = client.post(URL, headers=auth_headers, data=_payload(
        mod="salvare", decizii=decizii, amprenta=prop["amprenta"]))
    assert r.status_code == 400
    assert "Lipsesc deciziile" in r.get_json()["error"]


def test_a_wrong_data_h_is_400(client, auth_headers, curat):
    prop = client.post(URL, headers=auth_headers,
                       data=_payload(mod="propunere")).get_json()
    decizii = [{"rand_istoric": i["rand_istoric"], "data_h": i["data_h"],
                "actiune": "ignorat"} for i in prop["instantanee"]]
    decizii[0]["data_h"] = "2001-01-01T00:00:00"
    r = client.post(URL, headers=auth_headers, data=_payload(
        mod="salvare", decizii=decizii, amprenta=prop["amprenta"]))
    assert r.status_code == 400
    assert "învechit" in r.get_json()["error"]


def test_the_save_phase_commits_and_is_repeatable(client, auth_headers, curat):
    """
    D13: retrimiterea aceleiasi sarcini utile nu adauga nimic. A doua propunere de dupa o
    salvare nu mai are nimic nou de scris in istoric.
    """
    prop = client.post(URL, headers=auth_headers,
                       data=_payload(mod="propunere")).get_json()
    decizii = [{"rand_istoric": i["rand_istoric"], "data_h": i["data_h"],
                "actiune": "ignorat"} for i in prop["instantanee"]]
    r = client.post(URL, headers=auth_headers, data=_payload(
        mod="salvare", decizii=decizii, amprenta=prop["amprenta"]))
    assert r.status_code == 200
    assert r.get_json()["faza"] == "salvare"

    din_nou = client.post(URL, headers=auth_headers,
                          data=_payload(mod="propunere")).get_json()
    assert din_nou["scrise"]["FX_Istoric"] == 0
