"""
Local preview of the PUBLIC SITE with made-up data.   DEVELOPMENT ONLY - never deployed.

What it is for: look at the site (presentation page, «Cere mai multe detalii», the web area for
registered users) in a browser BEFORE anything is uploaded to the real server, change what is wrong,
reload, and only at the end publish.

    PYTHON\\.venv\\Scripts\\python.exe tools\\SitePreview\\preview_server.py [--port 5050] [--lan]
    (or  tools\\SitePreview\\Start-SitePreview.ps1 [-Lan] [-Port 5050])

It uses the REAL files of the site: the templates and `content.json` of the landing page, the form
page and its script, the portal page, the grid, the PDF viewer, the stylesheets. Only what would
reach the database or the mail server is replaced:

  * the form «Cere mai multe detalii» accepts and throws the request away (nothing stored, no mail);
  * the portal signs in with ANY e-mail and the password  demo  and the code  123456;
    it offers three made-up units with made-up angajamente, reservations, receptions and payments;
  * the signed documents are the sample PDFs of this repository when they are on this disk
    (`DDF_V008_*.pdf` in the repository root, `Surse/ETAPE ORDONANTARE/Etapa3_Semnata.pdf`,
    `Surse/CAB+ERRRRRRR/NOTA CAB 23.pdf`; the last two are not in git);
  * `/inregistrare` shows the registration page, but its buttons need the server and will not work.

Templates, CSS, JS and `content.json` changes show on a plain reload. A change to a Python file
needs the server restarted. By default it listens on this computer only; `--lan` opens it to the
local network so a phone on the same Wi-Fi can open it (Windows may ask to allow it).
"""
import argparse
import json
import os
import sys
import tempfile
from datetime import datetime, timedelta

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, "..", ".."))
PYTHON_DIR = os.path.join(REPO, "PYTHON")
sys.path.insert(0, PYTHON_DIR)

from flask import Flask, jsonify, request, send_file, send_from_directory  # noqa: E402

from routes.detalii import detalii as detalii_mod  # noqa: E402
from routes.landing.landing import landing_bp  # noqa: E402
from routes.portal import portal as portal_mod  # noqa: E402
import routes.portal.date as date_mod  # noqa: E402  (registers /api/portal/date/* on portal_bp)
import routes.portal.admin as admin_mod  # noqa: E402  (registers /api/portal/admin/* on portal_bp)
from routes.landing import vizite as vizite_mod  # noqa: E402

DEMO_PASSWORD = "demo"
DEMO_CODE = "123456"

UNITS = [
    {"DC": "000_DEMO", "NumeUnitate": "Unitatea Demo", "CF": "10000001", "Rol": "Contabil"},
    {"DC": "111_GRAD", "NumeUnitate": "Grădinița cu Program Prelungit nr. 28, Ploiești", "CF": "14374579", "Rol": "Director"},
    {"DC": "222_SCOALA", "NumeUnitate": "Școala Gimnazială „Sfântul Vasile”", "CF": "29164800", "Rol": "Contabil"},
]
PERIODS = [
    {"AN": 2026, "SS": "01A", "CodProgram": "02A"}, {"AN": 2026, "SS": "02A", "CodProgram": "02A"},
    {"AN": 2026, "SS": "02E", "CodProgram": "02E"}, {"AN": 2025, "SS": "01A", "CodProgram": "01A"},
]

# Sample documents: (id -> file relative to the repository root).
SAMPLE_PDFS = {
    "ddf-pdf": {1: "DDF_V008_2026_02_17.pdf"},
    "ord-pdf": {7: "Surse/ETAPE ORDONANTARE/Etapa3_Semnata.pdf"},
    "nc-pdf": {3: "Surse/CAB+ERRRRRRR/NOTA CAB 23.pdf"},
}


def build_app():
    app = Flask(__name__, static_folder=os.path.join(PYTHON_DIR, "static"))
    app.config["TEMPLATES_AUTO_RELOAD"] = True
    app.config["SEND_FILE_MAX_AGE_DEFAULT"] = 0     # a reload always fetches the newest CSS / JS
    app.jinja_env.auto_reload = True

    # ---- the form: accept, store nothing, mail nothing
    detalii_mod.MIN_SECONDS = 1
    detalii_mod._insert = lambda *a, **k: 1
    detalii_mod._mark_notified = lambda *a, **k: None
    detalii_mod.mailer.send_details_notice = lambda *a, **k: None
    detalii_mod.mailer.details_address = lambda: "previzualizare@exemplu.ro"

    # ---- the portal: demo sign-in and made-up data
    portal_mod.verify_operator = lambda email, password: password == DEMO_PASSWORD
    portal_mod._user_units = lambda email: list(UNITS)
    portal_mod._unit_periods = lambda db_name: list(PERIODS)
    portal_mod.log_action = lambda *a, **k: None
    portal_mod.mailer.is_configured = lambda: True
    portal_mod.mailer.send_portal_code = lambda *a, **k: None
    portal_mod.secrets.randbelow = lambda n: int(DEMO_CODE)   # the mailed code is always 123456
    # a person looking at the site does not want to be locked out by the brute-force limiter
    portal_mod.LIMITER.is_blocked = lambda *a, **k: False
    detalii_mod.LIMITER.is_blocked = lambda *a, **k: False

    install_demo_data()
    install_admin_demo()

    app.register_blueprint(landing_bp)
    app.register_blueprint(vizite_mod.vizite_bp)
    app.register_blueprint(detalii_mod.detalii_bp)
    app.register_blueprint(portal_mod.portal_bp)

    @app.route("/inregistrare")
    @app.route("/inregistrare/")
    def registration_page():
        return send_from_directory(app.static_folder, "inregistrare.html")

    @app.route("/demo")
    def demo_page():
        return DEMO_HTML

    # Chrome DevTools "Automatic Workspace Folders": Chrome asks for this file and offers to connect
    # the folder, so CSS edited in DevTools is saved straight into PYTHON\static. The root is PYTHON\
    # because the URLs start with /static/. Answered to this PC only.
    @app.route("/.well-known/appspecific/com.chrome.devtools.json")
    def devtools_workspace():
        if request.remote_addr not in ("127.0.0.1", "::1"):
            return ("", 404)
        return jsonify({"workspace": {"root": PYTHON_DIR, "uuid": "6f0c1c52-3f0e-4b8e-9d57-4b6f6c2a9e10"}})

    return app


# ---------------------------------------------------------------------------- made-up data
def install_demo_data():
    stari = ["În derulare", "Recepționat", "Plătit"]
    descrieri = ["Furnizare materiale de curățenie", "Servicii de pază", "Energie electrică", "Apă și canal",
                 "Încălzire", "Materiale didactice", "Mobilier", "Servicii informatice", "Alimente", "Reparații curente"]

    def tree():
        n = 60
        rows = [{
            "CodAngajament": "ANG-%04d" % (1000 + i), "IDDF": None,
            "Descriere": "%s nr. %d" % (descrieri[i % len(descrieri)], 1 + i // len(descrieri)),
            "Stare": stari[i % 3], "DataCreare": "2026-%02d-%02dT10:00:00" % (1 + i % 9, 1 + i % 27),
            "Surse": ["01A", "02A;02E", "01A;02A"][i % 3], "AreRezervari": True, "AreReceptii": i % 2 == 0,
            "ArePlati": i % 3 == 0, "AreDDF": i % 4 == 0, "AreOrd": i % 5 == 0,
        } for i in range(n)]
        return jsonify({"db_name": "demo", "count": n, "rows": rows})

    def sumar():
        return jsonify({"header": None, "rows": [{
            "clsf": "65.02.04.0%d" % i, "cod_indicator": "IND%d" % i, "partener": "Furnizor %d SRL" % i,
            "credit_bug": 10000.0 * i, "total_rezervari": 1200.5 * i, "total_receptii": 800.25 * i,
            "total_plati": 400.0 * i, "total_revizii": 0.0, "total_ordonantari": 100.0 * i} for i in range(1, 6)]})

    def rezervari():
        return jsonify({"rows": [{
            "idrz": i, "cod_indicator": "IND%d" % (1 + i % 3), "clsf": "65.02.04.01", "denumire": "Alte cheltuieli de funcționare",
            "data_rezervare": "2026-%02d-%02d" % (1 + i % 5, 5 + i), "r_credit_bug": 5000.0, "r_initiala": 1000.0 * i,
            "r_valoare": 900.0 * i, "r_definitiva": 900.0 * i, "are_ddf": i % 2 == 0} for i in range(1, 11)]})

    def receptii():
        return jsonify({"cod": request.args.get("cod"), "plati": [], "receptii": [{
            "idrr": i, "nrcrt_r": i, "data_r": "2026-%02d-05" % (1 + i % 4), "suma_antet": 1500.0 * i,
            "idrh": 100 + i, "total": 750.0 * i, "sters_h": False, "reconstituit": False,
            "descriere_r": "Recepție factură %d" % i, "data_h": "2026-%02d-06T09:30:00" % (1 + i % 4),
            "clsf": "65.02.04.01", "denumire": "Alte cheltuieli de funcționare", "cod_indicator": "IND1",
            "valoare": 750.0 * i, "dif": 750.0 * i, "este_stergere": False} for i in range(1, 9)]})

    def plati():
        return jsonify({"cod": request.args.get("cod"), "plati": [{
            "data_plata": "2026-%02d-15" % (1 + i % 6), "data_banca": "2026-%02d-%02d" % (1 + i % 6, 15 + i % 3), "nr_op": "OP%d" % (100 + i), "clsf": "65.02.04.01",
            "denumire": "Alte cheltuieli de funcționare", "cod_indicator": "IND1", "suma": 333.33 * i, "are_ord": i % 2 == 0,
            "platitor_nume": "Primăria", "platitor_cui": "RO11", "platitor_iban": "RO49TREZ000000000011", "suma_debit": 333.33 * i, "suma_credit": 0.0, "referinta": "REF%d" % i, "data_doc": "15.01.2026", "nr_doc_extras": "D%d" % i, "explicatii": "Plată factură %d" % i} for i in range(1, 9)]})

    def ddf():
        return jsonify({"cod": request.args.get("cod"), "antet": [{"cod_angajament": request.args.get("cod"), "cual": 1, "comp": "Financiar-contabil", "obiect_ddf": "Furnizare materiale de curățenie", "data_creare": "2026-02-10", "part_ang": True, "nume_partener": "Furnizor 1 SRL", "cod_fiscal": "RO1234567"}], "revizii": [
            {"idrev": 1, "numar_rev": 1, "data_rev": "2026-02-17", "total_revizie": 12500.0, "semnatura": "A,B,Ordonator", "pdf_sha256": "x", "desc_scurta": "Fundamentare inițială"},
            {"idrev": 2, "numar_rev": 2, "data_rev": "2026-03-02", "total_revizie": -500.0, "semnatura": "", "pdf_sha256": None, "desc_scurta": "Micșorare"}],
            "linii": [
                {"id_sec_a": 1, "idrev": 1, "clsf": "65.02.04.01", "element_fund": "Materiale de curățenie", "val_prec": 0.0, "val_cur": 12500.0, "val_tot": 12500.0},
                {"id_sec_a": 2, "idrev": 2, "clsf": "65.02.04.01", "element_fund": "Materiale de curățenie", "val_prec": 12500.0, "val_cur": -500.0, "val_tot": 12000.0}]})

    def ordonantari():
        return jsonify({"cod": request.args.get("cod"), "ordonantari": [
            {"idordp": 7, "nr_ord": 1, "data_ord": "2026-09-30", "total_ord": 1.0, "semnatura": "AB,CD,Ordonator", "pdf_sha256": "y"},
            {"idordp": 8, "nr_ord": 2, "data_ord": "2026-10-02", "total_ord": 250.0, "semnatura": "", "pdf_sha256": None}],
            "linii": [
                {"idordtblp": 1, "idordp": 7, "clsf": "65.02.04.01", "descriere": "Alte cheltuieli", "den_bene": "Furnizor 1 SRL", "cod_fiscal": "RO1234567", "cont_iban": "RO49TREZ0000000000123456", "obiect_ddf": "Materiale", "total_receptii": 1.0, "plati_ant": 0.0, "valoare": 1.0, "ramas": 0.0, "doc_just": "Factura 12"},
                {"idordtblp": 2, "idordp": 8, "clsf": "65.02.04.01", "descriere": "Alte cheltuieli", "den_bene": "Furnizor 2 SRL", "cod_fiscal": "RO7654321", "cont_iban": "RO49TREZ0000000000654321", "obiect_ddf": "Servicii", "total_receptii": 500.0, "plati_ant": 100.0, "valoare": 250.0, "ramas": 150.0, "doc_just": "Factura 15"}]})

    def istoric():
        return jsonify({"randuri": [{
            "id": i, "data_fx": "2026-%02d-%02dT10:00:00" % (1 + i % 4, 3 + i), "clsf": "65.02.04.01", "tip_rand": ["Rezervare", "Recepție", "Plată"][i % 3],
            "cod_indicator": "IND%d" % (1 + i % 3), "descriere": "Operațiune %d" % i, "val_rezervare_i": 1000.0 * i, "val_plata": 100.0 * i,
            "doc": "D%d" % i, "observatii": ""} for i in range(1, 13)], "clasificatii": [], "credit_initial_citit": True})

    def extrase():
        return jsonify({"antete": [{"idexh": 1, "data_extras": "2026-02-10", "clsf": "65.02.04.01", "numar_extras": "5", "denumire": "Cont", "cont": "RO..", "cod_iban": "RO00TREZ"}],
                        "operatiuni": [{"idfxe": i, "idfxh": 1, "data_banca": "2026-02-%02d" % (1 + i), "data_doc": "2026-02-%02d" % (1 + i), "nr_doc": "N%d" % i,
                                        "platitor_nume": "Furnizor %d" % i, "suma_debit": 100.0 * i, "suma_credit": 0.0, "cod_contract": request.args.get("cod"),
                                        "rand_contract": "IND1", "explicatii": "Plată %d" % i} for i in range(1, 7)]})

    def note():
        return jsonify({"note": [{"idnc": 3, "nr_nota": 23, "data_nota": "2026-09-28", "semnatura": "S1,S2", "pdf_sha256": "z"}]})

    date_mod._READERS.update({"tree": tree, "sumar": sumar, "rezervari": rezervari, "receptii": receptii,
                              "plati": plati, "ddf": ddf, "ord": ordonantari, "note": note,
                              "istoric": istoric, "extrase": extrase})

    def pdf_reader(kind):
        def read(doc_id):
            name = SAMPLE_PDFS[kind].get(doc_id)
            path = os.path.join(REPO, *name.split("/")) if name else None
            if not path or not os.path.isfile(path):
                return jsonify({"error": "Nu există PDF semnat pentru acest document."}), 404
            response = send_file(path, mimetype="application/pdf")
            response.headers["ETag"] = '"preview"'
            return response
        return read

    for kind in SAMPLE_PDFS:
        date_mod._PDF_READERS[kind] = pdf_reader(kind)


# ---------------------------------------------------------------------------- the admin page (slice 0110-09)
class _DemoCursor:
    """Answers the admin queries with made-up rows, chosen by what the SQL mentions."""

    def __init__(self, dictionary, visits):
        self._dictionary = dictionary
        self._visits = visits
        self._rows = []

    def execute(self, sql, params=None):
        text = " ".join(sql.split())
        now = datetime.now()
        if "FROM Unitati ORDER BY DC" in text:
            rows = [{"DC": u["DC"], "NumeUnitate": u["NumeUnitate"], "CF": u["CF"]} for u in UNITS]
            if "SELECT DC, NumeUnitate FROM" in text:      # the log filters ask for two columns only
                rows = [{"DC": r["DC"], "NumeUnitate": r["NumeUnitate"]} for r in rows]
        elif "FROM Unitati_Utilizatori" in text:
            rows = [{"DC": u["DC"], "n": 2 + i} for i, u in enumerate(UNITS)]
        elif "FROM Unitati_Ani" in text:
            rows = [{"DC": u["DC"], "an_min": 2024, "an_max": 2026} for u in UNITS]
        elif "information_schema.TABLES" in text and "GROUP BY" in text:
            rows = [{"db": u["DC"], "tables_n": 61, "bytes_n": 8.5e6 * (i + 1), "rows_n": 41000 * (i + 1),
                     "last_write": now - timedelta(days=i)} for i, u in enumerate(UNITS)]
            rows.append({"db": "AVACONT_COMUN", "tables_n": 24, "bytes_n": 3.2e6, "rows_n": 9000, "last_write": now})
        elif "information_schema.TABLES" in text:
            rows = [{"db": u["DC"], "name": name, "rows_n": n} for u in UNITS
                    for name, n in (("FX_Receptii", 18200), ("FX_Plati", 9100), ("FX_Rezervari", 4400), ("FX_Extra", 10))]
        elif "FROM Jurnal" in text:
            rows = [{"DC": u["DC"], "last_action": now - timedelta(days=i), "last_login": now - timedelta(days=i + 1),
                     "users_30d": 1 + i, "logins_30d": 12 * (i + 1)} for i, u in enumerate(UNITS)]
        elif "FROM schema_diff_log" in text:
            rows = [{"target_db": UNITS[1]["DC"].lower(), "pending": 5, "destructive": 1, "errors": 0}]
        elif "FROM Vizite_Site" in text:
            rows = list(self._visits)
        else:
            raise AssertionError("the preview has no answer for: " + text[:80])
        self._rows = rows if self._dictionary else [tuple(r.values()) for r in rows]

    def fetchall(self):
        return self._rows


class _DemoConnection:
    def __init__(self, visits):
        self._visits = visits

    def cursor(self, dictionary=False):
        return _DemoCursor(dictionary, self._visits)

    def is_connected(self):
        return True

    def close(self):
        pass


def install_admin_demo():
    """Sign in as scavatarsoft@gmail.com to see «Administrare». The log files are REAL files (made
    here, in a temp folder) read by the real parsers; the database answers come from _DemoCursor;
    a visit to the presentation page is really counted (kept in memory, lost at restart)."""
    folder = tempfile.mkdtemp(prefix="kbot_preview_logs_")
    now = datetime.now()
    sep = "=" * 100

    def stamp(minutes):
        return (now - timedelta(minutes=minutes)).strftime("%Y-%m-%d %H:%M:%S")

    def line(minutes, level, ip, msg):
        return "%s,123 - %s - %s - %s" % (stamp(minutes), level, ip, msg)

    def tag(user, dc):
        return "{s=ab12cd34 u=%s dc=%s} " % (user, dc)

    def write(name, lines):
        with open(os.path.join(folder, name), "w", encoding="utf-8") as out:
            out.write("\n".join(lines) + ("\n" if lines else ""))

    write("api_server.log", [
        line(600, "INFO", "-", "--- SERVER LOGGING INITIALIZED ---"),
        line(120, "INFO", "84.115.209.41", tag("ana@scoala.ro", "111_GRAD") + "[forexe] GET /api/forexe/tree 200"),
        line(118, "WARNING", "84.115.209.41", tag("ana@scoala.ro", "111_GRAD") + "[forexe.receptii] suma antet diferita"),
        line(90, "ERROR", "86.120.4.7", tag("ion@gradinita.ro", "222_SCOALA") + "[forexe.plati] eroare la citire"),
        "Traceback (most recent call last):",
        '  File "routes/forexe/plati.py", line 88, in get_plati',
        "ValueError: invalid literal for int() with base 10: 'x'",
        line(60, "INFO", "86.120.4.7", tag("ion@gradinita.ro", "222_SCOALA") + "[forexe] GET /api/forexe/sumar 200"),
        line(30, "INFO", "203.0.113.9", "AUTH_LOGIN_OK un=mihai@primarie.ro"),
        line(5, "ERROR", "203.0.113.9", "AUTH_LOGIN_FAIL un=x reason=BAD_PASSWORD"),
    ])
    write("api_server_vba.log", [line(45, "INFO", "10.0.0.5", "/api/nomenclatoare/clasificatii 200")])
    write("forexe_timing.log", [
        sep, "%s.120  rulare 17  [GET /api/forexe/tree]  status 200" % stamp(70),
        "  session=ab12cd34 user=ana@scoala.ro dc=111_GRAD", "-" * 100, "   total ms |    sql ms", "      41.0 |      30.0",
        sep, "%s.220  rulare 18  [POST /api/forexe/asociere]  status 500" % stamp(20),
        "  session=ef56ab78 user=ion@gradinita.ro dc=222_SCOALA", "-" * 100, "   total ms |    sql ms", "     310.0 |     120.0",
    ])
    write("asociere.log", [
        sep, "%s.300  rulare 3  [asociere]  status 409  80 ms" % stamp(15),
        "  dc=000_DEMO user=scavatarsoft@gmail.com", "RESPINS F14: lantul nu se inchide",
    ])
    write("schema_sync.log", [])

    admin_mod._files = lambda: {
        "server": ("api_server.log", os.path.join(folder, "api_server.log"), 5, "lines"),
        "vba": ("api_server_vba.log", os.path.join(folder, "api_server_vba.log"), 5, "lines"),
        "timing": ("forexe_timing.log", os.path.join(folder, "forexe_timing.log"), 5, "blocks"),
        "asociere": ("asociere.log", os.path.join(folder, "asociere.log"), 5, "blocks"),
        "schema": ("schema_sync.log", os.path.join(folder, "schema_sync.log"), 5, "lines"),
    }

    visits = [
        {"IdVizita": 1, "Prima": now - timedelta(hours=3), "Ultima": now - timedelta(hours=3), "IP": "86.120.4.7",
         "Tara": "RO", "Mobil": 0, "Bot": 0, "Referrer": "google.com", "Ecran": "1920x1080", "Limba": "ro-RO",
         "UserAgent": "Mozilla/5.0 (Windows NT 10.0) Chrome/126", "DurataSec": 214,
         "Sectiuni": '{"acasa":30,"tutoriale":120,"pentru-contabil":64}'},
        {"IdVizita": 2, "Prima": now - timedelta(hours=26), "Ultima": now - timedelta(hours=26), "IP": "86.120.4.7",
         "Tara": "RO", "Mobil": 1, "Bot": 0, "Referrer": "", "Ecran": "390x844", "Limba": "ro-RO",
         "UserAgent": "Mozilla/5.0 (iPhone; CPU iPhone OS 17) Mobile Safari", "DurataSec": 45,
         "Sectiuni": '{"acasa":40,"extrase":5}'},
        {"IdVizita": 3, "Prima": now - timedelta(days=3), "Ultima": now - timedelta(days=3), "IP": "91.198.174.192",
         "Tara": "DE", "Mobil": 0, "Bot": 0, "Referrer": "", "Ecran": "1366x768", "Limba": "de",
         "UserAgent": "Mozilla/5.0 (X11; Linux) Firefox/127", "DurataSec": 12, "Sectiuni": '{"acasa":12}'},
    ]
    admin_mod._connect = lambda: _DemoConnection(visits)

    def save(ip, agent, data):
        """The real route's job without the database: one row per visit id, the largest total wins."""
        for row in visits:
            if row.get("VisitId") == data["visit"]:
                if data["total"] >= row["DurataSec"]:
                    row["DurataSec"], row["Sectiuni"] = data["total"], json.dumps(data["sections"])
                row["Ultima"] = datetime.now()
                return
        visits.insert(0, {
            "IdVizita": len(visits) + 1, "VisitId": data["visit"], "Prima": datetime.now(), "Ultima": datetime.now(),
            "IP": ip, "Tara": None, "Mobil": 1 if vizite_mod._MOBILE_UA.search(agent) else 0, "Bot": 0,
            "Referrer": data["referrer"], "Ecran": data["screen"], "Limba": data["lang"], "UserAgent": agent,
            "DurataSec": data["total"], "Sectiuni": json.dumps(data["sections"])})

    vizite_mod._save = save
    vizite_mod.MAX_PER_WINDOW = 10000


DEMO_HTML = """<!doctype html><html lang="ro"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Previzualizare K-BOT</title>
<style>body{font:16px/1.6 system-ui,sans-serif;max-width:640px;margin:32px auto;padding:0 16px;color:#16233a}
code{background:#eef3fc;padding:2px 6px;border-radius:4px}li{margin:6px 0}.n{color:#5c6b85;font-size:.9rem}</style></head><body>
<h1>Previzualizare site K-BOT</h1>
<p class="n">Variantă locală, cu date inventate. Nu e serverul real; nimic de aici nu se salvează și nu pleacă pe e-mail.</p>
<ul>
<li><a href="/">Pagina de prezentare</a></li>
<li><a href="/detalii">Cere mai multe detalii</a> (formularul se poate trimite, cererea se aruncă)</li>
<li><a href="/portal">Zona utilizatorilor</a> &mdash; orice e-mail, parola <code>demo</code>, codul <code>123456</code>; cu <code>scavatarsoft@gmail.com</code> apare meniul <b>Administrare</b> (baze, jurnale, vizitatori)</li>
<li><a href="/inregistrare">Înregistrare unitate</a> (doar aspectul; butoanele nu funcționează aici)</li>
</ul>
<p class="n">Documentele din fila «Documente» sunt PDF-urile-exemplu de pe acest calculator, dacă există.</p>
</body></html>"""


def main():
    parser = argparse.ArgumentParser(description="Local preview of the public K-BOT site (made-up data).")
    parser.add_argument("--port", type=int, default=5050)
    parser.add_argument("--lan", action="store_true", help="listen on the local network too (for a phone)")
    args = parser.parse_args()
    host = "0.0.0.0" if args.lan else "127.0.0.1"
    app = build_app()
    print(f"\n  K-BOT site preview (made-up data, nothing is stored or mailed)\n  http://localhost:{args.port}/demo   <- links and demo login\n")
    app.run(host=host, port=args.port, threaded=True, debug=False)


if __name__ == "__main__":
    main()
