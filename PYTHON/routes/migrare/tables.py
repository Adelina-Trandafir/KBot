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
OWN_DC_THEN_UNIT = "own_dc_then_unit"   # coloana DC proprie, altfel IdUnitate
OWN_UNIT = "own_unit"                   # IdUnitate propriu
BY_ANGAJAMENT = "by_angajament"         # CodAngajament, prin multimea angajamentelor
BY_REZERVARE = "by_rezervare"           # IDRZ, prin multimea rezervarilor
TWO_PARENTS = "two_parents"             # doi parinti candidati
BY_EXTRAS = "by_extras"                 # IDEXF, prin antetele din FX_Extrase_H


class SeedTable(object):
    """Descrierea unui tabel migrat. Fără logică."""

    def __init__(self, name, primary_key, selection,
                 key_column=None, key_column2=None, ddf_columns=None):
        self.name = name
        self.primary_key = primary_key
        self.selection = selection
        self.key_column = key_column
        self.key_column2 = key_column2
        # Coloanele care arata spre familia DDF (IDDF / IDREV). Se VERIFICA pe
        # MariaDB inainte de scriere, niciodata traduse: acolo id-urile sunt
        # AUTO_INCREMENT si nu pastreaza id-ul Access alaturi.
        self.ddf_columns = list(ddf_columns or [])


ALL = [
    # FX_Angajamente e radacina, si singurul tabel care poarta si IdUnitate si DC:
    # de acolo se afla ce IdUnitate are baza aleasa de operator.
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
    # Doi parinti: IDRR intai, apoi IDRH.
    SeedTable("FX_Receptii_IMG", "IDRDC", TWO_PARENTS,
              key_column="IDRR", key_column2="IDRH"),
    SeedTable("FX_Plati", "IdPlataFX", OWN_UNIT, ddf_columns=["IDREV"]),
    # Doi parinti, in ordinea inversa celui de mai sus.
    SeedTable("FX_Receptii_Plati", "IDRP", TWO_PARENTS,
              key_column="IDRH", key_column2="IDRR"),
    # Un fisier de extras poate purta linii ale mai multor unitati; e al nostru
    # daca macar un antet din FX_Extrase_H al lui e al unitatii alese.
    SeedTable("FX_Extrase_F", "IDEXF", BY_EXTRAS, key_column="IDEXF"),
    SeedTable("FX_Extrase_H", "IDEXH", OWN_UNIT),
    SeedTable("FX_Extrase", "IDFXE", OWN_UNIT),
]

BY_NAME = dict((t.name, t) for t in ALL)

# Tabelul MariaDB al fiecarui id DDF verificat.
DDF_ID_TABLE = {"IDDF": ("FX_DDF", "IDDF"), "IDREV": ("FX_DDF_REV", "IDREV")}

# Declarate in afara domeniului. Daca apar in fisier sunt RAPORTATE si NU migrate.
OUT_OF_SCOPE = ("FX_PRT_EXPL", "FX_CopacAngajamente")


def by_name(name):
    """Cheie necunoscută → excepție, niciodată un no-op tăcut."""
    table = BY_NAME.get(name)
    if table is None:
        raise KeyError("Tabelul «%s» nu face parte din setul migrat." % name)
    return table


def selected(names):
    """
    Tabelele bifate de operator, în ORDINEA DE SCRIERE (părinții înaintea
    copiilor), nu în ordinea în care le-a bifat el. `None` înseamnă toate.

    Un nume care nu face parte din setul migrat oprește cu excepție; nu se
    ignoră tăcut, altfel operatorul ar crede că a bifat ceva ce nu se scrie.
    """
    if names is None:
        return list(ALL)
    wanted = set()
    for name in names:
        by_name(name)
        wanted.add(name)
    return [t for t in ALL if t.name in wanted]
