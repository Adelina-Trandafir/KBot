"""KBotAdmin commands `adechit_plata_analiza` (read only) and `adechit_plata_muta` (WRITES) - SLICE-ME05-01.

Moves ONE whole payment (a receipt, TIP 2, or an "other payment", TIP 1) from one child to another child of the SAME subunit and
the SAME month. The payment row itself is kept: its receipt / document, its number and its date stay exactly as issued; only the
child (IDP), the month row of that child (IDZ) and the payer (IDS) change.

Typed value (both commands): `IDPL;tinta` or `IDPL;tinta;IDS` (KBotAdmin builds it from its two boxes)
  IDPL        the payment (column IDPL of the block "Plati si incasari" of `adechit_copil`; IDPL is unique in the database)
  tinta       the child who receives it, in the same subunit: his CNP (13 digits) or his ID (IDP, 1 to 9 digits)
  IDS         the payer of the target child; optional when he has exactly one active payer

Everything is done with the code the web application uses, so the saved monthly statement (AD_SS_Buget) is rebuilt by the same rules:
  * the target child gets a month row with zero days when he has none (bank_targets.ensure_payment_row, which also stores his
    statement when the month is closed);
  * the month rule of the application is applied (service.temporal): a closed month is touched only when it is the one right before
    the open month;
  * when the month is closed, service.rewrite_snapshot rebuilds the statement of BOTH children from their payments (Plata, Plati,
    Retur, Compensare, Anticipat, SFD, SFC, Detalii); an open month is calculated live, nothing is stored for it.

Refused (nothing is written): payment cancelled; payment tied to a bank statement line (AD_EXTRASE_ASOCIERI keeps child, payer and
month of its own); target = same child; target not found / found twice in the subunit; payer not active or not the target's;
a month that is too old; a missing or doubled statement row.

One transaction, under the same subunit locks the web application takes; the Jurnal gets the whole before / after picture.
"""
import re

from routes.admin_cli.admin_common import AdminError, _journal, _rows
from routes.adechit import bank_targets, service
from routes.adechit.domain import DomainError
from routes.adechit.repository import Repository

TITLE = "Mutare plata intre copii"
_RX_ID = re.compile(r"^[0-9]{1,9}$")
_RX_CNP13 = re.compile(r"^[0-9]{13}$")


def _parse(param):
    parts = [p.strip() for p in (param or "").split(";")]
    valid = len(parts) in (2, 3) and bool(_RX_ID.match(parts[0])) and bool(_RX_CNP13.match(parts[1]) or _RX_ID.match(parts[1]))
    if valid and len(parts) == 3:
        valid = bool(_RX_ID.match(parts[2]))
    if not valid:
        raise AdminError("Scrie IDPL-ul platii, apoi CNP-ul (13 cifre) sau ID-ul (pana la 9 cifre) al copilului tinta, optional si IDS-ul platitorului.")
    # 13 digits = CNP; shorter = the child's ID (IDP)
    target = ("cnp", parts[1]) if _RX_CNP13.match(parts[1]) else ("idp", int(parts[1]))
    return int(parts[0]), target, int(parts[2]) if len(parts) == 3 else None


def _check_database(conn, db):
    have = {r["n"] for r in _rows(conn, "SELECT TABLE_NAME AS n FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s AND TABLE_NAME IN "
                                        "('AD_Platitori','AD_Plati','AD_Prezenta','AD_SS_Buget','AD_LunaD','AD_Platitori_sub')", (db,))}
    if len(have) < 6:
        raise AdminError("Baza «%s» nu este o baza ADECHIT (lipsesc tabelele AD_)." % db)


def _subunit_of(conn, idpl):
    rows = _rows(conn, "SELECT SubunitId FROM AD_Plati WHERE IDPL=%s", (idpl,))
    if len(rows) != 1:
        raise AdminError("Plata %d nu exista in baza aleasa." % idpl)
    return int(rows[0]["SubunitId"])


def _plan(repo, idpl, ref, ids):
    """(facts, blockers): what the move would do, read under the repository's subunit. Writes nothing."""
    blockers = []
    payment = repo.get("Plati", idpl)
    month = repo.get("LunaD", payment["IDL"])
    source = repo.get("Platitori", payment["IDP"])
    source_att = repo.get("Prezenta", payment["IDZ"])
    if payment.get("Anulata"):
        blockers.append("Plata este anulata.")
    if repo.query("SELECT 1 AS v FROM AD_EXTRASE_ASOCIERI WHERE SUBUNIT_ID=%s AND IDPL=%s LIMIT 1", (repo.scope(), idpl)):
        blockers.append("Plata vine dintr-un extras de banca (AD_EXTRASE_ASOCIERI); asocierea isi tine singura copilul, platitorul si luna. "
                        "Nu se muta de aici.")
    kind, value = ref
    label = "CNP-ul %s" % value if kind == "cnp" else "ID-ul %d" % value
    if kind == "cnp":
        targets = repo.query("SELECT * FROM AD_Platitori WHERE SubunitId=%s AND TRIM(CNP)=%s", (repo.scope(), value))
    else:
        targets = repo.query("SELECT * FROM AD_Platitori WHERE SubunitId=%s AND IDP=%s", (repo.scope(), value))
    target = targets[0] if len(targets) == 1 else None
    if not targets:
        blockers.append("Niciun copil cu %s in subunitatea platii." % label)
    elif len(targets) > 1:
        blockers.append("%s apare la %d copii din aceasta subunitate (IDP %s); nu se alege singur."
                        % (label.capitalize(), len(targets), ", ".join(str(t["IDP"]) for t in targets)))
    payer = None
    target_att = None
    if target is not None:
        if target["IDP"] == source["IDP"]:
            blockers.append("Copilul tinta este chiar copilul de la care pleaca plata.")
        active = [p for p in repo.query("SELECT * FROM AD_Platitori_sub WHERE SubunitId=%s AND IDP=%s", (repo.scope(), target["IDP"]))
                  if p.get("Activ")]
        if ids is None:
            if len(active) == 1:
                payer = active[0]
            else:
                blockers.append("Copilul tinta are %d platitori activi; scrie IDS-ul ales ca al treilea element (IDS: %s)."
                                % (len(active), ", ".join("%s %s" % (p["IDS"], p["Nume"]) for p in active) or "niciunul"))
        else:
            payer = next((p for p in active if p["IDS"] == ids), None)
            if payer is None:
                blockers.append("IDS %d nu este un platitor activ al copilului tinta." % ids)
        atts = repo.query("SELECT * FROM AD_Prezenta WHERE SubunitId=%s AND IDP=%s AND IDL=%s", (repo.scope(), target["IDP"], month["IDL"]))
        if len(atts) > 1:
            blockers.append("Copilul tinta are %d randuri de prezenta in aceasta luna." % len(atts))
        target_att = atts[0] if len(atts) == 1 else None
    try:
        service.temporal(repo, month["IDL"])
    except DomainError as error:
        blockers.append(str(error))
    if month["Inchisa"]:
        counts = [("copilul de la care pleaca", source_att["IDZ"])]
        if target_att is not None:
            counts.append(("copilul tinta", target_att["IDZ"]))
        for who, idz in counts:
            n = len(repo.query("SELECT ID FROM AD_SS_Buget WHERE SubunitId=%s AND IDZ=%s", (repo.scope(), idz)))
            if n != 1:
                blockers.append("Situatia salvata a lunii pentru %s lipseste sau este dublata (%d randuri)." % (who, n))
    documents = repo.query("SELECT CONCAT(IFNULL(Serie,''),' ',Numar) AS d FROM AD_Chitante WHERE SubunitId=%s AND IDPL=%s", (repo.scope(), idpl))
    documents += [{"d": "document " + (r["NrDoc"] or "")} for r in
                  repo.query("SELECT NrDoc FROM AD_AlteDoc WHERE SubunitId=%s AND IDPL=%s", (repo.scope(), idpl))]
    return {"payment": payment, "month": month, "source": source, "source_att": source_att, "target": target, "target_att": target_att,
            "payer": payer, "documents": [d["d"].strip() for d in documents]}, blockers


def _facts(db, facts, blockers, block, closing):
    p, m = facts["payment"], facts["month"]
    t, payer = facts["target"], facts["payer"]
    rows = [["Plata", "IDPL %d, %s lei, %s, data %s" % (p["IDPL"], p["Plata"], "incasare cu chitanta (TIP 2)" if p.get("TIP") == 2 else "alta plata (TIP %s)" % p.get("TIP"), p["Data"])],
            ["Documente (raman neschimbate)", "; ".join(facts["documents"]) or "niciunul"],
            ["Luna", "%s-%02d, %s" % (m["Anul"], m["Luna"], "INCHISA: situatia salvata se reface" if m["Inchisa"] else "deschisa: se calculeaza la citire")],
            ["De la copilul", "%s (IDP %d, prezenta IDZ %d)" % (facts["source"]["Nume"], facts["source"]["IDP"], facts["source_att"]["IDZ"])],
            ["Platitor actual", "IDS %s" % p.get("IDS")]]
    if t is not None:
        rows.append(["La copilul", "%s (IDP %d, %s)" % (t["Nume"], t["IDP"], ("prezenta IDZ %d" % facts["target_att"]["IDZ"]) if facts["target_att"]
                                                         else "nu are prezenta in luna: se creeaza una cu zero zile")])
    if payer is not None:
        rows.append(["Platitor nou", "IDS %d, %s" % (payer["IDS"], payer["Nume"])])
    if blockers:
        rows += [["NU SE POATE MUTA", b] for b in blockers]
    note = "Nu se scrie nimic." if not closing else ""
    return block(db, TITLE + (" - analiza" if not closing else ""), ["element", "valoare"], rows, note, raw=True)


def _open(conn, db, param):
    idpl, ref, ids = _parse(param)
    _check_database(conn, db)
    sub = _subunit_of(conn, idpl)
    repo = Repository(conn)
    repo.subunit_id = sub
    return repo, idpl, ref, ids, sub


def analyse(conn, db, param, block):
    """Read only: the picture of the move, or the reasons it cannot be done."""
    repo, idpl, ref, ids, _sub = _open(conn, db, param)
    try:
        facts, blockers = _plan(repo, idpl, ref, ids)
    except DomainError as error:
        raise AdminError(str(error)) from error
    finally:
        repo.cursor.close()
    return [_facts(db, facts, blockers, block, False)]


def move(conn, db, param, block):
    """WRITES. All or nothing; refuses on any blocker the analysis shows."""
    repo, idpl, ref, ids, sub = _open(conn, db, param)
    conn.start_transaction(isolation_level="READ COMMITTED")
    try:
        repo.acquire_lock(0)
        repo.acquire_lock(sub)
        facts, blockers = _plan(repo, idpl, ref, ids)      # read again under the locks
        if blockers:
            raise AdminError("Plata nu se muta: " + " ".join(blockers) + " Nu s-a scris nimic.")
        payment, month = facts["payment"], facts["month"]
        target, payer = facts["target"], facts["payer"]
        target_att = bank_targets.ensure_payment_row(repo, repo.get("Platitori", target["IDP"]), month, facts["target_att"])
        moved = repo.update("Plati", payment, {"IDP": target["IDP"], "IDZ": target_att["IDZ"], "IDS": payer["IDS"]})
        if month["Inchisa"]:
            service.rewrite_snapshot(repo, repo.get("Prezenta", facts["source_att"]["IDZ"]))
            service.rewrite_snapshot(repo, repo.get("Prezenta", target_att["IDZ"]))
        details = ("plata %d (%s lei, %s) mutata in %s, luna %s-%02d (%s): copil %d -> %d, prezenta %d -> %d, platitor %s -> %d%s; documente: %s."
                   % (idpl, payment["Plata"], payment["Data"], db, month["Anul"], month["Luna"], "inchisa, situatii refacute" if month["Inchisa"] else "deschisa",
                      facts["source"]["IDP"], target["IDP"], facts["source_att"]["IDZ"], target_att["IDZ"], payment.get("IDS"), payer["IDS"],
                      "" if facts["target_att"] else "; prezenta noua cu zero zile pentru copilul tinta", ", ".join(facts["documents"]) or "niciunul"))
        _journal(conn, "adechit_plata_muta", db, str(idpl), details[:60000])
        conn.commit()
    except DomainError as error:
        conn.rollback()
        raise AdminError("%s Nu s-a scris nimic." % error) from error
    except Exception:
        conn.rollback()
        raise
    finally:
        repo.cursor.close()
    summary = block(db, TITLE, ["rezultat"],
                    [["Plata %d (%s lei) a trecut de la %s la %s, platitor %s." % (idpl, moved["Plata"], facts["source"]["Nume"], target["Nume"], payer["Nume"])],
                     ["Situatiile salvate ale lunii au fost refacute pentru amandoi copiii." if month["Inchisa"] else
                      "Luna este deschisa: nu exista situatie salvata, se calculeaza la citire."]],
                    "Chitanta sau documentul a ramas neschimbat. Verifica rezultatul cu «Informatii copil» pe cei doi copii.", raw=True)
    return [summary]


def list_payments(conn, db, param, block):
    """Read only: the payments of ONE child (his CNP, 13 digits, or his ID, IDP) that could be moved; KBotAdmin turns the rows into a choice list.
    The block title and the first column (IDPL) are read by KBotAdmin: do not rename them."""
    ref = (param or "").strip()
    if not (_RX_CNP13.match(ref) or _RX_ID.match(ref)):
        raise AdminError("Scrie CNP-ul (13 cifre) sau ID-ul (pana la 9 cifre) al copilului de la care pleaca plata.")
    _check_database(conn, db)
    by_cnp = bool(_RX_CNP13.match(ref))
    kids = _rows(conn, "SELECT SubunitId, IDP, Nume FROM AD_Platitori WHERE " + ("TRIM(CNP)=%s" if by_cnp else "IDP=%s"), (ref,))
    if not kids:
        return [block(db, TITLE + " - plati", ["rezultat"], [["Niciun copil cu %s %s in baza %s." % ("CNP-ul" if by_cnp else "ID-ul", ref, db)]])]
    rows = []
    for kid in kids:
        for r in _rows(
                conn,
                "SELECT t.IDPL, l.Anul AS anul, l.Luna AS lunan, l.Inchisa AS inchisa, t.Data AS data, t.Plata AS suma, t.TIP AS tip, "
                "(SELECT GROUP_CONCAT(CONCAT(IFNULL(c.Serie,''),' ',c.Numar) ORDER BY c.IDC SEPARATOR '; ') FROM AD_Chitante c "
                " WHERE c.SubunitId=t.SubunitId AND c.IDPL=t.IDPL AND c.Anulata=0) AS chitanta, "
                "(SELECT GROUP_CONCAT(a.NrDoc ORDER BY a.IDA SEPARATOR '; ') FROM AD_AlteDoc a "
                " WHERE a.SubunitId=t.SubunitId AND a.IDPL=t.IDPL AND a.Anulata=0) AS alt_doc, "
                "EXISTS(SELECT 1 FROM AD_EXTRASE_ASOCIERI e WHERE e.SUBUNIT_ID=t.SubunitId AND e.IDPL=t.IDPL) AS banca "
                "FROM AD_Plati t LEFT JOIN AD_LunaD l ON l.SubunitId=t.SubunitId AND l.IDL=t.IDL "
                "WHERE t.SubunitId=%s AND t.IDP=%s AND t.Anulata=0 ORDER BY t.Data DESC, t.IDPL DESC LIMIT 301",
                (kid["SubunitId"], kid["IDP"])):
            rows.append([r["IDPL"], kid["Nume"], "%s-%02d" % (r["anul"], int(r["lunan"])) if r["anul"] is not None else "", str(r["data"])[:10],
                         r["suma"], "incasare cu chitanta" if r["tip"] == 2 else "alta plata", r["chitanta"] or r["alt_doc"] or "",
                         "inchisa" if r["inchisa"] else "deschisa", "da" if r["banca"] else "nu"])
    note = "Alege plata de mutat din lista de sub caseta. Platile din extras de banca (banca = da) nu se pot muta de aici."
    if len(rows) > 300:
        rows = rows[:300]
        note += " Lista este taiata la 300 de plati."
    if not rows:
        note = "Copilul nu are nicio plata neanulata."
    return [block(db, TITLE + " - plati", ["IDPL", "copil", "luna", "data", "suma", "fel", "document", "luna e", "banca"], rows, note)]
