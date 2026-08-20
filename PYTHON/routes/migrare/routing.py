# routes/migrare/routing.py
# -----------------------------------------------------------------------------
# Which rows of the pushed file belong to the unit the operator picked?
#
# ONE ANSWER, and the file itself carries everything needed to give it.
#
# The operator picks a target database (a DC, e.g. 045_CTER). A single
# FX_<an>.accdb may well hold SEVERAL units -- that is the normal case, not the
# exception -- and only the rows of the picked one may be written. Everything
# else is left alone: not an error, simply another unit's data.
#
# WHERE THE UNIT COMES FROM (verified against the Access export, TABLES/*.md):
#   * FX_Angajamente carries BOTH `IdUnitate` and `DC`, so the pair
#     IdUnitate <-> DC is in the file itself;
#   * FX_Indicatori carries `IdUnitate` next to `CodAngajament`, which fills in
#     any commitment whose FX_Angajamente row has no unit.
# There is NO cale.accdb anywhere in this pipeline -- the [Cai] table it carried
# said exactly the same thing (IdUnitate -> DC) and is not needed.
#
# Only six of the sixteen tables carry a unit of their own; the rest reach one
# through a chain of parents, so the key sets are built BEFORE any row is tested:
#
#   unitate    : IdUnitate       (din FX_Angajamente + FX_Indicatori)
#   angajament : CodAngajament   (FX_Angajamente, unitatea lui)
#   rezervare  : IDRZ            (FX_Rezervari, prin angajament)
#   receptie_r : IDRR            (FX_Receptii_R, prin angajament)
#   receptie_h : IDRH            (FX_Receptii_H, prin angajament)
#   extras     : IDEXF           (FX_Extrase_H, prin unitate)
#
# Every set is kept TWICE: `ours` (cheile unității alese) and `known` (tot ce
# poartă fișierul). Diferența dintre ele e diferența dintre «rândul e al altei
# unități» — se sare tăcut, e normal — și «cheia nu există nicăieri în fișier»,
# care e o constatare de integritate și se raportează cu motiv.
# -----------------------------------------------------------------------------

import logging

from . import accdb, tables

logger = logging.getLogger(__name__)


class RoutingError(Exception):
    """Selecția nu poate continua deloc — mesaj în română."""


def _as_long(value):
    """Numărul dintr-o valoare mdb-json, sau None. Nu ghicește: «12,5» nu e 12."""
    if value is None:
        return None
    if isinstance(value, bool):
        return int(value)
    if isinstance(value, int):
        return value
    if isinstance(value, float):
        return int(value) if float(value).is_integer() else None
    text = str(value).strip()
    if not text:
        return None
    try:
        return int(text)
    except ValueError:
        return None


def _as_text(value):
    if value is None:
        return None
    text = str(value).strip()
    return text or None


def _key(value):
    """Cheia de comparare a codurilor de angajament: litere mici."""
    return value.lower() if isinstance(value, str) else value


# -----------------------------------------------------------------------------
# Multimile de chei
# -----------------------------------------------------------------------------

# Starea unei chei fata de unitatea aleasa.
A_NOASTRA = "a noastră"
ALTA_UNITATE = "altă unitate"
NECUNOSCUTA = "necunoscută"
LIPSA = "lipsă"


class KeySet(object):
    """Cheile unei familii: ale unității alese, și toate cele din fișier."""

    def __init__(self):
        self.ours = set()
        self.known = set()

    def add(self, key, ours):
        self.known.add(key)
        if ours:
            self.ours.add(key)

    def state(self, key):
        if key is None:
            return LIPSA
        if key in self.ours:
            return A_NOASTRA
        if key in self.known:
            return ALTA_UNITATE
        return NECUNOSCUTA


class UnitPlan(object):
    """
    Cine aparține unității alese. Se rezolvă O SINGURĂ DATĂ, la începutul
    analizei, și e apoi refolosit identic la scriere — altfel selecția s-ar putea
    schimba între măsurare și scriere.
    """

    def __init__(self, db_name, sets, units, all_units, single_unit):
        self.db_name = db_name
        self.sets = sets                  # familie -> KeySet
        self.units = sorted(units)        # IdUnitate ale bazei alese
        self.all_units = sorted(all_units)
        # Fisierul poarta o singura unitate (sau niciuna declarata): un rand fara
        # IdUnitate e atunci al nostru, fiindca nu are cui altcuiva sa fie.
        self.single_unit = single_unit

    def selector_for(self, table):
        return TableSelector(table, self)

    def describe(self):
        if self.single_unit:
            return ("Fișierul poartă o singură unitate (%s), deci tot ce e în el merge "
                    "în baza aleasă, «%s»."
                    % (", ".join(str(u) for u in self.units) or "niciuna declarată",
                       self.db_name))
        return ("Fișierul poartă %d unități (%s). Se scriu DOAR rândurile unității "
                "«%s» (%s); restul rămân neatinse."
                % (len(self.all_units), ", ".join(str(u) for u in self.all_units),
                   self.db_name, ", ".join(str(u) for u in self.units)))


class TableSelector(object):
    """
    Ține sau nu rândul ăsta? Întoarce `(keep, reject)`:

      keep=True             — rândul e al unității alese și se scrie;
      keep=False, reject=None — rândul e al ALTEI unități; se sare, e normal;
      keep=False, reject="…"  — cheia nu se rezolvă nicăieri în fișier. Rândul
                                nu se pierde tăcut: pleacă în raport, cu motivul.
    """

    def __init__(self, table, plan):
        self.table = table
        self.plan = plan

    def primary_key_of(self, row):
        value = row.get(self.table.primary_key)
        return "?" if value is None else str(value)

    def keep(self, row):
        kind = self.table.selection

        if kind == tables.OWN_DC_THEN_UNIT:
            dc = _as_text(row.get("DC"))
            if dc:
                return dc.lower() == self.plan.db_name.lower(), None
            return self._by_unit(row)

        if kind == tables.OWN_UNIT:
            return self._by_unit(row)

        if kind == tables.BY_ANGAJAMENT:
            cod = _as_text(row.get(self.table.key_column))
            # Cheia e pe litere mici, dar motivul arata codul asa cum e in Access:
            # operatorul il cauta acolo, nu in multimea noastra.
            return self._by_set("angajament", _key(cod), self.table.key_column,
                                "FX_Angajamente", shown=cod)

        if kind == tables.BY_REZERVARE:
            return self._by_set("rezervare", _as_long(row.get(self.table.key_column)),
                                self.table.key_column, "FX_Rezervari")

        if kind == tables.BY_EXTRAS:
            return self._by_set("extras", _as_long(row.get(self.table.key_column)),
                                self.table.key_column, "FX_Extrase_H")

        if kind == tables.TWO_PARENTS:
            return self._two_parents(row)

        raise RoutingError("Regulă de selecție necunoscută pentru «%s»." % self.table.name)

    # --- ajutoare ------------------------------------------------------------

    def _by_unit(self, row):
        unit = _as_long(row.get("IdUnitate"))
        if unit is None:
            # Fisier cu o singura unitate: nu are cui altcuiva sa fie. Cu mai
            # multe, un rand fara unitate nu se poate atribui, si a-l scrie in
            # baza aleasa ar fi exact ghiceala care muta date gresit.
            if self.plan.single_unit:
                return True, None
            return False, ("IdUnitate lipsește, iar fișierul poartă mai multe unități "
                           "(%s)" % ", ".join(str(u) for u in self.plan.all_units))
        return unit in self.plan.sets["unitate"].ours, None

    def _by_set(self, family, key, column, parent_table, shown=None):
        if key is None:
            return False, "%s lipsește" % column
        state = self.plan.sets[family].state(key)
        if state == A_NOASTRA:
            return True, None
        if state == ALTA_UNITATE:
            return False, None
        return False, ("%s «%s» nu există în %s"
                       % (column, key if shown is None else shown, parent_table))

    def _two_parents(self, row):
        """
        Doi părinți candidați. Dacă unul e al unității alese și celălalt e sigur
        al alteia, aceea e eroare dură, nu retragere: legăturile din Access se
        contrazic, iar alegerea unuia ar muta date în baza greșită.
        """
        first_family, first_parent = self._family_of(self.table.key_column)
        second_family, second_parent = self._family_of(self.table.key_column2)

        first = self.plan.sets[first_family].state(_as_long(row.get(self.table.key_column)))
        second = self.plan.sets[second_family].state(_as_long(row.get(self.table.key_column2)))

        if A_NOASTRA in (first, second) and ALTA_UNITATE in (first, second):
            raise RoutingError(
                "%s, rândul cu cheia «%s»: cei doi părinți nu sunt de acord — %s duce la "
                "unitatea aleasă, iar %s la alta. Migrarea se oprește; nu ghicim care are "
                "dreptate."
                % (self.table.name, self.primary_key_of(row),
                   self.table.key_column if first == A_NOASTRA else self.table.key_column2,
                   self.table.key_column2 if first == A_NOASTRA else self.table.key_column))

        if A_NOASTRA in (first, second):
            return True, None
        if ALTA_UNITATE in (first, second):
            return False, None
        if first == LIPSA and second == LIPSA:
            return False, ("nici %s, nici %s nu sunt completate"
                           % (self.table.key_column, self.table.key_column2))
        return False, ("nici %s (%s), nici %s (%s) nu există în fișier"
                       % (self.table.key_column, first_parent,
                          self.table.key_column2, second_parent))

    @staticmethod
    def _family_of(column):
        if column.upper() == "IDRR":
            return "receptie_r", "FX_Receptii_R"
        if column.upper() == "IDRH":
            return "receptie_h", "FX_Receptii_H"
        if column.upper() == "IDRZ":
            return "rezervare", "FX_Rezervari"
        raise RoutingError("Coloana de părinte «%s» nu are o familie de chei." % column)


# -----------------------------------------------------------------------------
# Construirea planului
# -----------------------------------------------------------------------------

def build_plan(fx_path, db_name, progress=None):
    """
    Citește tabelele-cheie ale fișierului și decide ce aparține bazei «db_name».

    Tabelele grele la Memo (FX_Receptii_IMG, FX_Rezervarii_IMG) NU sunt printre
    ele: cheile lor vin de la părinți, deci pasul ăsta nu plătește prețul
    imaginilor.

    Un tabel-cheie absent din fișier nu oprește construirea: lipsa lui e o
    constatare a analizei, iar aici întrebarea e alta.
    """
    def say(msg):
        logger.info("migrare/selecție: %s", msg)
        if progress:
            progress(msg)

    cod_unit = {}       # cod (litere mici) -> IdUnitate
    cod_dc = {}         # cod (litere mici) -> DC scris chiar pe rând
    dc_units = {}       # DC (litere mici) -> set(IdUnitate)
    all_units = set()

    say("Se citește FX_Angajamente (IdUnitate + DC).")
    for row in _rows(fx_path, "FX_Angajamente", say):
        cod = _key(_as_text(row.get("CodAngajament")))
        unit = _as_long(row.get("IdUnitate"))
        dc = _as_text(row.get("DC"))
        if unit is not None:
            all_units.add(unit)
        if dc and unit is not None:
            dc_units.setdefault(dc.lower(), set()).add(unit)
        if cod:
            if unit is not None:
                cod_unit[cod] = unit
            if dc:
                cod_dc[cod] = dc

    say("Se citește FX_Indicatori (IdUnitate pentru angajamentele rămase).")
    for row in _rows(fx_path, "FX_Indicatori", say):
        unit = _as_long(row.get("IdUnitate"))
        if unit is None:
            continue
        all_units.add(unit)
        cod = _key(_as_text(row.get("CodAngajament")))
        if cod and cod not in cod_unit:
            cod_unit[cod] = unit

    units = set(dc_units.get(db_name.lower(), ()))
    single_unit = False

    if not units:
        if len(all_units) <= 1:
            # Fisier cu o singura unitate: aia e, oricum ar fi scris DC-ul.
            units = set(all_units)
            single_unit = True
            say("Fișierul nu numește baza «%s» pe niciun rând, dar poartă o singură "
                "unitate (%s) — se ia aceea."
                % (db_name, ", ".join(str(u) for u in sorted(all_units)) or "niciuna"))
        else:
            raise RoutingError(
                "Fișierul poartă %d unități (%s), dar niciun rând din FX_Angajamente nu "
                "arată spre baza «%s» (DC-urile găsite: %s). Alegeți baza care se "
                "potrivește fișierului, ori împingeți fișierul unității ăsteia."
                % (len(all_units), ", ".join(str(u) for u in sorted(all_units)),
                   db_name, ", ".join(sorted(dc_units)) or "niciunul"))
    elif len(all_units) <= 1:
        single_unit = True

    sets = dict((name, KeySet()) for name in
                ("unitate", "angajament", "rezervare", "receptie_r", "receptie_h", "extras"))

    for unit in all_units:
        sets["unitate"].add(unit, unit in units)

    for cod in set(list(cod_unit.keys()) + list(cod_dc.keys())):
        dc = cod_dc.get(cod)
        if dc:
            ours = dc.lower() == db_name.lower()
        else:
            ours = cod_unit.get(cod) in units
        sets["angajament"].add(cod, ours)

    say("Unitatea «%s»: %d angajamente din %d."
        % (db_name, len(sets["angajament"].ours), len(sets["angajament"].known)))

    _children_of_angajament(fx_path, "FX_Rezervari", "IDRZ", sets, "rezervare", say)
    _children_of_angajament(fx_path, "FX_Receptii_R", "IDRR", sets, "receptie_r", say)
    _children_of_angajament(fx_path, "FX_Receptii_H", "IDRH", sets, "receptie_h", say)

    say("Se citește FX_Extrase_H (fișierele de extras ale unității).")
    for row in _rows(fx_path, "FX_Extrase_H", say):
        idexf = _as_long(row.get("IDEXF"))
        if idexf is None:
            continue
        unit = _as_long(row.get("IdUnitate"))
        ours = unit in units if unit is not None else single_unit
        # Un fisier de extras poate purta linii ale mai multor unitati; daca UN
        # antet e al nostru, fisierul e si al nostru.
        sets["extras"].add(idexf, ours or idexf in sets["extras"].ours)
    say("Extrase: %d fișiere ale unității din %d."
        % (len(sets["extras"].ours), len(sets["extras"].known)))

    plan = UnitPlan(db_name, sets, units, all_units, single_unit)
    say(plan.describe())
    return plan


def _children_of_angajament(fx_path, table, key_column, sets, family, say):
    say("Se citește %s." % table)
    for row in _rows(fx_path, table, say):
        key = _as_long(row.get(key_column))
        if key is None:
            continue
        cod = _key(_as_text(row.get("CodAngajament")))
        sets[family].add(key, cod in sets["angajament"].ours if cod else False)
    say("%s: %d chei ale unității din %d."
        % (table, len(sets[family].ours), len(sets[family].known)))


def _rows(fx_path, table, say):
    """
    Rândurile unui tabel-cheie. Un tabel care lipsește sau nu se poate citi e
    SPUS în jurnal și sărit: lipsa lui e o constatare a analizei, nu a selecției.
    """
    try:
        for row in accdb.iter_rows(fx_path, table):
            yield row
    except accdb.AccdbError as exc:
        say("«%s» nu a putut fi citit la construirea selecției: %s" % (table, exc))
