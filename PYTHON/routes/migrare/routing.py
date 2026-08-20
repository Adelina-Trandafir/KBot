# routes/migrare/routing.py
# -----------------------------------------------------------------------------
# Which unit database (DC) does a row belong to?
#
# Port of src/KBot.Migrator/Routing/{RoutingMaps,RowRouter}.vb, moved server-side
# now that the .accdb itself is on the server. Only six of the sixteen tables carry
# IdUnitate; the rest reach a DC through a chain of parents, so every map is built
# BEFORE any row is routed.
#
#   Cai : IdUnitate -> DC          (din cale.accdb, tabelul [Cai])
#   A   : CodAngajament -> DC      (FX_Angajamente)
#   B   : IDRZ -> DC               (FX_Rezervari, rutat prin A)
#   C   : IDRR -> DC               (FX_Receptii_R, rutat prin A)
#   D   : IDRH -> DC               (FX_Receptii_H, rutat prin A)
#   E   : IDEXF -> {DC}            (FX_Extrase_H) -- multime, nu valoare
# -----------------------------------------------------------------------------

import logging

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


class RowRouter(object):
    """
    Rutează rândurile UNUI tabel. Un rând a cărui cheie nu se rezolvă nu se scrie
    și nu se pierde tăcut: pleacă în lista de respinse, cu cheia primară și motivul.
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
