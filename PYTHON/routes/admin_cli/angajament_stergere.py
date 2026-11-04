"""Delete ONE angajament for the private KBotAdmin CLI: everything the unit database holds for the code, except the e-invoice (EF_) side.

WRITES. IRREVERSIBLE. ONE unit database, ONE code, one transaction (all or nothing).

What goes (the same picture `fisa_angajament` prints, in child-before-parent order, every step counted):
  FX_Angajamente (+ _Grupe), FX_Indicatori, FX_Istoric, FX_DDF (+ Parteneri, REV, REV_SA/SB/PRT/ATT/ATT_IMG, PDF), FX_Rezervari (+ IMG),
  FX_Receptii_R / _H / _RHR / FX_Receptii (+ IMG), FX_Plati, FX_Operatiuni, FX_ORD (+ ATT/ATT_IMG/DOC/PART/PDF/TBL/TBL_REC),
  FX_PDF_SEMNATURI of those documents, FX_NoteCAB_Corectii and - only when no other correction keeps it - the note itself
  (FX_NoteCAB, its PDF and recipisa), FX_NumberLock, and any other FX_ table that has a CodAngajament column.

What stays:
  * every EF_ table (received invoices, their lines, their links to receptions). The links keep the code and the reception ids
    as plain numbers (no foreign key), so they simply point at nothing until the angajament is downloaded again.
    ONE unavoidable exception: EF_PrimiteAsocieri (invoice <-> DDF, chosen by hand) has a CASCADE foreign key to FX_DDF, so its rows
    for this angajament's DDF go with it. They are written in full to the Jurnal first.
  * Clasificatii, Parteneri and Parteneri_Angajamente* (supplier links keyed by the code), FX_Extrase*, FX_PDF_BUCATI (shared chunks).
  * TZ_Oper rows made from the deleted ordonantare lines are kept; the database sets their IdOrdTbl to NULL (counted in the result).

The typed value is the code; the client asks for a confirmation that names it. Server side the code must exist in FX_Angajamente.
"""
import re

from routes.admin_cli.admin_common import COMMON_DB, AdminError, _exec, _journal, _q, _rows

TITLE = "Stergere angajament"
_RX_COD = re.compile(r"^[A-Za-z0-9_./-]{1,60}$")

# The tables named in the plan below (Jurnal is checked too). All must exist and be InnoDB, or nothing is touched.
PLAN_TABLES = (
    "FX_ORD_ATT_IMG", "FX_ORD_ATT", "FX_ORD_DOC", "FX_ORD_TBL_REC", "FX_ORD_TBL", "FX_ORD_PART", "FX_ORD_PDF", "FX_PDF_SEMNATURI", "FX_ORD",
    "FX_Rezervarii_IMG", "FX_Rezervari", "FX_Receptii_IMG", "FX_Receptii", "FX_Receptii_RHR", "FX_Receptii_H", "FX_Receptii_R",
    "FX_Plati", "FX_Operatiuni", "FX_Istoric",
    "FX_DDF_REV_ATT_IMG", "FX_DDF_REV_ATT", "FX_DDF_PDF", "FX_DDF_REV_PRT", "FX_DDF_REV_SA", "FX_DDF_REV_SB", "FX_DDF_Parteneri",
    "FX_DDF_REV", "FX_DDF",
    "FX_NoteCAB_Corectii", "FX_NoteCAB_Recipisa", "FX_NoteCAB_PDF", "FX_NoteCAB",
    "FX_NumberLock", "FX_Indicatori", "FX_Angajamente_Grupe", "FX_Angajamente",
)


def _valid_code(param):
    cod = (param or "").strip()
    if not _RX_COD.match(cod):
        raise AdminError("Codul angajamentului nu este valid (litere, cifre, _ . / - , pana la 60 de caractere).")
    return cod


def _ids(conn, sql, params=()):
    return [r["v"] for r in _rows(conn, sql, params)]


def _in(column, values):
    """('col IN (%s,%s)', values) or an always-false condition for an empty list."""
    if not values:
        return "0", []
    return "%s IN (%s)" % (column, ",".join(["%s"] * len(values))), list(values)


def _where(*parts):
    """OR of (sql, params) parts."""
    sql = " OR ".join("(%s)" % p[0] for p in parts)
    params = []
    for p in parts:
        params.extend(p[1])
    return sql, params


def _sets(conn, cod):
    """The keys that tie the rows of the code together, read under the lock."""
    s = {}
    s["ddf"] = _ids(conn, "SELECT IDDF AS v FROM FX_DDF WHERE CodAngajament=%s", (cod,))
    c_ddf = _in("IDDF", s["ddf"])
    s["rev"] = _ids(conn, "SELECT IDREV AS v FROM FX_DDF_REV WHERE CodAngajament=%s OR " + c_ddf[0], [cod] + c_ddf[1])
    s["ind"] = _ids(conn, "SELECT CodAI AS v FROM FX_Indicatori WHERE CodAngajament=%s", (cod,))
    s["ist"] = _ids(conn, "SELECT ID AS v FROM FX_Istoric WHERE CodAngajament=%s", (cod,))
    s["rr"] = _ids(conn, "SELECT IDRR AS v FROM FX_Receptii_R WHERE CodAngajament=%s", (cod,))
    c_rr = _in("IDRR", s["rr"])
    s["rh"] = _ids(conn, "SELECT IDRH AS v FROM FX_Receptii_H WHERE CodAngajament=%s OR " + c_rr[0], [cod] + c_rr[1])
    c = _where(("CodAngajament=%s", [cod]), _in("CodAI", s["ind"]), _in("IDH", s["ist"]), _in("IDRH", s["rh"]))
    s["recs"] = _ids(conn, "SELECT IDR AS v FROM FX_Receptii WHERE " + c[0], c[1])
    c = _where(("CodAngajament=%s", [cod]), _in("CodAI", s["ind"]), _in("IDH", s["ist"]))
    s["plati"] = _ids(conn, "SELECT IdPlataFX AS v FROM FX_Plati WHERE " + c[0], c[1])
    c = _where(("CodAngajament=%s", [cod]), _in("CodAI", s["ind"]), _in("IDH", s["ist"]), _in("IDREV", s["rev"]))
    s["rez"] = _ids(conn, "SELECT IDRZ AS v FROM FX_Rezervari WHERE " + c[0], c[1])
    c = _where(("CodAngajament=%s", [cod]), c_ddf)
    s["ordp"] = _ids(conn, "SELECT IDORDP AS v FROM FX_ORD WHERE " + c[0], c[1])
    c = _where(("CodAngajament=%s", [cod]), _in("IDORDP", s["ordp"]), _in("CodAI", s["ind"]))
    s["tbl"] = _ids(conn, "SELECT IDORDTBLP AS v FROM FX_ORD_TBL WHERE " + c[0], c[1])
    c = _in("IDORDP", s["ordp"])
    s["ordatt"] = _ids(conn, "SELECT IDORDATTP AS v FROM FX_ORD_ATT WHERE " + c[0], c[1])
    c = _where(c_ddf, _in("IDREV", s["rev"]))
    s["att"] = _ids(conn, "SELECT IdRevAtt AS v FROM FX_DDF_REV_ATT WHERE " + c[0], c[1])
    s["nc"] = _ids(conn, "SELECT DISTINCT IDNC AS v FROM FX_NoteCAB_Corectii WHERE CodAngajament=%s", (cod,))
    return s


def _steps(cod, s):
    """(table, (sql, params)) in the order they run: children before parents. The note steps are added by `_delete` after the corrections."""
    c_ddf = _in("IDDF", s["ddf"])
    return [
        ("FX_ORD_ATT_IMG", _in("IDORDATTP", s["ordatt"])),
        ("FX_ORD_ATT", _in("IDORDP", s["ordp"])),
        ("FX_ORD_DOC", _in("IDORDP", s["ordp"])),
        ("FX_ORD_TBL_REC", _where(_in("IDORDTBLP", s["tbl"]), _in("IdPlataFX", s["plati"]))),
        ("FX_ORD_TBL", _in("IDORDTBLP", s["tbl"])),
        ("FX_ORD_PART", _in("IDORDP", s["ordp"])),
        ("FX_ORD_PDF", _in("IDORDP", s["ordp"])),
        ("FX_PDF_SEMNATURI", _where(_in("IDORDP", s["ordp"]), _in("IDREV", s["rev"]))),
        ("FX_ORD", _in("IDORDP", s["ordp"])),
        ("FX_Rezervarii_IMG", _in("IDRZ", s["rez"])),
        ("FX_Rezervari", _in("IDRZ", s["rez"])),
        ("FX_Receptii_IMG", _where(_in("IDRR", s["rr"]), _in("IDRH", s["rh"]))),
        ("FX_Receptii", _in("IDR", s["recs"])),
        ("FX_Receptii_RHR", _where(("CodAngajament=%s", [cod]), _in("IDRR", s["rr"]))),
        ("FX_Receptii_H", _in("IDRH", s["rh"])),
        ("FX_Receptii_R", _in("IDRR", s["rr"])),
        ("FX_Plati", _in("IdPlataFX", s["plati"])),
        ("FX_Operatiuni", ("CodAngajament=%s", [cod])),
        ("FX_Istoric", _in("ID", s["ist"])),
        ("FX_DDF_REV_ATT_IMG", _in("IdRevAtt", s["att"])),
        ("FX_DDF_REV_ATT", _in("IdRevAtt", s["att"])),
        ("FX_DDF_PDF", _in("IDREV", s["rev"])),
        ("FX_DDF_REV_PRT", _where(("CodAngajament=%s", [cod]), c_ddf, _in("IDREV", s["rev"]))),
        ("FX_DDF_REV_SA", _where(("CodAngajament=%s", [cod]), c_ddf, _in("IDREV", s["rev"]))),
        ("FX_DDF_REV_SB", _where(("CodAngajament=%s", [cod]), c_ddf, _in("IDREV", s["rev"]))),
        ("FX_DDF_Parteneri", c_ddf),
        ("FX_DDF_REV", _in("IDREV", s["rev"])),
        ("FX_DDF", c_ddf),
        ("FX_NumberLock", ("CodAngajament=%s", [cod])),
        ("FX_Indicatori", ("CodAngajament=%s", [cod])),
        ("FX_Angajamente_Grupe", ("CodAngajament=%s", [cod])),
    ]


def _identity(conn, db, cod):
    """(name, detail line) of the angajament, so the operator sees WHAT is deleted, not only the code: the name the operator gave
    (Alias, else the FOREXE Descriere) plus the object and partner of the first DDF. Columns missing in an older database are skipped."""
    cols = {r["c"] for r in _rows(conn, "SELECT COLUMN_NAME AS c FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=%s AND TABLE_NAME='FX_Angajamente'", (db,))}
    pick = [c for c in ("Alias", "Descriere", "Stare") if c in cols]
    row = _rows(conn, "SELECT " + ",".join(_q(c) for c in pick) + " FROM FX_Angajamente WHERE CodAngajament=%s", (cod,))[0] if pick else {}
    ddf = _rows(conn, "SELECT ObiectDDF,NumePartener FROM FX_DDF WHERE CodAngajament=%s ORDER BY IDDF LIMIT 1", (cod,))
    alias = str(row.get("Alias") or "").strip()
    descriere = str(row.get("Descriere") or "").strip()
    obiect = str(ddf[0]["ObiectDDF"] or "").strip() if ddf else ""
    partener = str(ddf[0]["NumePartener"] or "").strip() if ddf else ""
    name = alias or descriere or obiect or "(fara denumire)"
    parts = ["Denumire (Alias): " + (alias or "-"), "Descriere: " + (descriere or "-"), "Obiect DDF: " + (obiect or "-"),
             "Partener DDF: " + (partener or "-"), "Stare: " + (str(row.get("Stare") or "").strip() or "-")]
    return name, " | ".join(parts)


def _note_steps(conn, cod, s):
    """The CAB notes of the code: a note goes only when no correction of ANOTHER code keeps it.
    Returns (notes that go, notes that stay, [(table, label, condition)] for the note rows in delete order)."""
    c_nc = _in("IDNC", s["nc"])
    shared = set(_ids(conn, "SELECT DISTINCT IDNC AS v FROM FX_NoteCAB_Corectii WHERE CodAngajament<>%s AND " + c_nc[0], [cod] + c_nc[1])) if s["nc"] else set()
    drop_nc = [n for n in s["nc"] if n not in shared]
    c_drop = _in("IDNC", drop_nc)
    pdfs = _ids(conn, "SELECT IDPDF AS v FROM FX_NoteCAB_PDF WHERE " + c_drop[0], c_drop[1]) if drop_nc else []
    steps = [("FX_NoteCAB_Recipisa", "FX_NoteCAB_Recipisa", _in("IDPDF", pdfs)),
             ("FX_PDF_SEMNATURI", "FX_PDF_SEMNATURI (note CAB)", c_drop),
             ("FX_NoteCAB_PDF", "FX_NoteCAB_PDF", c_drop),
             ("FX_NoteCAB", "FX_NoteCAB", c_drop)]
    return drop_nc, sorted(shared), steps


def _check_tables(conn, db):
    wanted = list(PLAN_TABLES)
    engines = {(r["db"], r["table"]): str(r["engine"] or "").upper() for r in _rows(
        conn, "SELECT TABLE_SCHEMA AS db,TABLE_NAME AS `table`,ENGINE AS engine FROM information_schema.TABLES "
              "WHERE (TABLE_SCHEMA=%s AND TABLE_NAME IN (" + ",".join(["%s"] * len(wanted)) + ")) OR (TABLE_SCHEMA=%s AND TABLE_NAME='Jurnal')",
        tuple([db] + wanted + [COMMON_DB]))}
    missing = [t for t in wanted if (db, t) not in engines]
    if missing:
        raise AdminError("Stergerea nu poate rula: lipsesc tabelele " + ", ".join(missing) + ". Nu s-a sters nimic.")
    if any(engines.get(t) != "INNODB" for t in [(db, t) for t in wanted] + [(COMMON_DB, "Jurnal")]):
        raise AdminError("Stergerea cere toate tabelele implicate si Jurnalul in InnoDB, ca totul sa se aplice intr-o singura tranzactie.")


def _other_fx_tables(conn, db):
    """Other FX_ base tables that carry a CodAngajament column and are not in the plan (a table added later): deleted by the code too."""
    return [r["t"] for r in _rows(
        conn, "SELECT DISTINCT C.TABLE_NAME AS t FROM information_schema.COLUMNS C JOIN information_schema.TABLES T "
              "ON T.TABLE_SCHEMA=C.TABLE_SCHEMA AND T.TABLE_NAME=C.TABLE_NAME AND T.TABLE_TYPE='BASE TABLE' "
              "WHERE C.TABLE_SCHEMA=%s AND C.COLUMN_NAME='CodAngajament' AND C.TABLE_NAME LIKE 'FX\\_%' ORDER BY C.TABLE_NAME", (db,))
            if r["t"] not in PLAN_TABLES]


def _count(conn, table, where, params):
    return int(_rows(conn, "SELECT COUNT(*) AS n FROM " + _q(table) + " WHERE " + where, params)[0]["n"])


def _count_if_exists(conn, db, table, where, params):
    """The tables that are only counted (never deleted from) may be missing in an older database: then 0."""
    if not _rows(conn, "SELECT 1 AS v FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s", (db, table)):
        return 0
    return _count(conn, table, where, params)


def delete(conn, db, param, block):
    """WRITES, IRREVERSIBLE. All or nothing. The Jurnal gets the counts and the EF_PrimiteAsocieri rows that cascade away."""
    cod = _valid_code(param)
    _check_tables(conn, db)
    others = _other_fx_tables(conn, db)
    conn.start_transaction(isolation_level="READ COMMITTED")
    try:
        if not _rows(conn, "SELECT CodAngajament FROM FX_Angajamente WHERE CodAngajament=%s FOR UPDATE", (cod,)):
            raise AdminError("Angajamentul «%s» nu exista in FX_Angajamente, baza %s. Nu s-a sters nimic." % (cod, db))
        s = _sets(conn, cod)
        name, identity = _identity(conn, db, cod)

        # what the database does on its own, counted before: the invoice <-> DDF links (CASCADE) and the accounting operations (SET NULL)
        c_ddf = _in("IDDF", s["ddf"])
        has_asoc = bool(_rows(conn, "SELECT 1 AS v FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_NAME='EF_PrimiteAsocieri'", (db,)))
        asocieri = _rows(conn, "SELECT * FROM EF_PrimiteAsocieri WHERE " + c_ddf[0], c_ddf[1]) if (s["ddf"] and has_asoc) else []
        c_tbl = _in("IdOrdTbl", s["tbl"])
        oper_free = _count_if_exists(conn, db, "TZ_Oper", c_tbl[0], c_tbl[1]) if s["tbl"] else 0
        ef_receptii = _count_if_exists(conn, db, "EF_PrimiteReceptii", "CodAngajament=%s", (cod,))
        parteneri_leg = _count_if_exists(conn, db, "Parteneri_Angajamente", "CodAng=%s", (cod,))

        removed = []

        def run(table, cond):
            n = _exec(conn, "DELETE FROM " + _q(table) + " WHERE " + cond[0], cond[1]) if cond[0] != "0" else 0
            removed.append((table, n))

        steps = _steps(cod, s)
        drop_nc, kept_notes, note_steps = _note_steps(conn, cod, s)
        # the corrections first: their foreign key to FX_Angajamente is RESTRICT
        run("FX_NoteCAB_Corectii", ("CodAngajament=%s", [cod]))
        for table, _label, cond in note_steps:
            run(table, cond)

        for table, cond in steps:
            run(table, cond)
        for table in others:
            run(table, ("CodAngajament=%s", [cod]))
        if _exec(conn, "DELETE FROM FX_Angajamente WHERE CodAngajament=%s", (cod,)) != 1:
            raise AdminError("Datele s-au schimbat in timpul stergerii. Nu s-a sters nimic; reluati comanda.")
        removed.append(("FX_Angajamente", 1))

        # the guard: nothing may be left that still carries the code in an FX_ table
        left = []
        for table in list(PLAN_TABLES) + others:
            has = _rows(conn, "SELECT 1 AS v FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s AND COLUMN_NAME='CodAngajament'", (db, table))
            if has:
                n = _count(conn, table, "CodAngajament=%s", (cod,))
                if n:
                    left.append("%s (%d)" % (table, n))
        if left:
            raise AdminError("Dupa stergere au ramas randuri cu codul in: %s. Nu s-a sters nimic." % ", ".join(left))

        total = sum(n for _t, n in removed)
        details = ("angajament %s (%s) sters din %s: %d randuri in %d tabele (%s). EF_ neatinse, cu o exceptie: %d legaturi factura-DDF (EF_PrimiteAsocieri) "
                   "au plecat odata cu FX_DDF%s; %d operatii TZ_Oper au ramas fara legatura la ordonantare; EF_PrimiteReceptii cu codul ramas: %d; "
                   "Parteneri_Angajamente cu codul ramas: %d; note CAB ramase (au alte corectii): %s."
                   % (cod, identity, db, total, sum(1 for _t, n in removed if n), ", ".join("%s %d" % (t, n) for t, n in removed if n), len(asocieri),
                      (": " + " | ".join(str(dict(a)) for a in asocieri)) if asocieri else "", oper_free, ef_receptii, parteneri_leg,
                      ", ".join(map(str, kept_notes)) or "niciuna"))
        _journal(conn, "angajament_stergere", db, cod, details[:60000])
        conn.commit()
    except Exception:
        conn.rollback()
        raise

    rows = [[t, str(n)] for t, n in removed if n]
    notes = ["TZ_Oper: %d operatii contabile au ramas, fara legatura la linia de ordonantare stearsa." % oper_free,
             "EF_PrimiteReceptii cu acest cod (neatinse, arata acum spre nimic): %d." % ef_receptii,
             "Parteneri_Angajamente cu acest cod (neatinse): %d." % parteneri_leg]
    if asocieri:
        notes.append("Legaturi factura-DDF (EF_PrimiteAsocieri) pierdute odata cu DDF-ul: %d (sunt in Jurnal)." % len(asocieri))
    if kept_notes:
        notes.append("Notele CAB %s au ramas: au corectii si pe alte angajamente." % ", ".join(map(str, kept_notes)))
    summary = block(db, TITLE, ["rezultat"],
                    [["Angajamentul %s - %s - a fost sters din baza %s: %d randuri in %d tabele." % (cod, name, db, total, len(rows))], [identity]] + [[n] for n in notes],
                    "Nu se poate anula. Randurile EF_ nu au fost atinse, cu exceptia legaturilor factura-DDF de mai sus. Jurnalul are numarul de randuri pe tabela.",
                    raw=True)
    detail = block(db, TITLE + " - pe tabele", ["tabela", "randuri sterse"], rows)
    return [summary, detail]


def analyze(conn, db, param, block):
    """READ ONLY. What `delete` would remove for the code, table by table, and what it would leave. Changes nothing."""
    cod = _valid_code(param)
    _check_tables(conn, db)
    if not _rows(conn, "SELECT 1 AS v FROM FX_Angajamente WHERE CodAngajament=%s", (cod,)):
        raise AdminError("Angajamentul «%s» nu exista in FX_Angajamente, baza %s." % (cod, db))
    s = _sets(conn, cod)
    name, identity = _identity(conn, db, cod)
    drop_nc, kept_notes, note_steps = _note_steps(conn, cod, s)

    def counted(table, cond):
        if cond[0] == "0":
            return 0
        return _count(conn, table, cond[0], cond[1])

    rows = [("FX_NoteCAB_Corectii", counted("FX_NoteCAB_Corectii", ("CodAngajament=%s", [cod])))]
    rows += [(label, counted(table, cond)) for table, label, cond in note_steps]
    rows += [(table, counted(table, cond)) for table, cond in _steps(cod, s)]
    rows += [(table, counted(table, ("CodAngajament=%s", [cod]))) for table in _other_fx_tables(conn, db)]
    rows.append(("FX_Angajamente", 1))
    total = sum(n for _t, n in rows)
    shown = [[t, str(n)] for t, n in rows if n]

    c_ddf = _in("IDDF", s["ddf"])
    has_asoc = bool(_rows(conn, "SELECT 1 AS v FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_NAME='EF_PrimiteAsocieri'", (db,)))
    asocieri = _count(conn, "EF_PrimiteAsocieri", c_ddf[0], c_ddf[1]) if (s["ddf"] and has_asoc) else 0
    c_tbl = _in("IdOrdTbl", s["tbl"])
    oper_free = _count_if_exists(conn, db, "TZ_Oper", c_tbl[0], c_tbl[1]) if s["tbl"] else 0
    ef_receptii = _count_if_exists(conn, db, "EF_PrimiteReceptii", "CodAngajament=%s", (cod,))
    ef_factura = 0
    if ef_receptii:
        ef_factura = int(_rows(conn, "SELECT COUNT(DISTINCT IdPrimita) AS n FROM EF_PrimiteReceptii WHERE CodAngajament=%s", (cod,))[0]["n"])
    parteneri_leg = _count_if_exists(conn, db, "Parteneri_Angajamente", "CodAng=%s", (cod,))

    summary = block(db, TITLE + " - analiza", ["cod angajament", "denumire", "randuri de sters", "tabele"],
                    [[cod, name, total, len(shown)]],
                    "Analiza nu a schimbat nimic. Daca se potriveste cu ce vrei, butonul «Sterge angajamentul» sterge exact acestea, intr-o singura tranzactie.")
    attention = [
        "ANGAJAMENT: %s - %s" % (cod, name),
        identity,
        "SE STERG %d randuri din %d tabele FX_ (lista de mai jos)." % (total, len(shown)),
        "EF_ NU se ating: facturi primite legate de acest cod prin EF_PrimiteReceptii: %d factura(i), %d legaturi; ele raman si arata spre nimic pana la o noua descarcare." % (ef_factura, ef_receptii),
        ("EXCEPTIE: %d legaturi factura-DDF alese de mana (EF_PrimiteAsocieri) pleaca odata cu DDF-ul (cheie CASCADE); se scriu integral in Jurnal." % asocieri)
        if asocieri else "Nicio legatura factura-DDF de mana (EF_PrimiteAsocieri) nu este afectata.",
        "TZ_Oper: %d operatii contabile raman, dar pierd legatura la linia de ordonantare stearsa." % oper_free if oper_free else "TZ_Oper: nicio operatie contabila nu e legata de ordonantarile acestui cod.",
        ("Note CAB care RAMAN (au corectii si pe alte angajamente): %s." % ", ".join(map(str, kept_notes))) if kept_notes
        else ("Note CAB care se sterg odata cu corectiile: %d." % len(drop_nc)),
        "Raman: Clasificatii, Parteneri, Parteneri_Angajamente (%d legaturi cu acest cod), FX_Extrase*, FX_PDF_BUCATI." % parteneri_leg,
    ]
    return [summary,
            block(db, TITLE + " - ce se intampla", ["observatie"], [[a] for a in attention], raw=True),
            block(db, TITLE + " - pe tabele", ["tabela", "randuri de sters"], shown)]
