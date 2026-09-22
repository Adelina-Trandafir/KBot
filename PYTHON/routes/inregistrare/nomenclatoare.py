# routes/inregistrare/nomenclatoare.py
"""
The three lists the registration page picks from (slice 0075-02, plan 4 steps 4-5).

  sector-sources  -- `DefaSursaSector`, fourteen rows, a checkbox list
  ClsfF           -- `DefaClsfF`, the funding classification, a checkbox tree
  ClsfE           -- `DefaClsfE`, the spending classification, a checkbox tree

All three live in `AVACONT_COMUN` on the K-BOT server. Every function here takes an
open connection and returns plain rows; the routes do the HTTP.

WHY ClsfE IS FILTERED AND ClsfF IS NOT. `Clasificatii` carries four foreign keys into
this database, and two of them are on values DERIVED from the E code:

    Articol = Left(E,2) + "." + Mid(E,3,2)      -> DefaArticol
    Titlu   = Left(E,2)                          -> DefaTitlu

Fifteen codes in `DefaClsfE` derive to an `Articol` or a `Titlu` that does not exist
in those dictionaries (counted on the server, 22.09.2026: eight missing an Articol,
four missing a Titlu). Offering them would mean a tree where some leaves cannot be
saved -- the applicant picks one, waits for approval, and the provisioning job dies
on a `1452` at the very last step. So the filter is the join itself: a code is
offered only if the row it would become can find both its parents.

That also means the list cannot drift from the constraint. If someone adds the
missing dictionary rows tomorrow, the codes appear on their own; nothing here holds a
list of fifteen exceptions that would quietly go stale.

`DefaClsfF` needs no such filter -- `ClsfF` is its own foreign key, so every row in
the dictionary is by definition insertable.

WHAT 2a SAYS ABOUT THESE TABLES, and why the queries look defensive:
  * `DefaClsfF` has NO primary key, `ClsfF` is nullable, and its index is not unique
    -- so duplicates and NULLs are both possible. Hence GROUP BY and the NULL guard.
  * The caption column is `Denumire` on all four (this closed an old doubt about
    `Explicatie`). It is `mediumtext` / `longtext`, not a short varchar.
  * Codes are six characters throughout.
  * D16: a `DefaClsfE` row with no caption is not shown. On 22.09.2026 there were
    none, but the rule is the operator's and the data can change.
"""
import logging

logger = logging.getLogger(__name__)

# One row per sector-source. `SursaSector` is the code `Clasificatii.SS` must match.
_SQL_SURSASECTOR = "SELECT * FROM AVACONT_COMUN.DefaSursaSector ORDER BY SursaSector"

# GROUP BY, not DISTINCT: two rows with the same code and different captions would
# otherwise both come through and the tree would show the code twice.
_SQL_CLSF_F = (
    "SELECT ClsfF AS cod, MAX(Denumire) AS denumire "
    "FROM AVACONT_COMUN.DefaClsfF "
    "WHERE ClsfF IS NOT NULL AND ClsfF <> '' "
    "GROUP BY ClsfF "
    "ORDER BY ClsfF"
)

# The join IS the filter -- see the note at the top of the file.
_SQL_CLSF_E = (
    "SELECT e.ClsfE AS cod, MAX(e.Denumire) AS denumire "
    "FROM AVACONT_COMUN.DefaClsfE e "
    "JOIN AVACONT_COMUN.DefaArticol a "
    "  ON a.Articol = CONCAT(LEFT(e.ClsfE, 2), '.', SUBSTRING(e.ClsfE, 3, 2)) "
    "JOIN AVACONT_COMUN.DefaTitlu t "
    "  ON t.Titlu = LEFT(e.ClsfE, 2) "
    "WHERE e.ClsfE IS NOT NULL AND e.ClsfE <> '' "
    "  AND e.Denumire IS NOT NULL AND TRIM(e.Denumire) <> '' "
    "GROUP BY e.ClsfE "
    "ORDER BY e.ClsfE"
)

# Caption column names tried, in order, on DefaSursaSector. See read_sursasector.
_CAPTION_COLUMNS = ("Denumire", "Explicatie", "Descriere")


def read_sursasector(conn) -> list:
    """
    The fourteen sector-sources, as `[{"cod", "denumire"}]`.

    `SELECT *` on purpose. The code column is certain -- `SursaSector` is the target
    of the `Clasificatii__DefaSS` foreign key, so it is named in the DDL. The CAPTION
    column is NOT: nothing in the repository joins this table, and the fourteen values
    were read off the server as bare codes. So whichever of the usual caption names is
    present gets used, and when none is, the code stands in for its own label. A
    checkbox list of fourteen codes is worth more than a 500 over a column name.
    """
    cur = conn.cursor(dictionary=True)
    cur.execute(_SQL_SURSASECTOR)
    rows = cur.fetchall()

    caption = None
    for name in _CAPTION_COLUMNS:
        if rows and name in rows[0]:
            caption = name
            break
    if caption is None and rows:
        logger.warning(
            "DefaSursaSector has no caption column among %s; showing codes only",
            ", ".join(_CAPTION_COLUMNS),
        )

    out = []
    for row in rows:
        cod = _text(row.get("SursaSector"))
        if not cod:
            continue
        label = _text(row.get(caption)) if caption else ""
        out.append({"cod": cod, "denumire": label or cod})
    return out


def read_clasificatii(conn, tip) -> list:
    """
    The flat code list for one tree, as `[{"cod", "denumire"}]`.

    `tip` is `F` or `E`. The page turns the flat list into a tree itself (plan 4.1):
    `xx0000` is a root, `xxxx00` a level-1 node, anything else a leaf.
    """
    sql = _SQL_CLSF_F if tip == "F" else _SQL_CLSF_E
    cur = conn.cursor(dictionary=True)
    cur.execute(sql)
    return [
        {"cod": _text(row.get("cod")), "denumire": _text(row.get("denumire"))}
        for row in cur.fetchall()
        if _text(row.get("cod"))
    ]


def read_codes(conn, tip) -> set:
    """
    Just the codes of one tree, for validating what came back from the page.

    Deliberately the SAME query as `read_clasificatii`, so what is validated at
    `/cerere` is exactly what was offered -- including the ClsfE filter. Building the
    check from a different query is how a page ends up offering a code its own server
    then refuses.
    """
    return {row["cod"] for row in read_clasificatii(conn, tip)}


def read_sursasector_codes(conn) -> set:
    """The fourteen codes alone, for the same reason as read_codes."""
    return {row["cod"] for row in read_sursasector(conn)}


def _text(value) -> str:
    return "" if value is None else str(value).strip()
