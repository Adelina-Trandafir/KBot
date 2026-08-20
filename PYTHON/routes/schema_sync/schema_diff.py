"""
schema_diff.py -- compare AVACONT_SURSA against a target and emit DDL.

Replaces proc_SchemaDiff_DDL and proc_SchemaDiff_CreateTable. Every rule
from those procedures is here, plus the collation repair, plus the
charset fixes they were missing.

Two things the SQL version got wrong that are fixed here:

  * Column definitions are emitted WITH their charset and collation.
    The procedures used COLUMN_TYPE alone, which carries no charset, so
    every added, renamed or recreated column silently took the target
    table's default -- which is the mismatch the collation repair exists
    to remove.

  * CREATE TABLE now carries ENGINE, DEFAULT CHARSET and ROW_FORMAT.
    Without ENGINE=InnoDB the foreign keys in the same statement are
    accepted and ignored by some engines.

SAFE vs FORCE is unchanged: SAFE adds only. FORCE also modifies and
drops. Collation repair runs in BOTH -- it is a correctness fix, and
leaving a mismatch in place makes later key creation fail.
"""

from dataclasses import dataclass

from .schema_common import SOURCE_DB, priority_of
from .schema_introspect import read_schema

# Rules that never route through the diff.
SYSTEM_SCHEMAS = {"information_schema", "performance_schema", "mysql", "sys"}


def same_schema(a: str, b: str) -> bool:
    """True when two schema names denote the same schema.

    Compared case-insensitively on purpose. information_schema reports
    REFERENCED_TABLE_SCHEMA in whatever case the server stores it, and a
    server running with lower_case_table_names=1 folds it -- so an exact
    match against the SOURCE_DB constant silently fails there. What that
    failure costs is severe and quiet: the self-schema rewrite stops
    firing, and a unit ends up with a foreign key pointing into
    AVACONT_SURSA, the template every unit is cloned from.
    """
    if a is None or b is None:
        return False
    return a.lower() == b.lower()


@dataclass
class Statement:
    target_db: str
    table_name: str
    object_name: str
    object_type: str
    action_type: str
    ddl_sql: str
    is_destructive: bool = False
    error_msg: str = None

    @property
    def priority(self) -> int:
        return priority_of(self.object_type, self.action_type)


# ---------------------------------------------------------------------
# Fragment builders
# ---------------------------------------------------------------------

def q(name: str) -> str:
    """Backtick-quote an identifier, escaping embedded backticks."""
    return "`" + str(name).replace("`", "``") + "`"


def quote_literal(value: str) -> str:
    """Single-quote a string literal for DDL."""
    return "'" + str(value).replace("\\", "\\\\").replace("'", "''") + "'"


# Defaults that are functions, not literals, and must never be quoted.
_FUNCTION_DEFAULTS = {
    "current_timestamp", "current_timestamp()", "now()",
    "current_date", "current_date()", "curdate()",
    "current_time", "current_time()", "curtime()",
    "utc_timestamp", "utc_timestamp()", "utc_date()", "utc_time()",
    "uuid()", "null",
}

_NUMERIC_TYPES = {
    "int", "bigint", "smallint", "tinyint", "mediumint", "decimal", "numeric",
    "float", "double", "bit", "year",
}


def format_default(col) -> str:
    """Render the DEFAULT clause for one column, or '' if there is none.

    This is what fn_sql_default did on the server side, with the
    expression-vs-raw distinction handled explicitly instead of guessed.
    """
    if col.is_auto_increment:
        return ""
    if col.is_generated:
        return ""

    raw = col.default

    if raw is None:
        # No default at all. A nullable column defaults to NULL anyway;
        # spelling it out keeps generated DDL comparable.
        return " DEFAULT NULL" if col.is_nullable else ""

    text = str(raw)

    if col.expr_defaults:
        # MariaDB 10.2.7+: already a valid expression. The one case to
        # normalise is the literal string "NULL", which means DEFAULT NULL.
        if text.upper() == "NULL":
            return " DEFAULT NULL"
        return f" DEFAULT {text}"

    # Older servers hand back raw values.
    lowered = text.lower().strip()
    if lowered in _FUNCTION_DEFAULTS:
        return " DEFAULT NULL" if lowered == "null" else f" DEFAULT {text}"
    if col.data_type in _NUMERIC_TYPES:
        return f" DEFAULT {text}"
    return f" DEFAULT {quote_literal(text)}"


def format_charset(col) -> str:
    """CHARACTER SET / COLLATE for a character column, else ''.

    Always emitted. This is the fix for the whole class of bugs where a
    column inherits the wrong charset from its table.
    """
    if not col.charset:
        return ""
    return f" CHARACTER SET {col.charset} COLLATE {col.collation}"


def column_definition(col, name: str = None, comment: str = None) -> str:
    """Full column definition as it appears in CREATE or ALTER.

    `name` overrides the column name (used by CHANGE COLUMN).
    `comment` overrides the comment (used to strip a rename: marker).
    """
    parts = [q(name or col.name), " ", col.column_type]
    parts.append(format_charset(col))
    parts.append("" if col.is_nullable else " NOT NULL")
    if col.is_nullable and col.default is None and not col.is_generated:
        pass  # format_default emits DEFAULT NULL below
    parts.append(format_default(col))
    if col.extra:
        parts.append(f" {col.extra}")
    final_comment = col.effective_comment if comment is None else comment
    if final_comment:
        parts.append(f" COMMENT {quote_literal(final_comment)}")
    return "".join(parts)


def columns_differ(src, tgt) -> bool:
    """True when anything other than charset differs.

    effective_comment, not comment: a rename: marker is an instruction,
    and comparing it raw would make a renamed column look permanently
    out of sync with itself.
    """
    return (src.column_type != tgt.column_type
            or src.is_nullable != tgt.is_nullable
            or _norm_default(src) != _norm_default(tgt)
            or (src.extra or "") != (tgt.extra or "")
            or src.effective_comment != tgt.effective_comment)


def _norm_default(col) -> str:
    """Normalise a default so 'NULL' and None compare equal."""
    if col.default is None:
        return None
    text = str(col.default)
    return None if text.upper() == "NULL" else text


def charset_differs(src, tgt) -> bool:
    if not src.charset or not tgt.charset:
        return False
    return (src.charset != tgt.charset or src.collation != tgt.collation)


def is_narrowing(src, tgt) -> bool:
    """True when the target loses representational range.

    utf8mb4 -> anything else drops supplementary-plane characters.
    Romanian diacritics are BMP and survive; emoji and rare CJK do not.
    """
    return tgt.charset == "utf8mb4" and src.charset != "utf8mb4"


# ---------------------------------------------------------------------
# The diff
# ---------------------------------------------------------------------

class SchemaDiff:
    """Compares one target against the source and produces Statements."""

    def __init__(self, source, target, target_db, mode, logger):
        self.src = source
        self.tgt = target
        self.db = target_db
        self.force = (mode.upper() == "FORCE")
        self.mode = mode.upper()
        self.log = logger
        self.statements = []
        # (table, column) pairs whose charset or collation is being
        # changed -- by the collation repair OR by a FORCE-mode MODIFY /
        # RENAME that carries a charset change with it. All three are
        # blocked by a foreign key in exactly the same way, so all three
        # must register here or the unblocking pass misses them.
        self._recollated = set()
        # (table, fk_name) already scheduled, so the unblocking pass
        # below does not emit a second DROP or a second ADD.
        self._fk_dropped = set()
        self._fk_created = set()

    # -- helper ------------------------------------------------------
    def _emit(self, table, obj_name, obj_type, action, sql,
              destructive=False, error=None):
        self.statements.append(Statement(
            target_db=self.db, table_name=table, object_name=obj_name,
            object_type=obj_type, action_type=action, ddl_sql=sql,
            is_destructive=destructive, error_msg=error))

    def _t(self, table_name) -> str:
        return f"{q(self.db)}.{q(table_name)}"

    # -- entry point -------------------------------------------------
    def run(self) -> list:
        self._tables()
        for name, src_tbl in sorted(self.src.tables.items()):
            tgt_tbl = self.tgt.tables.get(name)
            if tgt_tbl is None:
                continue          # handled by CREATE TABLE
            self._columns(src_tbl, tgt_tbl)
            self._primary_key(src_tbl, tgt_tbl)
            self._indexes(src_tbl, tgt_tbl)
            self._foreign_keys(src_tbl, tgt_tbl)
        self._unblock_collation()
        return self.statements

    # -- 1 / 2: tables -----------------------------------------------
    def _tables(self):
        for name in sorted(set(self.tgt.tables) - set(self.src.tables)):
            self._emit(name, None, "TABLE", "DROP",
                       f"DROP TABLE {self._t(name)};", destructive=True)

        for name in sorted(set(self.src.tables) - set(self.tgt.tables)):
            self._emit(name, None, "TABLE", "CREATE",
                       self._create_table(self.src.tables[name]))

    def _create_table(self, tbl) -> str:
        # A brand-new table has nothing to rename FROM, so the rename:
        # marker must not be written into it -- otherwise the marker
        # outlives the cleanup pass and the column carries a stale
        # instruction as its comment for good.
        lines = ["  " + column_definition(c, comment=c.comment_after_rename)
                 for c in tbl.columns_in_order()]

        pk = tbl.primary_key
        if pk:
            cols = ", ".join(q(c) for c in pk.columns)
            lines.append(f"  PRIMARY KEY ({cols})")

        for idx in sorted(tbl.indexes.values(), key=lambda i: i.name):
            if idx.is_primary:
                continue
            cols = ", ".join(q(c) for c in idx.columns)
            unique = "UNIQUE " if idx.unique else ""
            lines.append(f"  {unique}INDEX {q(idx.name)} ({cols})")

        for fk in sorted(tbl.foreign_keys.values(), key=lambda f: f.name):
            lines.append("  " + self._fk_clause(fk))

        # ENGINE / CHARSET / ROW_FORMAT taken from the source table, not
        # left to the server default: the FKs above need InnoDB, and the
        # charset must not be inherited from the target database.
        engine = tbl.engine or "InnoDB"
        charset = tbl.charset or "utf8"
        collation = tbl.collation or "utf8_general_ci"
        row_format = tbl.row_format or "Dynamic"

        return (f"CREATE TABLE {self._t(tbl.name)} (\n"
                + ",\n".join(lines)
                + f"\n) ENGINE={engine} DEFAULT CHARSET={charset} "
                  f"COLLATE={collation} ROW_FORMAT={row_format};")

    def _fk_clause(self, fk) -> str:
        cols = ", ".join(q(c) for c in fk.columns)
        ref_cols = ", ".join(q(c) for c in fk.ref_columns)
        # A reference into the source schema means "this unit's own
        # schema" -- rewrite it. A reference elsewhere (AVACONT_COMUN)
        # is left alone.
        ref_schema = (self.db if same_schema(fk.ref_schema, SOURCE_DB)
                      else fk.ref_schema)
        return (f"CONSTRAINT {q(fk.name)} FOREIGN KEY ({cols}) "
                f"REFERENCES {q(ref_schema)}.{q(fk.ref_table)} ({ref_cols}) "
                f"ON DELETE {fk.delete_rule} ON UPDATE {fk.update_rule}")

    # -- 3 / 4: columns ----------------------------------------------
    def _columns(self, src_tbl, tgt_tbl):
        # Old names referenced by a rename: marker -- must not be dropped.
        rename_sources = {c.rename_from for c in src_tbl.columns.values()
                          if c.rename_from}

        for col in src_tbl.columns_in_order():
            if col.is_generated:
                continue                       # generated columns skipped
            old_name = col.rename_from
            tgt_col = tgt_tbl.columns.get(col.name)

            # A rename is only OUTSTANDING while the new name is absent
            # from the target. Once it exists -- because the rename ran
            # earlier, or the table was created fresh -- the column is
            # an ordinary column and must go through the normal checks,
            # or its collation would never be repaired.
            if old_name and tgt_col is None:
                self._rename_column(src_tbl, tgt_tbl, col, old_name)
                continue

            if tgt_col is None:
                self._add_column(src_tbl, col)
            elif columns_differ(col, tgt_col):
                if self.force:
                    self._modify_column(src_tbl, col, tgt_col)
            elif charset_differs(col, tgt_col):
                self._repair_collation(src_tbl, col, tgt_col)

        # DROP COLUMN: in the target, absent from the source, and not the
        # old half of a pending rename.
        for name in sorted(set(tgt_tbl.columns) - set(src_tbl.columns)):
            if name in rename_sources:
                continue
            if tgt_tbl.columns[name].is_generated:
                continue
            self._emit(tgt_tbl.name, name, "COLUMN", "DROP",
                       f"ALTER TABLE {self._t(tgt_tbl.name)} "
                       f"DROP COLUMN {q(name)};", destructive=True)

    def _add_column(self, tbl, col):
        sql = (f"ALTER TABLE {self._t(tbl.name)} "
               f"ADD COLUMN {column_definition(col)}")
        # An auto_increment column must be a key from the moment it
        # exists. If the table already has a PK, replace it in the same
        # statement -- MariaDB will not tolerate the gap.
        if col.is_auto_increment:
            tgt_tbl = self.tgt.tables.get(tbl.name)
            has_pk = tgt_tbl is not None and tgt_tbl.primary_key is not None
            if has_pk:
                sql += f", DROP PRIMARY KEY, ADD PRIMARY KEY ({q(col.name)})"
            else:
                sql += " PRIMARY KEY"
        self._emit(tbl.name, col.name, "COLUMN", "ADD", sql + ";")

    def _modify_column(self, tbl, col, tgt_col=None):
        self._emit(tbl.name, col.name, "COLUMN", "MODIFY",
                   f"ALTER TABLE {self._t(tbl.name)} "
                   f"MODIFY COLUMN {column_definition(col)};")
        # A MODIFY that also moves the charset is blocked by a foreign
        # key just as the collation repair is -- errno 1832 on the
        # referencing side, 1833 on the referenced one. Register it so
        # the unblocking pass drops the key first.
        if tgt_col is not None and charset_differs(col, tgt_col):
            self._recollated.add((tbl.name, col.name))

    def _repair_collation(self, tbl, src_col, tgt_col):
        """Charset-only change. Runs in SAFE as well as FORCE.

        The definition is otherwise the TARGET's, so nothing but charset
        and collation moves. Type changes stay the job of MODIFY COLUMN.
        """
        definition = column_definition(tgt_col)
        # Swap the target's charset clause for the source's.
        old_clause = format_charset(tgt_col)
        new_clause = format_charset(src_col)
        definition = definition.replace(old_clause, new_clause, 1)
        self._emit(tbl.name, src_col.name, "COLLATION", "MODIFY",
                   f"ALTER TABLE {self._t(tbl.name)} "
                   f"MODIFY COLUMN {definition};",
                   destructive=is_narrowing(src_col, tgt_col))
        self._recollated.add((tbl.name, src_col.name))

    def _rename_column(self, src_tbl, tgt_tbl, col, old_name):
        """CHANGE COLUMN old -> new, driven by a rename: comment.

        Only called when the new name is absent from the target.
        """
        if old_name not in tgt_tbl.columns:
            self._emit(tgt_tbl.name, col.name, "COLUMN", "RENAME", "",
                       error=(f"RENAME imposibil: coloana veche "
                              f"`{old_name}` nu există în "
                              f"`{self.db}`.`{tgt_tbl.name}`"))
            return
        definition = column_definition(col, comment=col.comment_after_rename)
        self._emit(tgt_tbl.name, col.name, "COLUMN", "RENAME",
                   f"ALTER TABLE {self._t(tgt_tbl.name)} "
                   f"CHANGE COLUMN {q(old_name)} {definition};")
        # Same block as a MODIFY when the rename also moves the charset.
        # Registered under the OLD name: that is the name any existing
        # foreign key still refers to at the moment the key is dropped.
        if charset_differs(col, tgt_tbl.columns[old_name]):
            self._recollated.add((tgt_tbl.name, old_name))

    # -- 5 / 6 / 7: primary key --------------------------------------
    def _primary_key(self, src_tbl, tgt_tbl):
        src_pk = src_tbl.primary_key
        tgt_pk = tgt_tbl.primary_key

        if src_pk is None and tgt_pk is not None:
            self._emit(tgt_tbl.name, "PRIMARY", "PK", "DROP",
                       f"ALTER TABLE {self._t(tgt_tbl.name)} "
                       f"DROP PRIMARY KEY;", destructive=True)
            return

        if src_pk is None:
            return

        # An ADD COLUMN ... auto_increment already carried its own PK
        # clause; adding another here would be a duplicate.
        if self._pk_handled_by_add_column(src_tbl, tgt_tbl, src_pk):
            return

        cols = ", ".join(q(c) for c in src_pk.columns)

        if tgt_pk is None:
            self._emit(src_tbl.name, "PRIMARY", "PK", "CREATE",
                       f"ALTER TABLE {self._t(src_tbl.name)} "
                       f"ADD PRIMARY KEY ({cols});")
        elif src_pk.columns != tgt_pk.columns and self.force:
            # Combined: MariaDB will not leave an auto_increment column
            # keyless, even momentarily.
            self._emit(src_tbl.name, "PRIMARY", "PK", "MODIFY",
                       f"ALTER TABLE {self._t(src_tbl.name)} "
                       f"DROP PRIMARY KEY, ADD PRIMARY KEY ({cols});",
                       destructive=True)

    def _pk_handled_by_add_column(self, src_tbl, tgt_tbl, src_pk) -> bool:
        """True when the PK will be created by an ADD COLUMN statement."""
        for name in src_pk.columns:
            col = src_tbl.columns.get(name)
            if col is None:
                continue
            if col.is_auto_increment and name not in tgt_tbl.columns:
                return True
        return False

    # -- 8 / 9 / 10 / 11: indexes ------------------------------------
    def _indexes(self, src_tbl, tgt_tbl):
        src_idx = {n: i for n, i in src_tbl.indexes.items() if not i.is_primary}
        tgt_idx = {n: i for n, i in tgt_tbl.indexes.items() if not i.is_primary}

        for name in sorted(set(tgt_idx) - set(src_idx)):
            self._emit(tgt_tbl.name, name, "INDEX", "DROP",
                       f"DROP INDEX {q(name)} ON {self._t(tgt_tbl.name)};",
                       destructive=True)

        for name in sorted(src_idx):
            s = src_idx[name]
            t = tgt_idx.get(name)
            if t is None:
                self._emit(src_tbl.name, name, "INDEX", "CREATE",
                           self._create_index(src_tbl.name, s))
            elif (s.columns != t.columns or s.unique != t.unique) and self.force:
                self._emit(src_tbl.name, name, "INDEX", "DROP",
                           f"DROP INDEX {q(name)} ON {self._t(src_tbl.name)};",
                           destructive=True)
                self._emit(src_tbl.name, name, "INDEX", "CREATE",
                           self._create_index(src_tbl.name, s))

    def _create_index(self, table_name, idx) -> str:
        cols = ", ".join(q(c) for c in idx.columns)
        unique = "UNIQUE " if idx.unique else ""
        return (f"CREATE {unique}INDEX {q(idx.name)} "
                f"ON {self._t(table_name)} ({cols});")

    # -- 12 / 13 / 14 / 15: foreign keys -----------------------------
    def _foreign_keys(self, src_tbl, tgt_tbl):
        for name in sorted(set(tgt_tbl.foreign_keys)
                           - set(src_tbl.foreign_keys)):
            self._drop_fk(tgt_tbl.name, name)

        for name in sorted(src_tbl.foreign_keys):
            s = src_tbl.foreign_keys[name]
            t = tgt_tbl.foreign_keys.get(name)
            if t is None:
                self._create_fk(src_tbl.name, name, s)
            elif s.signature() != t.signature() and self.force:
                self._drop_fk(src_tbl.name, name)
                self._create_fk(src_tbl.name, name, s)

    def _drop_fk(self, table_name, fk_name):
        if (table_name, fk_name) in self._fk_dropped:
            return
        self._fk_dropped.add((table_name, fk_name))
        self._emit(table_name, fk_name, "FK", "DROP",
                   f"ALTER TABLE {self._t(table_name)} "
                   f"DROP FOREIGN KEY {q(fk_name)};", destructive=True)

    def _create_fk(self, table_name, fk_name, fk):
        if (table_name, fk_name) in self._fk_created:
            return
        self._fk_created.add((table_name, fk_name))
        self._emit(table_name, fk_name, "FK", "CREATE",
                   f"ALTER TABLE {self._t(table_name)} "
                   f"ADD {self._fk_clause(fk)};")

    # -- unblocking a collation change locked under a foreign key ----
    def _unblock_collation(self):
        """Drop and recreate any FK that would block a collation change.

        MariaDB refuses to alter the charset of a column that takes part
        in a foreign key -- errno 1832, "Cannot change column: used in a
        foreign key constraint", on the referencing side, and errno 1833
        on the referenced one. Both verified on 10.3.32, not assumed.

        The ordinary FK diff only drops keys that DIFFER from the source.
        A key that is identical on both sides is never dropped, and it
        blocks the repair. So every FK touching a recollated column --
        on either side of the reference -- gets a DROP at priority 1 and
        an ADD back at priority 11, with the collation change at 8 in
        between.
        """
        if not self._recollated:
            return

        for tbl in self.tgt.tables.values():
            src_tbl = self.src.tables.get(tbl.name)
            if src_tbl is None:
                # The table itself is being dropped (priority 5). Its
                # keys go with it, and an ADD CONSTRAINT at priority 11
                # would land on a table that no longer exists -- errno
                # 1146.
                continue
            for name, fk in tbl.foreign_keys.items():
                if not self._fk_blocks(fk):
                    continue
                # The DROP is what actually unblocks the column, so ask
                # for it unconditionally. _drop_fk is idempotent, so a
                # key the ordinary diff already dropped is not dropped
                # twice.
                self._drop_fk(fk.table, name)
                src_fk = src_tbl.foreign_keys.get(name)
                if src_fk is None:
                    # A key the source does not have. The ordinary diff
                    # dropped it deliberately; putting it back here
                    # would resurrect it, silently and without error.
                    continue
                # Rebuilt from the SOURCE definition, so the key comes
                # back in its intended shape.
                self._create_fk(fk.table, name, src_fk)

    def _fk_blocks(self, fk) -> bool:
        """True when this FK touches a column whose charset is changing.

        Both directions matter: the referencing column and the
        referenced one are equally locked.
        """
        for col in fk.columns:
            if (fk.table, col) in self._recollated:
                return True
        # A key pointing INTO a recollated column, within this database.
        if (same_schema(fk.ref_schema, self.db)
                or same_schema(fk.ref_schema, SOURCE_DB)):
            for col in fk.ref_columns:
                if (fk.ref_table, col) in self._recollated:
                    return True
        return False


# ---------------------------------------------------------------------
# Rename cleanup and the top-level driver
# ---------------------------------------------------------------------

def rename_cleanup(source, mode) -> list:
    """Strip rename: markers from AVACONT_SURSA once every target is done.

    Emitted once, not once per target, and run last (priority 99).
    """
    out = []
    for tbl in sorted(source.tables.values(), key=lambda t: t.name):
        for col in tbl.columns_in_order():
            if not col.rename_from:
                continue
            definition = column_definition(
                col, comment=col.comment_after_rename)
            # An empty comment must be written explicitly to clear it.
            if not col.comment_after_rename:
                definition += " COMMENT ''"
            out.append(Statement(
                target_db=SOURCE_DB, table_name=tbl.name,
                object_name=col.name, object_type="RENAME_CLEANUP",
                action_type="MODIFY",
                ddl_sql=(f"ALTER TABLE {q(SOURCE_DB)}.{q(tbl.name)} "
                         f"MODIFY COLUMN {definition};")))
    return out


def build_diff(conn, targets, mode, logger) -> list:
    """Compare every target against the source. Returns Statements in
    execution order."""
    from .schema_common import server_version

    is_mariadb, version = server_version(conn)
    logger.info("Server: %s (defaults ca %s).", version,
                "expresii" if is_mariadb else "valori brute")

    source = read_schema(conn, SOURCE_DB, expr_defaults=is_mariadb)
    logger.info("Sursa %s: %d tabele.", SOURCE_DB, len(source.tables))

    statements = []
    for db in targets:
        if db in SYSTEM_SCHEMAS:
            raise ValueError(f"Schema de sistem ca țintă: {db}")
        target = read_schema(conn, db, expr_defaults=is_mariadb)
        if not target.exists:
            logger.warning("Baza `%s` nu există — ignorată.", db)
            continue
        produced = SchemaDiff(source, target, db, mode, logger).run()
        logger.info("  %s: %d instrucțiuni.", db, len(produced))
        statements.extend(produced)

    statements.extend(rename_cleanup(source, mode))
    statements.sort(key=lambda s: s.priority)
    return statements
