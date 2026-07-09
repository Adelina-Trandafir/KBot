# gunicorn.conf.py — configuratia de rulare AVACONT (folosita de avacont.service).
#
# Deployment: se copiaza in /root/AVACONT/gunicorn.conf.py pe server, iar unit-ul
# systemd o refera cu:  ExecStart=.../gunicorn -c /root/AVACONT/gunicorn.conf.py main:app
# TOATE setarile de mai jos inlocuiesc fostele flag-uri din linia ExecStart. Daca
# lipseste vreuna, gunicorn revine TACUT la default-ul ei — de aceea le tinem pe
# toate aici, explicit.
#
# Starea aplicatiei (session_store.STORE, _upload_sessions) traieste in memoria
# procesului. Cu >1 worker s-ar fragmenta tacut (fiecare worker ar avea alt STORE),
# deci refuzam pornirea pana la o eventuala mutare pe Redis. (vezi session_store.py)

workers = 1
threads = 4
worker_class = "gthread"
bind = "127.0.0.1:5009"
timeout = 300

accesslog = "/var/log/avacont/access.log"
errorlog = "/var/log/avacont/error.log"
capture_output = True


def on_starting(server):
    # Ruleaza in procesul master, unde numarul de workeri e deja cunoscut.
    # Garda musca doar daca acest fisier e chiar folosit de ExecStart.
    if server.cfg.workers != 1:
        raise RuntimeError(
            f"AVACONT trebuie pornit cu un singur worker (workers={server.cfg.workers}). "
            "Starea de sesiune e in memorie; multi-worker cere intai migrarea pe Redis."
        )
