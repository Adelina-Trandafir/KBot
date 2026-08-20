# routes/migrare/migrare.py
# -----------------------------------------------------------------------------
# The routes KBot.Migrator drives, in the order the operator meets them:
#
#   GET  /api/migrare/baze          -> bazele de unitate de pe MariaDB
#   GET  /api/migrare/fisiere       -> ce fișiere Access sunt deja pe server
#   POST /api/migrare/push/init     -> deschide o încărcare în bucăți
#   POST /api/migrare/push/bucata   -> o bucată (multipart), cu amprentă
#   POST /api/migrare/push/final    -> lipește, verifică amprenta, mută în loc
#   POST /api/migrare/analiza       -> pornește analiza; întoarce un id de lucrare
#   POST /api/migrare/rulare        -> pornește scrierea; cere id-ul analizei
#   GET  /api/migrare/stare/<id>    -> starea unei lucrări + jurnalul ei
#
# Garda e X-Api-Key, ca pe rutele de seed pe care le înlocuiește. Migratorul este
# un utilitar de administrare, nu aplicația operatorului: nu are token bearer.
# -----------------------------------------------------------------------------

import json
import logging
import re

from flask import Blueprint, Response, request

from utils.database import get_db_connection
from utils.security import require_api_key

from . import accdb, execute, jobs, routing, storage, tables, validate

migrare_bp = Blueprint("migrare", __name__)
logger = logging.getLogger(__name__)

# Numele unei baze de unitate. Aceeasi forma ca in routes/forexe/seed.py.
_DBNAME_RE = re.compile(r"^[0-9]{3}_[A-Za-z0-9]+$")

# Bazele de serviciu, care nu sunt baze de unitate si nu se ofera ca tinta.
_NOT_UNIT_DBS = {"information_schema", "mysql", "performance_schema", "sys",
                 "AVACONT_COMUN", "AVACONT_SURSA"}


def _json(payload, status=200):
    return Response(json.dumps(payload, ensure_ascii=False), status=status,
                    mimetype="application/json; charset=utf-8")


def _err(message, status=400):
    return _json({"ok": False, "error": message}, status)


def _fail(where, exc):
    """Nicio excepție nu se pierde: jurnal cu urmă completă, mesaj în română afară."""
    logger.exception("migrare/%s a eșuat", where)
    return _err(str(exc), 500)


# -----------------------------------------------------------------------------
# 1. Bazele de pe MariaDB
# -----------------------------------------------------------------------------

@migrare_bp.route("/api/migrare/baze", methods=["GET"])
@require_api_key
def baze():
    """
    Bazele de unitate existente pe server, cu numarul de tabele FX_ pe care le au
    deja. Numaratoarea e acolo ca operatorul sa vada dintr-o privire daca schema
    e instalata -- migrarea NU creeaza tabele.
    """
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        cur.execute("SHOW DATABASES")
        names = []
        for (name,) in cur.fetchall():
            text = name if isinstance(name, str) else name.decode("utf-8", "replace")
            if text in _NOT_UNIT_DBS or not _DBNAME_RE.match(text):
                continue
            names.append(text)

        wanted = [t.name for t in tables.ALL]
        out = []
        if names:
            placeholders = ",".join(["%s"] * len(names))
            table_slots = ",".join(["%s"] * len(wanted))
            cur.execute(
                "SELECT TABLE_SCHEMA, COUNT(*) FROM information_schema.TABLES "
                " WHERE TABLE_SCHEMA IN (" + placeholders + ") "
                "   AND TABLE_NAME IN (" + table_slots + ") "
                " GROUP BY TABLE_SCHEMA",
                tuple(names + wanted))
            counts = dict((k if isinstance(k, str) else k.decode("utf-8", "replace"), int(v))
                          for k, v in cur.fetchall())
            for name in sorted(names):
                out.append({
                    "nume": name,
                    "tabele_fx": counts.get(name, 0),
                    "complet": counts.get(name, 0) == len(wanted),
                })
        cur.close()
        return _json({"ok": True, "baze": out, "tabele_așteptate": len(wanted)})

    except Exception as exc:
        return _fail("baze", exc)
    finally:
        if conn is not None:
            conn.close()


# -----------------------------------------------------------------------------
# 2. Fisierele Access de pe server
# -----------------------------------------------------------------------------

@migrare_bp.route("/api/migrare/fisiere", methods=["GET"])
@require_api_key
def fisiere():
    try:
        return _json({"ok": True, "fișiere": storage.list_pushed()})
    except storage.StorageError as exc:
        return _err(str(exc), 400)
    except Exception as exc:
        return _fail("fisiere", exc)


# -----------------------------------------------------------------------------
# 3. Impingerea fisierului, in bucati
# -----------------------------------------------------------------------------

@migrare_bp.route("/api/migrare/push/init", methods=["POST"])
@require_api_key
def push_init():
    body = request.get_json(silent=True) or {}
    try:
        upload_id, name = storage.begin_upload(
            body.get("fel"), body.get("an"), body.get("dc"),
            body.get("octeți") or body.get("total_size"),
            body.get("sha256"))
        return _json({"ok": True, "id": upload_id, "nume": name,
                      "bucată_maximă": storage.MAX_CHUNK_BYTES})
    except storage.StorageError as exc:
        return _err(str(exc), 400)
    except Exception as exc:
        return _fail("push/init", exc)


@migrare_bp.route("/api/migrare/push/bucata", methods=["POST"])
@require_api_key
def push_bucata():
    try:
        upload_id = (request.form.get("id") or "").strip()
        index = request.form.get("index")
        sha = request.form.get("sha256")
        if "fișier" in request.files:
            payload = request.files["fișier"].read()
        elif "file" in request.files:
            payload = request.files["file"].read()
        else:
            return _err("Bucata de fișier lipsește din cerere.", 400)

        storage.store_chunk(upload_id, index, payload, sha)
        return _json({"ok": True})
    except storage.StorageError as exc:
        return _err(str(exc), 400)
    except Exception as exc:
        return _fail("push/bucata", exc)


@migrare_bp.route("/api/migrare/push/final", methods=["POST"])
@require_api_key
def push_final():
    body = request.get_json(silent=True) or {}
    try:
        info = storage.finish_upload(body.get("id"), body.get("bucăți") or body.get("total_chunks"))
        return _json({"ok": True, "fișier": info})
    except storage.StorageError as exc:
        return _err(str(exc), 400)
    except Exception as exc:
        return _fail("push/final", exc)


# -----------------------------------------------------------------------------
# 4. Analiza -- citeste, ruteaza, masoara. Nu scrie nimic.
# -----------------------------------------------------------------------------

@migrare_bp.route("/api/migrare/analiza", methods=["POST"])
@require_api_key
def analiza():
    body = request.get_json(silent=True) or {}
    db_name = body.get("baza")
    an = body.get("an")
    dc = body.get("dc") or db_name

    if not db_name or not _DBNAME_RE.match(db_name):
        return _err("Numele bazei de unitate este invalid: «%s»." % (db_name or ""), 400)

    try:
        accdb.ensure_tools()
        fx_path = storage.pushed_path(storage.fx_file_name(an, dc))
        cai_path = storage.pushed_path(storage.cai_file_name())
    except (storage.StorageError, accdb.AccdbError) as exc:
        return _err(str(exc), 400)
    except Exception as exc:
        return _fail("analiza/pregătire", exc)

    def work(job):
        conn = None
        try:
            job.say("Se citește «%s»." % fx_path)
            present = accdb.list_tables(fx_path)
            lipsă = [t.name for t in tables.ALL if t.name not in present]
            if lipsă:
                raise validate.ValidationError(
                    "Fișierul Access nu conține tabelele: %s." % ", ".join(lipsă))
            afară = [t for t in tables.OUT_OF_SCOPE if t in present]
            if afară:
                job.say("ATENȚIE: tabele în afara domeniului există în fișier și NU se "
                        "migrează: %s." % ", ".join(afară))

            # Cate unitati are fisierul decide daca mai e nevoie de rutare. Una
            # singura -> totul merge in baza aleasa, si cale.accdb nici nu se
            # atinge. Mai multe -> se ruteaza prin [Cai], iar daca acela lipseste
            # rularea se OPRESTE cu unitatile numite; nu se cade inapoi pe „totul
            # in baza aleasa", fiindca exact asa ar intra tacut randurile altei
            # unitati in baza asta.
            plan = routing.resolve_plan(fx_path, cai_path, db_name, progress=job.say)
            job.plan = plan
            if plan.mode == routing.RoutingPlan.PRIN_CAI:
                if db_name not in plan.maps.all_dcs():
                    job.say("ATENȚIE: baza «%s» nu apare deloc în [Cai]. Se vor găsi "
                            "doar rândurile care poartă chiar ele DC-ul." % db_name)

            conn = get_db_connection(db_name)
            report = validate.analyze(conn, db_name, fx_path, plan, progress=job.say)
            job.report = report

            data = report.to_dict()
            job.say("Analiză încheiată: %s." %
                    ("nicio problemă" if data["curat"] else
                     "%d feluri de constatări" % len(data["pe_fel"])))
            return data
        finally:
            if conn is not None:
                conn.close()

    job = jobs.start("analiză", work)
    return _json({"ok": True, "lucrare": job.id})


# -----------------------------------------------------------------------------
# 5. Rularea -- scrie. Cere id-ul analizei care a aprobat-o.
# -----------------------------------------------------------------------------

@migrare_bp.route("/api/migrare/rulare", methods=["POST"])
@require_api_key
def rulare():
    body = request.get_json(silent=True) or {}
    analiza_id = body.get("analiză") or body.get("analiza")
    force = bool(body.get("forțat") or body.get("force"))

    sursa = jobs.get(analiza_id) if analiza_id else None
    if sursa is None or sursa.report is None:
        return _err(
            "Analiza indicată nu mai există pe server. Rulați din nou analiza "
            "înainte de scriere.", 400)
    if sursa.state != jobs.GATA:
        return _err("Analiza indicată nu s-a încheiat cu bine.", 400)

    report = sursa.report
    db_name = report.db_name
    an = body.get("an")
    dc = body.get("dc") or db_name

    if report.has_blocking():
        return _err(
            "Analiza a găsit probleme blocante (tip, dimensiune, coloană sau tabel "
            "lipsă). Nici «Forțează rularea» nu trece peste ele.", 409)
    if not force and not report.is_clean():
        return _err(
            "Analiza a găsit probleme de integritate. Folosiți «Forțează rularea» "
            "dacă acceptați ca rândurile vinovate să fie sărite.", 409)

    # Planul de rutare vine de la analiza, nu se rezolva din nou: altfel ramura
    # aleasa s-ar putea schimba intre masurare si scriere.
    plan = sursa.plan
    if plan is None:
        return _err(
            "Analiza indicată nu a lăsat un plan de rutare. Rulați din nou analiza "
            "înainte de scriere.", 400)

    try:
        fx_path = storage.pushed_path(storage.fx_file_name(an, dc))
    except storage.StorageError as exc:
        return _err(str(exc), 400)

    def work(job):
        conn = None
        try:
            job.say(plan.describe())
            conn = get_db_connection(db_name)
            totals = execute.run(conn, db_name, fx_path, plan, report, force,
                                 progress=job.say)
            scrise = sum(s["scrise"] for s in totals.values())
            sărite = sum(s["sărite"] for s in totals.values())
            job.say("Scriere încheiată: %d rânduri scrise, %d sărite." % (scrise, sărite))
            return {"baza": db_name, "forțat": force, "pe_tabel": totals,
                    "scrise": scrise, "sărite": sărite}
        finally:
            if conn is not None:
                conn.close()

    job = jobs.start("scriere", work)
    return _json({"ok": True, "lucrare": job.id})


# -----------------------------------------------------------------------------
# 6. Starea unei lucrari
# -----------------------------------------------------------------------------

@migrare_bp.route("/api/migrare/stare/<job_id>", methods=["GET"])
@require_api_key
def stare(job_id):
    job = jobs.get(job_id)
    if job is None:
        return _err("Lucrarea «%s» nu există sau a expirat." % job_id, 404)
    try:
        since = int(request.args.get("de_la", 0))
    except (TypeError, ValueError):
        since = 0
    return _json({"ok": True, "lucrare": job.snapshot(since)})
