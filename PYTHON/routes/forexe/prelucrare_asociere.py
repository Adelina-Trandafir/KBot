# routes/forexe/prelucrare_asociere.py
"""
Pasul 4c al ingestiei FOREXE -- ASOCIEREA instantaneelor cu receptiile -- plus tot ce
tine de contractul in doua faze: amprenta, forma deciziilor, validarile si
reconstituirea receptiilor disparute. Felia 0048-03.

DE CE EXISTA FISIERUL ASTA SEPARAT. Tot restul conductei (prelucrare_pasi.py) ruleaza
IDENTIC in ambele faze. Aici e singurul loc in care ele difera:

  * faza «propunere» ruleaza trecerea AUTOMATA si o raporteaza ca SUGESTIE, fara sa
    scrie vreun `IDRR`;
  * faza «salvare» aplica `decizii` si IGNORA complet trecerea automata -- operatorul
    i-a vazut sugestiile si fie le-a acceptat, fie le-a suprascris; a o rula din nou ar
    insemna sa ne batem cu propriul om.

PROBLEMA DE FOND, in doua propozitii (docs/FUNDAMENT_Asociere_Receptii.md, partea 1).
`FX_Receptii_R` stie CARE receptie, dar nu are axa timpului. `FX_Receptii_H` are axa
timpului, dar nu stie care receptie -- istoricul FOREXE nu numeste niciodata receptia
(F4). Legatura dintre ele nu exista in date si nu poate fi dedusa: valoarea nu e cheie
(F5), data nu e cheie (F6), iar o salvare care nu schimba nimic produce oricum un
instantaneu complet (F7). Trecerea automata poate aseza doar ULTIMUL instantaneu al
unui lant (F9), deci restul ajung, prin constructie, la operator (F10).

O ASOCIERE GRESITA E TACUTA SI PERMANENTA (F12): strica `TotalReceptii` / `PlatiAnt` /
`Ramas` pentru fiecare plata de dupa acea data, si nimic nu compara cifrele cu nimic.
De-asta fiecare validare de mai jos RIDICA in loc sa corecteze.

CUM SE CITESTE FISIERUL (comentariile spun CE face un rand; cititorul stie SQL si
VB.NET, nu idiomuri Python):

  * `@dataclass`            o clasa cu campuri, fara sa scrii constructorul. Un Type
                            din VB6, cu nume.
  * `set(a) <= set(b)`      «a e submultime a lui b». Testul F14 intr-un singur operator.
  * `sorted(x, key=...)`    sortare dupa o cheie calculata, ca ORDER BY.
  * `dict.setdefault(k, [])` «ia lista de la cheia k, sau pune una goala si ia-o».
  * `f"..."`                interpolare de sir. Buna pentru mesaje, NICIODATA pentru SQL.
"""
import hashlib
import logging
from typing import Dict, List, Optional, Set, Tuple

from .prelucrare_helpers import fx_receptii_h_get_hash_ident

logger = logging.getLogger(__name__)

# Codul-motiv pe care clientul il recunoaste cand baza s-a schimbat intre cele doua faze.
# Acelasi tipar ca 401-urile din routes/auth/guard.py si ca ALEGERE_UNITATE din 0048-02.
REASON_STARE_MODIFICATA = "STARE_MODIFICATA"
MSG_STARE_MODIFICATA = (
    "Angajamentul s-a modificat de când a fost făcută propunerea. "
    "Descărcați-l din nou și reluați asocierea; nu s-a scris nimic."
)

# Cele patru actiuni, si nicio alta.
ACTIUNE_ASOCIAT = "asociat"
ACTIUNE_IGNORAT = "ignorat"
ACTIUNE_STERGERE = "stergere"
ACTIUNE_RECONSTITUIRE = "reconstituire"
ACTIUNI = (ACTIUNE_ASOCIAT, ACTIUNE_IGNORAT, ACTIUNE_STERGERE, ACTIUNE_RECONSTITUIRE)


class DecizieInvalida(ValueError):
    """Cererea de salvare e de nefolosit. Devine 400; nimic nu se scrie."""


class StareModificata(Exception):
    """Amprenta nu se potriveste. Devine 409 STARE_MODIFICATA; nimic nu se scrie."""


# ===========================================================================
# AMPRENTA (2.3)
# ===========================================================================
# Faza a doua trebuie sa vada aceeasi baza pe care a vazut-o faza intai, altfel deciziile
# descriu un tablou care nu mai exista.
#
# CONTINUT, si e documentat aici fiindca planul cere sa fie scris undeva anume:
#   FX_Istoric      -- COUNT(*), MAX(ID), MAX(DataFX)
#   FX_Receptii_R   -- COUNT(*), MAX(IDRR)
#   FX_Receptii_H   -- COUNT(*), MAX(IDRH), si COUNT-ul celor inca neasociate
# toate filtrate pe `CodAngajament`, toate concatenate si trecute prin SHA-256.
#
# De ce si COUNT si MAX: MAX singur nu se misca la o STERGERE de rand, COUNT singur nu
# se misca la o stergere urmata de o inserare. Impreuna, prind ambele cazuri.
# COUNT-ul celor neasociate e adaugat fiindca EL e chiar multimea despre care operatorul
# ia decizii: daca alta sesiune a asociat ceva intre timp, deciziile din fisierul local
# nu mai descriu aceeasi lista.
#
# Nimic din ce se schimba la CITIRE nu intra aici (fara DTQ, fara ceasuri).
_AMPRENTA_SQL = (
    "SELECT "
    " (SELECT COUNT(*) FROM FX_Istoric WHERE CodAngajament = %s) AS ic, "
    " (SELECT COALESCE(MAX(ID), 0) FROM FX_Istoric WHERE CodAngajament = %s) AS im, "
    " (SELECT COALESCE(MAX(DataFX), '1900-01-01') FROM FX_Istoric "
    "   WHERE CodAngajament = %s) AS id_, "
    " (SELECT COUNT(*) FROM FX_Receptii_R WHERE CodAngajament = %s) AS rc, "
    " (SELECT COALESCE(MAX(IDRR), 0) FROM FX_Receptii_R WHERE CodAngajament = %s) AS rm, "
    " (SELECT COUNT(*) FROM FX_Receptii_H WHERE CodAngajament = %s) AS hc, "
    " (SELECT COALESCE(MAX(IDRH), 0) FROM FX_Receptii_H WHERE CodAngajament = %s) AS hm, "
    " (SELECT COUNT(*) FROM FX_Receptii_H WHERE CodAngajament = %s "
    "    AND IDRR IS NULL AND COALESCE(Sters, 0) = 0) AS hn"
)


def amprenta(cursor, cod: str) -> str:
    """
    Amprenta starii curente a angajamentului, ca sir hex scurt.

    SE CALCULEAZA INAINTE DE ORICE SCRIERE, in ambele faze. Daca s-ar calcula la coada
    fazei intai, ar descrie starea SCRISA -- care e apoi derulata inapoi -- si faza a
    doua nu s-ar potrivi niciodata.
    """
    cursor.execute(_AMPRENTA_SQL, (cod,) * 8)
    r = cursor.fetchone()
    brut = "|".join([
        f"ic={r['ic']}", f"im={r['im']}", f"id={r['id_']}",
        f"rc={r['rc']}", f"rm={r['rm']}",
        f"hc={r['hc']}", f"hm={r['hm']}", f"hn={r['hn']}",
    ])
    return hashlib.sha256(brut.encode("utf-8")).hexdigest()[:32]


# ===========================================================================
# Citirea tabloului: receptii si instantanee
# ===========================================================================
_RECEPTII_SQL = (
    "SELECT IDRR, NRCRT, DataR, SumaAntet, Descriere, Sters, Reconstituit "
    "FROM FX_Receptii_R WHERE CodAngajament = %s ORDER BY DataR, IDRR"
)
_RHR_SQL = (
    "SELECT IDRR, CodIndicator, CodAI, CodSSI, CreditBugetar, Valoare, ValoareN "
    "FROM FX_Receptii_RHR WHERE CodAngajament = %s ORDER BY IDRR, CodIndicator"
)
# D-F: TOATE instantaneele neasezate intra in joc, si cele din rularea asta si cele
# ramase din rulari anterioare. Selectia e `IDRR IS NULL AND Sters = 0`, NU «inserate de
# rularea curenta». Marcajul Access `OrigIDRH IS NULL` nu are echivalent aici si nu se
# reconstruieste.
_INSTANTANEE_SQL = (
    "SELECT IDRH, IDH, DataH, Total, Descriere, EsteStergere "
    "FROM FX_Receptii_H "
    "WHERE CodAngajament = %s AND IDRR IS NULL AND COALESCE(Sters, 0) = 0 "
    "ORDER BY DataH, IDRH"
)
_LINII_SQL = (
    "SELECT IDRH, CodIndicator, CodAI, CodSSI, IdClsf, Valoare "
    "FROM FX_Receptii WHERE CodAngajament = %s ORDER BY IDRH, CodIndicator"
)


def citeste_receptii(cursor, cod: str) -> List[dict]:
    """
    Fiecare receptie a angajamentului, cu liniile ei -- INCLUSIV cele pe care rularea
    asta nu le-a atins si inclusiv cele sterse.

    Formularul are nevoie de toate ca tinte de plasare: o receptie stearsa poate primi
    in continuare un instantaneu ANTERIOR stergerii ei (regula de veto e pe data, F13,
    nu pe steagul de stergere).
    """
    cursor.execute(_RHR_SQL, (cod,))
    linii: Dict[int, List[dict]] = {}
    for r in cursor.fetchall():
        linii.setdefault(int(r["IDRR"]), []).append({
            "cod_indicator": r["CodIndicator"] or "",
            "cod_ai": r["CodAI"] or "",
            "cod_ssi": r["CodSSI"] or "",
            "credit_bugetar": float(r["CreditBugetar"] or 0),
            "valoare": float(r["Valoare"] or 0),
            "valoare_n": float(r["ValoareN"] or 0),
        })

    cursor.execute(_RECEPTII_SQL, (cod,))
    out = []
    for r in cursor.fetchall():
        idrr = int(r["IDRR"])
        out.append({
            "idrr": idrr,
            "nr_crt": r["NRCRT"],
            "data_r": r["DataR"],
            "suma_antet": float(r["SumaAntet"] or 0),
            "descriere": r["Descriere"] or "",
            "sters": bool(r["Sters"]),
            "reconstituit": bool(r["Reconstituit"]),
            "rhr": linii.get(idrr, []),
        })
    return out


def citeste_instantanee(cursor, cod: str, index_la_id: Dict[int, int],
                        warnings: List[str]) -> List[dict]:
    """
    Instantaneele inca neasezate, fiecare cu indicele randului lui de istoric.

    `rand_istoric` (F24) e INDICELE de la zero al randului in `TabelIstoric`, nu o cheie
    de baza de date. Ancorarea pe indice e ce face contractul in doua faze sa functioneze:
    id-urile atribuite in timpul propunerii dispar la rollback si nu se intorc identice,
    dar indicele e stabil prin constructie, fiindca AMBELE faze poarta acelasi payload.

    UN INSTANTANEU AL CARUI RAND DE ISTORIC NU E IN PAYLOAD nu poate primi indice, deci
    nu poate fi decis in aceasta rulare. Se poate intampla daca site-ul a paginat
    istoricul altfel sau daca randul a disparut de acolo. NU se strecoara tacut in lista:
    se lasa afara si se numara intr-un avertisment, ca sa se vada ca a ramas ceva
    nerezolvabil in loc sa para ca nu exista.
    """
    # Harta inversa: FX_Istoric.ID -> indicele in payload. Doua randuri identice din
    # payload arata catre acelasi ID; se pastreaza PRIMUL indice, care e si cel pe care
    # l-ar trimite clientul.
    id_la_index: Dict[int, int] = {}
    for idx in sorted(index_la_id.keys()):
        id_la_index.setdefault(index_la_id[idx], idx)

    cursor.execute(_LINII_SQL, (cod,))
    linii: Dict[int, List[dict]] = {}
    for r in cursor.fetchall():
        if r["IDRH"] is None:
            continue
        linii.setdefault(int(r["IDRH"]), []).append({
            "cod_indicator": r["CodIndicator"] or "",
            "cod_ai": r["CodAI"] or "",
            "cod_ssi": r["CodSSI"] or "",
            "id_clsf": r["IdClsf"],
            "valoare": float(r["Valoare"] or 0),
        })

    cursor.execute(_INSTANTANEE_SQL, (cod,))
    out = []
    fara_indice = 0
    for r in cursor.fetchall():
        idrh = int(r["IDRH"])
        idh = r["IDH"]
        idx = id_la_index.get(int(idh)) if idh is not None else None
        if idx is None:
            fara_indice += 1
            continue
        out.append({
            "idrh": idrh,
            "rand_istoric": idx,
            "data_h": r["DataH"],
            "descriere": r["Descriere"] or "",
            "total": float(r["Total"] or 0),
            "stergere": bool(r["EsteStergere"]),
            "linii": linii.get(idrh, []),
        })

    if fara_indice:
        warnings.append(
            f"{fara_indice} instantanee neasociate nu au rândul lor de istoric în "
            f"această descărcare și nu pot fi rezolvate acum. Rămân neasociate."
        )
    return out


# ===========================================================================
# PASUL 4c, FAZA UNU -- trecerea automata (sugestii, fara scriere)
# ===========================================================================
# Port al lui TMP_Asociaza_Receptii_Istoric: doua treceri, LIFO, fiecare receptie
# consumata cel mult o data.
#
# DOUA ABATERI, amandoua deliberate:
#
# 1. ORDINEA. Access ordona dupa `ID`, cu comentariul «autonumber = ordine cronologica
#    in tmp». Aici se ordoneaza dupa `DataH` (apoi `IDRH` ca departajare stabila).
#    `DataH` ESTE axa timpului (F2); sub plasare manuala ordinea de inserare inceteaza sa
#    mai fie cea cronologica. Cand totul e automat, cele doua coincid.
#
# 2. RECEPTIILE STERSE NU SUNT CANDIDATE. Nimic nu se mai poate adauga pe site unei
#    receptii sterse, deci o potrivire automata pe ea ar fi mereu o coliziune -- acelasi
#    rationament ca F25 la pasul 4b. Plasarea MANUALA pe o receptie stearsa ramane
#    permisa (un instantaneu dinaintea stergerii ii apartine pe drept); doar SUGESTIA
#    automata se abtine.
_R_CANDIDATI_AUTO_SQL = (
    "SELECT IDRR, SumaAntet FROM FX_Receptii_R "
    "WHERE CodAngajament = %s AND Sters = 0 ORDER BY DataR DESC, IDRR DESC"
)


def _cheie_suma(valoare) -> str:
    """
    Cheia de potrivire pe suma.

    Access folosea `CStr(<Double>)`, care e dependent de locale. Aici e rotunjire la doi
    zecimali, formatata cu punct: acelasi rezultat pentru orice suma reala, fara sa
    depinda de ce limba are serverul.
    """
    return f"{round(float(valoare or 0), 2):.2f}"


def pas4c_automat(cursor, cod: str, instantanee: List[dict]) -> Dict[int, int]:
    """
    Trecerea automata. Intoarce {IDRH: IDRR} -- SUGESTII, nu scrieri.

    Nu scrie nimic. Faza intai raporteaza rezultatul ca `sugestie_idrr` /
    `sugestie_automata`, ca sa poata fi aratat drept propunere, nu drept fapt (F18):
    sub F11, trecerea automata poate fi GRESITA, nu doar incompleta.
    """
    cursor.execute(_R_CANDIDATI_AUTO_SQL, (cod,))
    # {suma: [IDRR, ...]} cu cea mai recenta receptie prima (LIFO).
    dic_r: Dict[str, List[int]] = {}
    for r in cursor.fetchall():
        dic_r.setdefault(_cheie_suma(r["SumaAntet"]), []).append(int(r["IDRR"]))

    folosite: Set[int] = set()
    sugestii: Dict[int, int] = {}

    def _incearca(lista: List[dict]) -> None:
        for inst in lista:
            if inst["idrh"] in sugestii:
                continue
            cheie = _cheie_suma(inst["total"])
            candidati = dic_r.get(cheie)
            if not candidati:
                continue
            while candidati:
                idrr = candidati.pop(0)
                if idrr not in folosite:
                    folosite.add(idrr)
                    sugestii[inst["idrh"]] = idrr
                    break

    # RUN 1 -- instantanee de la cel mai nou catre cel mai vechi.
    _incearca(sorted(instantanee, key=lambda x: (x["data_h"], x["idrh"]), reverse=True))
    # RUN 2 -- ce a ramas, de la cel mai vechi catre cel mai nou.
    _incearca(sorted(instantanee, key=lambda x: (x["data_h"], x["idrh"])))
    return sugestii


# ===========================================================================
# Forma deciziilor (2.2)
# ===========================================================================
def normalizeaza_decizii(brut) -> List[dict]:
    """
    Verifica forma lui `decizii` si o intoarce curatata.

    Nimic nu se corecteaza aici -- se ridica. O decizie pe care nu o putem citi nu e
    acelasi lucru cu absenta unei decizii, iar tacerea NU are voie sa fie interpretabila
    drept alegere.
    """
    if not isinstance(brut, list):
        raise DecizieInvalida("Câmpul «decizii» trebuie să fie o listă.")
    out: List[dict] = []
    for i, item in enumerate(brut):
        if not isinstance(item, dict):
            raise DecizieInvalida(f"«decizii»[{i}] nu este un obiect.")
        try:
            rand = int(item.get("rand_istoric"))
        except (TypeError, ValueError) as err:
            raise DecizieInvalida(
                f"«decizii»[{i}]: «rand_istoric» lipsește sau nu este un număr."
            ) from err
        actiune = str(item.get("actiune") or "").strip()
        if actiune not in ACTIUNI:
            raise DecizieInvalida(
                f"«decizii»[{i}]: «actiune» «{actiune}» nu este cunoscută "
                f"(permise: {', '.join(ACTIUNI)})."
            )
        data_h = item.get("data_h")
        if not data_h:
            raise DecizieInvalida(f"«decizii»[{i}]: «data_h» este obligatorie.")

        idrr = item.get("idrr")
        eticheta = item.get("receptie_noua")
        if idrr is not None:
            try:
                idrr = int(idrr)
            except (TypeError, ValueError) as err:
                raise DecizieInvalida(
                    f"«decizii»[{i}]: «idrr» nu este un număr.") from err
        if eticheta is not None:
            eticheta = str(eticheta).strip()
            if eticheta == "":
                raise DecizieInvalida(
                    f"«decizii»[{i}]: «receptie_noua» nu poate fi șir gol.")

        # `asociat` si `stergere` cer EXACT una dintre cele doua tinte.
        if actiune in (ACTIUNE_ASOCIAT, ACTIUNE_STERGERE):
            if (idrr is None) == (eticheta is None):
                raise DecizieInvalida(
                    f"«decizii»[{i}] ({actiune}): trebuie exact una dintre «idrr» și "
                    f"«receptie_noua», nu ambele și nu niciuna."
                )
        elif actiune == ACTIUNE_RECONSTITUIRE:
            if eticheta is None:
                raise DecizieInvalida(
                    f"«decizii»[{i}] (reconstituire): «receptie_noua» este obligatorie.")
            if idrr is not None:
                raise DecizieInvalida(
                    f"«decizii»[{i}] (reconstituire): «idrr» nu are sens — recepția "
                    f"încă nu există.")
        else:   # ignorat
            if idrr is not None or eticheta is not None:
                raise DecizieInvalida(
                    f"«decizii»[{i}] (ignorat): nu poate purta o recepție.")

        out.append({
            "rand_istoric": rand,
            "actiune": actiune,
            "data_h": str(data_h),
            "idrr": idrr,
            "receptie_noua": eticheta,
        })
    return out


def _acelasi_moment(a, b) -> bool:
    """
    Compara `data_h` sosita ca text cu `DataH` din baza, ca DATETIME COMPLET.

    Clientul trimite ISO ("2026-05-20T00:36:12" sau cu spatiu). Se normalizeaza ambele
    la «YYYY-MM-DD HH:MM:SS» si se compara ca siruri: aceeasi rezolutie ca datetime-ul
    din MariaDB, fara sa depindem de parsarea unui fus orar care nu exista in date.
    """
    def _norm(v) -> str:
        if v is None:
            return ""
        s = str(v).strip().replace("T", " ")
        if "." in s:                 # taie fractiunile de secunda, daca sosesc
            s = s.split(".", 1)[0]
        if len(s) == 10:             # doar data -> miezul noptii
            s += " 00:00:00"
        return s
    return _norm(a) == _norm(b)


def verifica_acoperirea(decizii: List[dict], instantanee: List[dict]) -> Dict[int, dict]:
    """
    Fiecare instantaneu trebuie sa apara in `decizii`, o singura data, cu `data_h` potrivit.

    Intoarce {rand_istoric: decizie}.

    Un instantaneu LIPSA e 400, nu o valoare implicita: tacerea nu are voie sa insemne
    «ignora-l». O `data_h` care nu se potriveste cu randul de la acel indice e tot 400 --
    asa un fisier de decizii invechit cade zgomotos in loc sa asocieze tacut alt rand.
    """
    dupa_rand = {i["rand_istoric"]: i for i in instantanee}

    vazute: Dict[int, dict] = {}
    for d in decizii:
        rand = d["rand_istoric"]
        inst = dupa_rand.get(rand)
        if inst is None:
            raise DecizieInvalida(
                f"Decizia pentru rândul de istoric {rand} nu corespunde niciunui "
                f"instantaneu de rezolvat."
            )
        if rand in vazute:
            raise DecizieInvalida(
                f"Rândul de istoric {rand} apare de două ori în «decizii».")
        if not _acelasi_moment(d["data_h"], inst["data_h"]):
            raise DecizieInvalida(
                f"Rândul de istoric {rand}: «data_h» trimisă ({d['data_h']}) nu se "
                f"potrivește cu cea din descărcare ({inst['data_h']}). Fișierul de "
                f"decizii este învechit — reluați descărcarea."
            )
        vazute[rand] = d

    lipsa = [i["rand_istoric"] for i in instantanee if i["rand_istoric"] not in vazute]
    if lipsa:
        raise DecizieInvalida(
            f"Lipsesc deciziile pentru {len(lipsa)} instantanee "
            f"(rânduri: {', '.join(str(x) for x in sorted(lipsa)[:20])}"
            f"{'…' if len(lipsa) > 20 else ''})."
        )
    return vazute


def verifica_etichetele(decizii: List[dict]) -> Dict[str, dict]:
    """
    Regulile etichetelor `receptie_noua` (§4c-bis).

    * fiecare eticheta e DECLARATA de exact o `reconstituire`;
    * fiecare eticheta folosita altundeva e declarata;
    * fiecare lant reconstituit contine EXACT o `stergere`.

    Ultima regula nu e pedanterie. O receptie reconstituita exista TOCMAI fiindca a fost
    stearsa (F26); un lant fara stergere inseamna ca operatorul a grupat gresit, iar
    rezultatul ar fi o receptie care nu apare niciodata in `ListaReceptii` si nu se poate
    reconcilia cu nimic. Mai bine se opreste rularea.
    """
    declarate: Dict[str, dict] = {}
    for d in decizii:
        if d["actiune"] != ACTIUNE_RECONSTITUIRE:
            continue
        et = d["receptie_noua"]
        if et in declarate:
            raise DecizieInvalida(
                f"Eticheta «{et}» este declarată de două ori prin «reconstituire»; "
                f"o recepție reconstituită începe într-un singur loc."
            )
        declarate[et] = d

    folosite = {d["receptie_noua"] for d in decizii if d["receptie_noua"]}
    nedeclarate = folosite - set(declarate)
    if nedeclarate:
        raise DecizieInvalida(
            f"Etichetele {', '.join('«%s»' % x for x in sorted(nedeclarate))} sunt "
            f"folosite dar nu sunt declarate prin «reconstituire»."
        )

    for et in declarate:
        stergeri = [d for d in decizii
                    if d["receptie_noua"] == et and d["actiune"] == ACTIUNE_STERGERE]
        if len(stergeri) != 1:
            raise DecizieInvalida(
                f"Lanțul reconstituit «{et}» are {len(stergeri)} rânduri de ștergere; "
                f"trebuie exact unul. O recepție reconstituită există tocmai pentru că "
                f"a fost ștearsă."
            )
    return declarate


# ===========================================================================
# PASUL 4c-bis -- materializarea receptiilor reconstituite
# ===========================================================================
_MAX_NRCRT_R_SQL = "SELECT MAX(NRCRT) AS MaxNr FROM FX_Receptii_R WHERE CodAngajament = %s"
_R_INSERT_RECONST_SQL = (
    "INSERT INTO FX_Receptii_R "
    "(NRCRT, CodAngajament, Tip, DataR, SumaAntet, Descriere, TipReceptie, HASH, "
    " Preluat, Sters, Reconstituit) "
    "VALUES (%s, %s, NULL, %s, %s, %s, 'NOU', NULL, 1, 1, 1)"
)
_RHR_INSERT_SQL = (
    "INSERT INTO FX_Receptii_RHR "
    "(IDRR, CodAngajament, CodIndicator, CodAI, IdClsf, IdUnitate, CodSSI, "
    " CreditBugetar, Valoare, ValoareN, TipIntern) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, 0, 'NOU')"
)
# CreditBugetar e creditul bugetar AL INDICATORULUI, nu o cifra per receptie: datele de
# proba il arata constant pe indicator, pe toate receptiile unui angajament (AAB =
# 10502,19 pe TmpID 268-272; AA2 = 1263,39 pe toate cinci). Se ia dintr-un rand RHR
# existent cu acelasi CodAI. DACA NU EXISTA NICIUNUL, SE RIDICA -- un zero aici nu se
# deosebeste de un zero real si ar fi citit ca fapt pe veci.
_CREDIT_BUGETAR_SQL = (
    "SELECT CreditBugetar FROM FX_Receptii_RHR "
    "WHERE CodAngajament = %s AND CodAI = %s AND CreditBugetar IS NOT NULL "
    "ORDER BY IDRHR DESC LIMIT 1"
)
_IND_SQL = (
    "SELECT CodAI, IdClsf, IdUnitate FROM FX_Indicatori WHERE CodAngajament = %s"
)


def materializeaza_reconstituite(cursor, cod: str, decizii: List[dict],
                                 instantanee: List[dict],
                                 etichete: Dict[str, dict],
                                 warnings: List[str]) -> Dict[str, int]:
    """
    Creeaza receptiile pe care operatorul le-a declarat prin «reconstituire» (F26).

    DE CE EXISTA. «O receptie trebuie sa existe inainte sa poata fi stearsa» e adevarat
    PE SITE. Local e adevarat doar daca K-BOT a descarcat INAINTE de stergere. Un
    angajament descarcat prima oara pe 26/08, a carui receptie a fost creata in ianuarie
    si stearsa in martie, livreaza crearea, modificarile si stergerea intr-un singur
    istoric -- iar `ListaReceptii` nu o contine, deci nu exista niciun rand
    `FX_Receptii_R` de care lantul sa se agate. Cum tot restantul se ingereaza retroactiv
    chiar acum, cazul e de asteptat sa fie frecvent, nu rar.

    NIMIC NU SE INVENTEAZA. Fiecare camp are o sursa in istoric:
      DataR      <- DataH al CELUI MAI VECHI instantaneu din lant
      SumaAntet  <- `Total` al randului de stergere (cat valora cand a plecat)
      Descriere  <- `Receptie: <text>` al randului de stergere
      NrCrt      <- MAX+1 pe angajament, ca peste tot
      Sters = 1, Reconstituit = 1, HASH = NULL (nu exista bloc de payload de hasuit)
      RHR        <- liniile pe indicator ale ULTIMULUI instantaneu DINAINTEA stergerii
      CreditBugetar <- un rand RHR existent cu acelasi CodAI

    `Preluat`, `Incarcat` si `TipReceptie` se pun EXACT ca la o receptie nou inserata de
    ingestie (pasul 4b): TipReceptie = 'NOU', Preluat = 1, Incarcat NEATINS (ramane NULL).
    Consemnat in worklog, fiindca randurile astea nu au trecut niciodata prin
    `ListaReceptii` si acolo valorile inseamna ceva usor diferit.

    Se ruleaza INAINTE de aplicarea deciziilor, ca fiecare eticheta sa fie deja un `IDRR`
    real cand se ajunge la `asociat` / `stergere` -- si restul pasului 4c sa nu fie nevoit
    sa stie ca receptiile astea sunt speciale.
    """
    if not etichete:
        return {}

    dupa_rand = {i["rand_istoric"]: i for i in instantanee}
    cursor.execute(_IND_SQL, (cod,))
    indicatori = {str(r["CodAI"]): r for r in cursor.fetchall()}

    cursor.execute(_MAX_NRCRT_R_SQL, (cod,))
    row = cursor.fetchone()
    nr_crt = int((row or {}).get("MaxNr") or 0) + 1

    rezolvate: Dict[str, int] = {}

    for eticheta in sorted(etichete):
        lant = [dupa_rand[d["rand_istoric"]] for d in decizii
                if d["receptie_noua"] == eticheta]
        lant.sort(key=lambda x: (x["data_h"], x["idrh"]))

        stergerea = next(d for d in decizii
                         if d["receptie_noua"] == eticheta
                         and d["actiune"] == ACTIUNE_STERGERE)
        inst_stergere = dupa_rand[stergerea["rand_istoric"]]

        # Stergerea trebuie sa fie ULTIMUL instantaneu al lantului. Daca nu e, gruparea
        # e gresita: nimic nu se poate intampla cu o receptie dupa ce a fost stearsa.
        if lant[-1]["idrh"] != inst_stergere["idrh"]:
            raise DecizieInvalida(
                f"Lanțul reconstituit «{eticheta}»: rândul de ștergere "
                f"({inst_stergere['data_h']}) nu este ultimul din lanț. O recepție nu "
                f"mai poate fi modificată după ce a fost ștearsă."
            )

        cursor.execute(_R_INSERT_RECONST_SQL, (
            nr_crt, cod, lant[0]["data_h"], inst_stergere["total"],
            inst_stergere["descriere"],
        ))
        idrr = int(cursor.lastrowid)
        nr_crt += 1
        rezolvate[eticheta] = idrr

        # Liniile: ultimul instantaneu DINAINTEA stergerii care are linii. Randul de
        # stergere nu are (F21), deci se merge inapoi pana la primul care are.
        sursa = None
        for inst in reversed(lant[:-1]):
            if inst["linii"]:
                sursa = inst
                break
        if sursa is None:
            warnings.append(
                f"Recepția reconstituită «{eticheta}» nu are niciun instantaneu cu "
                f"linii pe indicator; se creează fără rânduri RHR."
            )
            continue

        for linie in sursa["linii"]:
            cheie_ai = linie["cod_ai"]
            cursor.execute(_CREDIT_BUGETAR_SQL, (cod, cheie_ai))
            cb = cursor.fetchone()
            if cb is None:
                raise DecizieInvalida(
                    f"Recepția reconstituită «{eticheta}»: indicatorul {cheie_ai} nu "
                    f"are niciun rând RHR existent pe acest angajament, deci creditul "
                    f"bugetar nu poate fi preluat. Nu se scrie zero — zero ar fi citit "
                    f"ca valoare reală."
                )
            ind = indicatori.get(cheie_ai)
            cursor.execute(_RHR_INSERT_SQL, (
                idrr, cod, linie["cod_indicator"], cheie_ai,
                None if ind is None else ind["IdClsf"],
                None if ind is None else ind["IdUnitate"],
                linie["cod_ssi"], float(cb["CreditBugetar"]), linie["valoare"],
            ))

    return rezolvate


# ===========================================================================
# Validarile de plasare (F13, F14, F15, F16)
# ===========================================================================
def _indicatori_receptie(rec: dict) -> Set[str]:
    return {l["cod_indicator"] for l in rec["rhr"] if l["cod_indicator"]}


def _indicatori_instantaneu(inst: dict) -> Set[str]:
    return {l["cod_indicator"] for l in inst["linii"] if l["cod_indicator"]}


def valideaza_plasarile(lanturi: Dict[int, List[dict]],
                        receptii: Dict[int, dict]) -> None:
    """
    Toate regulile care pot spune «nu acolo», rulate pe tabloul REZULTAT.

    Clientul face veto la momentul plasarii; serverul NU se increde in el. Fiecare
    verificare RIDICA -- nu corecteaza, nu avertizeaza si merge mai departe -- fiindca o
    asociere gresita e tacuta si permanenta (F12).
    """
    for idrr, lant in lanturi.items():
        rec = receptii[idrr]
        lant = sorted(lant, key=lambda x: (x["data_h"], x["idrh"]))

        ind_rec = _indicatori_receptie(rec)

        precedente: Set[str] = set()
        for inst in lant:
            # --- F13, vetoul de data. TIMESTAMP COMPLET, nu trunchiat la zi:
            # operatorii salveaza aceeasi receptie de mai multe ori intr-un minut.
            if rec["data_r"] is not None and inst["data_h"] is not None:
                if _ca_datetime(rec["data_r"]) > _ca_datetime(inst["data_h"]):
                    raise DecizieInvalida(
                        f"Recepția {idrr} este creată la {rec['data_r']}, după "
                        f"instantaneul de la {inst['data_h']}. O recepție nu poate "
                        f"deține un instantaneu anterior creării ei."
                    )

            ind_inst = _indicatori_instantaneu(inst)

            # --- F14, submultimea de indicatori. Slab (majoritatea angajamentelor au un
            # singur indicator), dar corect: indicatorii se adauga la o receptie in timp
            # si cad la zero, insa nu dispar din bloc.
            if ind_inst and not ind_inst <= ind_rec:
                lipsa = ", ".join(sorted(ind_inst - ind_rec))
                raise DecizieInvalida(
                    f"Instantaneul de la {inst['data_h']} numește indicatorii {lipsa}, "
                    f"pe care recepția {idrr} nu îi are."
                )

            # --- F16, multimile doar cresc, de-a lungul lantului ordonat dupa DataH.
            if ind_inst and not precedente <= ind_inst:
                pierduti = ", ".join(sorted(precedente - ind_inst))
                raise DecizieInvalida(
                    f"Instantaneul de la {inst['data_h']} pierde indicatorii "
                    f"{pierduti}, prezenți mai devreme în lanțul recepției {idrr}. "
                    f"Un indicator poate cădea la zero, dar nu poate dispărea."
                )
            if ind_inst:
                precedente = precedente | ind_inst

        # --- F15, capatul lantului --------------------------------------------
        if not lant:
            continue
        ultimul = lant[-1]
        if ultimul["stergere"]:
            # SARIT DELIBERAT. Ultimul instantaneu al unei receptii sterse E randul de
            # stergere; a-l compara cu starea de ACUM nu inseamna nimic. Receptiile
            # reconstituite sunt mereu in categoria asta.
            continue
        if round(ultimul["total"], 2) != round(rec["suma_antet"], 2):
            raise DecizieInvalida(
                f"Recepția {idrr}: ultimul instantaneu ({ultimul['data_h']}) are "
                f"totalul {ultimul['total']:.2f}, dar recepția valorează acum "
                f"{rec['suma_antet']:.2f}. Lanțul nu se închide."
            )
        val_inst = {l["cod_indicator"]: round(l["valoare"], 2)
                    for l in ultimul["linii"]}
        val_rec = {l["cod_indicator"]: round(l["valoare"], 2) for l in rec["rhr"]}
        if val_inst and val_inst != val_rec:
            raise DecizieInvalida(
                f"Recepția {idrr}: liniile ultimului instantaneu nu se potrivesc cu "
                f"cele ale recepției. Lanțul nu se închide."
            )


def _ca_datetime(v):
    """Datele vin din driver ca `datetime` sau `date`; se compara pe acelasi teren."""
    from datetime import date, datetime
    if isinstance(v, datetime):
        return v
    if isinstance(v, date):
        return datetime(v.year, v.month, v.day)
    return v


# ===========================================================================
# PASUL 4c, FAZA DOUA -- aplicarea deciziilor
# ===========================================================================
_H_ASOCIAZA_SQL = (
    "UPDATE FX_Receptii_H SET IDRR = %s, Sters = 0, EsteStergere = %s WHERE IDRH = %s"
)
_H_IGNORA_SQL = (
    "UPDATE FX_Receptii_H SET IDRR = NULL, Sters = 1 WHERE IDRH = %s"
)
_R_MARCHEAZA_STEARSA_SQL = "UPDATE FX_Receptii_R SET Sters = 1 WHERE IDRR = %s"
_H_TIP_SQL = "UPDATE FX_Receptii_H SET TipReceptie = %s, HASH = %s WHERE IDRH = %s"
_H_LANT_SQL = (
    "SELECT IDRH, DataH, Descriere, TipReceptie, CodAngajament FROM FX_Receptii_H "
    "WHERE IDRR = %s ORDER BY DataH, IDRH"
)


def aplica_decizii(cursor, cod: str, decizii: List[dict], instantanee: List[dict],
                   receptii: List[dict], warnings: List[str]) -> Dict[str, int]:
    """
    Faza a doua a pasului 4c. Aplica `decizii` si ignora complet trecerea automata.

    Intoarce numaratorile scrise.
    """
    dupa_rand = {i["rand_istoric"]: i for i in instantanee}

    verifica_acoperirea(decizii, instantanee)
    etichete = verifica_etichetele(decizii)

    # 4c-bis mai intai: dupa asta fiecare eticheta e un IDRR real.
    noi = materializeaza_reconstituite(cursor, cod, decizii, instantanee,
                                       etichete, warnings)

    # Tabloul receptiilor se reciteste, ca sa contina si cele tocmai create.
    toate = {r["idrr"]: r for r in citeste_receptii(cursor, cod)}

    def _tinta(d: dict) -> int:
        if d["idrr"] is not None:
            if d["idrr"] not in toate:
                raise DecizieInvalida(
                    f"Recepția {d['idrr']} nu există pe acest angajament.")
            return d["idrr"]
        return noi[d["receptie_noua"]]

    # --- se construiesc lanturile REZULTATE si se valideaza INAINTE de a scrie ------
    lanturi: Dict[int, List[dict]] = {}
    for d in decizii:
        if d["actiune"] == ACTIUNE_IGNORAT:
            continue
        inst = dupa_rand[d["rand_istoric"]]
        lanturi.setdefault(_tinta(d), []).append(inst)

    # Instantaneele DEJA asociate ale acelorasi receptii fac parte din lant si ele --
    # F15 si F16 se refera la lantul intreg, nu doar la ce se adauga acum.
    for idrr in list(lanturi):
        cursor.execute(
            "SELECT H.IDRH, H.DataH, H.Total, H.EsteStergere FROM FX_Receptii_H H "
            "WHERE H.IDRR = %s", (idrr,))
        for r in cursor.fetchall():
            idrh = int(r["IDRH"])
            if any(x["idrh"] == idrh for x in lanturi[idrr]):
                continue
            cursor.execute(
                "SELECT CodIndicator, CodAI, CodSSI, IdClsf, Valoare FROM FX_Receptii "
                "WHERE IDRH = %s", (idrh,))
            linii = [{"cod_indicator": x["CodIndicator"] or "",
                      "cod_ai": x["CodAI"] or "", "cod_ssi": x["CodSSI"] or "",
                      "id_clsf": x["IdClsf"], "valoare": float(x["Valoare"] or 0)}
                     for x in cursor.fetchall()]
            lanturi[idrr].append({
                "idrh": idrh, "rand_istoric": -1, "data_h": r["DataH"],
                "descriere": "", "total": float(r["Total"] or 0),
                "stergere": bool(r["EsteStergere"]), "linii": linii,
            })

    valideaza_plasarile(lanturi, toate)

    # --- scrierea ---------------------------------------------------------------
    numarat = {"asociat": 0, "ignorat": 0, "stergere": 0, "reconstituit": len(noi)}
    for d in decizii:
        inst = dupa_rand[d["rand_istoric"]]
        if d["actiune"] == ACTIUNE_IGNORAT:
            # F17 / 1.6: o salvare care nu a consemnat nicio schimbare. `Sters = 1` pe
            # instantaneu, `IDRR` lasat gol. A ignora nu pierde nimic; a forta pe o
            # receptie injecteaza o valoare falsa in cronologia ei, la acea data, si
            # verificarea de capat de lant NU o prinde daca aterizeaza la mijloc.
            cursor.execute(_H_IGNORA_SQL, (inst["idrh"],))
            numarat["ignorat"] += 1
            continue

        if d["actiune"] == ACTIUNE_RECONSTITUIRE:
            # Randul care PORNESTE lantul. Se ataseaza ca orice altul; receptia lui
            # tocmai a fost creata mai sus.
            cursor.execute(_H_ASOCIAZA_SQL, (_tinta(d), 0, inst["idrh"]))
            numarat["asociat"] += 1
            continue

        idrr = _tinta(d)
        este_stergere = 1 if d["actiune"] == ACTIUNE_STERGERE else 0
        cursor.execute(_H_ASOCIAZA_SQL, (idrr, este_stergere, inst["idrh"]))
        if este_stergere:
            cursor.execute(_R_MARCHEAZA_STEARSA_SQL, (idrr,))
            numarat["stergere"] += 1
        else:
            numarat["asociat"] += 1

    # --- Final / Partial, o singura data per receptie ---------------------------
    for idrr in sorted(lanturi):
        recalculeaza_final(cursor, idrr)

    return numarat


def recalculeaza_final(cursor, idrr: int) -> None:
    """
    `Final` e cel mai TARZIU instantaneu dupa `DataH` din lant; tot ce e mai devreme e
    `Partial`.

    ASTA E O SCHIMBARE FATA DE ACCESS SI TREBUIE CITITA CA ATARE. `AsociazaFinal` facea
    `Final` orice tocmai atasase si retrograda restul. Cu plasare MANUALA regula aceea ar
    lasa un instantaneu din ianuarie sa devina `Final` pe o receptie care are deja unul
    din mai -- fiindca «tocmai atasat» nu mai inseamna «cel mai nou». Se recalculeaza per
    receptie, o data, dupa aplicarea TUTUROR deciziilor, nu la fiecare atasare.

    `HASH` se rescrie pe orice instantaneu al carui `TipReceptie` se schimba, exact ca in
    VBA -- hash-ul de identitate al antetului contine tipul.
    """
    cursor.execute(_H_LANT_SQL, (idrr,))
    lant = cursor.fetchall()
    if not lant:
        return
    ultimul_idrh = int(lant[-1]["IDRH"])
    for r in lant:
        dorit = "Final" if int(r["IDRH"]) == ultimul_idrh else "Partial"
        if (r["TipReceptie"] or "") == dorit:
            continue
        h = fx_receptii_h_get_hash_ident(
            r["CodAngajament"] or "", r["DataH"], dorit, r["Descriere"] or "")
        cursor.execute(_H_TIP_SQL, (dorit, h, int(r["IDRH"])))
