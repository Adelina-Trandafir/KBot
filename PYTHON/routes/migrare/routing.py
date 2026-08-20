# routes/migrare/routing.py
# -----------------------------------------------------------------------------
# Which unit database (DC) does a row belong to?
#
# TWO ANSWERS, and the file itself decides which one applies (see resolve_plan).
#
# 1. DIRECT -- the file holds ONE unit, so every row goes to the database the
#    operator picked. No maps, no cale.accdb, no parent chains. This is the
#    normal case and it is the whole point: the operator already told us the
#    destination, so computing it again is ceremony.
#
# 2. PRIN [Cai] -- the file holds SEVERAL units, so the destination has to be
#    worked out per row. Sending everything to the chosen database would write
#    another unit's rows into it, silently, with no error to notice. That is the
#    only thing this machinery buys, and it is why it stays.
#
# Which one applies is MEASURED, not assumed: distinct_units() reads the seven
# tables that carry IdUnitate and counts what it finds. A multi-unit file
# without cale.accdb stops with the unit numbers named, so the operator decides
# instead of us guessing.
#
# The maps below are a port of src/KBot.Migrator/Routing/{RoutingMaps,RowRouter}.vb
# and are built only on branch 2. Only six of the sixteen tables carry IdUnitate;
# the rest reach a DC through a chain of parents, so every map is built BEFORE
# any row is routed.
#
#   Cai : IdUnitate -> DC          (din cale.accdb, tabelul [Cai])
#   A   : CodAngajament -> DC      (FX_Angajamente)
#   B   : IDRZ -> DC               (FX_Rezervari, rutat prin A)
#   C   : IDRR -> DC               (FX_Receptii_R, rutat prin A)
#   D   : IDRH -> DC               (FX_Receptii_H, rutat prin A)
#   E   : IDEXF -> {DC}            (FX_Extrase_H) -- multime, nu valoare
# -----------------------------------------------------------------------------

import logging
import os

from . import accdb, tables

logger = logging.getLogger(__name__)


class RoutingError(Exception):
    """Rutarea nu poate continua deloc — mesaj în română."""


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


class RoutingMaps(object):

    def __init__(self):
        self.unit_to_dc = {}        # IdUnitate -> DC
        self.angajament = {}        # CodAngajament (lower) -> DC
        self.rezervare = {}         # IDRZ -> DC
        self.receptie_r = {}        # IDRR -> DC
        self.receptie_h = {}        # IDRH -> DC
        self.extras_file = {}       # IDEXF -> set(DC)

    def all_dcs(self):
        return sorted(set(v for v in self.unit_to_dc.values() if v))


def build_maps(fx_path, cai_path, progress=None):
    """
    Construieste toate hartile, in ordinea in care depind una de alta.

    `progress` primeste mesaje pentru jurnalul din interfata.
    """
    def say(msg):
        logger.info("migrare/rutare: %s", msg)
        if progress:
            progress(msg)

    maps = RoutingMaps()

    # --- Cai: IdUnitate -> DC ------------------------------------------------
    say("Se citește [Cai] din «cale.accdb».")
    seen = 0
    for row in accdb.iter_rows(cai_path, "Cai"):
        unit = _as_long(row.get("IdUnitate"))
        dc = _as_text(row.get("DC"))
        if unit is None or not dc:
            continue
        maps.unit_to_dc[unit] = dc
        seen += 1
    if not maps.unit_to_dc:
        raise RoutingError(
            "Tabelul [Cai] din «cale.accdb» nu a dat nicio pereche IdUnitate → DC. "
            "Fără ea nu se poate ruta niciun rând.")
    say("[Cai]: %d unități, %d baze distincte." % (seen, len(maps.all_dcs())))

    # --- A: CodAngajament -> DC ---------------------------------------------
    say("Se construiește harta angajamentelor.")
    for row in accdb.iter_rows(fx_path, "FX_Angajamente"):
        cod = _as_text(row.get("CodAngajament"))
        if not cod:
            continue
        dc = _as_text(row.get("DC"))
        if not dc:
            unit = _as_long(row.get("IdUnitate"))
            dc = maps.unit_to_dc.get(unit) if unit is not None else None
        if dc:
            maps.angajament[cod.lower()] = dc
    say("Harta angajamentelor: %d coduri." % len(maps.angajament))

    # --- B / C / D: copiii care se ruteaza prin angajament -------------------
    _build_from_parent(fx_path, "FX_Rezervari", "IDRZ", maps.rezervare, maps, say)
    _build_from_parent(fx_path, "FX_Receptii_R", "IDRR", maps.receptie_r, maps, say)
    _build_from_parent(fx_path, "FX_Receptii_H", "IDRH", maps.receptie_h, maps, say)

    # --- E: IDEXF -> multime de DC ------------------------------------------
    say("Se construiește harta fișierelor de extras.")
    for row in accdb.iter_rows(fx_path, "FX_Extrase_H"):
        idexf = _as_long(row.get("IDEXF"))
        if idexf is None:
            continue
        unit = _as_long(row.get("IdUnitate"))
        dc = maps.unit_to_dc.get(unit) if unit is not None else None
        if dc:
            maps.extras_file.setdefault(idexf, set()).add(dc)
    say("Harta extraselor: %d fișiere." % len(maps.extras_file))

    return maps


def _build_from_parent(fx_path, table, key_column, target, maps, say):
    say("Se construiește harta «%s»." % table)
    for row in accdb.iter_rows(fx_path, table):
        key = _as_long(row.get(key_column))
        cod = _as_text(row.get("CodAngajament"))
        if key is None or not cod:
            continue
        dc = maps.angajament.get(cod.lower())
        if dc:
            target[key] = dc
    say("Harta «%s»: %d chei." % (table, len(target)))


class DirectRouter(object):
    """
    Ramura obișnuită: fișierul are o singură unitate, deci fiecare rând merge în
    baza aleasă de operator. Nu se calculează nimic — destinația era deja știută.

    Singura verificare rămasă: dacă rândul poartă el însuși o coloană `DC`
    completată și aceea spune ALTCEVA, rândul e respins cu motivul. Nu e o
    formalitate — e fix cazul în care presupunerea «un singur fișier, o singură
    unitate» s-ar dovedi greșită, iar tăcerea ar însemna date în baza greșită.
    """

    def __init__(self, table, db_name):
        self.table = table
        self.db_name = db_name

    def primary_key_of(self, row):
        value = row.get(self.table.primary_key)
        return "?" if value is None else str(value)

    def route(self, row):
        propriu = _as_text(row.get("DC"))
        if propriu and propriu.lower() != self.db_name.lower():
            return [], ("rândul poartă DC «%s», iar ținta aleasă este «%s»"
                        % (propriu, self.db_name))
        return [self.db_name], None


class RowRouter(object):
    """
    Ramura prin [Cai]: rutează rândurile UNUI tabel. Un rând a cărui cheie nu se
    rezolvă nu se scrie și nu se pierde tăcut: pleacă în lista de respinse, cu
    cheia primară și motivul.
    """

    def __init__(self, table, maps):
        self.table = table
        self.maps = maps

    def primary_key_of(self, row):
        value = row.get(self.table.primary_key)
        return "?" if value is None else str(value)

    def route(self, row):
        """
        Întoarce (dcs, reject). `dcs` e o listă (mai multe DOAR la FX_Extrase_F),
        `reject` e motivul sau None.
        """
        kind = self.table.routing

        if kind == tables.OWN_DC_THEN_UNIT:
            dc = _as_text(row.get("DC"))
            if not dc:
                dc, reject = self._dc_from_unit(row)
                if reject:
                    return [], reject
            return ([dc], None) if dc else ([], "DC/IdUnitate nu se rezolvă în nicio bază")

        if kind == tables.OWN_UNIT:
            dc, reject = self._dc_from_unit(row)
            if reject:
                return [], reject
            return ([dc], None) if dc else ([], "IdUnitate nu se rezolvă în nicio bază")

        if kind == tables.BY_ANGAJAMENT:
            cod = _as_text(row.get(self.table.route_column))
            if not cod:
                return [], "%s lipsește" % self.table.route_column
            dc = self.maps.angajament.get(cod.lower())
            if not dc:
                return [], "CodAngajament «%s» nu există în FX_Angajamente" % cod
            return [dc], None

        if kind == tables.BY_REZERVARE:
            return self._by_long_map(row, self.table.route_column,
                                     self.maps.rezervare, "FX_Rezervari")

        if kind == tables.TWO_PARENTS:
            return self._two_parents(row)

        if kind == tables.FAN_OUT_EXTRAS:
            idexf = _as_long(row.get(self.table.route_column))
            if idexf is None:
                return [], "IDEXF lipsește"
            found = self.maps.extras_file.get(idexf)
            if not found:
                return [], "IDEXF %d nu apare în FX_Extrase_H" % idexf
            # Multiplicare INTENTIONATA: un fisier de extras poate purta linii
            # pentru mai multe unitati.
            return sorted(found), None

        raise RoutingError("Regulă de rutare necunoscută pentru «%s»." % self.table.name)

    # --- ajutoare ------------------------------------------------------------

    def _dc_from_unit(self, row):
        unit = _as_long(row.get("IdUnitate"))
        if unit is None:
            return None, "IdUnitate lipsește"
        dc = self.maps.unit_to_dc.get(unit)
        if not dc:
            return None, "IdUnitate %d nu există în [Cai]" % unit
        return dc, None

    def _by_long_map(self, row, column, mapping, parent_table):
        key = _as_long(row.get(column))
        if key is None:
            return [], "%s lipsește" % column
        dc = mapping.get(key)
        if not dc:
            return [], "%s %d nu apare în %s" % (column, key, parent_table)
        return [dc], None

    def _two_parents(self, row):
        """
        Primul părinte, cu retragere pe al doilea. Dacă amândoi sunt prezenți și NU
        sunt de acord, aceea e eroare dură, nu retragere: legăturile din Access se
        contrazic, iar alegerea unuia ar muta date în baza greșită.
        """
        first = self._lookup_parent(row, self.table.route_column)
        second = self._lookup_parent(row, self.table.route_column2)

        if first and second and first.lower() != second.lower():
            raise RoutingError(
                "%s, rândul cu cheia «%s»: cei doi părinți nu sunt de acord — %s duce "
                "la «%s», iar %s la «%s». Migrarea se oprește; nu ghicim care are dreptate."
                % (self.table.name, self.primary_key_of(row), self.table.route_column,
                   first, self.table.route_column2, second))

        dc = first or second
        if not dc:
            return [], ("nici %s, nici %s nu se rezolvă"
                        % (self.table.route_column, self.table.route_column2))
        return [dc], None

    def _lookup_parent(self, row, column):
        key = _as_long(row.get(column))
        if key is None:
            return None
        mapping = self.maps.receptie_r if column.upper() == "IDRR" else self.maps.receptie_h
        return mapping.get(key)


# -----------------------------------------------------------------------------
# Cate unitati are fisierul, si prin urmare care ramura se aplica
# -----------------------------------------------------------------------------

# Tabelele care poarta chiar ele IdUnitate. Restul ajung la o baza prin lantul
# de parinti, deci nu spun nimic in plus despre cate unitati are fisierul.
TABELE_CU_IDUNITATE = ("FX_Angajamente", "FX_Indicatori", "FX_Receptii",
                       "FX_Receptii_RHR", "FX_Plati", "FX_Extrase_H", "FX_Extrase")


def distinct_units(fx_path, progress=None):
    """
    Ce valori de IdUnitate apar in fisier, si in ce tabel s-a vazut fiecare.

    Se citesc doar cele sapte tabele care poarta coloana. Cele grele la Memo
    (FX_Receptii_IMG, FX_Rezervarii_IMG) nu sunt printre ele, deci pasul asta
    nu plateste pretul imaginilor.

    Un tabel absent din fisier nu opreste numaratoarea: lipsa lui e o constatare
    a analizei, iar aici intrebarea e alta.
    """
    def say(msg):
        logger.info("migrare/unități: %s", msg)
        if progress:
            progress(msg)

    gasite = {}
    for nume in TABELE_CU_IDUNITATE:
        try:
            for row in accdb.iter_rows(fx_path, nume):
                unit = _as_long(row.get("IdUnitate"))
                if unit is not None:
                    gasite.setdefault(unit, set()).add(nume)
        except accdb.AccdbError as exc:
            say("«%s» nu a putut fi citit la numărarea unităților: %s" % (nume, exc))

    say("Unități găsite în fișier: %s."
        % (", ".join(str(u) for u in sorted(gasite)) if gasite else "niciuna"))
    return gasite


class RoutingPlan(object):
    """
    Cum se rutează rândurile fișierului ăstuia. Se rezolvă O SINGURĂ DATĂ, la
    începutul analizei, și e apoi refolosit identic la scriere.
    """

    DIRECT = "direct"
    PRIN_CAI = "prin [Cai]"

    def __init__(self, mode, db_name, maps=None, units=None):
        self.mode = mode
        self.db_name = db_name
        self.maps = maps
        self.units = sorted(units or [])

    def router_for(self, table):
        if self.mode == self.DIRECT:
            return DirectRouter(table, self.db_name)
        return RowRouter(table, self.maps)

    def describe(self):
        if self.mode == self.DIRECT:
            return ("Fișierul are o singură unitate (%s), deci toate rândurile merg "
                    "în baza aleasă, «%s». Nu e nevoie de «cale.accdb»."
                    % (self.units[0] if self.units else "niciuna declarată", self.db_name))
        return ("Fișierul are %d unități (%s), deci fiecare rând se rutează prin [Cai]."
                % (len(self.units), ", ".join(str(u) for u in self.units)))


def resolve_plan(fx_path, cai_path, db_name, progress=None):
    """
    Masoara fisierul si alege ramura.

    O unitate (sau niciuna declarata) -> DIRECT, si cale.accdb nici nu se atinge.
    Mai multe unitati -> PRIN_CAI, iar daca cale.accdb lipseste se OPRESTE cu
    numerele unitatilor in mesaj. Nu se cade inapoi pe DIRECT: exact acolo ar
    ajunge randurile altei unitati in baza aleasa, tacut.
    """
    def say(msg):
        logger.info("migrare/plan: %s", msg)
        if progress:
            progress(msg)

    gasite = distinct_units(fx_path, progress=progress)

    if len(gasite) <= 1:
        plan = RoutingPlan(RoutingPlan.DIRECT, db_name, units=gasite.keys())
        say(plan.describe())
        return plan

    if not cai_path or not os.path.isfile(cai_path):
        raise RoutingError(
            "Fișierul conține %d unități (%s), deci rândurile trebuie despărțite "
            "între baze. Pentru asta e nevoie de «cale.accdb», care nu se află pe "
            "server. Fie îl împingeți și pe el, fie folosiți un fișier FOREXE care "
            "conține o singură unitate."
            % (len(gasite), ", ".join(str(u) for u in sorted(gasite))))

    plan = RoutingPlan(RoutingPlan.PRIN_CAI, db_name,
                       maps=build_maps(fx_path, cai_path, progress=progress),
                       units=gasite.keys())
    say(plan.describe())
    return plan
