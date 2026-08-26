# Route tests for POST /api/forexe/prelucrare (slices 0048-02 and 0048-03).
#
# OFFLINE, and deliberately so. Two things usually force a route test to be host-only:
# the database, and `from main import app` (which drags in the whole server, pandas
# included). Neither is needed here:
#
#   * the database is replaced by a fake connection that answers by SQL shape, and
#   * the app is a bare Flask instance with ONLY forexe_bp registered -- the same
#     blueprint main.py registers, so the route, its @require_session guard and its
#     JSON bodies are the real ones.
#
# The assertion that matters most is the rollback one: a 409 must leave nothing behind,
# not even the angajament written by step 1.
import json

import pytest
from flask import Flask

try:
    from routes.forexe import forexe_bp
    from routes.auth.session_store import STORE
    import routes.forexe.prelucrare as prelucrare
except Exception as e:                              # pragma: no cover - broken install
    pytest.skip(f"blueprint imports unavailable: {e}", allow_module_level=True)

# One app for the whole module. Registering a blueprint twice on one app raises, so it
# is built once here rather than per test.
app = Flask(__name__)
app.register_blueprint(forexe_bp)

DB_NAME = "000_DEMO"
URL = "/api/forexe/prelucrare"

COD = "AAB37CNBK95"
RAW_02E = "02E- 65. 04. 02. 20. 01. 01"


def payload(rows=None, alegeri=None, mod=None, decizii=None, amprenta=None):
    body = {
        "cod": COD,
        "workflow": "adlop - Prelucrare Completa.wfl",
        "moment": "2026-08-25T10:12:00",
        "scalari": {"DescriereAngajament": "Test", "StareAngajament": "Definitivat",
                    "DataAngajament": "10/02/2026"},
        "tabele": {"TabelIndicatori_results": rows if rows is not None else []},
    }
    if alegeri is not None:
        body["alegeri"] = alegeri
    if mod is not None:
        body["mod"] = mod
    if decizii is not None:
        body["decizii"] = decizii
    if amprenta is not None:
        body["amprenta"] = amprenta
    return json.dumps(body)


def indicator_row(cod="AAB", raw=RAW_02E):
    return {"Indicator_ang": cod, "Sector_Sursa_Indicator": raw,
            "Credit_bugetar": "1.000,00", "Total_credit_angajament": "2.000,00",
            "Angajament_legal": "1.500,00",
            "Credit_bugetar_rezervat_definitiv_an_curent": "1.200,00"}


# ---------------------------------------------------------------------------
# The fake database
# ---------------------------------------------------------------------------
# Interogarile de CITIRE pe care pasii 3-8 le emit chiar si cu un payload gol. Toate
# raspund cu zero randuri, deci conducta trece prin ele fara sa faca nimic. Sunt
# enumerate explicit, nu prinse cu un `startswith("SELECT")` general: paza de la coada
# trebuie sa prinda in continuare orice SQL pe care testul nu il asteapta -- si mai ales
# orice SCRIERE nedorita, care e chiar ce testul asta pazeste.
_SELECTS_GOALE = (
    "SELECT MAX(Rez_Ord)",              # 3a, multiplicatorul Rez_Ord
    "SELECT ID, DataFX",                # 3a, randurile de istoric existente
    "SELECT I.CodAI",                   # read_indicatori
    "SELECT ID, HASH",                  # 4a, randurile de receptie neprelucrate
    "SELECT ID, Observatii",            # 5, randurile de plata neprelucrate
    "SELECT IDRR, NRCRT",               # citeste_receptii, antetele
    "SELECT IDRR, CodIndicator",        # citeste_receptii, liniile RHR
    "SELECT IDRH, IDH",                 # citeste_instantanee, antetele
    "SELECT IDRH, CodIndicator",        # citeste_instantanee, liniile
    "SELECT IDRR, SumaAntet",           # 4c, candidatii trecerii automate
    "SELECT DISTINCT IDRR",             # 4d, recepțiile de recalculat
)

# Amprenta (2.3): o baza goala are zero peste tot. Valorile conteaza doar prin faptul ca
# sunt STABILE intre cele doua faze -- testul de salvare se sprijina pe asta.
_AMPRENTA_GOALA = {"ic": 0, "im": 0, "id_": "1900-01-01",
                   "rc": 0, "rm": 0, "hc": 0, "hm": 0, "hn": 0}


class FakeCursor:
    def __init__(self, conn):
        self.conn = conn
        self._result = []

    def execute(self, sql, params=None):
        self.conn.executed.append((sql, params))
        if sql.startswith("SELECT 1 FROM FX_Angajamente"):
            self._result = [{"1": 1}] if self.conn.angajament_exists else []
        elif sql.startswith("SELECT 1 FROM FX_Indicatori"):
            self._result = []
        elif "(SELECT COUNT(*) FROM FX_Istoric" in sql:
            self._result = [dict(_AMPRENTA_GOALA)]
        # ATENTIE LA ORDINE: interogarea lui read_indicatori poarta si ea
        # «FROM Clasificatii C», in subinterogarile ei scalare pentru Clsf si CodSSI.
        # Daca ramura de candidati ar veni prima, ar inghiti-o si ar cauta parametrul
        # al doilea intr-un tuplu care are unul singur.
        elif sql.startswith(_SELECTS_GOALE):
            self._result = []
        elif "FROM Clasificatii C" in sql:
            self._result = list(self.conn.candidates.get((params[0], params[1]), []))
        elif sql.startswith("SELECT SS, ClsfE, IdUnitate"):
            self._result = [{"SS": ss, "ClsfE": ce, "IdUnitate": idu}
                            for (ss, ce), idu in self.conn.remembered.items()]
        elif "INSERT INTO FX_Alegeri_Unitate" in sql:
            self.conn.remembered[(params[0], params[1])] = params[2]
            self._result = []
        elif "SELECT DISTINCT IdClsfAcc" in sql:
            self._result = list(self.conn.clsf.get((params[0], params[1]), []))
        elif sql.startswith(("INSERT INTO FX_Angajamente", "UPDATE FX_Angajamente",
                             "INSERT INTO FX_Indicatori", "UPDATE FX_Indicatori")):
            self._result = []
        else:                                        # pragma: no cover - guard
            raise AssertionError(f"unexpected SQL: {sql}")

    def fetchall(self):
        return self._result

    def fetchone(self):
        return self._result[0] if self._result else None

    def close(self):
        pass


class FakeConnection:
    def __init__(self, candidates=None, remembered=None, clsf=None,
                 angajament_exists=False):
        self.candidates = candidates or {}
        self.remembered = remembered or {}
        self.clsf = clsf or {}
        self.angajament_exists = angajament_exists
        self.executed = []
        self.committed = False
        self.rolled_back = False

    def start_transaction(self):
        pass

    def cursor(self, dictionary=False):
        return FakeCursor(self)

    def commit(self):
        self.committed = True

    def rollback(self):
        self.rolled_back = True

    def is_connected(self):
        return True

    def close(self):
        pass

    def wrote(self, prefix):
        return sum(1 for sql, _ in self.executed if sql.startswith(prefix))


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
def fake_db(monkeypatch):
    """Installs a FakeConnection and hands the test the one that was used."""
    holder = {}

    def install(conn):
        holder["conn"] = conn
        monkeypatch.setattr(prelucrare, "get_kbot_connection", lambda db: conn)
        return conn

    return install


def unit(id_unitate, detalii, sursa="02E", program="PRG"):
    return {"IdUnitate": id_unitate, "Detalii": detalii,
            "SursaSector": sursa, "CodProgram": program, "Cnt": 1}


# ---------------------------------------------------------------------------
# Guard and validation
# ---------------------------------------------------------------------------
def test_missing_token_is_rejected(client):
    r = client.post(URL, headers={"Content-Type": "application/json"},
                    data=payload())
    assert r.status_code == 401


def test_missing_cod_returns_400(client, auth_headers):
    r = client.post(URL, headers=auth_headers, data=json.dumps({"scalari": {}}))
    assert r.status_code == 400


def test_malformed_alegeri_returns_400(client, auth_headers):
    r = client.post(URL, headers=auth_headers,
                    data=payload(alegeri=[{"ss": "02E"}]))
    assert r.status_code == 400


# ---------------------------------------------------------------------------
# The happy path
# ---------------------------------------------------------------------------
def _fake_unit():
    return FakeConnection(
        candidates={("02E", "200101"): [unit(76, "ENERGETIC ISJ")]},
        clsf={(76, "650402200101"): [{"IdClsfAcc": 1204}]})


def test_the_default_mode_is_the_proposal_and_it_never_commits(client, auth_headers,
                                                               fake_db):
    """
    Un client care nu trimite «mod» primeste faza care NU scrie.

    Asta e chiar poarta contractului in doua faze: tacerea nu are voie sa insemne
    «salveaza». Pasii chiar ruleaza -- INSERT-urile sunt emise -- dar tranzactia se
    deruleaza inapoi neconditionat, deci nimic nu ramane.
    """
    conn = fake_db(_fake_unit())
    r = client.post(URL, headers=auth_headers, data=payload(rows=[indicator_row()]))
    assert r.status_code == 200
    body = r.get_json()
    assert body["faza"] == "propunere"
    assert body["amprenta"]
    assert conn.rolled_back and not conn.committed
    # Conducta chiar a rulat: scrierile s-au emis si abia apoi au fost anulate.
    assert conn.wrote("INSERT INTO FX_Angajamente") == 1
    assert conn.wrote("INSERT INTO FX_Indicatori") == 1


def test_the_save_phase_commits_and_echoes_the_fingerprint(client, auth_headers,
                                                           fake_db):
    """Drumul complet: propunere, apoi salvare cu amprenta primita inapoi."""
    conn = fake_db(_fake_unit())
    prop = client.post(URL, headers=auth_headers,
                       data=payload(rows=[indicator_row()])).get_json()
    assert prop["instantanee"] == []      # baza falsa e goala, deci nimic de asezat

    conn2 = fake_db(_fake_unit())
    r = client.post(URL, headers=auth_headers, data=payload(
        rows=[indicator_row()], mod="salvare", decizii=[],
        amprenta=prop["amprenta"]))
    assert r.status_code == 200
    body = r.get_json()
    assert body["faza"] == "salvare"
    assert body["are"]["Indicatori"] is True
    assert body["scrise"]["FX_Angajamente"] == 1
    assert body["scrise"]["FX_Indicatori"] == 1
    assert conn2.committed and not conn2.rolled_back


def test_a_stale_fingerprint_is_409_and_writes_nothing(client, auth_headers, fake_db):
    conn = fake_db(_fake_unit())
    r = client.post(URL, headers=auth_headers, data=payload(
        rows=[indicator_row()], mod="salvare", decizii=[],
        amprenta="amprenta-dintr-o-alta-viata"))
    assert r.status_code == 409
    assert r.get_json()["reason"] == "STARE_MODIFICATA"
    assert conn.rolled_back and not conn.committed
    # Verificarea se face INAINTEA oricarei scrieri.
    assert conn.wrote("INSERT INTO FX_Angajamente") == 0


def test_save_without_decizii_or_amprenta_is_400(client, auth_headers, fake_db):
    fake_db(_fake_unit())
    r = client.post(URL, headers=auth_headers,
                    data=payload(rows=[indicator_row()], mod="salvare",
                                 amprenta="x"))
    assert r.status_code == 400
    r = client.post(URL, headers=auth_headers,
                    data=payload(rows=[indicator_row()], mod="salvare", decizii=[]))
    assert r.status_code == 400


def test_an_unknown_mode_is_400(client, auth_headers, fake_db):
    fake_db(_fake_unit())
    r = client.post(URL, headers=auth_headers,
                    data=payload(rows=[indicator_row()], mod="poate"))
    assert r.status_code == 400


def test_the_response_says_step_8_did_not_run(client, auth_headers, fake_db):
    """
    Pasul 8 scrie doar FX_Extrase, care e in afara scopului feliei. Nu e uitat si nu e
    inlocuit cu ceva aproximativ: fiecare raspuns spune ca nu s-a executat.
    """
    fake_db(FakeConnection())
    r = client.post(URL, headers=auth_headers, data=payload(rows=[]))
    assert r.status_code == 200
    assert any("Pasul 8" in w for w in r.get_json()["avertismente"])


def test_diacritics_are_literal_utf8_not_escaped(client, auth_headers, fake_db):
    fake_db(FakeConnection())
    r = client.post(URL, headers=auth_headers, data=payload(rows=[]))
    text = r.data.decode("utf-8")
    assert "FX_Extrase" in text
    assert "în afara scopului" in text
    assert "\\u" not in text


# ---------------------------------------------------------------------------
# The question
# ---------------------------------------------------------------------------
def test_two_candidates_answer_409_and_roll_everything_back(client, auth_headers,
                                                            fake_db):
    conn = fake_db(FakeConnection(candidates={("02E", "200101"): [
        unit(75, "SC29 LOCAL", "02A", "P75"),
        unit(76, "ENERGETIC ISJ", "02E", "P76")]}))
    r = client.post(URL, headers=auth_headers, data=payload(rows=[indicator_row()]))

    assert r.status_code == 409
    body = r.get_json()
    assert body["reason"] == "ALEGERE_UNITATE"
    assert body["cod"] == COD
    q = body["alegeri_necesare"][0]
    assert q["cod_indicator"] == "AAB"
    assert q["clsf"] == RAW_02E
    assert [u["detalii"] for u in q["unitati"]] == ["SC29 LOCAL", "ENERGETIC ISJ"]

    # Nothing half-written: step 1 already inserted the angajament, and the
    # rollback is what takes it away again.
    assert conn.rolled_back and not conn.committed
    assert conn.wrote("INSERT INTO FX_Angajamente") == 1
    assert conn.wrote("INSERT INTO FX_Indicatori") == 0


def test_resending_with_the_choice_writes_it(client, auth_headers, fake_db):
    conn = fake_db(FakeConnection(
        candidates={("02E", "200101"): [unit(75, "SC29 LOCAL"),
                                        unit(76, "ENERGETIC ISJ")]},
        clsf={(76, "650402200101"): [{"IdClsfAcc": 1204}]}))
    r = client.post(URL, headers=auth_headers, data=payload(
        rows=[indicator_row()],
        alegeri=[{"ss": "02E", "clsfe": "200101", "id_unitate": 76,
                  "retine": False}]))
    assert r.status_code == 200
    # Cererea nu poarta «mod», deci e o PROPUNERE: raspunsul e 200 si tranzactia se
    # deruleaza inapoi. Ce dovedeste testul e ca alegerea operatorului a ajuns in
    # INSERT -- nu ca s-a comis, ceea ce e treaba fazei a doua.
    assert conn.rolled_back and not conn.committed
    # The chosen unit is what went into the row.
    ins = [p for sql, p in conn.executed if sql.startswith("INSERT INTO FX_Indicatori")]
    assert 76 in ins[0]
    # The box was unticked, so nothing was remembered.
    assert conn.remembered == {}


def test_the_ticked_box_stores_the_choice(client, auth_headers, fake_db):
    conn = fake_db(FakeConnection(
        candidates={("02E", "200101"): [unit(75, "a"), unit(76, "b")]},
        clsf={(76, "650402200101"): [{"IdClsfAcc": 1204}]}))
    r = client.post(URL, headers=auth_headers, data=payload(
        rows=[indicator_row()],
        alegeri=[{"ss": "02E", "clsfe": "200101", "id_unitate": 76,
                  "retine": True}]))
    assert r.status_code == 200
    assert conn.remembered == {("02E", "200101"): 76}


def test_a_stored_choice_means_no_question_at_all(client, auth_headers, fake_db):
    conn = fake_db(FakeConnection(
        candidates={("02E", "200101"): [unit(75, "a"), unit(76, "b")]},
        remembered={("02E", "200101"): 76},
        clsf={(76, "650402200101"): [{"IdClsfAcc": 1204}]}))
    r = client.post(URL, headers=auth_headers, data=payload(rows=[indicator_row()]))
    # Nicio intrebare: 200, si niciun `alegeri_necesare` in corp.
    assert r.status_code == 200
    assert "alegeri_necesare" not in r.get_json()
    ins = [p for sql, p in conn.executed if sql.startswith("INSERT INTO FX_Indicatori")]
    assert 76 in ins[0]


def test_a_classification_with_no_unit_is_400_and_rolls_back(client, auth_headers,
                                                             fake_db):
    conn = fake_db(FakeConnection(candidates={}))
    r = client.post(URL, headers=auth_headers, data=payload(rows=[indicator_row()]))
    assert r.status_code == 400
    assert "AAB" in r.get_json()["error"]
    assert conn.rolled_back and not conn.committed
