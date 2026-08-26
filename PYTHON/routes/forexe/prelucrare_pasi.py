# routes/forexe/prelucrare_pasi.py
"""
Pasii 3, 4a, 4b, 4d, 5 si 7 ai ingestiei FOREXE (felia 0048-03).

Pasii 0-2 (tranzactia, FX_Angajamente, FX_Indicatori) stau in prelucrare.py, de unde
au venit in felia 0048-02. Pasul 4c (asocierea) sta in prelucrare_asociere.py, fiindca
e SINGURUL loc in care cele doua faze -- propunere si salvare -- se comporta diferit.
Tot ce e aici ruleaza IDENTIC in ambele faze; propunerea nu are nicio ramura proprie.

DE CE CONTEAZA CA E ACELASI COD. Faza «propunere» ruleaza pipeline-ul intreg intr-o
tranzactie si apoi o DERULEAZA INAPOI neconditionat. Nu e o rulare pe uscat, nu e o
ramura paralela: e exact acelasi drum, terminat cu rollback() in loc de commit().
Doua implementari ar aluneca una fata de alta, iar alunecarea nu s-ar vedea pana cand
n-ar produce cifre gresite pe care nimeni nu le mai compara cu nimic.

CUM SE CITESTE FISIERUL (operatorul a cerut explicit: comentariile spun CE face un
rand, nu doar de ce; cititorul stie SQL si VB.NET, nu idiomuri Python):

  * `cursor.execute(SQL, (a, b))`  ruleaza SQL cu parametri. `%s` e locul in care
                                   driverul pune valoarea, deja escapata. Niciodata
                                   f-string in SQL.
  * `cursor.lastrowid`             cheia AUTO_INCREMENT tocmai atribuita -- echivalentul
                                   lui `rs.Bookmark = rs.LastModified` urmat de `rs!ID`.
  * `dict` / `set`                 Scripting.Dictionary / un dictionar fara valori.
  * `list.append(x)`               Collection.Add.
  * `for x in lista:`              For Each.
  * `enumerate(lista)`             For Each care da si indexul, pornind de la 0.
  * `raise ValueError(...)`        Err.Raise. Niciodata inghitit aici.

FIDELITATE. Unde VBA-ul are un defect, se reproduce COMPORTAMENTUL OBSERVABIL si se
spune in comentariu, fiindca datele deja aflate in MariaDB au fost produse de acel
comportament. Abaterile sunt numite una cate una, la fata locului.
"""
import json
import logging
from typing import Dict, List, Optional, Tuple

from .prelucrare_helpers import (
    cod_ai,
    extract_number_after_label,
    extract_numar_rev,
    extract_obs_value,
    extract_rezervare_definitiva,
    extract_text_after_label,
    extract_text_between,
    fx_extract_cod_indicator,
    fx_receptii_istoric_get_indent,
    get_hash_for_row_istoric,
    get_tip_rand,
    is_rand_contract_row,
    is_stergere_receptie,
    null_if_empty,
    parse_amount,
    parse_english_date,
    parse_timp_istoric,
)

logger = logging.getLogger(__name__)

# Numele tabelului de istoric din `tabele`. Fara sufixul `_results`, spre deosebire de
# celelalte doua -- asa il trimite executorul (planul parinte 5.4).
TABLE_ISTORIC = "TabelIstoric"
TABLE_RECEPTII = "ListaReceptii_results"


# ===========================================================================
# Indicatorii angajamentului, cititi o singura data
# ===========================================================================
# Access: ClasificatiiG INNER JOIN FX_Indicatori ON (IdUnitate) AND (IdClsf).
#
# Pe MariaDB clasificatia se ia prin SUBINTEROGARE SCALARA cu LIMIT 1, nu prin JOIN:
# nomenclatorul are duplicate reale pe (IdClsfAcc, IdUnitate) -- MAPARE_NOMENCLATOARE.md
# 3.2 -- si un JOIN ar multiplica randurile de indicator. E tiparul pe care rutele de
# CITIRE il folosesc deja (routes/forexe/receptii.py); aici e la fel de sigur, fiindca
# duplicatele difera doar prin `IDClsf`, iar `IDClsf` nu se scrie nicaieri (D7).
#
# `CodSSI` NU exista ca si coloana pe MariaDB (MAPARE_NOMENCLATOARE.md rand 261: «Memo,
# 01A650402100101. No column on the target»). E `SS` lipit de `ClsfSal`, exact ce produce
# si FX_Receptii_NormalizeSSI din textul brut al payload-ului. Se calculeaza, nu se citeste.
#
# ABATERE DE LA VBA, deliberata si consemnata: Access folosea INNER JOIN, deci un
# indicator a carui clasificatie lipseste CADEA din recordset si randurile lui de istoric
# ramaneau tacut fara CodAI. Aici indicatorul RAMANE, cu `Clsf`/`CodSSI` NULL, si se emite
# un avertisment. E aceeasi alegere ca D19 din felia 0048-02 (o clasificatie neredusa
# lasa coloana goala si avertizeaza, nu sterge randul), si respecta regula casei: fara
# no-op-uri tacute.
_INDICATORI_SQL = (
    "SELECT I.CodAI, I.CodIndicator, I.IdClsf, I.IdUnitate, I.SS, I.IndicatorFX, "
    "  (SELECT C.Clsf FROM Clasificatii C "
    "    WHERE C.IdUnitate = I.IdUnitate AND C.IdClsfAcc = I.IdClsf LIMIT 1) AS Clsf, "
    "  (SELECT CONCAT(C.SS, C.ClsfSal) FROM Clasificatii C "
    "    WHERE C.IdUnitate = I.IdUnitate AND C.IdClsfAcc = I.IdClsf LIMIT 1) AS CodSSI, "
    "  I.NrCrt "
    "FROM FX_Indicatori I WHERE I.CodAngajament = %s ORDER BY I.NrCrt"
)


def read_indicatori(cursor, cod: str, warnings: List[str]) -> Dict[str, dict]:
    """
    Indicatorii angajamentului, indexati dupa CodAI.

    Se citesc O SINGURA DATA si se tin in memorie: VBA-ul deschidea un snapshot si facea
    FindFirst pe el la fiecare rand de istoric, ceea ce aici ar fi un SELECT per rand.
    Un dictionar da acelasi raspuns fara drumul la server.
    """
    cursor.execute(_INDICATORI_SQL, (cod,))
    out: Dict[str, dict] = {}
    for row in cursor.fetchall():
        out[str(row["CodAI"])] = dict(row)
        if row["Clsf"] is None:
            warnings.append(
                f"Indicatorul {row['CodIndicator']} nu are clasificație în nomenclator "
                f"(IdClsf {row['IdClsf']}, unitatea {row['IdUnitate']}); rândurile lui "
                f"de istoric rămân fără «Clsf» și fără «CodSSI»."
            )
    return out


# ===========================================================================
# PASUL 3a -- FX_Istoric_Populeaza_Istoric
# ===========================================================================
_ISTORIC_EXISTENTE_SQL = (
    "SELECT ID, DataFX, Utilizator, Descriere, Observatii "
    "FROM FX_Istoric WHERE CodAngajament = %s"
)
_ISTORIC_MAXORD_SQL = (
    "SELECT MAX(Rez_Ord) AS MaxOrd FROM FX_Istoric WHERE CodAngajament = %s"
)
_ISTORIC_INSERT_SQL = (
    "INSERT INTO FX_Istoric "
    "(CodAngajament, Descriere, Observatii, Utilizator, HASH, Rez_Ord, DataFX, Prelucrat) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, 0)"
)


def _cheie_istoric(data_fx, utilizator, descriere, observatii) -> tuple:
    """
    Identitatea unui rand de istoric: cele PATRU campuri din care Access construia hash-ul.

    D9 din planul parinte: deduplicarea merge pe cheia naturala, NU pe sirul de hash.
    Motivul e ca hash-urile deja aflate in MariaDB au fost calculate de invelisul BCrypt
    din Access, si doua lucruri despre acel sir sunt necunoscute din export -- codificarea
    pe octeti (ANSI vs UTF-8, conteaza in clipa in care o Observatie poarta «a») si daca
    hexul iesea cu majuscule. Daca hash-ul Python difera cu un octet, FIECARE rand vazut
    inainte pare nou si angajamentul se dubleaza singur la prima re-descarcare. Tacut.

    Un `tuple` e o lista imutabila; Python poate folosi tuple drept cheie de dictionar sau
    membru de `set`, ceea ce face testul «l-am mai vazut?» o cautare in tabela de dispersie.
    """
    return (
        data_fx,
        (utilizator or "").strip(),
        (descriere or "").strip(),
        (observatii or "").strip(),
    )


def step3a_populeaza_istoric(cursor, cod: str, randuri: List[dict],
                             indicatori: Dict[str, dict]) -> Tuple[Dict[int, int], List[int]]:
    """
    Scrie randurile NOI de istoric si intoarce doua lucruri.

    Intoarce:
      * `index_la_id`  -- {indicele randului in TabelIstoric: FX_Istoric.ID}, pentru
                          FIECARE rand din payload, si cele gasite si cele inserate.
                          Asta e ancora intregului contract in doua faze: `rand_istoric`
                          din `decizii` e chiar acest indice (F24). Randurile VECHI trebuie
                          si ele sa fie in harta -- FOREXE trimite istoricul intreg la
                          fiecare descarcare, iar D-F cere ca instantaneele ramase
                          neasociate din rulari anterioare sa poata fi decise acum.
      * `ids_inserate` -- doar cele scrise ACUM. Pasul 7 marcheaza exact pe acestea.

    `Rez_Ord` e siretlicul de ordonare si e usor de gresit. Multiplicatorul 0/100/1000 se
    alege din MAX(Rez_Ord) existent, apoi se MUTA pe parcursul buclei cand apare un rand
    de «Initial ->» sau «definitivare ->». Un rand ale carui Observatii incep cu
    «RAND CONTRACT:» primeste ordinalul indicatorului plus multiplicatorul curent.
    """
    cursor.execute(_ISTORIC_MAXORD_SQL, (cod,))
    rand = cursor.fetchone()
    max_ord = (rand or {}).get("MaxOrd") or 0
    # Access: If lMaxOrd >= 1000 Then Multi = 1000 ElseIf >= 100 Then Multi = 100.
    multi = 1000 if max_ord >= 1000 else (100 if max_ord >= 100 else 0)

    # Ordinea indicatorilor -- portul lui FX_DicInd_Ordine: {CodAI: NrCrt}.
    ordine = {k: int(v["NrCrt"] or 0) for k, v in indicatori.items()}

    cursor.execute(_ISTORIC_EXISTENTE_SQL, (cod,))
    existente: Dict[tuple, int] = {}
    for r in cursor.fetchall():
        existente[_cheie_istoric(r["DataFX"], r["Utilizator"],
                                 r["Descriere"], r["Observatii"])] = int(r["ID"])

    index_la_id: Dict[int, int] = {}
    ids_inserate: List[int] = []

    for i, row in enumerate(randuri):
        if not isinstance(row, dict):
            raise ValueError(f"{TABLE_ISTORIC}[{i}] nu este un obiect.")
        descriere = row.get("Descriere") or ""
        observatii = row.get("Observatii") or ""
        utilizator = row.get("Utilizator") or ""
        data_fx = parse_timp_istoric(row.get("Timp"))
        if data_fx is None:
            raise ValueError(
                f"{TABLE_ISTORIC}[{i}] nu are «Timp»; rândul nu poate fi datat."
            )

        cheie = _cheie_istoric(data_fx, utilizator, descriere, observatii)
        gasit = existente.get(cheie)
        if gasit is not None:
            # Deja in baza -- din aceasta rulare (un rand identic mai devreme in acelasi
            # payload) sau dintr-una anterioara. In ambele cazuri NU se rescrie nimic.
            index_la_id[i] = gasit
            continue

        # --- Rez_Ord, exact ramurile din VBA, in exact aceeasi ordine -------------
        rez_ord: Optional[int] = None
        d_low = descriere.lower()
        if "angajament nou" in d_low:
            # Primul rand de creare, chiar daca in FOREXE apare al doilea.
            rez_ord = 0
        elif "initial ->" in d_low:
            rez_ord = 100
            multi = 100
        elif "definitivare ->" in d_low:
            rez_ord = 1000
            multi = 1000
        elif observatii[:14].upper() == "RAND CONTRACT:":
            cod_ind = fx_extract_cod_indicator(observatii)
            cheie_ai = cod_ai(cod, cod_ind or "")
            # VBA: dInd(CodAI) + Multi. Un CodAI absent dintr-un Scripting.Dictionary
            # da Empty, adica 0 la adunare -- deci un indicator necunoscut nu ridica
            # acolo. Se pastreaza: `.get(..., 0)` face acelasi lucru, explicit.
            rez_ord = ordine.get(cheie_ai, 0) + multi

        # HASH se scrie in continuare, chiar daca deduplicarea nu se sprijina pe el:
        # coloana ramane populata si o comparatie viitoare ramane posibila (D9).
        # Cheia hash-ului e construita din TEXTUL brut al payload-ului, nu din data
        # parsata -- asa o construia si Access (GetHashForRow_Istoric peste `Timp`).
        row_hash = get_hash_for_row_istoric({
            "Timp": row.get("Timp"),
            "Utilizator": row.get("Utilizator"),
            "Descriere": row.get("Descriere"),
            "Observatii": row.get("Observatii"),
        })

        cursor.execute(_ISTORIC_INSERT_SQL,
                       (cod, descriere, observatii, utilizator, row_hash,
                        rez_ord, data_fx))
        new_id = int(cursor.lastrowid)
        index_la_id[i] = new_id
        ids_inserate.append(new_id)
        # Adaugarea in `existente` NU e o optimizare: doua randuri identice in acelasi
        # payload trebuie sa se colapseze intr-unul, exact cum se colapsau in Access
        # (rcHis.FindFirst mergea peste un recordset care crestea cu fiecare AddNew).
        # Fara ea, al doilea ar lovi indexul UNIQUE de pe FX_Istoric.HASH.
        existente[cheie] = new_id

    return index_la_id, ids_inserate


# ===========================================================================
# PASUL 3b -- FX_Istoric_Prelucreaza_Observatii
# ===========================================================================
_NEPRELUCRATE_SQL = (
    "SELECT ID, Descriere, Observatii, CodAngajament "
    "FROM FX_Istoric WHERE Prelucrat = 0 AND CodAngajament = %s ORDER BY ID"
)
# Ramura Edit din VBA reseteaza INTAI totul, apoi scrie. Se pastreaza intocmai: un rand
# reprocesat nu are voie sa pastreze o valoare dintr-o parsare anterioara.
_OBS_UPDATE_SQL = (
    "UPDATE FX_Istoric SET CodIndicator = %s, CodAI = %s, IdClsf = %s, Clsf = %s, "
    "Val_Receptie = %s, Val_Rezervare_I = %s, Val_Plata = %s, Val_Rezervare_D = %s, "
    "Val_AngLeg = %s, IdTrezor = %s, Doc = %s, Val_Rezervare_Dif = %s, "
    "Val_Rezervare_Ant = %s, IDREV = %s, TipRand = %s "
    "WHERE ID = %s"
)
_IDREV_LOOKUP_SQL = (
    "SELECT IDREV FROM FX_DDF_REV WHERE CodAngajament = %s AND NumarRev = %s LIMIT 1"
)


def _idrev_din_descriere(cursor, descriere: str, cod: str) -> Optional[int]:
    """Port al lui GetIDREV_FromDescriere: «(REV:nn)» -> FX_DDF_REV.IDREV, sau None."""
    nr = extract_numar_rev(descriere)
    if nr is None or (cod or "").strip() == "":
        return None
    cursor.execute(_IDREV_LOOKUP_SQL, (cod, int(nr)))
    row = cursor.fetchone()
    return None if row is None else int(row["IDREV"])


def step3b_prelucreaza_observatii(cursor, cod: str,
                                  indicatori: Dict[str, dict]) -> None:
    """
    Umple coloanele derivate din textul liber al Observatiilor.

    Functia asta e parsare pura si e cel mai dens lucru din toata conducta. E portata
    rand cu rand; NU e restructurata. Ordinea testelor conteaza si e pastrata.
    """
    cursor.execute(_NEPRELUCRATE_SQL, (cod,))
    randuri = cursor.fetchall()

    for r in randuri:
        obs = r["Observatii"] or ""
        descr = r["Descriere"] or ""
        contract = r["CodAngajament"] or ""

        # --- resetare, exact ca in VBA -------------------------------------
        cod_indicator = None
        cheie_ai = None
        id_clsf = None
        clsf = None
        val_receptie = 0.0
        val_rez_i = 0.0
        val_plata = 0.0
        val_rez_d = 0.0
        val_angleg = 0.0
        id_trezor = None
        doc = None
        val_rez_dif = 0.0
        val_rez_ant = 0.0
        idrev = None
        tip_rand = None

        # --- indicatorul -----------------------------------------------------
        ci = fx_extract_cod_indicator(obs)
        if ci is not None:
            ind = indicatori.get(cod_ai(contract, ci))
            if ind is not None:
                cod_indicator = ci
                if contract != "":
                    cheie_ai = cod_ai(contract, ci)
                    id_clsf = ind["IdClsf"]
                    clsf = ind["Clsf"]

        # --- rand de contract: cele trei valori de rezervare -----------------
        if "rand contract:" in obs.lower():
            val_angleg = extract_number_after_label(obs, "angajament legal:")
            val_rez_i = extract_number_after_label(
                obs, "credit de angajament rezervat intial:")
            val_rez_d = extract_rezervare_definitiva(obs)

        # --- receptie, linie ---------------------------------------------------
        if "suma receptie:" in obs.lower():
            val_receptie = parse_amount(
                extract_text_between(obs, "Suma receptie:", "RON")) or 0.0
            tip_rand = get_tip_rand(obs)

        # --- receptie, antet ---------------------------------------------------
        # Testul e SEPARAT, nu `elif`: GetTipRand da prioritate lui «suma receptie:»,
        # deci un rand care le poarta pe amandoua ramane «Receptie», dar Val_Receptie
        # se rescrie aici din «valoare:». Fidel VBA-ului.
        if "(activ:true)" in obs.lower():
            val_receptie = parse_amount(
                extract_text_between(obs, "valoare:", ", (activ:true)")) or 0.0
            tip_rand = get_tip_rand(obs)

        # --- plata ------------------------------------------------------------
        if "plata:" in obs.lower():
            val_plata = extract_number_after_label(obs, "valoare:")
            id_trezor = null_if_empty(extract_text_after_label(obs, "IdTrezor:"))
            doc = null_if_empty(extract_text_between(obs, "document:", ", data:"))
            # Cautarea in Descriere e SENSIBILA la litere mari/mici in VBA (InStr fara
            # vbTextCompare) si asa ramane. «ncasare» prinde si «Incasare» si «Incasare»
            # -- de-asta era taiat prefixul acolo, si de-asta nu se «repara» aici.
            if "ncasare" in descr:
                tip_rand = "PLATA_INCASARE"
            elif "Retur" in descr:
                tip_rand = "PLATA_RETUR"
            else:
                tip_rand = "PLATA_PLATA"

        idrev = _idrev_din_descriere(cursor, descr, contract)

        cursor.execute(_OBS_UPDATE_SQL, (
            cod_indicator, cheie_ai, id_clsf, clsf,
            val_receptie, val_rez_i, val_plata, val_rez_d, val_angleg,
            id_trezor, doc, val_rez_dif, val_rez_ant, idrev, tip_rand,
            int(r["ID"]),
        ))

    _calculeaza_val_rezervare_dif(cursor, cod)


# ---------------------------------------------------------------------------
# CalculeazaValRezervareDif -- apelata la coada lui 3b, ca in VBA
# ---------------------------------------------------------------------------
# Planul nu o numeste, dar FX_Istoric_Prelucreaza_Observatii se termina cu ea si TOATE
# valorile `TipRand` de rezervare («Rez_Initiala», «Rez_Definitiva», «Rez_Influenta»,
# «Rez_Zero» si cele trei cu «+») sunt puse AICI, nu mai sus. Pasii 3c/3d filtreaza pe
# ele, deci fara functia asta rezervarile ar fi mereu zero.
_SEED_LAST_ANG_SQL = (
    "SELECT T.CodIndicator, T.Val_AngLeg FROM FX_Istoric AS T "
    "INNER JOIN (SELECT CodIndicator, MAX(ID) AS MaxID FROM FX_Istoric "
    "            WHERE CodAngajament = %s AND Prelucrat = 1 "
    "              AND COALESCE(CodIndicator, '') <> '' "
    "              AND TipRand IN ('Rez_Initiala','Rez_Definitiva','Rez_Influenta','Rez_Zero') "
    "            GROUP BY CodIndicator) AS Q "
    "  ON T.CodIndicator = Q.CodIndicator AND T.ID = Q.MaxID"
)
_SEED_TIPRAND_SQL = (
    "SELECT DISTINCT CodIndicator FROM FX_Istoric "
    "WHERE CodAngajament = %s AND Prelucrat = 1 "
    "  AND COALESCE(CodIndicator, '') <> '' AND TipRand = %s"
)
# Portul lui qFX_ISTORIC_REZERVARI_TIPRAND. `CDate(Format(DataFX,'Short Date'))` =
# trunchiere la zi; in MariaDB, DATE(DataFX).
_TIPRAND_RANDURI_SQL = (
    "SELECT FIS.ID, FIS.Observatii, FIS.Descriere, FIS.CodIndicator, FIS.Val_AngLeg "
    "FROM FX_Istoric FIS "
    "WHERE FIS.Rez_Ord IS NOT NULL AND FIS.CodAngajament = %s AND FIS.Prelucrat = 0 "
    "  AND COALESCE(FIS.TipRand, '') = '' "
    "ORDER BY DATE(FIS.DataFX), FIS.Rez_Ord, FIS.ID"
)
_SET_TIPRAND_SQL = "UPDATE FX_Istoric SET TipRand = %s WHERE ID = %s"
_SET_REZ_SQL = (
    "UPDATE FX_Istoric SET TipRand = %s, Val_Rezervare_Ant = %s, "
    "Val_Rezervare_Dif = %s WHERE ID = %s"
)


def _calculeaza_val_rezervare_dif(cursor, cod: str) -> None:
    """Port al lui CalculeazaValRezervareDif. Se citeste alaturi de sursa VBA."""
    cursor.execute(_SEED_LAST_ANG_SQL, (cod,))
    last_ang: Dict[str, float] = {}
    for r in cursor.fetchall():
        ci = (r["CodIndicator"] or "").strip()
        if ci:
            last_ang[f"{cod}|{ci}"] = float(r["Val_AngLeg"] or 0)

    cursor.execute(_SEED_TIPRAND_SQL, (cod, "Rez_Initiala"))
    are_initiala = {f"{cod}|{(r['CodIndicator'] or '').strip()}"
                    for r in cursor.fetchall()}
    cursor.execute(_SEED_TIPRAND_SQL, (cod, "Rez_Definitiva"))
    are_definitiva = {f"{cod}|{(r['CodIndicator'] or '').strip()}"
                      for r in cursor.fetchall()}

    cursor.execute(_TIPRAND_RANDURI_SQL, (cod,))
    randuri = cursor.fetchall()

    # Cele trei stari care se muta pe masura ce bucla inainteaza. In VBA sunt variabile
    # locale NEinitializate, deci False la intrare -- si asta conteaza: primul rand de
    # «Rand contract:» dinaintea oricarui «Angajament nou.» cade pe ramura ELSE.
    r_init = False
    r_def = False

    for r in randuri:
        obs = r["Observatii"] or ""
        descr = r["Descriere"] or ""
        cod_ind = (r["CodIndicator"] or "").strip()
        rid = int(r["ID"])

        if descr == "sume nemodificate":
            cursor.execute(_SET_TIPRAND_SQL, ("Rez_Zero", rid))
            continue

        if is_rand_contract_row(obs) and cod and cod_ind:
            k = f"{cod}|{cod_ind}"
            val_curent = float(r["Val_AngLeg"] or 0)
            val_anterior = float(last_ang.get(k, 0))

            if r_init:
                # Primul rand -- initializare angajament sau adaugare rand.
                cursor.execute(_SET_REZ_SQL, ("Rez_Initiala", 0, 0, rid))
                are_initiala.add(k)
            elif r_def:
                # Primul rand dupa Initiala. VBA-ul scrie ZERO in amandoua coloanele
                # aici, cu valorile reale comentate alaturi -- se pastreaza zero.
                cursor.execute(_SET_REZ_SQL, ("Rez_Definitiva", 0, 0, rid))
                are_definitiva.add(k)
            elif k in are_definitiva:
                dif = round(val_curent - val_anterior, 2)
                tip = "Rez_Zero" if (val_curent == 0 and val_anterior == 0) \
                    else "Rez_Influenta"
                cursor.execute(_SET_REZ_SQL, (tip, val_anterior, dif, rid))
            else:
                # Indicator adaugat fara Initiala -- direct Influenta.
                cursor.execute(_SET_REZ_SQL, ("Rez_Influenta", 0, val_curent, rid))
                are_definitiva.add(k)

            last_ang[k] = val_curent
        else:
            # Randurile de STARE. Ele muta cele trei steaguri pentru randurile care
            # urmeaza; de-asta ordinea interogarii (zi, Rez_Ord, ID) nu e decorativa.
            if descr == "Angajament nou.":
                r_init, r_def = True, False
                cursor.execute(_SET_TIPRAND_SQL, ("Rez_Initiala+", rid))
            elif "definitivare" in descr and "derulare" not in descr:
                r_init, r_def = False, True
                cursor.execute(_SET_TIPRAND_SQL, ("Rez_Definitiva+", rid))
            elif "derulare" in descr:
                r_init, r_def = False, False
                cursor.execute(_SET_TIPRAND_SQL, ("Rez_Derulare+", rid))


# ===========================================================================
# PASII 3c / 3d -- FX_Istoric_Populeaza_Rezervari (initiale, apoi influente)
# ===========================================================================
# Portul lui qFX_ISTORIC_REZ_INIT si qFX_ISTORIC_REZ. Doua note de traducere:
#
#   * `CDate(Format([DataFX],"Short Date"))` = trunchiere la zi -> `DATE(DataFX)`.
#   * Ambele interogari Access faceau JOIN cu ClasificatiiG DOAR pentru cheia de sortare
#     `Clsf`. Aici e subinterogare scalara cu LIMIT 1, din acelasi motiv ca mai sus:
#     un JOIN peste un nomenclator cu duplicate ar multiplica randurile de rezervare.
#
#   * `FX_Istoric.ID NOT IN (SELECT IDH FROM FX_Rezervari)` e ce face pasul repetabil.
#     Se pastreaza intocmai. (FX_Rezervari.IDH are si un index UNIQUE pe MariaDB, deci
#     o a doua scriere ar esua zgomotos oricum -- dar filtrul e cel care o previne.)
_REZ_SELECT = (
    "SELECT H.ID, I.CodAI, I.CodAngajament, I.CodIndicator, I.IdClsf, "
    "  DATE(H.DataFX) AS DataRezervare, I.Prevedere_Bugetara_Initiala AS R_CreditBug, "
    "  H.Val_Rezervare_I AS R_Initiala, H.Val_AngLeg AS R_Definitiva, "
    "  {valoare} AS R_Valoare, H.Val_Rezervare_Ant, H.IDREV, "
    "  (SELECT C.Clsf FROM Clasificatii C "
    "    WHERE C.IdUnitate = I.IdUnitate AND C.IdClsfAcc = I.IdClsf LIMIT 1) AS ClsfSort "
    "FROM FX_Indicatori I INNER JOIN FX_Istoric H ON I.CodAI = H.CodAI "
    "WHERE H.ID NOT IN (SELECT IDH FROM FX_Rezervari WHERE IDH IS NOT NULL) "
    "  AND I.CodAngajament = %s AND H.TipRand = %s "
    "ORDER BY DATE(H.DataFX), ClsfSort"
)
_REZ_INSERT_SQL = (
    "INSERT INTO FX_Rezervari "
    "(IDH, CodAI, CodAngajament, CodIndicator, IdClsf, DataRezervare, R_CreditBug, "
    " R_Initiala, R_Definitiva, R_Valoare, IDREV, EInitiala, R_Anterioara, "
    " EMicsorare, EMarire) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
)
_DDF_REV_EXISTA_SQL = "SELECT 1 FROM FX_DDF_REV WHERE IDREV = %s"


def step3cd_populeaza_rezervari(cursor, cod: str, initiala: bool,
                                warnings: List[str]) -> int:
    """
    Scrie FX_Rezervari. Apelata de doua ori: `initiala=True`, apoi `initiala=False`.

    D-E: un `IDREV` care arata catre o revizie absenta din FX_DDF_REV se scrie NULL, se
    numara si se intoarce ca avertisment. Nu ar trebui sa se intample; daca se intampla,
    avertismentul e singurul mod in care cineva afla. Alternativa -- sa lasam INSERT-ul
    sa cada -- ar lua toata tranzactia cu el (eroare 1452) pentru o legatura pe care
    pasul 3e o poate umple mai tarziu oricum.
    """
    valoare = "H.Val_AngLeg" if initiala else "H.Val_Rezervare_Dif"
    tip_rand = "Rez_Definitiva" if initiala else "Rez_Influenta"
    cursor.execute(_REZ_SELECT.format(valoare=valoare), (cod, tip_rand))
    randuri = cursor.fetchall()

    scrise = 0
    for r in randuri:
        idrev = r["IDREV"]
        if idrev is not None:
            cursor.execute(_DDF_REV_EXISTA_SQL, (int(idrev),))
            if cursor.fetchone() is None:
                warnings.append(
                    f"FX_Rezervari: revizia {idrev} nu există în FX_DDF_REV — "
                    f"legătura rămâne goală (indicator {r['CodIndicator']})."
                )
                idrev = None

        r_valoare = float(r["R_Valoare"] or 0)
        cursor.execute(_REZ_INSERT_SQL, (
            int(r["ID"]), r["CodAI"], r["CodAngajament"], r["CodIndicator"],
            r["IdClsf"], r["DataRezervare"], r["R_CreditBug"],
            r["R_Initiala"], r["R_Definitiva"], r_valoare, idrev,
            # EInitiala pe ramura initiala; pe cealalta, cele trei coloane de influenta.
            1 if initiala else 0,
            None if initiala else r["Val_Rezervare_Ant"],
            0 if initiala else (1 if r_valoare < 0 else 0),
            0 if initiala else (1 if r_valoare > 0 else 0),
        ))
        scrise += 1
    return scrise


# ===========================================================================
# PASUL 3e -- FX_Rezervari_Asocieaza_IDREV
# ===========================================================================
_REZ_CU_IDREV_SQL = (
    "SELECT IDRZ, IDREV FROM FX_Rezervari "
    "WHERE CodAngajament = %s AND IDREV IS NOT NULL AND AreDDF = 0"
)
_REZ_FARA_IDREV_SQL = (
    "SELECT IDRZ, CodIndicator FROM FX_Rezervari "
    "WHERE CodAngajament = %s AND IDREV IS NULL AND AreDDF = 0"
)
_MIN_IDREV_SA_SQL = (
    "SELECT MIN(SA.IDREV) AS MinIDREV FROM FX_DDF_REV_SA SA "
    "WHERE SA.CodAngajament = %s AND SA.CodIndicator = %s "
    "  AND SA.IDREV NOT IN (SELECT IDREV FROM FX_Rezervari "
    "                       WHERE CodAngajament = %s AND IDREV IS NOT NULL)"
)


def step3e_asociaza_idrev(cursor, cod: str) -> None:
    """
    Doua cazuri, exact ca in VBA.

    Cazul 1 -- randul poarta deja un IDREV explicit din parser («(REV:x)»): primeste
    `AreDDF = 1`, iar revizia numita primeste `Incarcat = 1`.

    Cazul 2 -- randul nu are IDREV: se cauta cea mai mica revizie disponibila din
    FX_DDF_REV_SA pentru acelasi indicator, care nu e deja folosita de o alta rezervare.
    Primeste `AreDDF = 1`, dar NU si `Incarcat` pe FX_DDF_REV -- diferenta e in VBA si e
    intentionata acolo: legatura e dedusa, nu declarata de operator.
    """
    cursor.execute(_REZ_CU_IDREV_SQL, (cod,))
    for r in cursor.fetchall():
        cursor.execute("UPDATE FX_Rezervari SET AreDDF = 1 WHERE IDRZ = %s",
                       (int(r["IDRZ"]),))
        cursor.execute(
            "UPDATE FX_DDF_REV SET Incarcat = 1 WHERE IDREV = %s AND CodAngajament = %s",
            (int(r["IDREV"]), cod))

    cursor.execute(_REZ_FARA_IDREV_SQL, (cod,))
    for r in cursor.fetchall():
        # Interogarea se re-ruleaza pentru FIECARE rand, fiindca `NOT IN (...)` de
        # deasupra trebuie sa vada rezervarile actualizate la pasii anteriori ai
        # aceleiasi bucle. VBA-ul face la fel, si aici chiar conteaza: altfel doua
        # rezervari ale aceluiasi indicator ar primi aceeasi revizie.
        cursor.execute(_MIN_IDREV_SA_SQL, (cod, r["CodIndicator"] or "", cod))
        row = cursor.fetchone()
        min_idrev = None if row is None else row["MinIDREV"]
        if min_idrev is not None:
            cursor.execute(
                "UPDATE FX_Rezervari SET IDREV = %s, AreDDF = 1 WHERE IDRZ = %s",
                (int(min_idrev), int(r["IDRZ"])))


# ===========================================================================
# PASUL 4a -- FX_Istoric_Populeaza_Receptii
# ===========================================================================
_RECEPTII_ISTORIC_SQL = (
    "SELECT ID, HASH, CodAI, CodAngajament, CodIndicator, IdClsf, DataFX, TipRand, "
    "       Descriere, Observatii, Val_Receptie "
    "FROM FX_Istoric "
    "WHERE CodAngajament = %s AND Prelucrat = 0 AND INSTR(COALESCE(TipRand,''), 'Receptie') <> 0 "
    "ORDER BY ID"
)
_MAX_NRCRT_H_SQL = (
    "SELECT MAX(NrCrt) AS MaxNr FROM FX_Receptii_H WHERE CodAngajament = %s"
)
_H_INSERT_SQL = (
    "INSERT INTO FX_Receptii_H "
    "(IDH, NrCrt, CodAngajament, DataH, Total, Descriere, EsteStergere, Sters) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, 0)"
)
_REC_INSERT_SQL = (
    "INSERT INTO FX_Receptii "
    "(IDRH, IDH, IdClsf, CodSSI, Clsf, IdUnitate, CodAI, CodAngajament, CodIndicator, "
    " Data, Valoare, ValoareOrig, HASH, TipIntern) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
)
_INDICATORI_VAZUTI_SQL = (
    "SELECT CodIndicator FROM FX_Receptii WHERE CodAngajament = %s GROUP BY CodIndicator"
)


def step4a_populeaza_receptii(cursor, cod: str,
                              indicatori: Dict[str, dict]) -> int:
    """
    Construieste instantaneele (FX_Receptii_H) si liniile lor (FX_Receptii) din istoric.

    Merge peste randurile NEPRELUCRATE de receptie, in ordinea `ID`, si acumuleaza:

      * un rand ale carui Observatii contin `(activ:true)` e un ANTET -> FX_Receptii_H,
        iar liniile stranse pana atunci se varsa sub el;
      * un rand cu `Val_Receptie <> 0` e o LINIE -> se pune deoparte.

    F21 -- STERGEREA. Un rand cu `Descriere = "Stergere receptie"` poarta si el
    `(activ:true)`, deci devine antet pe calea NORMALA. NU e deviat, NU e filtrat, NU e
    tratat special aici in afara de steagul `EsteStergere`. Nu are randuri pe indicator,
    deci nu produce nicio linie -- si asta iese de la sine, fiindca liniile lui nu exista
    in istoric, nu fiindca le-am opri noi. E ultimul instantaneu al lantului lui, si
    formularul trebuie sa il poata vedea si fixa.

    D3 -- fara tabele temporare. Access scria in tmpFX_Receptii_H / tmpFX_Receptii si le
    salva la pasul 6; aici se scrie direct in tabelele vii, inauntrul tranzactiei.
    `IDRR` ramane NULL: cine este receptia se hotaraste la pasul 4c.
    """
    cursor.execute(_RECEPTII_ISTORIC_SQL, (cod,))
    randuri = cursor.fetchall()
    if not randuri:
        return 0

    # Indicatorii care APAR DEJA in receptiile acestui angajament. Decid `TipIntern`
    # ('VECHI' / 'NOU') la scrierea liniilor.
    cursor.execute(_INDICATORI_VAZUTI_SQL, (cod,))
    vazuti = {str(r["CodIndicator"]) for r in cursor.fetchall()
              if r["CodIndicator"] is not None}

    cursor.execute(_MAX_NRCRT_H_SQL, (cod,))
    row = cursor.fetchone()
    nr_crt = int((row or {}).get("MaxNr") or 0) + 1

    tampon: List[dict] = []      # liniile stranse pana la urmatorul antet
    antete = 0

    for r in randuri:
        obs = r["Observatii"] or ""

        if "(activ:true)" in obs:
            # --- ANTET -----------------------------------------------------
            este_stergere = is_stergere_receptie(r["Descriere"])
            descriere = extract_text_between(obs, "Receptie: ", ",")
            cursor.execute(_H_INSERT_SQL, (
                int(r["ID"]), nr_crt, str(r["CodAngajament"]), r["DataFX"],
                float(r["Val_Receptie"] or 0), descriere,
                1 if este_stergere else 0,
            ))
            idrh = int(cursor.lastrowid)
            nr_crt += 1
            antete += 1

            for linie in tampon:
                ci = linie["CodIndicator"]
                cursor.execute(_REC_INSERT_SQL, (
                    idrh, linie["IDH"], linie["IdClsf"], linie["CodSSI"],
                    linie["Clsf"], linie["IdUnitate"], linie["CodAI"],
                    linie["CodAngajament"], ci, linie["Data"], linie["Valoare"],
                    linie["ValoareOrig"], linie["HASH"],
                    "VECHI" if ci in vazuti else "NOU",
                ))
                vazuti.add(ci)
            tampon = []

        elif float(r["Val_Receptie"] or 0) != 0:
            # --- LINIE -----------------------------------------------------
            ind = indicatori.get(str(r["CodAI"] or ""))
            if ind is None:
                # Se pastreaza ca EROARE, ca in VBA. O linie de receptie al carei
                # indicator nu e in FX_Indicatori nu are unde sa se duca, iar FK-ul de
                # pe CodAI ar refuza-o oricum -- mai bine cu un mesaj care il numeste.
                raise ValueError(
                    f"Nu am găsit indicatorul {r['CodIndicator']} în FX_Indicatori "
                    f"(rând de istoric {r['ID']})."
                )
            tampon.append({
                "IDH": int(r["ID"]),
                "IdClsf": r["IdClsf"],
                "CodSSI": ind["CodSSI"],
                "Clsf": ind["Clsf"],
                "IdUnitate": ind["IdUnitate"],
                "CodAI": str(r["CodAI"]),
                "CodAngajament": str(r["CodAngajament"]),
                "CodIndicator": str(r["CodIndicator"]),
                "Data": r["DataFX"],
                "Valoare": float(r["Val_Receptie"] or 0),
                "ValoareOrig": float(r["Val_Receptie"] or 0),
                # VBA: FX_Receptii_Istoric_GetIndent(CodAng, CodInd, DataFX, rcInd!Clsf,
                # Val_Receptie). Al patrulea argument e `Clsf`, NU `CodSSI`, desi numele
                # parametrului din helper e `CodSSI`. Fidel: hash-urile deja migrate au
                # fost calculate asa.
                "HASH": fx_receptii_istoric_get_indent(
                    str(r["CodAngajament"]), str(r["CodIndicator"]), r["DataFX"],
                    ind["Clsf"] or "", float(r["Val_Receptie"] or 0)),
            })

    # Un tampon nevidat la sfarsit inseamna linii fara antet. Access le pierdea tacut
    # (colectia se arunca la iesirea din functie). Aici nu se pierde nimic tacut, dar
    # nici nu se inventeaza un antet: raman in istoric, nemarcate, si vor fi prinse de
    # urmatoarea rulare cand antetul lor soseste.
    if tampon:
        logger.info("PRELUCRARE cod=%s: %s linii de receptie fara antet, amanate",
                    cod, len(tampon))

    return antete


# ===========================================================================
# PASUL 4b -- Receptii_Prelucrare
# ===========================================================================
# Compara `ListaReceptii_results` cu ce e DEJA stocat, in tabelele vii (D3).
#
# F25 -- REGULA ADAUGATA, si motivul ei merita citit inainte de a o sterge din greseala.
# Potrivirea Access e `CLng(DataR) = CLng(dtDataR)`: GRANULARITATE DE ZI. O receptie
# stearsa nu mai poate aparea in `ListaReceptii`, deci orice potrivire aparenta cu ea e o
# CIOCNIRE, nu o identificare. Fara `Sters = 0` mai jos, o receptie creata azi in aceeasi
# zi calendaristica in care fusese creata una stearsa in martie s-ar potrivi peste aceea
# si i-ar suprascrie tacut valorile.
_R_CANDIDATI_SQL = (
    "SELECT IDRR, DataR, SumaAntet FROM FX_Receptii_R "
    "WHERE CodAngajament = %s AND Sters = 0 AND DATE(DataR) = %s"
)
_RHR_SNAP_SQL = (
    "SELECT IDRHR, CodIndicator, Valoare FROM FX_Receptii_RHR WHERE IDRR = %s"
)
_MAX_NRCRT_R_SQL = (
    "SELECT MAX(NRCRT) AS MaxNr FROM FX_Receptii_R WHERE CodAngajament = %s"
)
_R_INSERT_SQL = (
    "INSERT INTO FX_Receptii_R "
    "(NRCRT, CodAngajament, Tip, DataR, SumaAntet, Descriere, TipReceptie, HASH, "
    " Preluat, Sters, Reconstituit) "
    "VALUES (%s, %s, %s, %s, %s, %s, 'NOU', %s, 1, 0, 0)"
)
_R_UPDATE_SUMA_SQL = (
    "UPDATE FX_Receptii_R SET SumaAntet = %s, TipReceptie = 'EDIT' WHERE IDRR = %s"
)
_RHR_INSERT_SQL = (
    "INSERT INTO FX_Receptii_RHR "
    "(IDRR, CodAngajament, CodIndicator, CodAI, IdClsf, IdUnitate, CodSSI, "
    " CreditBugetar, Valoare, ValoareN, TipIntern) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
)
_RHR_UPDATE_SQL = (
    "UPDATE FX_Receptii_RHR SET CreditBugetar = %s, Valoare = %s, ValoareN = %s, "
    "TipIntern = 'EDIT' WHERE IDRHR = %s"
)
_RHR_INDICATORI_VAZUTI_SQL = (
    "SELECT CodIndicator FROM FX_Receptii_RHR WHERE CodAngajament = %s "
    "GROUP BY CodIndicator"
)


def _detaliu(det: dict, cod: str, indicatori: Dict[str, dict]) -> dict:
    """
    Port al lui ObtineDateDetaliu -- o linie din `Detaliu`.

    `IdClsf` si `IdUnitate` vin din FX_Indicatori, nu din payload; un indicator care nu
    se gaseste lasa amandoua goale, exact ca `rsInd.NoMatch` din VBA (care lasa vIdClsf =
    Null si lIdUnitate = 0). Aici `IdUnitate` ramane None in loc de 0: coloana e
    nulabila, iar zero ar fi un id de unitate care nu exista, adica o minciuna.
    """
    from .prelucrare_helpers import fx_receptii_normalize_ssi, parse_loose_number

    ci = str(det.get("Cod") or "").strip()
    cheie = cod_ai(cod, ci)
    ind = indicatori.get(cheie)
    return {
        "CodIndicator": ci,
        "CodAI": cheie,
        "CodSSI": fx_receptii_normalize_ssi(det.get("Sector_Sursa_Indicator") or ""),
        "CreditBugetar": parse_loose_number(det.get("Credit_bugetar_rezervat_definitiv")),
        "Valoare": parse_loose_number(det.get("Valoare")),
        "ValoareN": parse_loose_number(det.get("Valoare_nereceptionata")),
        "IdClsf": None if ind is None else ind["IdClsf"],
        "IdUnitate": None if ind is None else ind["IdUnitate"],
    }


def _ca_lista(valoare, unde: str):
    """
    `Detaliu` soseste in DOUA forme, si amandoua sunt legitime.

    * Ca LISTA imbricata -- asa arata sarcina utila produsa de FOREXE/Access
      (FB_JOBS/resend/*.json), unde JSON-ul e pastrat structurat.
    * Ca SIR care contine JSON -- asa il trimite clientul K-BOT. `JobResult.Tables` e
      `Dictionary(Of String, String)`, iar `ForexeRunner.TryParseTable` aplaneaza fiecare
      celula cu `prop.Value.ToString()`; pentru un JArray imbricat, asta da chiar textul
      JSON. Verificat citind functia, nu presupus.

    Se accepta amandoua fiindca serverul chiar le primeste pe amandoua: fisierele de
    retrimitere din era Access si clientul nou. Un sir care NU e o lista JSON ridica --
    nu se degradeaza tacut in «fara detalii», ceea ce ar scrie o receptie fara linii.
    """
    if isinstance(valoare, list):
        return valoare
    if isinstance(valoare, str):
        text = valoare.strip()
        if text == "":
            raise ValueError(f"{unde}: «Detaliu» este gol.")
        try:
            parsat = json.loads(text)
        except ValueError as err:
            raise ValueError(f"{unde}: «Detaliu» nu este JSON valid.") from err
        if not isinstance(parsat, list):
            raise ValueError(f"{unde}: «Detaliu» nu este o listă.")
        return parsat
    raise ValueError(f"{unde}: «Detaliu» nu este o listă.")


def step4b_receptii_prelucrare(cursor, cod: str, randuri: List[dict],
                               indicatori: Dict[str, dict]) -> Tuple[int, int]:
    """
    Aduce `FX_Receptii_R` / `FX_Receptii_RHR` la zi din payload.

    Trei cazuri, exact ca in VBA:
      * receptie gasita si suma identica  -> nu se atinge `R`, dar `RHR` SE VERIFICA
        oricum (suma poate fi aceeasi cu distributia schimbata: AAB -100, AAC +100);
      * receptie gasita si suma diferita  -> UPDATE `R` + `RHR`;
      * receptie negasita                 -> INSERT `R` + toate `RHR`.

    Intoarce (receptii scrise, linii RHR scrise).
    """
    from .prelucrare_helpers import (
        fx_receptii_h_get_hash_ident, fx_receptii_parse_ro_date, parse_loose_number,
    )

    if not randuri:
        return 0, 0

    cursor.execute(_RHR_INDICATORI_VAZUTI_SQL, (cod,))
    vazuti = {str(r["CodIndicator"]) for r in cursor.fetchall()
              if r["CodIndicator"] is not None}

    cursor.execute(_MAX_NRCRT_R_SQL, (cod,))
    row = cursor.fetchone()
    nr_crt = int((row or {}).get("MaxNr") or 0) + 1

    r_scrise = 0
    rhr_scrise = 0

    for i, rec in enumerate(randuri):
        if not isinstance(rec, dict):
            raise ValueError(f"{TABLE_RECEPTII}[{i}] nu este un obiect.")
        if "Detaliu" not in rec or rec["Detaliu"] is None:
            raise ValueError(f"{TABLE_RECEPTII}[{i}]: lipsește colecția «Detaliu».")
        detalii = _ca_lista(rec["Detaliu"], f"{TABLE_RECEPTII}[{i}]")

        data_r = fx_receptii_parse_ro_date(rec.get("Data"))
        if data_r is None:
            raise ValueError(f"{TABLE_RECEPTII}[{i}]: «Data» lipsește sau e invalidă.")
        suma = parse_loose_number(rec.get("Suma"))
        # Hash-ul de identitate foloseste `Tip`, NU `TipReceptie` -- verificat citind
        # ObtineDateHeader. (Planul parinte 5.3 cerea un 400 la lipsa lui `TipReceptie`;
        # functia reala nu citeste niciodata cheia aceea, deci cerinta cade. Consemnat.)
        hash_ident = fx_receptii_h_get_hash_ident(
            cod, data_r, rec.get("Tip"), rec.get("DescriereReceptie"))

        cursor.execute(_R_CANDIDATI_SQL, (cod, data_r))
        candidati = cursor.fetchall()
        gasit = candidati[0] if candidati else None

        if gasit is not None:
            idrr = int(gasit["IDRR"])
            if round(float(gasit["SumaAntet"] or 0), 2) != round(suma, 2):
                cursor.execute(_R_UPDATE_SUMA_SQL, (suma, idrr))
                r_scrise += 1

            cursor.execute(_RHR_SNAP_SQL, (idrr,))
            existente = {str(x["CodIndicator"]): x for x in cursor.fetchall()}

            for det in detalii:
                d = _detaliu(det, cod, indicatori)
                vechi = existente.get(d["CodIndicator"])
                if vechi is not None:
                    if round(float(vechi["Valoare"] or 0), 2) != round(d["Valoare"], 2):
                        cursor.execute(_RHR_UPDATE_SQL, (
                            d["CreditBugetar"], d["Valoare"], d["ValoareN"],
                            int(vechi["IDRHR"])))
                        rhr_scrise += 1
                else:
                    cursor.execute(_RHR_INSERT_SQL, (
                        idrr, cod, d["CodIndicator"], d["CodAI"], d["IdClsf"],
                        d["IdUnitate"], d["CodSSI"], d["CreditBugetar"],
                        d["Valoare"], d["ValoareN"],
                        "VECHI" if d["CodIndicator"] in vazuti else "NOU"))
                    vazuti.add(d["CodIndicator"])
                    rhr_scrise += 1
        else:
            cursor.execute(_R_INSERT_SQL, (
                nr_crt, cod, rec.get("Tip"), data_r, suma,
                rec.get("DescriereReceptie"), hash_ident))
            idrr = int(cursor.lastrowid)
            nr_crt += 1
            r_scrise += 1

            for det in detalii:
                d = _detaliu(det, cod, indicatori)
                cursor.execute(_RHR_INSERT_SQL, (
                    idrr, cod, d["CodIndicator"], d["CodAI"], d["IdClsf"],
                    d["IdUnitate"], d["CodSSI"], d["CreditBugetar"],
                    d["Valoare"], d["ValoareN"],
                    "VECHI" if d["CodIndicator"] in vazuti else "NOU"))
                vazuti.add(d["CodIndicator"])
                rhr_scrise += 1

    return r_scrise, rhr_scrise


# ===========================================================================
# PASUL 4d -- FX_CalculeazaDIF_Receptii_Tmp
# ===========================================================================
# Port al functiei din mdl_FX_Helpers, rulata per receptie asociata. Regulile DIF
# hranesc eticheta plutitoare din Receptii, iar o greseala aici ramane invizibila pana
# cand cineva citeste un total.
#
# Access ordona instantaneele dupa `ID` («autonumber = ordine cronologica in tmp»). Aici
# ordinea e dupa `DataH`, apoi `IDRH`: sub plasare MANUALA, ordinea de inserare nu mai e
# ordinea cronologica -- operatorul poate atasa un instantaneu din ianuarie dupa unul din
# mai. `DataH` E axa timpului (F2), deci ea ordoneaza. Cu plasare pur automata cele doua
# ordini coincid, deci nimic din datele deja produse nu se schimba.
#
# LANTUL UNEI RECEPTII STERSE SE TERMINA LA INSTANTANEUL DE STERGERE. Nu exista randuri
# dupa el, deci nu exista o regula de inventat pentru ele.
_DIF_H_SQL = (
    "SELECT IDRH, Total FROM FX_Receptii_H "
    "WHERE CodAngajament = %s AND IDRR = %s ORDER BY DataH, IDRH"
)
_DIF_H_UPDATE_SQL = "UPDATE FX_Receptii_H SET DIFH = %s, DIFHC = %s WHERE IDRH = %s"
_DIF_R_SQL = (
    "SELECT R.IDR, R.CodAI, R.IdClsf, R.Valoare FROM FX_Receptii R "
    "INNER JOIN FX_Receptii_H H ON R.IDRH = H.IDRH "
    "WHERE R.CodAngajament = %s AND H.IDRR = %s "
    "ORDER BY R.CodAI, R.IdClsf, H.DataH, H.IDRH"
)
_DIF_R_UPDATE_SQL = "UPDATE FX_Receptii SET DIF = %s, DIFC = %s WHERE IDR = %s"


def step4d_calculeaza_dif(cursor, cod: str, idrr: int) -> None:
    """Recalculeaza DIFH/DIFHC pe antete si DIF/DIFC pe linii, pentru o receptie."""
    # --- antete ---------------------------------------------------------
    cursor.execute(_DIF_H_SQL, (cod, idrr))
    precedent = None      # valoarea instantaneului anterior
    primul = None         # valoarea PRIMULUI instantaneu al lantului
    for r in cursor.fetchall():
        curent = float(r["Total"] or 0)
        if precedent is None:
            dif, difc = curent, 0.0
            primul = curent
        else:
            dif, difc = curent - precedent, curent - primul
        precedent = curent
        cursor.execute(_DIF_H_UPDATE_SQL, (round(dif, 2), round(difc, 2),
                                           int(r["IDRH"])))

    # --- linii, grupate pe (CodAI, IdClsf) -------------------------------
    cursor.execute(_DIF_R_SQL, (cod, idrr))
    prec: Dict[str, float] = {}
    prim: Dict[str, float] = {}
    for r in cursor.fetchall():
        cheie = f"{r['CodAI'] or ''}|{r['IdClsf'] or 0}"
        curent = float(r["Valoare"] or 0)
        if cheie in prec:
            dif, difc = curent - prec[cheie], curent - prim[cheie]
        else:
            dif, difc = curent, 0.0
            prim[cheie] = curent
        prec[cheie] = curent
        cursor.execute(_DIF_R_UPDATE_SQL, (round(dif, 2), round(difc, 2),
                                           int(r["IDR"])))


# ===========================================================================
# PASUL 5 -- FX_Istoric_Populeaza_Plati_Incasari
# ===========================================================================
_PLATI_ISTORIC_SQL = (
    "SELECT ID, Observatii, DataFX FROM FX_Istoric "
    "WHERE CodAngajament = %s AND Prelucrat = 0 "
    "  AND INSTR(COALESCE(TipRand,''), 'PLATA_') <> 0 ORDER BY ID"
)
_PLATI_DEDUP_SQL = (
    "SELECT Referinta_TREZOR FROM FX_Plati WHERE CodAngajament = %s "
    "GROUP BY Referinta_TREZOR"
)
_PLATI_INSERT_SQL = (
    "INSERT INTO FX_Plati "
    "(IDH, IdClsf, IdUnitate, CodAI, NrOP, CodAngajament, CodIndicator, Data_plata, "
    " Indicator_IBAN, Clsf, Program, Referinta_TREZOR, Suma, Tip, Preluat) "
    "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, 1)"
)
_PROGRAM_SQL = "SELECT CodProgram FROM Unitati WHERE IdUnitate = %s"


def _format_clsf_iban(indicator_fx: Optional[str]) -> str:
    """
    Port al lui Format(IndicatorFX, "@@.@@.@@.@@.@@.@@").

    Masca `@` din VBA umple de la DREAPTA la STANGA si pastreaza caracterele literale.
    Pentru "650301200301" (12 cifre) da "65.03.01.20.03.01". Un sir mai scurt se
    alinieaza tot la dreapta, cu grupurile din stanga incomplete -- de-asta se lucreaza
    de la coada, nu de la cap.
    """
    s = (indicator_fx or "").strip()
    if s == "":
        return ""
    grupuri = []
    i = len(s)
    while i > 0:
        grupuri.append(s[max(0, i - 2):i])
        i -= 2
    grupuri.reverse()
    return ".".join(grupuri)


def step5_plati_incasari(cursor, cod: str, indicatori: Dict[str, dict],
                         warnings: List[str]) -> Tuple[int, bool, bool]:
    """
    Scrie FX_Plati din randurile de istoric `PLATA_*`.

    Intoarce (scrise, are_plati, are_incasari).

    Regulile pastrate exact:
      * deduplicare pe `Referinta_TREZOR`, pornita din platile deja stocate;
      * `Rand:` sau `IdTrezor:` lipsa -> se SARE randul si se logheaza (asta chiar e o
        sarire, nu o eroare -- e ce face VBA-ul);
      * data neparsabila -> se sare;
      * `Tip` = INCASARE cand suma e negativa, altfel PLATA;
      * un indicator negasit in FX_Indicatori RIDICA.

    `Program` venea din globala `globCodProgram`. Aici e `Unitati.CodProgram` pentru
    unitatea INDICATORULUI -- planul parinte 5.2a: fiecare plata isi rezolva deja
    indicatorul, deci `IdUnitate` se ia din randul acela, nu din clasificatie a doua oara.
    """
    cursor.execute(_PLATI_ISTORIC_SQL, (cod,))
    randuri = cursor.fetchall()
    if not randuri:
        return 0, False, False

    cursor.execute(_PLATI_DEDUP_SQL, (cod,))
    dedup = {str(r["Referinta_TREZOR"]) for r in cursor.fetchall()
             if r["Referinta_TREZOR"]}

    # CodProgram per unitate, citit o data si tinut minte.
    programe: Dict[int, Optional[str]] = {}

    scrise = 0
    are_plati = False
    are_incasari = False

    for r in randuri:
        obs = r["Observatii"] or ""
        if obs == "":
            continue

        cod_ind = extract_obs_value(obs, "Rand:")
        nr_op = extract_obs_value(obs, "document:")
        s_data = extract_obs_value(obs, "data:", "valoare:")
        s_suma = extract_obs_value(obs, "valoare:")
        id_trezor = extract_obs_value(obs, "IdTrezor:", "")

        if cod_ind == "" or id_trezor == "":
            logger.info("PRELUCRARE cod=%s: plata fara CodInd/IdTrezor, sarita "
                        "(doc=%s data=%s suma=%s)", cod, nr_op, s_data, s_suma)
            continue
        if id_trezor in dedup:
            logger.info("PRELUCRARE cod=%s: plata duplicat %s, sarita", cod, id_trezor)
            continue

        data_plata = parse_english_date(s_data.strip())
        if data_plata is None:
            logger.info("PRELUCRARE cod=%s: data platii neparsabila «%s», sarita",
                        cod, s_data)
            continue

        cheie = cod_ai(cod, cod_ind)
        ind = indicatori.get(cheie)
        if ind is None:
            raise ValueError(f"Nu am găsit indicatorul {cheie} în FX_Indicatori.")

        id_unitate = ind["IdUnitate"]
        if id_unitate is not None and id_unitate not in programe:
            cursor.execute(_PROGRAM_SQL, (int(id_unitate),))
            row = cursor.fetchone()
            programe[id_unitate] = None if row is None else row["CodProgram"]
        program = programe.get(id_unitate) if id_unitate is not None else None
        if program is None:
            warnings.append(
                f"Unitatea {id_unitate} nu are «CodProgram»; plata {nr_op} "
                f"({cod_ind}) se scrie fără program."
            )

        clsf_fmt = _format_clsf_iban(ind["IndicatorFX"])
        ind_iban = f"{ind['SS']}-{clsf_fmt}" if clsf_fmt else ""

        suma = parse_amount(s_suma) or 0.0
        tip = "INCASARE" if suma < 0 else "PLATA"
        if tip == "INCASARE":
            are_incasari = True
        else:
            are_plati = True

        cursor.execute(_PLATI_INSERT_SQL, (
            int(r["ID"]), ind["IdClsf"], id_unitate, cheie, nr_op, cod, cod_ind,
            # VBA: rcPlati!Data_Plata = rcHis!DataFX -- data randului de ISTORIC, nu
            # cea din Observatii. `data:` e parsata doar ca sa fie VALIDATA (o data
            # neparsabila sare randul). Contraintuitiv, fidel, verificat in mdl_FX_Plati.
            r["DataFX"], ind_iban, clsf_fmt, program, id_trezor, suma, tip,
        ))
        dedup.add(id_trezor)
        scrise += 1

    return scrise, are_plati, are_incasari


# ===========================================================================
# PASUL 7 -- FX_Istoric_Actualizeaza_Rezolvat
# ===========================================================================
def step7_actualizeaza_rezolvat(cursor, ids: List[int]) -> int:
    """
    `Prelucrat = 1` pe EXACT randurile pe care le-a inserat rularea asta.

    Randurile care existau deja isi pastreaza ce aveau. Distinctia e ce impiedica o
    re-descarcare sa reproceseze istoric vechi -- VBA-ul marca doar randurile al caror
    `IDH` fusese scris in timpul acelei treceri.

    In faza «propunere» asta se deruleaza inapoi cu tot restul, deci o propunere nu
    marcheaza NICIODATA istoricul ca prelucrat. Exact asta face rularea repetabila.
    """
    if not ids:
        return 0
    # `IN (%s, %s, ...)` construit din numarul de id-uri. Nu e f-string peste VALORI:
    # sunt tot atatea `%s` cate id-uri, iar valorile trec prin driver ca oriunde.
    locuri = ", ".join(["%s"] * len(ids))
    cursor.execute(
        f"UPDATE FX_Istoric SET Prelucrat = 1 WHERE ID IN ({locuri})",
        tuple(int(x) for x in ids))
    return len(ids)


# ===========================================================================
# PASUL 8 -- FX_Indicatori_Actualizare_Extrase: NEPORTAT, DELIBERAT
# ===========================================================================
# Functia a fost CITITA (mdl_FX_Tasks_Receive_DWN.md, «Private Function
# FX_Indicatori_Actualizare_Extrase»). Are exact doua instructiuni si amandoua scriu
# `FX_Extrase`:
#
#   UPDATE FX_Extrase INNER JOIN FX_Plati ON FX_Extrase.Referinta     = FX_Plati.Referinta_TREZOR
#      SET FX_Extrase.CodAI = FX_Plati.CodAI WHERE FX_Extrase.CodAI Is Null
#   UPDATE FX_Extrase INNER JOIN FX_Plati ON FX_Extrase.ReferintaDest = FX_Plati.Referinta_TREZOR
#      SET FX_Extrase.CodAI = FX_Plati.CodAI WHERE FX_Extrase.CodAI Is Null
#
# `FX_Extrase` e scos din scop de planul parinte 12. Planul acestei felii 0 spune, textual:
# «If it requires FX_Extrase, report that and stop rather than porting half of it.»
#
# Deci pasul 8 NU se executa. Nu e uitat, nu e mort si nu e inlocuit cu ceva
# aproximativ: e raportat. D-G («pasul 8 e portat») ramane valabil ca intentie si se
# implementeaza in felia care aduce FX_Extrase in scop.
#
# ATENTIE la ce inseamna absenta lui: randurile din `FX_Extrase` care s-ar fi legat de
# platile scrise la pasul 5 raman cu `CodAI` NULL. Nimic nu se strica; o legatura nu se
# face. Cand FX_Extrase intra in scop, cele doua UPDATE-uri de mai sus recupereaza tot
# ce a ramas in urma, fiindca amandoua sunt filtrate pe `CodAI Is Null`.
MESAJ_PAS8 = (
    "Pasul 8 (FX_Indicatori_Actualizare_Extrase) nu s-a executat: scrie doar "
    "«FX_Extrase», care este în afara scopului acestei felii. Legăturile "
    "«FX_Extrase.CodAI» rămân goale și se pot completa ulterior."
)
