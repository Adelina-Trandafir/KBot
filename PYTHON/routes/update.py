# routes/update.py
"""
Application update channel for K-BOT (slice 0067).

Two PUBLIC routes -- no bearer, no API key. The client checks BEFORE login, when it
holds no credential at all (ApiOptions: bearer only, issued at login), and the
package is the same thing handed out as the installer, so nothing here is secret.

    GET /api/update/latest?access=0|1    -> the small JSON below (200) or 404 when
                                            nothing was ever published for that kind.
    GET /api/update/download?access=0|1  -> the package named in latest.json, as a file.

Slice 0104 -- TWO kinds of client, two packages. `access=1` is the package WITH the
Access components (the Migrare folder + KBot.Access.dll), `access=0` the one WITHOUT them. The
client learns which it is from POST /api/access/client-type (routes/access.py) and then
asks for its own kind. `access` missing = 1: every client built before this slice is
an Access client and never sends the parameter. Any other value is a 400, never a guess.

Everything is read from ONE folder, UPDATE_DIR. `push-update.ps1` on the build PC
fills it over SFTP: the packages and, last, `latest.json`. latest.json holds one block
per kind (a kind that was never published is simply absent):

    {
      "access":     { <block> },
      "non_access": { <block> }
    }

    <block> = {
      "version":       "1.0.31.0",        # KBot.App FileVersion inside the package
      "minimum":       "1.0.30.0",        # clients below this MUST update
      "file":          "KBot_1.0.31.0.zip",
      "size":          40445309,
      "sha256":        "...",             # hex, lowercase
      "published_utc": "2026-09-18T10:00:00Z",
      "notes":         "free text shown to the operator"
    }

Playwright driver package (the .playwright folder: node.exe + cli, ~100 MB, unchanged for months).
It is NOT inside the app packages: latest.json may carry ONE shared top-level `driver` block
(same for both kinds), written by push-update.ps1 only when the Playwright version changes:

    "driver": { "version": "1.49.0", "file": "KBot_Driver_1.49.0.zip", "size": 1, "sha256": "..." }

GET /api/update/latest adds it as `driver` to the answer; GET /api/update/driver downloads it.
A client downloads it only when the version it has installed differs.

Slice TRAY-02 -- the signer edition (KBot.Tray as a program of its own, for the people who only
sign) has its OWN channel, apart from latest.json: a push of the tray must never touch the
K-BOT blocks, and push-update.ps1 rewrites latest.json whole. push-tray.ps1 uploads the package
and, last, `tray.json` (same folder):

    { "version": "1.0.1.0", "file": "KBot_Tray_1.0.1.0.zip", "size": 1, "sha256": "...",
      "published_utc": "2026-11-10T10:00:00Z", "notes": "free text" }

    GET /api/update/tray/latest -> that block (200) or 404 when the tray was never published.
    GET /api/update/tray        -> the package it names, as a file.

Slice FS01-01 -- FISCOLINO (the ANAF program, repo ANAF) has its OWN channel too, `fiscolino.json`
(same folder), written LAST by ANAF's push-fiscolino.ps1. Same block as latest.json plus `minimum`:

    { "version": "13.0.1.0", "minimum": "13.0.0.0", "file": "Fiscolino_13.0.1.0.zip", "size": 1,
      "sha256": "...", "published_utc": "2026-11-15T10:00:00Z", "notes": "free text (ASCII lines)" }

    GET /api/update/fiscolino/latest -> that block (200) or 404 when never published.
    GET /api/update/fiscolino        -> the package it names, as a file.

The FISCOLINO scheme package (the ANAF schemes, xdp forms and SAF-T data the program downloads
from Options > Scheme XSD) is a channel of its own as well, `fiscolino-scheme.json`, written LAST
by ANAF's push-scheme.ps1. No `minimum`: the program only compares size and sha256.

    { "version": "2026.11.18", "file": "fiscolino-scheme-2026.11.18.zip", "size": 1,
      "sha256": "...", "published_utc": "2026-11-18T10:00:00Z" }

    GET /api/update/fiscolino-scheme/latest -> that block (200) or 404 when never published.
    GET /api/update/fiscolino-scheme        -> the package it names, as a file.

The OLD shape -- the block itself at the top level, written before slice 0104 -- is still
read, as the "access" block: the first push after this slice rewrites the file in the
new shape and keeps that block under "access".

The push script uploads to `*.part` and renames into place, and writes latest.json
AFTER the package, so a reader never sees a version whose file is not there yet.
No caching: the file is small and re-read on every call, so a push is visible
without restarting the service.

UPDATE_DIR: `config.UPDATE_DIR` when the server's config.py defines it, otherwise
`<PYTHON root>/updates`. config.py is host-only (never pushed), which is why the
default lives here and not there.
"""
import json
import logging
import os
import re

from flask import Blueprint, jsonify, request, send_file

logger = logging.getLogger(__name__)

update_bp = Blueprint("update", __name__)

LATEST_NAME = "latest.json"
TRAY_NAME = "tray.json"
FISCOLINO_NAME = "fiscolino.json"
FISCOLINO_SCHEME_NAME = "fiscolino-scheme.json"

# The package name is written by the push script and read back here. Anything
# that is not a plain file name (a path, an empty string) is refused before it
# reaches the file system -- latest.json is ours, but a typo must not turn into
# an arbitrary-file read.
_SAFE_FILE_RE = re.compile(r"^[A-Za-z0-9._-]+$")

# The version strings are compared by the CLIENT (System.Version); the server only
# checks that they look like x.x.x.x so a broken latest.json is reported as such
# instead of surfacing as a parse error on the operator's screen.
_VERSION_RE = re.compile(r"^\d+(\.\d+){1,3}$")

_REQUIRED_KEYS = ("version", "minimum", "file", "size", "sha256")

# The two blocks of latest.json (slice 0104).
KIND_ACCESS = "access"
KIND_NON_ACCESS = "non_access"


def _default_update_dir():
    return os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "updates")


def get_update_dir():
    """The folder that holds latest.json and the packages (see module docstring)."""
    try:
        import config  # host-only; may be absent in offline tests
    except Exception:
        return _default_update_dir()
    configured = getattr(config, "UPDATE_DIR", None)
    if configured:
        return configured
    return _default_update_dir()


class LatestUnavailable(Exception):
    """latest.json is missing -> nothing has been published yet (404)."""


class LatestInvalid(Exception):
    """latest.json exists but cannot be trusted (500)."""


def parse_access_param(raw):
    """
    The `access` query parameter: "1" -> True, "0" -> False, absent -> True (a client from
    before slice 0104 never sends it and is an Access client). Anything else raises
    ValueError -- the route answers 400, it does not guess which package to hand out.
    """
    if raw is None or raw == "1":
        return True
    if raw == "0":
        return False
    raise ValueError(f"access={raw!r}")


def _validate_block(data, where, required=_REQUIRED_KEYS):
    """One block: the whole old file, or latest.json["access"] / latest.json["non_access"]."""
    if not isinstance(data, dict):
        raise LatestInvalid(f"{where}: not an object")
    for key in required:
        if key not in data:
            raise LatestInvalid(f"{where}: missing key '{key}'")
    for key in ("version", "minimum"):
        if key not in required:
            continue
        if not isinstance(data[key], str) or not _VERSION_RE.match(data[key]):
            raise LatestInvalid(f"{where}: '{key}' is not a version: {data[key]!r}")
    if not isinstance(data["file"], str) or not _SAFE_FILE_RE.match(data["file"]):
        raise LatestInvalid(f"{where}: 'file' is not a plain file name: {data['file']!r}")
    if not isinstance(data["size"], int) or data["size"] < 0:
        raise LatestInvalid(f"{where}: 'size' is not a non-negative integer")
    if not isinstance(data["sha256"], str) or len(data["sha256"]) != 64:
        raise LatestInvalid(f"{where}: 'sha256' is not a 64-char hex digest")
    return data


def _validate_driver(data):
    """The optional shared driver block; None when absent."""
    block = data.get("driver") if isinstance(data, dict) else None
    if block is None:
        return None
    where = f"{LATEST_NAME}[driver]"
    if not isinstance(block, dict):
        raise LatestInvalid(f"{where}: not an object")
    for key in ("version", "file", "size", "sha256"):
        if key not in block:
            raise LatestInvalid(f"{where}: missing key {key}")
    if not isinstance(block["version"], str) or not _VERSION_RE.match(block["version"]):
        raise LatestInvalid(f"{where}: version is not a version: {block[version]!r}")
    if not isinstance(block["file"], str) or not _SAFE_FILE_RE.match(block["file"]):
        raise LatestInvalid(f"{where}: file is not a plain file name: {block[file]!r}")
    if not isinstance(block["size"], int) or block["size"] < 0:
        raise LatestInvalid(f"{where}: size is not a non-negative integer")
    if not isinstance(block["sha256"], str) or len(block["sha256"]) != 64:
        raise LatestInvalid(f"{where}: sha256 is not a 64-char hex digest")
    return block


def read_driver(update_dir=None):
    """The shared driver block of latest.json, or None when none was published."""
    update_dir = update_dir or get_update_dir()
    path = os.path.join(update_dir, LATEST_NAME)
    if not os.path.isfile(path):
        return None
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (OSError, ValueError) as e:
        raise LatestInvalid(f"{LATEST_NAME}: {e}") from e
    return _validate_driver(data)


def read_tray(update_dir=None):
    """
    The block of tray.json (the signer edition, slice TRAY-02), or None when the tray was never
    published. Raises LatestInvalid when the file is unreadable or the block cannot be trusted.
    """
    update_dir = update_dir or get_update_dir()
    path = os.path.join(update_dir, TRAY_NAME)
    if not os.path.isfile(path):
        return None
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (OSError, ValueError) as e:
        raise LatestInvalid(f"{TRAY_NAME}: {e}") from e
    if not isinstance(data, dict):
        raise LatestInvalid(f"{TRAY_NAME}: not an object")
    where = TRAY_NAME
    for key in ("version", "file", "size", "sha256"):
        if key not in data:
            raise LatestInvalid(f"{where}: missing key '{key}'")
    if not isinstance(data["version"], str) or not _VERSION_RE.match(data["version"]):
        raise LatestInvalid(f"{where}: 'version' is not a version: {data['version']!r}")
    if not isinstance(data["file"], str) or not _SAFE_FILE_RE.match(data["file"]):
        raise LatestInvalid(f"{where}: 'file' is not a plain file name: {data['file']!r}")
    if not isinstance(data["size"], int) or data["size"] < 0:
        raise LatestInvalid(f"{where}: 'size' is not a non-negative integer")
    if not isinstance(data["sha256"], str) or len(data["sha256"]) != 64:
        raise LatestInvalid(f"{where}: 'sha256' is not a 64-char hex digest")
    return data


def read_fiscolino(update_dir=None):
    """
    The block of fiscolino.json (the FISCOLINO program, slice FS01-01), or None when it was
    never published. Raises LatestInvalid when the file is unreadable or the block cannot be trusted.
    """
    update_dir = update_dir or get_update_dir()
    path = os.path.join(update_dir, FISCOLINO_NAME)
    if not os.path.isfile(path):
        return None
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (OSError, ValueError) as e:
        raise LatestInvalid(f"{FISCOLINO_NAME}: {e}") from e
    return _validate_block(data, FISCOLINO_NAME)


def read_fiscolino_scheme(update_dir=None):
    """
    The block of fiscolino-scheme.json (the FISCOLINO scheme package), or None when it was never
    published. Same checks as the other blocks, but there is no `minimum`. Raises LatestInvalid
    when the file is unreadable or the block cannot be trusted.
    """
    update_dir = update_dir or get_update_dir()
    path = os.path.join(update_dir, FISCOLINO_SCHEME_NAME)
    if not os.path.isfile(path):
        return None
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (OSError, ValueError) as e:
        raise LatestInvalid(f"{FISCOLINO_SCHEME_NAME}: {e}") from e
    return _validate_block(data, FISCOLINO_SCHEME_NAME, required=("version", "file", "size", "sha256"))


def read_latest(update_dir=None, access=True):
    """
    Read and validate latest.json and return the block of the asked kind (access=True ->
    "access", False -> "non_access"), as written by the push script.
    Raises LatestUnavailable when the file is absent or has no block of that kind (nothing
    was published for it), LatestInvalid when it is unreadable, not JSON, or the block is
    missing a key or names a file that is not a plain name.
    """
    update_dir = update_dir or get_update_dir()
    path = os.path.join(update_dir, LATEST_NAME)
    if not os.path.isfile(path):
        raise LatestUnavailable(path)
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
    except (OSError, ValueError) as e:
        raise LatestInvalid(f"{LATEST_NAME}: {e}") from e
    if not isinstance(data, dict):
        raise LatestInvalid(f"{LATEST_NAME}: not an object")

    kind = KIND_ACCESS if access else KIND_NON_ACCESS
    if KIND_ACCESS in data or KIND_NON_ACCESS in data:
        # The shape of slice 0104: one block per kind.
        if kind not in data:
            raise LatestUnavailable(f"{path} [{kind}]")
        return _validate_block(data[kind], f"{LATEST_NAME}[{kind}]")

    # The old shape: the block IS the file, and it was always the Access package.
    if not access:
        raise LatestUnavailable(f"{path} [{kind}]")
    return _validate_block(data, LATEST_NAME)


def _error(message, reason, status):
    return jsonify({"error": message, "reason": reason}), status


@update_bp.route("/api/update/latest", methods=["GET"])
def latest():
    """The published version of the asked kind (?access=0|1), or 404 when nothing has been published for it."""
    try:
        access = parse_access_param(request.args.get("access"))
    except ValueError:
        return _error("Parametrul «access» trebuie să fie 0 sau 1.", "ACCESS_PARAM_INVALID", 400)
    try:
        data = read_latest(access=access)
    except LatestUnavailable:
        return _error("Nu există nicio actualizare publicată.", "NO_UPDATE_PUBLISHED", 404)
    except LatestInvalid as e:
        logger.error("latest.json invalid: %s", e)
        return _error("Descrierea actualizării de pe server este invalidă.", "LATEST_INVALID", 500)

    body = {
        "version": data["version"],
        "minimum": data["minimum"],
        "file": data["file"],
        "size": data["size"],
        "sha256": data["sha256"],
        "published_utc": data.get("published_utc"),
        "notes": data.get("notes") or "",
    }
    try:
        driver_block = read_driver()
    except LatestInvalid as e:
        logger.error("latest.json driver block invalid: %s", e)
        driver_block = None      # the app package is still good; no driver offered
    if driver_block:
        body["driver"] = {k: driver_block[k] for k in ("version", "file", "size", "sha256")}
    return jsonify(body), 200


@update_bp.route("/api/update/download", methods=["GET"])
def download():
    """The package latest.json names for the asked kind (?access=0|1), streamed as a file (Range supported)."""
    try:
        access = parse_access_param(request.args.get("access"))
    except ValueError:
        return _error("Parametrul «access» trebuie să fie 0 sau 1.", "ACCESS_PARAM_INVALID", 400)
    try:
        data = read_latest(access=access)
    except LatestUnavailable:
        return _error("Nu există nicio actualizare publicată.", "NO_UPDATE_PUBLISHED", 404)
    except LatestInvalid as e:
        logger.error("latest.json invalid: %s", e)
        return _error("Descrierea actualizării de pe server este invalidă.", "LATEST_INVALID", 500)

    path = os.path.join(get_update_dir(), data["file"])
    if not os.path.isfile(path):
        # latest.json points at a package that is not there: the push was cut in
        # half, or someone deleted the file. Say so, do not send an empty body.
        logger.error("update package listed in latest.json is missing on disk: %s", path)
        return _error("Pachetul de actualizare lipsește de pe server.", "PACKAGE_MISSING", 500)

    try:
        response = send_file(
            path,
            mimetype="application/zip",            as_attachment=True,
            download_name=data["file"],
            conditional=True,
        )
        response.headers["X-KBot-Version"] = data["version"]
        response.headers["X-KBot-Sha256"] = data["sha256"]
        logger.info("update package sent: %s (v%s)", data["file"], data["version"])
        return response
    except Exception as e:
        logger.error("update package could not be sent: %s", e, exc_info=True)
        return _error("Pachetul de actualizare nu a putut fi trimis.", "PACKAGE_SEND_FAILED", 500)


@update_bp.route("/api/update/driver", methods=["GET"])
def driver():
    """The Playwright driver package latest.json names, streamed as a file (Range supported)."""
    try:
        block = read_driver()
    except LatestInvalid as e:
        logger.error("latest.json invalid: %s", e)
        return _error("Descrierea actualizării de pe server este invalidă.", "LATEST_INVALID", 500)
    if not block:
        return _error("Nu există niciun pachet driver publicat.", "NO_DRIVER_PUBLISHED", 404)
    path = os.path.join(get_update_dir(), block["file"])
    if not os.path.isfile(path):
        logger.error("driver package listed in latest.json is missing on disk: %s", path)
        return _error("Pachetul driver lipsește de pe server.", "PACKAGE_MISSING", 500)
    try:
        response = send_file(path, mimetype="application/zip", as_attachment=True,
                             download_name=block["file"], conditional=True)
        response.headers["X-KBot-Version"] = block["version"]
        response.headers["X-KBot-Sha256"] = block["sha256"]
        logger.info("driver package sent: %s (v%s)", block["file"], block["version"])
        return response
    except Exception as e:
        logger.error("driver package could not be sent: %s", e, exc_info=True)
        return _error("Pachetul driver nu a putut fi trimis.", "PACKAGE_SEND_FAILED", 500)


@update_bp.route("/api/update/tray/latest", methods=["GET"])
def tray_latest():
    """The published signer edition (tray.json), or 404 when it was never published."""
    try:
        block = read_tray()
    except LatestInvalid as e:
        logger.error("tray.json invalid: %s", e)
        return _error("Descrierea actualizării tray de pe server este invalidă.", "TRAY_INVALID", 500)
    if not block:
        return _error("Nu există nicio versiune tray publicată.", "NO_TRAY_PUBLISHED", 404)
    return jsonify({
        "version": block["version"],
        "file": block["file"],
        "size": block["size"],
        "sha256": block["sha256"],
        "published_utc": block.get("published_utc"),
        "notes": block.get("notes") or "",
    }), 200


@update_bp.route("/api/update/tray", methods=["GET"])
def tray_download():
    """The signer edition package tray.json names, streamed as a file (Range supported)."""
    try:
        block = read_tray()
    except LatestInvalid as e:
        logger.error("tray.json invalid: %s", e)
        return _error("Descrierea actualizării tray de pe server este invalidă.", "TRAY_INVALID", 500)
    if not block:
        return _error("Nu există nicio versiune tray publicată.", "NO_TRAY_PUBLISHED", 404)
    path = os.path.join(get_update_dir(), block["file"])
    if not os.path.isfile(path):
        logger.error("tray package listed in tray.json is missing on disk: %s", path)
        return _error("Pachetul tray lipsește de pe server.", "PACKAGE_MISSING", 500)
    try:
        response = send_file(path, mimetype="application/zip", as_attachment=True,
                             download_name=block["file"], conditional=True)
        response.headers["X-KBot-Version"] = block["version"]
        response.headers["X-KBot-Sha256"] = block["sha256"]
        logger.info("tray package sent: %s (v%s)", block["file"], block["version"])
        return response
    except Exception as e:
        logger.error("tray package could not be sent: %s", e, exc_info=True)
        return _error("Pachetul tray nu a putut fi trimis.", "PACKAGE_SEND_FAILED", 500)


@update_bp.route("/api/update/fiscolino/latest", methods=["GET"])
def fiscolino_latest():
    """The published FISCOLINO version (fiscolino.json), or 404 when it was never published."""
    try:
        block = read_fiscolino()
    except LatestInvalid as e:
        logger.error("fiscolino.json invalid: %s", e)
        return _error("Descrierea actualizarii Fiscolino de pe server este invalida.", "FISCOLINO_INVALID", 500)
    if not block:
        return _error("Nu exista nicio versiune Fiscolino publicata.", "NO_FISCOLINO_PUBLISHED", 404)
    return jsonify({
        "version": block["version"],
        "minimum": block["minimum"],
        "file": block["file"],
        "size": block["size"],
        "sha256": block["sha256"],
        "published_utc": block.get("published_utc"),
        "notes": block.get("notes") or "",
    }), 200


@update_bp.route("/api/update/fiscolino", methods=["GET"])
def fiscolino_download():
    """The FISCOLINO package fiscolino.json names, streamed as a file (Range supported)."""
    try:
        block = read_fiscolino()
    except LatestInvalid as e:
        logger.error("fiscolino.json invalid: %s", e)
        return _error("Descrierea actualizarii Fiscolino de pe server este invalida.", "FISCOLINO_INVALID", 500)
    if not block:
        return _error("Nu exista nicio versiune Fiscolino publicata.", "NO_FISCOLINO_PUBLISHED", 404)
    path = os.path.join(get_update_dir(), block["file"])
    if not os.path.isfile(path):
        logger.error("fiscolino package listed in fiscolino.json is missing on disk: %s", path)
        return _error("Pachetul Fiscolino lipseste de pe server.", "PACKAGE_MISSING", 500)
    try:
        response = send_file(path, mimetype="application/zip", as_attachment=True,
                             download_name=block["file"], conditional=True)
        response.headers["X-KBot-Version"] = block["version"]
        response.headers["X-KBot-Sha256"] = block["sha256"]
        logger.info("fiscolino package sent: %s (v%s)", block["file"], block["version"])
        return response
    except Exception as e:
        logger.error("fiscolino package could not be sent: %s", e, exc_info=True)
        return _error("Pachetul Fiscolino nu a putut fi trimis.", "PACKAGE_SEND_FAILED", 500)


@update_bp.route("/api/update/fiscolino-scheme/latest", methods=["GET"])
def fiscolino_scheme_latest():
    """The published FISCOLINO scheme package (fiscolino-scheme.json), or 404 when it was never published."""
    try:
        block = read_fiscolino_scheme()
    except LatestInvalid as e:
        logger.error("fiscolino-scheme.json invalid: %s", e)
        return _error("Descrierea pachetului de scheme de pe server este invalida.", "FISCOLINO_SCHEME_INVALID", 500)
    if not block:
        return _error("Nu exista niciun pachet de scheme publicat.", "NO_FISCOLINO_SCHEME_PUBLISHED", 404)
    return jsonify({
        "version": block["version"],
        "file": block["file"],
        "size": block["size"],
        "sha256": block["sha256"],
        "published_utc": block.get("published_utc"),
    }), 200


@update_bp.route("/api/update/fiscolino-scheme", methods=["GET"])
def fiscolino_scheme_download():
    """The scheme package fiscolino-scheme.json names, streamed as a file (Range supported)."""
    try:
        block = read_fiscolino_scheme()
    except LatestInvalid as e:
        logger.error("fiscolino-scheme.json invalid: %s", e)
        return _error("Descrierea pachetului de scheme de pe server este invalida.", "FISCOLINO_SCHEME_INVALID", 500)
    if not block:
        return _error("Nu exista niciun pachet de scheme publicat.", "NO_FISCOLINO_SCHEME_PUBLISHED", 404)
    path = os.path.join(get_update_dir(), block["file"])
    if not os.path.isfile(path):
        logger.error("scheme package listed in fiscolino-scheme.json is missing on disk: %s", path)
        return _error("Pachetul de scheme lipseste de pe server.", "PACKAGE_MISSING", 500)
    try:
        response = send_file(path, mimetype="application/zip", as_attachment=True,
                             download_name=block["file"], conditional=True)
        response.headers["X-KBot-Version"] = block["version"]
        response.headers["X-KBot-Sha256"] = block["sha256"]
        logger.info("fiscolino scheme package sent: %s (v%s)", block["file"], block["version"])
        return response
    except Exception as e:
        logger.error("fiscolino scheme package could not be sent: %s", e, exc_info=True)
        return _error("Pachetul de scheme nu a putut fi trimis.", "PACKAGE_SEND_FAILED", 500)


# The first install of a NEW user (no Access): the Inno Setup .exe, not the update zip.
# It is uploaded by hand next to latest.json under its build name and the newest one wins,
# so a new build needs no config change.
_SETUP_RE = re.compile(r"^[A-Za-z0-9._-]+_noaccess\.exe$")


def newest_setup_name(update_dir=None):
    """
    File name of the newest *_noaccess.exe in the update folder, or None. The version
    and the rest of the name do not matter: whichever such file is there is the one.
    Newest = last modified, so a rebuilt installer wins whatever it is called.
    """
    update_dir = update_dir or get_update_dir()
    try:
        names = [n for n in os.listdir(update_dir) if _SETUP_RE.match(n)]
        names.sort(key=lambda n: os.path.getmtime(os.path.join(update_dir, n)))
    except OSError:
        return None
    return names[-1] if names else None


@update_bp.route("/api/update/setup", methods=["GET"])
@update_bp.route("/api/update/setup/<path:file_name>", methods=["GET"])
def setup(file_name=None):
    """
    The installer .exe for a first install (clients without Access), streamed as a file.
    With a name in the address that exact file is sent (it must be a *_noaccess.exe in the
    update folder); without one, or when that file is gone, the newest one is.
    """
    update_dir = get_update_dir()
    name = file_name if file_name and _SETUP_RE.match(file_name) \
        and os.path.isfile(os.path.join(update_dir, file_name)) else newest_setup_name(update_dir)
    if not name:
        return _error("Programul de instalare nu este publicat.", "NO_SETUP_PUBLISHED", 404)
    try:
        return send_file(
            os.path.join(update_dir, name),
            mimetype="application/vnd.microsoft.portable-executable",
            as_attachment=True,
            download_name=name,
            conditional=True,
        )
    except Exception as e:
        logger.error("setup could not be sent: %s", e, exc_info=True)
        return _error("Programul de instalare nu a putut fi trimis.", "SETUP_SEND_FAILED", 500)
