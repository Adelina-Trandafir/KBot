# routes/migrare/tables.py
# -----------------------------------------------------------------------------
# The 16 migrated FX_ tables, in write order (parents before children), and the
# routing rule of each. Straight port of src/KBot.Migrator/ExportArtifacts/
# SeedTables.vb -- the set is identical to ALLOWED_TABLES in routes/forexe/seed.py.
#
# The list is FIXED. Nothing is discovered from relationships and no prefix is
# matched: discovery is exactly what falls over on this schema.
# -----------------------------------------------------------------------------

# --- routing kinds -----------------------------------------------------------
OWN_DC_THEN_UNIT = "own_dc_then_unit"   # coloana DC proprie, altfel IdUnitate
OWN_UNIT = "own_unit"                   # IdUnitate propriu, prin [Cai]
BY_ANGAJAMENT = "by_angajament"         # CodAngajament, prin harta A
BY_REZERVARE = "by_rezervare"           # IDRZ, prin harta B
TWO_PARENTS = "two_parents"             # doi parinti candidati, primul cu retragere
FAN_OUT_EXTRAS = "fan_out_extras"       # IDEXF, prin harta E -- poate da MAI MULTE DC-uri


class SeedTable(object):
    """Descrierea unui tabel migrat. Fără logică."""

    def __init__(self, name, primary_key, routing,
                 route_column=None, route_column2=None, ddf_columns=None):
        self.name = name
        self.primary_key = primary_key
        self.routing = routing
        self.route_column = route_column
        self.route_column2 = route_column2
        # Coloanele care arata spre familia DDF (IDDF / IDREV). Se VERIFICA pe
        # MariaDB inainte de scriere, niciodata traduse: acolo id-urile sunt
        # AUTO_INCREMENT si nu pastreaza id-ul Access alaturi.
        self.ddf_columns = list(ddf_columns or [])


ALL = [
    # FX_Angajamente e radacina. Exportul Access arata o coloana DC reala pe el,
    # deci ramura IdUnitate e o plasa, nu drumul obisnuit.
    SeedTable("FX_Angajamente", "CodAngajament", OWN_DC_THEN_UNIT, ddf_columns=["IDDF"]),
    SeedTable("FX_Indicatori", "CodAI", OWN_UNIT),
    SeedTable("FX_Istoric", "ID", BY_ANGAJAMENT, route_column="CodAngajament",
              ddf_columns=["IDREV"]),
    SeedTable("FX_Salarii", "IDFXS", BY_ANGAJAMENT, route_column="CodAngajament",
              ddf_columns=["IDDF", "IDREV"]),
    SeedTable("FX_Rezervari", "IDRZ", BY_ANGAJAMENT, route_column="CodAngajament",
              ddf_columns=["IDREV"]),
    SeedTable("FX_Rezervarii_IMG", "IDRZC", BY_REZERVARE, route_column="IDRZ"),
    SeedTable("FX_Receptii_R", "IDRR", BY_ANGAJAMENT, route_column="CodAngajament"),
    SeedTable("FX_Receptii_H", "IDRH", BY_ANGAJAMENT, route_column="CodAngajament"),
    SeedTable("FX_Receptii", "IDR", OWN_UNIT, ddf_columns=["IDREV"]),
    SeedTable("FX_Receptii_RHR", "IDRHR", OWN_UNIT),
    # Doi parinti: IDRR (harta C) intai, apoi IDRH (harta D).
    SeedTable("FX_Receptii_IMG", "IDRDC", TWO_PARENTS,
              route_column="IDRR", route_column2="IDRH"),
    SeedTable("FX_Plati", "IdPlataFX", OWN_UNIT, ddf_columns=["IDREV"]),
    # Doi parinti, in ordinea inversa celui de mai sus.
    SeedTable("FX_Receptii_Plati", "IDRP", TWO_PARENTS,
              route_column="IDRH", route_column2="IDRR"),
    # FX_Extrase_F se MULTIPLICA intentionat: un fisier de extras poate purta linii
    # pentru mai multe unitati, deci acelasi rand apartine legitim mai multor baze.
    SeedTable("FX_Extrase_F", "IDEXF", FAN_OUT_EXTRAS, route_column="IDEXF"),
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
