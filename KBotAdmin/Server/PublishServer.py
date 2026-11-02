"""SLICE-ME01-07/09. Server-only publication, debugging and Git finalization."""
import fcntl
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import stat
import subprocess
import sys
import tempfile
from datetime import datetime, timezone


def digest(path):
    value = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(block)
    return value.hexdigest()


def git(root, *args, env=None, data=None, allowed=(0,)):
    result = subprocess.run(
        ["git", "--literal-pathspecs", "-c", "core.hooksPath=/dev/null", "-C", str(root), *args],
        input=data, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        env=env, check=False,
    )
    if result.returncode not in allowed:
        detail = result.stderr.decode("utf-8", errors="replace").strip()
        raise RuntimeError("Git pe server a eșuat: " + detail)
    return result


def target_path(root, relative):
    parts = PurePosixPath(relative).parts
    if (not parts or relative.startswith("/") or "\\" in relative or "\0" in relative
            or any(part in ("", ".", "..") or part.lower() in (".git", ".kbotadmin-deployment") for part in relative.split("/"))):
        raise RuntimeError("Cale de publicare invalidă: " + relative)
    current = root
    for part in parts:
        current = current / part
        if current.is_symlink():
            raise RuntimeError("Legătură simbolică neacceptată: " + relative)
    if current.exists() and not current.is_file():
        raise RuntimeError("Destinația nu este fișier: " + relative)
    return current


def publish(root, stage, manifest):
    if not root.is_dir() or root.is_symlink():
        raise RuntimeError("Rădăcina de publicare nu este un director real.")
    root = root.resolve(strict=True)
    action = manifest["action"]
    if action not in ("publish", "commit_debug", "list_debug"):
        raise RuntimeError("Acțiune de publicare necunoscută.")
    state_dir = root / ".kbotadmin-deployment"
    if state_dir.is_symlink():
        raise RuntimeError("Directorul evidenței debugging nu poate fi o legătură simbolică.")
    if action == "list_debug" and not state_dir.exists():
        return {"ok": True, "debug_files": []}
    state_dir.mkdir(mode=0o700, exist_ok=True)
    descriptor = os.open(state_dir / "publish.lock", os.O_RDWR | os.O_CREAT | os.O_NOFOLLOW, 0o600)
    # The same lock protects uploads without Git and Git-only finalization.
    with os.fdopen(descriptor, "a+b") as lock:
        fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        state_path = state_dir / "debug.json"
        if state_path.is_symlink():
            raise RuntimeError("Evidență debugging invalidă.")
        state = json.loads(state_path.read_text(encoding="utf-8")) if state_path.exists() else {}
        for relative, entry in state.items():
            target_path(root, relative)
            if not re.fullmatch(r"[a-f0-9]{64}", entry["sha256"]):
                raise RuntimeError("Amprentă invalidă în evidența debugging.")
        if action == "list_debug":
            return {"ok": True, "debug_files": [{"path": path, **entry} for path, entry in state.items()]}
        return publish_locked(root, stage, manifest, state_path, state)


def snapshot(root, stage, message, items, paths, before):
    repo = Path(git(root, "rev-parse", "--show-toplevel").stdout.decode().strip()).resolve()
    git_dir = Path(git(root, "rev-parse", "--absolute-git-dir").stdout.decode().strip())
    for marker in ("MERGE_HEAD", "CHERRY_PICK_HEAD", "REVERT_HEAD", "rebase-merge", "rebase-apply"):
        if (git_dir / marker).exists():
            raise RuntimeError("Publicare blocată: operație Git neterminată pe server.")
    index_name = Path(git(root, "rev-parse", "--git-path", "index").stdout.decode().strip())
    if not index_name.is_absolute():
        index_name = root / index_name
    index_lock = Path(str(index_name) + ".lock")
    descriptor = os.open(index_lock, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
    os.close(descriptor)
    try:
        return snapshot_locked(root, repo, stage, message, items, paths, before, index_name, index_lock)
    finally:
        index_lock.unlink(missing_ok=True)


def snapshot_locked(root, repo, stage, message, items, paths, before, index_name, index_lock):
    has_head = git(root, "rev-parse", "--verify", "HEAD", allowed=(0, 128)).returncode == 0
    staged = git(root, "diff", "--cached", "--quiet", "--exit-code", allowed=(0, 1))
    if staged.returncode != 0:
        raise RuntimeError("Publicare blocată: există fișiere deja pregătite pentru commit pe server.")
    title = message.split("\r\n", 1)[0]
    if not re.fullmatch(r"SLICE-[A-Z0-9]{4}-[0-9]{2}(?:/[0-9]{2})?(?:; SLICE-[A-Z0-9]{4}-[0-9]{2}(?:/[0-9]{2})?)*", title):
        raise RuntimeError("Titlu de commit invalid.")
    env = os.environ.copy()
    # No caller-provided Git repository, index, hooks or configuration overrides.
    for key in tuple(env):
        if key.startswith("GIT_"):
            del env[key]
    temporary_index = stage / "snapshot.index"
    env["GIT_INDEX_FILE"] = str(temporary_index)
    git(repo, "read-tree", *(["HEAD"] if has_head else ["--empty"]), env=env)
    for number, (target, source, existed, previous, mode) in enumerate(paths):
        if existed and items[number]["include_git"]:
            # Exact bytes of the previous server file, without clean filters or EOL conversion.
            blob = git(repo, "hash-object", "-w", "--no-filters", str(before / str(number)), env=env).stdout.decode().strip()
            relative = target.relative_to(repo).as_posix()
            git_mode = "100755" if mode & 0o111 else "100644"
            git(repo, "update-index", "--add", "--cacheinfo", git_mode, blob, relative, env=env)

    message_path = stage / "message.txt"
    message_path.write_bytes(message.encode("utf-8"))
    hooks = stage / "hooks"
    hooks.mkdir(mode=0o700)
    git(repo, "-c", "core.hooksPath=" + str(hooks), "commit", "--allow-empty", "--cleanup=verbatim", "-F", str(message_path), env=env)
    commit = git(root, "rev-parse", "HEAD").stdout.decode().strip()
    # Synchronize the ordinary index to the new HEAD without changing any working files.
    shutil.copyfile(temporary_index, index_lock)
    os.replace(index_lock, index_name)
    print("Git server: " + commit, flush=True)
    return commit


def save_debug(state_path, state):
    fd, name = tempfile.mkstemp(prefix="debug-", dir=state_path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as stream:
            json.dump(state, stream, ensure_ascii=False)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(name, state_path)
    finally:
        Path(name).unlink(missing_ok=True)


def publish_locked(root, stage, manifest, state_path, state):
    items = manifest["files"]
    if not items or len({item["path"] for item in items}) != len(items):
        raise RuntimeError("Lot de publicare gol sau cu fișiere duplicate.")
    if any(type(item["include_git"]) is not bool or not re.fullmatch(r"[a-f0-9]{64}", item["sha256"]) for item in items):
        raise RuntimeError("Manifest de publicare invalid.")
    git_only = manifest["action"] == "commit_debug"
    if git_only and not all(item["include_git"] for item in items):
        raise RuntimeError("Finalizarea debugging necesită informații Git pentru fiecare fișier.")
    before = stage / "before"
    before.mkdir(mode=0o700)
    paths = []
    for number, item in enumerate(items):
        target = target_path(root, item["path"])
        source = stage / (str(number) + ".data")
        if git_only:
            if (state.get(item["path"], {}).get("sha256") != item["sha256"]
                    or not target.is_file() or digest(target) != item["sha256"]):
                raise RuntimeError("Finalizare oprită: versiunea debugging de pe server s-a schimbat: " + item["path"])
        elif not source.is_file() or digest(source) != item["sha256"]:
            raise RuntimeError("Conținut temporar invalid: " + item["path"])
        existed = target.exists()
        previous = None
        mode = 0o644
        if existed:
            mode = stat.S_IMODE(target.stat().st_mode)
            backup = before / str(number)
            shutil.copy2(target, backup)
            previous = digest(backup)
            if digest(target) != previous or (git_only and previous != item["sha256"]):
                raise RuntimeError("Fișier schimbat în timpul pregătirii: " + item["path"])
        paths.append((target, source, existed, previous, mode))

    git_count = sum(item["include_git"] for item in items)
    commit = snapshot(root, stage, manifest["message"], items, paths, before) if git_count else None

    # Refuse replacement if another process changed a destination meanwhile.
    for item, (target, source, existed, previous, mode) in zip(items, paths):
        target_path(root, item["path"])
        if target.exists() != existed or (existed and digest(target) != previous):
            raise RuntimeError("Fișier schimbat în timpul operației; publicare oprită: " + item["path"])
    if git_only:
        for item in items:
            del state[item["path"]]
        save_debug(state_path, state)
        return {"ok": True, "commit": commit, "count": len(items), "git_count": git_count, "action": "commit_debug"}
    replaced = []
    try:
        for number, (item, (target, source, existed, previous, mode)) in enumerate(zip(items, paths)):
            target_path(root, item["path"])
            target.parent.mkdir(parents=True, exist_ok=True)
            # Same filesystem as the destination: os.replace is atomic per file.
            fd, temp_name = tempfile.mkstemp(prefix=".kbot-publish-", dir=target.parent)
            temp = Path(temp_name)
            os.close(fd)
            try:
                shutil.copyfile(source, temp)
                os.chmod(temp, mode)
                if existed:
                    original = target.stat()
                    os.chown(temp, original.st_uid, original.st_gid)
                if manifest["preserve_mtime"]:
                    os.utime(temp, (item["mtime"], item["mtime"]))
                os.replace(temp, target)
                replaced.append((number, target, existed))
            finally:
                temp.unlink(missing_ok=True)
        for item, (target, source, existed, previous, mode) in zip(items, paths):
            if digest(target) != item["sha256"]:
                raise RuntimeError("Conținutul publicat diferă: " + item["path"])
        for item in items:
            if item["include_git"]:
                state.pop(item["path"], None)
            else:
                state[item["path"]] = {"sha256": item["sha256"], "deployed_utc": datetime.now(timezone.utc).isoformat()}
        save_debug(state_path, state)
    except Exception as failure:
        errors = []
        for number, target, existed in reversed(replaced):
            try:
                if existed:
                    shutil.copy2(before / str(number), target)
                else:
                    target.unlink()
            except Exception as rollback_error:
                errors.append(str(target) + ": " + str(rollback_error))
        if errors:
            raise RuntimeError("Publicare eșuată; restaurare incompletă. Copii temporare: " + str(before) + "; " + "; ".join(errors)) from failure
        raise
    return {"ok": True, "commit": commit, "count": len(paths), "git_count": git_count, "action": "publish"}


if __name__ == "__main__":
    try:
        stage_path = Path(__file__).resolve().parent
        request = json.loads((stage_path / "manifest.json").read_text(encoding="utf-8"))
        result = publish(Path(sys.argv[1]), stage_path, request)
        print("KBOT_PUBLISH_RESULT=" + json.dumps(result, ensure_ascii=False), flush=True)
    except Exception as error:
        print("Publicarea a eșuat: " + str(error), file=sys.stderr, flush=True)
        sys.exit(1)
