# routes/migrare/validate.py
# -----------------------------------------------------------------------------
# Does this Access row fit the MariaDB column it is headed for?
#
# The rules come from the TARGET, never from Access: MariaDB is what accepts or
# refuses the row, so information_schema is the only honest source for the type,
# the length, the nullability and the foreign keys.
#
# Findings fall into two classes, and the class is what drives the two buttons in
# the migrator:
#
#   BLOCANT  -- structura sau valoarea nu incap: tabel/coloana lipsa, tip gresit,
#               depasire de lungime sau de interval, NULL intr-o coloana NOT NULL.
#               Cat timp exista unul, NICIUN buton nu porneste.
#   FORTABIL -- integritatea legaturilor: cheie straina fara corespondent, id DDF
#               absent, cheie primara dubla, rand care nu se ruteaza in nicio baza.
#               «Rulează» ramane oprit, «Forțează rularea» porneste si SARE peste
#               randurile vinovate, fara sa le piarda din raport.
# -----------------------------------------------------------------------------

import datetime
import decimal
import logging

from . import routing, tables

logger = logging.getLogger(__name__)

BLOCANT = "blocant"
FORTABIL = "forțabil"

# --- felurile de constatare --------------------------------------------------
F_TABEL_LIPSA = "TABEL_LIPSĂ"
F_COLOANA_LIPSA = "COLOANĂ_LIPSĂ"
F_TIP = "TIP"
F_DIMENSIUNE = "DIMENSIUNE"
F_NUL_INTERZIS = "NUL_INTERZIS"
F_CHEIE_STRAINA = "CHEIE_STRĂINĂ"
F_DDF_LIPSA = "ID_DDF_LIPSĂ"
F_CHEIE_DUBLA = "CHEIE_DUBLĂ"
F_RUTARE = "RUTARE"

CLASS_OF = {
    F_TABEL_LIPSA: BLOCANT,
    F_COLOANA_LIPSA: BLOCANT,
    F_TIP: BLOCANT,
    F_DIMENSIUNE: BLOCANT,
    F_NUL_INTERZIS: BLOCANT,
    F_CHEIE_STRAINA: FORTABIL,
    F_DDF_LIPSA: FORTABIL,
    F_CHEIE_DUBLA: FORTABIL,
    F_RUTARE: FORTABIL,
}

# Cate exemple pastram pentru fiecare (tabel, coloana, fel). Numaratoarea e
# INTREAGA; doar lista de exemple e plafonata, ca raportul sa incapa pe ecran.
MAX_EXAMPLES = 25

# Cate valori punem intr-un singur `IN (...)` cand verificam cheile straine.
FK_BATCH = 1000

_INT_RANGES = {
    "tinyint": (-128, 127, 0, 255),
    "smallint": (-32768, 32767, 0, 65535),
    "mediumint": (-8388608, 8388607, 0, 16777215),
    "int": (-2147483648, 2147483647, 0, 4294967295),
    "integer": (-2147483648, 2147483647, 0, 4294967295),
    "bigint": (-9223372036854775808, 9223372036854775807, 0, 18446744073709551615),
}

_DATE_FORMATS = ("%Y-%m-%d %H:%M:%S", "%Y-%m-%d", "%m/%d/%y %H:%M:%S", "%Y-%m-%dT%H:%M:%S")


class ValidationError(Exception):
    """Analiza nu poate continua deloc — mesaj în română."""


# -----------------------------------------------------------------------------
# Schema tintei
# -----------------------------------------------------------------------------

class TargetSchema(object):
    """Coloanele, cheile primare și cheile străine ale bazei de unitate."""

    def __init__(self, conn, db_name, table_names):
        self.db_name = db_name
        self.columns = {}       # tabel -> {coloana: meta}
        self.primary_key = {}   # tabel -> [coloane]
        self.foreign_keys = {}  # tabel -> [ {coloana, tabel_ref, coloana_ref, nume} ]
        self._load(conn, table_names)

    def has(self, table):
        return table in self.columns

    def _load(self, conn, table_names):
        placeholders = ",".join(["%s"] * len(table_names))
        cur = conn.cursor()
        try:
            cur.execute(
                "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, COLUMN_TYPE, "
                "       CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, "
                "       IS_NULLABLE, COLUMN_DEFAULT, EXTRA, COLUMN_KEY "
                "  FROM information_schema.COLUMNS "
                " WHERE TABLE_SCHEMA = %s AND TABLE_NAME IN (" + placeholders + ") "
                " ORDER BY TABLE_NAME, ORDINAL_POSITION",
                tuple([self.db_name] + list(table_names)))
            for row in cur.fetchall():
                (table, name, data_type, column_type, char_len, precision, scale,
                 nullable, default, extra, key) = row
                self.columns.setdefault(table, {})[name] = {
                    "nume": name,
                    "tip": (data_type or "").lower(),
                    "tip_complet": column_type if isinstance(column_type, str)
                                   else (column_type or b"").decode("utf-8", "replace"),
                    "lungime": int(char_len) if char_len is not None else None,
                    "precizie": int(precision) if precision is not None else None,
                    "scara": int(scale) if scale is not None else None,
                    "acceptă_nul": (nullable or "").upper() == "YES",
                    "are_implicit": default is not None,
                    "auto": "auto_increment" in (extra or "").lower(),
                    "cheie": key,
                }

            cur.execute(
                "SELECT TABLE_NAME, COLUMN_NAME "
                "  FROM information_schema.KEY_COLUMN_USAGE "
                " WHERE TABLE_SCHEMA = %s AND CONSTRAINT_NAME = 'PRIMARY' "
                "   AND TABLE_NAME IN (" + placeholders + ") "
                " ORDER BY TABLE_NAME, ORDINAL_POSITION",
                tuple([self.db_name] + list(table_names)))
            for table, column in cur.fetchall():
                self.primary_key.setdefault(table, []).append(column)

            cur.execute(
                "SELECT TABLE_NAME, COLUMN_NAME, REFERENCED_TABLE_SCHEMA, "
                "       REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME, CONSTRAINT_NAME "
                "  FROM information_schema.KEY_COLUMN_USAGE "
                " WHERE TABLE_SCHEMA = %s AND REFERENCED_TABLE_NAME IS NOT NULL "
                "   AND TABLE_NAME IN (" + placeholders + ")",
                tuple([self.db_name] + list(table_names)))
            for table, column, ref_schema, ref_table, ref_column, name in cur.fetchall():
                self.foreign_keys.setdefault(table, []).append({
                    "coloana": column,
                    "schema_ref": ref_schema,
                    "tabel_ref": ref_table,
                    "coloana_ref": ref_column,
                    "nume": name,
                })
        finally:
            cur.close()


# -----------------------------------------------------------------------------
# Verificarea unei valori fata de coloana ei
# -----------------------------------------------------------------------------

def check_value(meta, value):
    """
    Întoarce (fel, mesaj) sau None dacă valoarea încape.

    Nu convertește nimic: doar spune dacă MariaDB ar accepta-o. Conversia reală o
    face driverul la scriere, din aceleași valori.
    """
    # NULL: mdb-json omite coloana, deci `value is None` inseamna chiar NULL in
    # Access, nu sir gol. Diferenta conteaza exact aici.
    if value is None:
        if not meta["acceptă_nul"] and not meta["are_implicit"] and not meta["auto"]:
            return F_NUL_INTERZIS, "coloana nu acceptă NULL și nu are valoare implicită"
        return None

    tip = meta["tip"]

    # Sirul gol intr-o coloana care nu e text: MariaDB il refuza sau il aduce la
    # zero tacut, dupa mod. Il tratam ca lipsa de valoare, nu ca eroare de tip.
    if isinstance(value, str) and not value.strip() and tip not in (
            "char", "varchar", "text", "tinytext", "mediumtext", "longtext", "enum", "set"):
        if not meta["acceptă_nul"] and not meta["are_implicit"] and not meta["auto"]:
            return F_NUL_INTERZIS, "valoare goală într-o coloană care nu acceptă NULL"
        return None

    # --- text -------------------------------------------------------------
    if tip in ("char", "varchar", "text", "tinytext", "mediumtext", "longtext", "enum", "set"):
        text = value if isinstance(value, str) else str(value)
        limit = meta["lungime"]
        if limit is not None and len(text) > limit:
            return (F_DIMENSIUNE,
                    "text de %d caractere într-o coloană de %d" % (len(text), limit))
        return None

    # --- intregi ----------------------------------------------------------
    if tip in _INT_RANGES:
        number = _as_int(value)
        if number is None:
            return F_TIP, "valoare care nu este un număr întreg: «%s»" % _short(value)
        low, high, ulow, uhigh = _INT_RANGES[tip]
        if "unsigned" in meta["tip_complet"].lower():
            low, high = ulow, uhigh
        if number < low or number > high:
            return (F_DIMENSIUNE,
                    "%d în afara intervalului %s (%d … %d)" % (number, tip, low, high))
        return None

    # --- zecimale ---------------------------------------------------------
    if tip in ("decimal", "numeric"):
        number = _as_decimal(value)
        if number is None:
            return F_TIP, "valoare care nu este un număr: «%s»" % _short(value)
        precision = meta["precizie"] or 10
        scale = meta["scara"] or 0
        whole = precision - scale
        limit = decimal.Decimal(10) ** whole
        if abs(number) >= limit:
            return (F_DIMENSIUNE,
                    "%s depășește DECIMAL(%d,%d)" % (number, precision, scale))
        return None

    if tip in ("float", "double", "real"):
        if _as_float(value) is None:
            return F_TIP, "valoare care nu este un număr: «%s»" % _short(value)
        return None

    # --- date -------------------------------------------------------------
    if tip in ("date", "datetime", "timestamp"):
        if _as_datetime(value) is None:
            return F_TIP, "valoare care nu este o dată: «%s»" % _short(value)
        return None

    if tip == "time":
        return None

    # --- binare / bit ------------------------------------------------------
    if tip in ("blob", "tinyblob", "mediumblob", "longblob", "varbinary", "binary"):
        raw = value if isinstance(value, (bytes, bytearray)) else str(value).encode("utf-8", "replace")
        limit = meta["lungime"]
        if limit is not None and len(raw) > limit:
            return F_DIMENSIUNE, "%d octeți într-o coloană de %d" % (len(raw), limit)
        return None

    if tip in ("bit", "boolean", "bool"):
        if _as_int(value) is None:
            return F_TIP, "valoare care nu este 0 sau 1: «%s»" % _short(value)
        return None

    # Tip pe care nu-l cunoastem: nu inventam o regula, il lasam sa treaca si sa
    # fie MariaDB cel care refuza, cu eroarea lui.
    return None


def _short(value):
    text = str(value)
    return text if len(text) <= 60 else text[:57] + "…"


def as_int(value):
    """Public: aceleași reguli și la pasul de scriere."""
    return _as_int(value)


def _as_int(value):
    if isinstance(value, bool):
        return int(value)
    if isinstance(value, int):
        return value
    if isinstance(value, float):
        return int(value) if float(value).is_integer() else None
    try:
        return int(str(value).strip())
    except (ValueError, TypeError):
        return None


def _as_decimal(value):
    try:
        return decimal.Decimal(str(value).strip())
    except (decimal.InvalidOperation, ValueError, TypeError):
        return None


def _as_float(value):
    try:
        return float(str(value).strip())
    except (ValueError, TypeError):
        return None


def _as_datetime(value):
    if isinstance(value, (datetime.datetime, datetime.date)):
        return value
    text = str(value).strip()
    if not text:
        return None
    for fmt in _DATE_FORMATS:
        try:
            return datetime.datetime.strptime(text, fmt)
        except ValueError:
            continue
    return None


# -----------------------------------------------------------------------------
# Raportul
# -----------------------------------------------------------------------------

class Report(object):

    def __init__(self, db_name):
        self.db_name = db_name
        self._buckets = {}      # (tabel, coloana, fel) -> {"număr", "exemple"}
        self.per_table = {}     # tabel -> {"citite", "rutate", "de_scris", "sărite"}
        # Valorile de cheie straina care LIPSESC pe tinta, pastrate ca sa poata fi
        # sarite la rulare fara sa mai intrebam serverul inca o data.
        self.missing_fk = {}    # (tabel, coloana) -> set(valori)

    def add(self, table, column, kind, key, message, value=None):
        bucket = self._buckets.setdefault((table, column, kind),
                                          {"număr": 0, "exemple": []})
        bucket["număr"] += 1
        if len(bucket["exemple"]) < MAX_EXAMPLES:
            bucket["exemple"].append({
                "cheie": key,
                "mesaj": message,
                "valoare": None if value is None else _short(value),
            })

    def counts(self):
        stats = {}
        for (table, column, kind), bucket in self._buckets.items():
            stats[kind] = stats.get(kind, 0) + bucket["număr"]
        return stats

    def has_blocking(self):
        return any(CLASS_OF.get(kind, BLOCANT) == BLOCANT
                   for (_, _, kind) in self._buckets)

    def is_clean(self):
        return not self._buckets

    def to_dict(self):
        constatari = []
        for (table, column, kind), bucket in sorted(self._buckets.items()):
            constatari.append({
                "tabel": table,
                "coloana": column,
                "fel": kind,
                "clasa": CLASS_OF.get(kind, BLOCANT),
                "număr": bucket["număr"],
                "exemple": bucket["exemple"],
            })
        # Blocantele primele: alea decid daca porneste ceva.
        constatari.sort(key=lambda c: (c["clasa"] != BLOCANT, c["tabel"], c["fel"]))
        return {
            "baza": self.db_name,
            "curat": self.is_clean(),
            "are_blocante": self.has_blocking(),
            "poate_rula": self.is_clean(),
            "poate_forța": (not self.is_clean()) and (not self.has_blocking()),
            "pe_fel": self.counts(),
            "pe_tabel": self.per_table,
            "constatări": constatari,
        }


# -----------------------------------------------------------------------------
# Analiza
# -----------------------------------------------------------------------------

def analyze(conn, db_name, fx_path, plan, progress=None):
    """
    Citeste fisierul o data, ruteaza fiecare rand, pastreaza randurile care ajung
    in `db_name` si le masoara fata de schema tintei.

    `plan` e RoutingPlan-ul rezolvat inainte: fie DIRECT (fisier cu o singura
    unitate, totul merge in baza aleasa), fie PRIN_CAI (mai multe unitati).

    Intoarce un Report. Nu scrie nimic, nicaieri.
    """
    def say(msg):
        logger.info("migrare/analiză: %s", msg)
        if progress:
            progress(msg)

    report = Report(db_name)
    schema = TargetSchema(conn, db_name, [t.name for t in tables.ALL])

    for table in tables.ALL:
        stats = {"citite": 0, "rutate": 0, "de_scris": 0, "sărite": 0}
        report.per_table[table.name] = stats

        if not schema.has(table.name):
            report.add(table.name, "", F_TABEL_LIPSA, "",
                       "tabelul lipsește din baza «%s»" % db_name)
            continue

        say("Se verifică «%s»." % table.name)
        target_columns = schema.columns[table.name]
        router = plan.router_for(table)
        pk_columns = schema.primary_key.get(table.name) or [table.primary_key]
        seen_keys = set()
        unknown_reported = set()
        # {(coloana): set(valori)} pentru fiecare cheie straina a tabelului
        fk_values = dict(((fk["coloana"], fk["nume"]), {})
                         for fk in schema.foreign_keys.get(table.name, []))
        ddf_values = dict((c, {}) for c in table.ddf_columns if c in target_columns)

        for row in _iter_table(fx_path, table.name, say):
            stats["citite"] += 1
            key = router.primary_key_of(row)

            dcs, reject = router.route(row)
            if reject:
                report.add(table.name, "", F_RUTARE, key, reject)
                continue
            if db_name not in dcs:
                continue
            stats["rutate"] += 1

            row_ok = True

            # coloane care exista in Access si lipsesc din tinta
            for column in row.keys():
                if column not in target_columns and column not in unknown_reported:
                    unknown_reported.add(column)
                    report.add(table.name, column, F_COLOANA_LIPSA, key,
                               "coloana există în Access și lipsește din «%s»" % db_name)
                    row_ok = False

            # valorile, fata de coloanele tintei
            for name, meta in target_columns.items():
                if meta["auto"] and name not in row:
                    continue
                problem = check_value(meta, row.get(name))
                if problem:
                    kind, message = problem
                    report.add(table.name, name, kind, key, message, row.get(name))
                    row_ok = False

            # cheie primara dubla in interiorul fisierului
            pk_value = tuple(row.get(c) for c in pk_columns)
            if pk_value in seen_keys:
                report.add(table.name, ",".join(pk_columns), F_CHEIE_DUBLA, key,
                           "cheia primară apare de mai multe ori în fișier")
                row_ok = False
            else:
                seen_keys.add(pk_value)

            # valorile de cheie straina, stranse acum si verificate dupa
            for (column, name), collected in fk_values.items():
                value = row.get(column)
                if value is not None and value != "":
                    collected.setdefault(value, key)
            for column, collected in ddf_values.items():
                value = row.get(column)
                number = _as_int(value)
                if number is not None and number != 0:
                    collected.setdefault(number, key)

            if row_ok:
                stats["de_scris"] += 1
            else:
                stats["sărite"] += 1

        _check_foreign_keys(conn, schema, table, fk_values, report, say)
        _check_ddf_ids(conn, db_name, table, ddf_values, report, say)

    return report


def _iter_table(fx_path, table_name, say):
    from . import accdb
    try:
        for row in accdb.iter_rows(fx_path, table_name):
            yield row
    except accdb.AccdbError as exc:
        raise ValidationError(
            "Tabelul «%s» nu a putut fi citit din fișierul Access: %s" % (table_name, exc))


def _check_foreign_keys(conn, schema, table, fk_values, report, say):
    """
    O interogare pe cheie straina, in loturi. Nu incarcam tabelul referit in
    memorie: pot fi nomenclatoare de zeci de mii de randuri.
    """
    for fk in schema.foreign_keys.get(table.name, []):
        collected = fk_values.get((fk["coloana"], fk["nume"]))
        if not collected:
            continue
        say("Se verifică cheia străină «%s» (%d valori distincte)."
            % (fk["nume"], len(collected)))

        ref = "`%s`.`%s`" % (fk["schema_ref"], fk["tabel_ref"])
        values = list(collected.keys())
        present = set()
        cur = conn.cursor()
        try:
            for start in range(0, len(values), FK_BATCH):
                batch = values[start:start + FK_BATCH]
                placeholders = ",".join(["%s"] * len(batch))
                cur.execute(
                    "SELECT DISTINCT `%s` FROM %s WHERE `%s` IN (%s)"
                    % (fk["coloana_ref"], ref, fk["coloana_ref"], placeholders),
                    tuple(batch))
                for (found,) in cur.fetchall():
                    present.add(found)
        finally:
            cur.close()

        missing = [v for v in values if v not in present]
        if missing:
            report.missing_fk.setdefault((table.name, fk["coloana"]), set()).update(missing)
            for value in missing:
                report.add(table.name, fk["coloana"], F_CHEIE_STRAINA, collected[value],
                           "valoarea nu există în %s.%s"
                           % (fk["tabel_ref"], fk["coloana_ref"]), value)


def _check_ddf_ids(conn, db_name, table, ddf_values, report, say):
    """
    IDDF / IDREV sunt AUTO_INCREMENT pe MariaDB si nu pastreaza id-ul Access
    alaturi, deci potrivirea celor doua parti e o presupunere. Se VERIFICA,
    niciodata nu se traduce.
    """
    for column, collected in ddf_values.items():
        if not collected:
            continue
        ref_table, ref_column = tables.DDF_ID_TABLE[column]
        say("Se verifică id-urile «%s» (%d valori)." % (column, len(collected)))

        values = list(collected.keys())
        present = set()
        cur = conn.cursor()
        try:
            for start in range(0, len(values), FK_BATCH):
                batch = values[start:start + FK_BATCH]
                placeholders = ",".join(["%s"] * len(batch))
                cur.execute(
                    "SELECT `%s` FROM `%s`.`%s` WHERE `%s` IN (%s)"
                    % (ref_column, db_name, ref_table, ref_column, placeholders),
                    tuple(batch))
                for (found,) in cur.fetchall():
                    present.add(int(found))
        finally:
            cur.close()

        missing = [v for v in values if v not in present]
        if missing:
            report.missing_fk.setdefault((table.name, column), set()).update(missing)
            for value in missing:
                report.add(table.name, column, F_DDF_LIPSA, collected[value],
                           "id-ul nu există în %s.%s" % (ref_table, ref_column), value)
