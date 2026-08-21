# routes/migrare/tables.py
# -----------------------------------------------------------------------------
# The 16 migrated FX_ tables, in write order (parents before children), and the
# rule that says which unit each row belongs to. The set is identical to
# ALLOWED_TABLES in routes/forexe/seed.py.
#
# The list is FIXED. Nothing is discovered from relationships and no prefix is
# matched: discovery is exactly what falls over on this schema.
# -----------------------------------------------------------------------------

# --- selection kinds ---------------------------------------------------------
OWN_DC_THEN_UNIT = "own_dc_then_unit"   # its own DC column, else IdUnitate
OWN_UNIT = "own_unit"                   # its own IdUnitate
BY_ANGAJAMENT = "by_angajament"         # CodAngajament, through the commitment set
BY_REZERVARE = "by_rezervare"           # IDRZ, through the reservation set
TWO_PARENTS = "two_parents"             # two candidate parents
BY_EXTRAS = "by_extras"                 # IDEXF, through the FX_Extrase_H headers


class SeedTable(object):
    """Description of one migrated table. No logic."""

    def __init__(self, name, primary_key, selection,
                 key_column=None, key_column2=None, ddf_columns=None):
        self.name = name
        self.primary_key = primary_key
        self.selection = selection
        self.key_column = key_column
        self.key_column2 = key_column2
        # The columns pointing at the DDF family (IDDF / IDREV). They are
        # CHECKED against MariaDB before writing, never translated: the ids there
        # are AUTO_INCREMENT and do not keep the Access id alongside.
        self.ddf_columns = list(ddf_columns or [])


ALL = [
    # FX_Angajamente is the root, and the only table carrying both IdUnitate and
    # DC: that is where the chosen database's IdUnitate comes from.
    SeedTable("FX_Angajamente", "CodAngajament", OWN_DC_THEN_UNIT, ddf_columns=["IDDF"]),
    SeedTable("FX_Indicatori", "CodAI", OWN_UNIT),
    SeedTable("FX_Istoric", "ID", BY_ANGAJAMENT, key_column="CodAngajament",
              ddf_columns=["IDREV"]),
    SeedTable("FX_Salarii", "IDFXS", BY_ANGAJAMENT, key_column="CodAngajament",
              ddf_columns=["IDDF", "IDREV"]),
    SeedTable("FX_Rezervari", "IDRZ", BY_ANGAJAMENT, key_column="CodAngajament",
              ddf_columns=["IDREV"]),
    SeedTable("FX_Rezervarii_IMG", "IDRZC", BY_REZERVARE, key_column="IDRZ"),
    SeedTable("FX_Receptii_R", "IDRR", BY_ANGAJAMENT, key_column="CodAngajament"),
    SeedTable("FX_Receptii_H", "IDRH", BY_ANGAJAMENT, key_column="CodAngajament"),
    SeedTable("FX_Receptii", "IDR", OWN_UNIT, ddf_columns=["IDREV"]),
    SeedTable("FX_Receptii_RHR", "IDRHR", OWN_UNIT),
    # Two parents: IDRR first, then IDRH.
    SeedTable("FX_Receptii_IMG", "IDRDC", TWO_PARENTS,
              key_column="IDRR", key_column2="IDRH"),
    SeedTable("FX_Plati", "IdPlataFX", OWN_UNIT, ddf_columns=["IDREV"]),
    # Two parents, in the reverse order of the one above.
    SeedTable("FX_Receptii_Plati", "IDRP", TWO_PARENTS,
              key_column="IDRH", key_column2="IDRR"),
    # A statement file can carry lines of several units; it is ours if at least
    # one of its FX_Extrase_H headers belongs to the chosen unit.
    SeedTable("FX_Extrase_F", "IDEXF", BY_EXTRAS, key_column="IDEXF"),
    SeedTable("FX_Extrase_H", "IDEXH", OWN_UNIT),
    SeedTable("FX_Extrase", "IDFXE", OWN_UNIT),
]

BY_NAME = dict((t.name, t) for t in ALL)

# The MariaDB table behind each checked DDF id.
DDF_ID_TABLE = {"IDDF": ("FX_DDF", "IDDF"), "IDREV": ("FX_DDF_REV", "IDREV")}

# Declared out of scope. If they appear in the file they are REPORTED, not migrated.
OUT_OF_SCOPE = ("FX_PRT_EXPL", "FX_CopacAngajamente")


def by_name(name):
    """Unknown key -> exception, never a silent no-op."""
    table = BY_NAME.get(name)
    if table is None:
        raise KeyError("Tabelul «%s» nu face parte din setul migrat." % name)
    return table


def selected(names):
    """
    The tables the operator ticked, in WRITE ORDER (parents before children), not
    in the order he ticked them. `None` means all of them.

    A name outside the migrated set raises; it is never ignored silently, or the
    operator would believe he ticked something that does not get written.
    """
    if names is None:
        return list(ALL)
    wanted = set()
    for name in names:
        by_name(name)
        wanted.add(name)
    return [t for t in ALL if t.name in wanted]
