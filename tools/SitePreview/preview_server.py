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
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, "..", ".."))
PYTHON_DIR = os.path.join(REPO, "PYTHON")
sys.path.insert(0, PYTHON_DIR)

from flask import Flask, jsonify, request, send_file, send_from_directory  # noqa: E402

from routes.detalii import detalii as detalii_mod  # noqa: E402
from routes.landing.landing import landing_bp  # noqa: E402
from routes.portal import portal as portal_mod  # noqa: E402
import routes.portal.date as date_mod  # noqa: E402  (registers /api/portal/date/* on portal_bp)

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

    app.register_blueprint(landing_bp)
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
            "descriere_r": "Recepție factură %d" % i, "data_h": "2026-%02d-06T09:30:00" % (1 + i % 4),
            "clsf": "65.02.04.01", "denumire": "Alte cheltuieli de funcționare", "cod_indicator": "IND1",
            "valoare": 750.0 * i, "dif": 750.0 * i, "este_stergere": False} for i in range(1, 9)]})

    def plati():
        return jsonify({"cod": request.args.get("cod"), "plati": [{
            "data_plata": "2026-%02d-15" % (1 + i % 6), "nr_op": "OP%d" % (100 + i), "clsf": "65.02.04.01",
            "denumire": "Alte cheltuieli de funcționare", "cod_indicator": "IND1", "suma": 333.33 * i, "are_ord": i % 2 == 0,
            "platitor_nume": "Primăria", "nr_doc_extras": "D%d" % i, "explicatii": "Plată factură %d" % i} for i in range(1, 9)]})

    def ddf():
        return jsonify({"cod": request.args.get("cod"), "revizii": [
            {"idrev": 1, "numar_rev": 1, "data_rev": "2026-02-17", "total_revizie": 12500.0, "semnatura": "A,B,Ordonator", "pdf_sha256": "x"},
            {"idrev": 2, "numar_rev": 2, "data_rev": "2026-03-02", "total_revizie": 13000.0, "semnatura": "", "pdf_sha256": None}]})

    def ordonantari():
        return jsonify({"cod": request.args.get("cod"), "ordonantari": [
            {"idordp": 7, "nr_ord": 1, "data_ord": "2026-09-30", "total_ord": 1.0, "semnatura": "AB,CD,Ordonator", "pdf_sha256": "y"}]})

    def note():
        return jsonify({"note": [{"idnc": 3, "nr_nota": 23, "data_nota": "2026-09-28", "semnatura": "S1,S2", "pdf_sha256": "z"}]})

    date_mod._READERS.update({"tree": tree, "sumar": sumar, "rezervari": rezervari, "receptii": receptii,
                              "plati": plati, "ddf": ddf, "ord": ordonantari, "note": note})

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


DEMO_HTML = """<!doctype html><html lang="ro"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Previzualizare K-BOT</title>
<style>body{font:16px/1.6 system-ui,sans-serif;max-width:640px;margin:32px auto;padding:0 16px;color:#16233a}
code{background:#eef3fc;padding:2px 6px;border-radius:4px}li{margin:6px 0}.n{color:#5c6b85;font-size:.9rem}</style></head><body>
<h1>Previzualizare site K-BOT</h1>
<p class="n">Variantă locală, cu date inventate. Nu e serverul real; nimic de aici nu se salvează și nu pleacă pe e-mail.</p>
<ul>
<li><a href="/">Pagina de prezentare</a></li>
<li><a href="/detalii">Cere mai multe detalii</a> (formularul se poate trimite, cererea se aruncă)</li>
<li><a href="/portal">Zona utilizatorilor</a> &mdash; orice e-mail, parola <code>demo</code>, codul <code>123456</code></li>
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
