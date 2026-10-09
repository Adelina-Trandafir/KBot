# SLICE-ADE1-05 — inițializare blueprint

09.10.2026. SCRIS LOCAL.

## Schimbare

Tracebackul furnizat de utilizator arată NameError la primul decorator `@bp.get`.
În create_blueprint lipsea instanțierea variabilei locale bp. Am adăugat
`bp = Blueprint('adechit', __name__)` înaintea înregistrării rutelor. Fiecare apel
al fabricii creează propria instanță, inclusiv pentru preview cu adaptoare locale.

## Fișiere

PYTHON/routes/adechit/__init__.py, ADECHIT_STATUS.md și statusul detaliat ADE0–ADE9.

## Verificări și predare

Cauza identificată prin citirea sursei și tracebackul utilizatorului. Nu am pornit
aplicația și nu am rulat teste, conform instrucțiunii explicite a utilizatorului.
Se reia aceeași comandă preview.py. Fără modificări de date/SQL, fără commit sau
publicare. Fișierul runtime se publică prin AvacontPush de către utilizator.
Următoarea subfelie liberă: ADE1-06.
