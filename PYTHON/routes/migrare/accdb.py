# routes/migrare/accdb.py
# -----------------------------------------------------------------------------
# Reading an .accdb on Linux, through mdbtools (mdb-tables / mdb-schema / mdb-json).
#
# WHY mdbtools and not a Python library: there is no maintained pure-Python reader
# for ACCDB. `pyaccdb` does not exist on PyPI at all; `access-parser` is a forensic
# parser with partial type coverage. mdbtools is the packaged, widely used one.
#
# WHY mdb-json and not mdb-export: mdb-export emits CSV, where a NULL and an empty
# string are the same empty field. mdb-json omits NULL columns from the object, so
# the two stay apart -- which matters, because "NULL into a NOT NULL column" is one
# of the findings the operator has to see.
#
# WHAT THIS CANNOT DO: decrypt. A file that still carries its database password
# («andreI» on the FOREXE databases) is refused with a named error; the operator
# removes the password in Access before pushing. See README.md.
# -----------------------------------------------------------------------------

import json
import logging
import os
import re
import subprocess

# config.py sta pe server, nu in depozit. Importul lene ii lasa pe modulele
# pure (validare, rutare, tabele) sa fie importabile si pe o statie fara config,
# ca suita de teste offline sa poata rula. Rutele, care chiar au nevoie de el,
# esueaza oricum la primul apel, cu numele cheii lipsa.
try:
    import config
except ImportError:                                  # pragma: no cover - off-host
    config = None

logger = logging.getLogger(__name__)

# Formatul in care cerem datele. Exact ce accepta MariaDB fara conversie.
DATE_FORMAT = "%Y-%m-%d %H:%M:%S"

# Cat asteptam dupa un mdb-tables / mdb-schema (operatii mici).
SHORT_TIMEOUT = 120


class AccdbError(Exception):
    """Eroare de citire a fișierului Access, cu mesaj în română."""


def _tool(name):
    """
    Calea unei unelte mdbtools. config.MDB_TOOLS_BIN, daca e pus, e folderul in
    care stau binarele; altfel se cauta in PATH.
    """
    folder = getattr(config, "MDB_TOOLS_BIN", None)
    return os.path.join(folder, name) if folder else name


def ensure_tools():
    """
    Uneltele exista si raspund? Apelat inaintea oricarei analize, ca esecul sa fie
    „mdbtools nu e instalat", nu o eroare de parsare cu zece randuri mai jos.
    """
    for name in ("mdb-tables", "mdb-schema", "mdb-json"):
        try:
            subprocess.run([_tool(name), "--help"],
                           stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                           timeout=30, check=False)
        except FileNotFoundError:
            raise AccdbError(
                "Unealta «%s» nu este instalată pe server. Instalați pachetul "
                "mdbtools (vezi README-ul feliei 0044)." % name)
        except OSError as exc:
            raise AccdbError("Unealta «%s» nu poate fi pornită: %s" % (name, exc))


def _run(args, timeout):
    try:
        proc = subprocess.run(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                              timeout=timeout, check=False)
    except FileNotFoundError:
        raise AccdbError("Unealta «%s» nu este instalată pe server." % args[0])
    except subprocess.TimeoutExpired:
        raise AccdbError("Citirea fișierului Access a depășit timpul alocat (%ds)." % timeout)

    if proc.returncode != 0:
        err = proc.stderr.decode("utf-8", "replace").strip()
        raise AccdbError("«%s» a eșuat: %s" % (os.path.basename(args[0]), err or "fără detalii"))
    return proc.stdout.decode("utf-8", "replace")


def list_tables(path):
    """
    Tabelele utilizator din fisier.

    Zero tabele inseamna, in practica, ca fisierul e inca protejat prin parola:
    mdbtools nu decripteaza si vede o structura goala. Mesajul spune exact asta,
    fiindca e cea mai probabila cauza si singura pe care o poate rezolva operatorul.
    """
    if not os.path.isfile(path):
        raise AccdbError("Fișierul «%s» nu se află pe server." % os.path.basename(path))

    out = _run([_tool("mdb-tables"), "-1", path], SHORT_TIMEOUT)
    tables = [t.strip() for t in out.splitlines() if t.strip()]
    if not tables:
        raise AccdbError(
            "Fișierul «%s» nu conține niciun tabel vizibil. Cel mai probabil este "
            "încă protejat prin parolă — scoateți parola din Access (Fișier ▸ "
            "Informații ▸ Decriptare bază de date) și împingeți din nou."
            % os.path.basename(path))
    return tables


_COL_RE = re.compile(r"^\s*`(?P<name>[^`]+)`\s+(?P<type>[A-Za-z0-9_ ]+?)\s*(?:\((?P<size>[0-9, ]+)\))?\s*,?\s*$")


def columns(path, table):
    """
    Coloanele unui tabel, in ordinea din Access: [{"nume", "tip", "marime"}].

    Sursa e `mdb-schema ... mysql`, adica DDL-ul pe care mdbtools il deduce el
    insusi. Tipul de aici NU decide nimic la validare -- ala vine din MariaDB,
    care e cel care accepta sau respinge randul. Serveste doar la raportarea
    coloanelor care exista in Access si lipsesc din tinta.
    """
    out = _run([_tool("mdb-schema"), "--table", table, path, "mysql"], SHORT_TIMEOUT)

    cols = []
    inside = False
    for line in out.splitlines():
        stripped = line.strip()
        if stripped.upper().startswith("CREATE TABLE"):
            inside = True
            continue
        if not inside:
            continue
        if stripped.startswith(")"):
            break
        m = _COL_RE.match(line)
        if m:
            size = m.group("size")
            cols.append({
                "nume": m.group("name"),
                "tip": m.group("type").strip().upper(),
                "marime": int(size.split(",")[0]) if size and size.strip().split(",")[0].strip().isdigit() else None,
            })

    if not cols:
        raise AccdbError(
            "Nu s-au putut citi coloanele tabelului «%s» din «%s»."
            % (table, os.path.basename(path)))
    return cols


def iter_rows(path, table, timeout=3600):
    """
    Randurile tabelului, unul cate unul, ca dictionare.

    O coloana NULL LIPSESTE din dictionar -- asa scrie mdb-json, si pe asta se
    bazeaza verificarea „NULL intr-o coloana NOT NULL". Apelantul completeaza
    lipsurile cu None dupa lista de coloane.

    Streaming, nu materializare: FX_2026.accdb are ~29 MB si tabelele cu imagini
    poarta Memo-uri de ordinul megaoctetului.
    """
    args = [_tool("mdb-json"), "-D", DATE_FORMAT, path, table]
    try:
        proc = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    except FileNotFoundError:
        raise AccdbError("Unealta «mdb-json» nu este instalată pe server.")

    try:
        for raw in proc.stdout:
            line = raw.decode("utf-8", "replace").strip()
            if not line:
                continue
            try:
                yield json.loads(line)
            except ValueError as exc:
                raise AccdbError(
                    "Un rând din «%s» nu a putut fi interpretat: %s" % (table, exc))
    finally:
        # Consumatorul poate opri devreme (o eroare in validare). Nu lasam procesul
        # in urma si nu inghitim codul de iesire cand a mers pana la capat.
        if proc.poll() is None:
            proc.kill()
            proc.stdout.close()
            proc.stderr.close()
            proc.wait()
        else:
            err = proc.stderr.read().decode("utf-8", "replace").strip()
            proc.stdout.close()
            proc.stderr.close()
            if proc.returncode != 0:
                raise AccdbError("Citirea tabelului «%s» a eșuat: %s"
                                 % (table, err or "fără detalii"))


def count_rows(path, table, timeout=3600):
    """
    Câte rânduri are tabelul, fără să interpreteze niciunul.

    mdbtools nu are un „count", deci tot fișierul trece prin `mdb-json` — dar
    aici se numără doar liniile, fără `json.loads` și fără dicționare, ceea ce e
    de câteva ori mai ieftin decât o citire adevărată. Asta ține pasul de
    inventar (lista de tabele cu bife) departe de prețul analizei.
    """
    args = [_tool("mdb-json"), "-D", DATE_FORMAT, path, table]
    try:
        proc = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    except FileNotFoundError:
        raise AccdbError("Unealta «mdb-json» nu este instalată pe server.")

    rows = 0
    try:
        for raw in proc.stdout:
            if raw.strip():
                rows += 1
    finally:
        if proc.poll() is None:
            proc.kill()
            proc.stdout.close()
            proc.stderr.close()
            proc.wait()
        else:
            err = proc.stderr.read().decode("utf-8", "replace").strip()
            proc.stdout.close()
            proc.stderr.close()
            if proc.returncode != 0:
                raise AccdbError("Numărarea rândurilor din «%s» a eșuat: %s"
                                 % (table, err or "fără detalii"))
    return rows
