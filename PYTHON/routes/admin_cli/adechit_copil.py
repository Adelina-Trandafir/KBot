"""KBotAdmin command `adechit_copil` (SLICE-ME03-01): everything an ADECHIT unit database holds about ONE child, read only.

The operator types the child's CNP. In the chosen database the CNP finds the child row(s) of AD_Platitori (one per subunit the child
was entered in: the same CNP can sit in more than one subunit, and the same subunit can hold it twice), and from the IDP of each
the whole trail is read, untruncated:

  1. summary: how many rows each table holds for the child
  2. the child (group, entry / exit dates, left or not)
  3. the payers tied to him (AD_Platitori_sub) and whether the parent has a portal account
  4. the history of group changes (AD_Platitori_Istoric)
  5. the months: attendance, daily rate, contract value (AD_Prezenta)
  6. the monthly statement as it was stored (AD_SS_Buget)
  7. payments and cash-ins (AD_Plati), each with its receipt
  8. receipts (AD_Chitante)
  9. other documents (AD_AlteDoc)
 10. returns (AD_Retur)
 11. set-offs between children (AD_Compensare, as debit or as credit)
 12. bank statement lines tied to the child (AD_EXTRASE_ASOCIERI + FX_Extrase)

Nothing is written. Every table is keyed by (SubunitId, IDP) as the foreign keys are, so a child of another subunit with the same
IDP number can never leak in.
"""
import re

from routes.admin_cli.admin_common import AdminError, _rows

ROW_LIMIT = 2000
_RX_CNP = re.compile(r"^[0-9]{1,13}$")

# The child rows the CNP finds, as a (SubunitId, IDP) set every other table is filtered with.
_KIDS = "SELECT SubunitId, IDP FROM AD_Platitori WHERE TRIM(CNP)=%s"
_KID = "(%s.SubunitId, %s.IDP) IN (" + _KIDS + ")"
_PAY_OF_KID = "SELECT SubunitId, IDPL FROM AD_Plati WHERE (SubunitId, IDP) IN (" + _KIDS + ")"


def _kid(alias):
    """The child filter for a table aliased `alias`: the third %s of _KID is the one inside _KIDS, so it is kept as a literal %s."""
    return _KID % (alias, alias, "%s")


def _month(row):
    """'2025-09' from the columns anul / lunan a query selects (apart, so the ordering stays numeric); '' when the row has no month."""
    if row.get("anul") is None or row.get("lunan") is None:
        return ""
    return "%s-%02d" % (row["anul"], int(row["lunan"]))


def _table(conn, title, columns, sql, params, note=""):
    """(title, columns, rows, note) with the rows as lists in the order of `columns`; the note says when the limit cut the list.
    A column called `luna` is built from the `anul` and `lunan` the query selects."""
    rows = _rows(conn, sql + " LIMIT %d" % (ROW_LIMIT + 1), params)
    cut = len(rows) > ROW_LIMIT
    rows = rows[:ROW_LIMIT]
    extra = ("Lista este taiata la %d randuri." % ROW_LIMIT) if cut else ""
    if "luna" in columns:
        for row in rows:
            row["luna"] = _month(row)
    return title, columns, [[row[c] for c in columns] for row in rows], " ".join(x for x in (note, extra) if x)


def profile(conn, db, param, block):
    """The blocks of the child with this CNP; `block` is admin_cli._block."""
    cnp = (param or "").strip()
    if not _RX_CNP.match(cnp):
        raise AdminError("CNP-ul trebuie sa contina doar cifre (pana la 13).")
    have = {r["n"] for r in _rows(conn, "SELECT TABLE_NAME AS n FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s "
                                        "AND TABLE_NAME IN ('AD_Platitori','AD_Plati','AD_Chitante','AD_Prezenta')", (db,))}
    if len(have) < 4:
        raise AdminError("Baza «%s» nu este o baza ADECHIT (lipsesc tabelele AD_)." % db)

    tables = []
    p = (cnp,)

    tables.append(_table(
        conn, "Copilul", ["subunitate", "IDP", "nume", "cnp", "grupa", "plecat", "SI", "data_intrare", "data_iesire", "creat", "modificat"],
        "SELECT s.Name AS subunitate, p.IDP, p.Nume AS nume, p.CNP AS cnp, g.Grupa AS grupa, p.Plecat AS plecat, p.SI, "
        "p.DataIntrare AS data_intrare, p.DataIesire AS data_iesire, p.CreatedAt AS creat, p.UpdatedAt AS modificat "
        "FROM AD_Platitori p LEFT JOIN AD_Subunits s ON s.SubunitId=p.SubunitId "
        "LEFT JOIN AD_Grupe g ON g.SubunitId=p.SubunitId AND g.IDG=p.IDG WHERE TRIM(p.CNP)=%s ORDER BY p.SubunitId, p.IDP", p))
    kids = tables[0][2]
    if not kids:
        # The CNP may be a payer's (parent's) instead of a child's: say whose.
        parents = _rows(conn, "SELECT p.IDP, p.Nume AS copil, ps.Nume AS platitor FROM AD_Platitori_sub ps "
                              "JOIN AD_Platitori p ON p.SubunitId=ps.SubunitId AND p.IDP=ps.IDP WHERE TRIM(ps.CNP_Platitor)=%s "
                              "ORDER BY p.Nume", p)
        if parents:
            return [block(db, "CNP de platitor, nu de copil", ["IDP copil", "copil", "platitor"],
                          [[r["IDP"], r["copil"], r["platitor"]] for r in parents],
                          "Niciun copil nu are acest CNP, dar este CNP-ul unui platitor. Cauta dupa CNP-ul copilului.")]
        return [block(db, "Informatii copil", ["rezultat"], [["Niciun copil cu CNP-ul %s in baza %s." % (cnp, db)]])]

    tables.append(_table(
        conn, "Platitorii copilului", ["IDS", "IDP", "nume", "cnp_platitor", "activ", "trimite_mail", "email", "telefon", "adresa", "cui",
                                       "cont", "banca", "cod_portal", "creat", "modificat"],
        "SELECT ps.IDS, ps.IDP, ps.Nume AS nume, ps.CNP_Platitor AS cnp_platitor, ps.Activ AS activ, ps.TrimiteMail AS trimite_mail, "
        "ps.EMail AS email, ps.Telefon AS telefon, ps.Adresa AS adresa, ps.CUI AS cui, ps.Cont AS cont, ps.Banca AS banca, "
        "IF(ps.CodAccesPortal IS NULL OR ps.CodAccesPortal='','nu','da') AS cod_portal, ps.CreatedAt AS creat, ps.UpdatedAt AS modificat "
        "FROM AD_Platitori_sub ps WHERE " + _kid("ps") + " ORDER BY ps.SubunitId, ps.IDS", p,
        "cod_portal = daca platitorul are un cod de acces permanent (codul in sine nu se arata)."))

    tables.append(_table(
        conn, "Cont de portal al parintilor", ["cnp_platitor", "email", "cod_acces", "creat", "modificat"],
        "SELECT pp.CNP AS cnp_platitor, pp.Email AS email, IF(pp.CodAccesPortal IS NULL OR pp.CodAccesPortal='','nu','da') AS cod_acces, "
        "pp.CreatedAt AS creat, pp.UpdatedAt AS modificat FROM AD_PortalParents pp WHERE CONVERT(pp.CNP USING utf8mb3) COLLATE utf8mb3_general_ci IN "
        "(SELECT TRIM(ps.CNP_Platitor) FROM AD_Platitori_sub ps WHERE " + _kid("ps") + ") ORDER BY pp.CNP", p))

    tables.append(_table(
        conn, "Istoric grupe", ["IDI", "IDP", "data", "tip", "din_grupa", "in_grupa", "dedus", "utilizator", "nota", "creat"],
        "SELECT i.IDI, i.IDP, i.Data AS data, i.Tip AS tip, gv.Grupa AS din_grupa, gn.Grupa AS in_grupa, i.Dedus AS dedus, "
        "i.Utilizator AS utilizator, i.Nota AS nota, i.CreatedAt AS creat FROM AD_Platitori_Istoric i "
        "LEFT JOIN AD_Grupe gv ON gv.SubunitId=i.SubunitId AND gv.IDG=i.IDG_Vechi "
        "LEFT JOIN AD_Grupe gn ON gn.SubunitId=i.SubunitId AND gn.IDG=i.IDG_Nou WHERE " + _kid("i") + " ORDER BY i.Data, i.IDI", p))

    tables.append(_table(
        conn, "Luni (prezenta si contract)", ["luna", "IDP", "IDZ", "grupa", "zile_prezenta", "taxa_zilnica", "valoare_contract",
                                              "valoare_totala", "luna_inchisa"],
        "SELECT l.Anul AS anul, l.Luna AS lunan, z.IDP, z.IDZ, g.Grupa AS grupa, z.ZilePrezenta AS zile_prezenta, v.TaxaZilnica AS taxa_zilnica, "
        "z.ValoareContract AS valoare_contract, z.ValoareTotala AS valoare_totala, l.Inchisa AS luna_inchisa FROM AD_Prezenta z "
        "LEFT JOIN AD_LunaD l ON l.SubunitId=z.SubunitId AND l.IDL=z.IDL LEFT JOIN AD_Grupe g ON g.SubunitId=z.SubunitId AND g.IDG=z.IDG "
        "LEFT JOIN AD_ValoriTaxe v ON v.SubunitId=z.SubunitId AND v.IDV=z.IDV WHERE " + _kid("z") + " ORDER BY l.Anul, l.Luna, z.IDZ", p))

    tables.append(_table(
        conn, "Situatia lunara stocata (AD_SS_Buget)", ["luna", "IDP", "grupa", "zile_prezenta", "SID", "SIC", "valoare_contract", "valoare_totala",
                                                         "plata", "plati", "retur", "compensare", "restanta", "anticipat", "SFD", "SFC", "plecat"],
        "SELECT l.Anul AS anul, l.Luna AS lunan, b.IDP, b.Grupa AS grupa, b.ZilePrezenta AS zile_prezenta, b.SID, b.SIC, "
        "b.ValoareContract AS valoare_contract, b.ValoareTotala AS valoare_totala, b.Plata AS plata, b.Plati AS plati, b.Retur AS retur, "
        "b.Compensare AS compensare, b.Restanta AS restanta, b.Anticipat AS anticipat, b.SFD, b.SFC, b.Plecat AS plecat FROM AD_SS_Buget b "
        "LEFT JOIN AD_LunaD l ON l.SubunitId=b.SubunitId AND l.IDL=b.IDL WHERE " + _kid("b") + " ORDER BY l.Anul, l.Luna, b.ID", p,
        "Plata = incasarile lunii (TIP 2); plati = celelalte plati; SID / SIC = sold initial debit / credit; SFD / SFC = sold final."))

    tables.append(_table(
        conn, "Plati si incasari", ["IDPL", "data", "suma", "fel", "anulata", "valida", "motiv", "luna", "luna_origine", "IDS", "platitor",
                                    "chitanta", "extras", "creat", "modificat"],
        "SELECT t.IDPL, t.Data AS data, t.Plata AS suma, IF(t.TIP=2,'incasare (TIP 2)',CONCAT('plata (TIP ',IFNULL(t.TIP,'?'),')')) AS fel, "
        "t.Anulata AS anulata, t.Valid AS valida, t.Motivul AS motiv, l.Anul AS anul, l.Luna AS lunan, "
        "IF(t.OriginYear IS NULL,'',CONCAT(t.OriginYear,'-',LPAD(t.OriginMonth,2,'0'))) AS luna_origine, t.IDS, ps.Nume AS platitor, "
        "(SELECT GROUP_CONCAT(CONCAT(IFNULL(c.Serie,''),' ',c.Numar,IF(c.NumarSub>0,CONCAT(',',c.NumarSub),''),IF(c.Anulata,' (anulata)','')) "
        "ORDER BY c.IDC SEPARATOR '; ') FROM AD_Chitante c WHERE c.SubunitId=t.SubunitId AND c.IDPL=t.IDPL) AS chitanta, "
        "(SELECT GROUP_CONCAT(a.IDFXE ORDER BY a.IDFXE SEPARATOR '; ') FROM AD_EXTRASE_ASOCIERI a WHERE a.SUBUNIT_ID=t.SubunitId "
        "AND a.IDPL=t.IDPL) AS extras, t.CreatedAt AS creat, t.UpdatedAt AS modificat FROM AD_Plati t "
        "LEFT JOIN AD_LunaD l ON l.SubunitId=t.SubunitId AND l.IDL=t.IDL LEFT JOIN AD_Platitori_sub ps ON ps.SubunitId=t.SubunitId AND ps.IDS=t.IDS "
        "WHERE " + _kid("t") + " ORDER BY t.Data, t.IDPL", p,
        "luna = luna din care face parte plata; luna_origine = luna pentru care s-a platit (daca difera); extras = id-urile randurilor de extras asociate."))

    tables.append(_table(
        conn, "Chitante", ["IDC", "IDPL", "data", "serie", "numar", "sub_numar", "explicatie", "anulata", "luna", "suma", "creat", "modificat"],
        "SELECT c.IDC, c.IDPL, c.Data AS data, c.Serie AS serie, c.Numar AS numar, c.NumarSub AS sub_numar, c.Explicatie AS explicatie, "
        "c.Anulata AS anulata, l.Anul AS anul, l.Luna AS lunan, t.Plata AS suma, c.CreatedAt AS creat, c.UpdatedAt AS modificat FROM AD_Chitante c "
        "LEFT JOIN AD_LunaD l ON l.SubunitId=c.SubunitId AND l.IDL=c.IDL LEFT JOIN AD_Plati t ON t.SubunitId=c.SubunitId AND t.IDPL=c.IDPL "
        "WHERE (c.SubunitId, c.IDPL) IN (" + _PAY_OF_KID + ") ORDER BY c.Data, c.IDC", p))

    tables.append(_table(
        conn, "Alte documente", ["IDA", "IDPL", "fel", "numar", "data", "explicatie", "anulata", "luna", "creat", "modificat"],
        "SELECT a.IDA, a.IDPL, a.FelDoc AS fel, a.NrDoc AS numar, a.DataDoc AS data, a.Explicatie AS explicatie, a.Anulata AS anulata, "
        "l.Anul AS anul, l.Luna AS lunan, a.CreatedAt AS creat, a.UpdatedAt AS modificat FROM AD_AlteDoc a "
        "LEFT JOIN AD_LunaD l ON l.SubunitId=a.SubunitId AND l.IDL=a.IDL "
        "WHERE (a.SubunitId, a.IDPL) IN (" + _PAY_OF_KID + ") ORDER BY a.DataDoc, a.IDA", p))

    tables.append(_table(
        conn, "Retururi", ["IDR", "data", "suma", "explicatie", "numar_doc", "anulat", "motiv_anulare", "luna", "luna_origine", "IDS", "creat", "modificat"],
        "SELECT r.IDR, r.Data AS data, r.Suma AS suma, r.Explicatie AS explicatie, r.NrDoc AS numar_doc, r.Anulat AS anulat, "
        "r.MotivAnulare AS motiv_anulare, l.Anul AS anul, l.Luna AS lunan, "
        "IF(r.OriginYear IS NULL,'',CONCAT(r.OriginYear,'-',LPAD(r.OriginMonth,2,'0'))) AS luna_origine, r.IDS, "
        "r.CreatedAt AS creat, r.UpdatedAt AS modificat FROM AD_Retur r LEFT JOIN AD_LunaD l ON l.SubunitId=r.SubunitId AND l.IDL=r.IDL "
        "WHERE " + _kid("r") + " ORDER BY r.Data, r.IDR", p))

    tables.append(_table(
        conn, "Compensari", ["IDCMP", "data", "suma", "rol", "cu_copilul", "nota", "utilizator", "creat"],
        "SELECT m.IDCMP, m.Data AS data, m.Suma AS suma, IF((m.SubunitId, m.IDP_Debit) IN (" + _KIDS + "),'debit (a dat)','credit (a primit)') AS rol, "
        "CONCAT(o.Nume,' (IDP ',o.IDP,')') AS cu_copilul, m.Nota AS nota, m.Utilizator AS utilizator, m.CreatedAt AS creat FROM AD_Compensare m "
        "JOIN AD_Platitori o ON o.SubunitId=m.SubunitId AND o.IDP=IF((m.SubunitId, m.IDP_Debit) IN (" + _KIDS + "),m.IDP_Credit,m.IDP_Debit) "
        "WHERE (m.SubunitId, m.IDP_Debit) IN (" + _KIDS + ") OR (m.SubunitId, m.IDP_Credit) IN (" + _KIDS + ") ORDER BY m.Data, m.IDCMP",
        p * 4, "Compensare = suma mutata intre doi copii; rol arata partea copilului cautat."))

    tables.append(_table(
        conn, "Extrase bancare asociate", ["IDFXE", "IDPL", "IDA", "data_banca", "nr_doc", "platitor_in_extras", "credit", "debit", "explicatii",
                                           "asociat_de", "asociat_la"],
        "SELECT a.IDFXE, a.IDPL, a.IDA, e.DataBanca AS data_banca, e.NrDoc AS nr_doc, e.platitor_nume AS platitor_in_extras, "
        "e.suma_credit AS credit, e.suma_debit AS debit, e.Explicatii AS explicatii, a.ASOCIAT_DE AS asociat_de, a.ASOCIAT_LA AS asociat_la "
        "FROM AD_EXTRASE_ASOCIERI a LEFT JOIN FX_Extrase e ON e.IDFXE=a.IDFXE "
        "WHERE (a.SUBUNIT_ID, a.IDP) IN (" + _KIDS + ") ORDER BY e.DataBanca, a.IDFXE", p))

    summary_rows = [[title, len(rows)] for title, _columns, rows, _note in tables]
    out = [block(db, "Informatii copil %s - sumar" % cnp, ["tabela", "randuri"], summary_rows,
                 "%d inregistrare(a) de copil gasite dupa CNP; tot ce urmeaza este citit dupa IDP-ul lor." % len(kids))]
    for title, columns, rows, note in tables:
        out.append(block(db, title, columns, rows, note))
    return out
