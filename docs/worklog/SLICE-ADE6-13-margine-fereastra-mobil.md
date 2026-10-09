# SLICE-ADE6-13 — margine exterioară pe mobil

09.10.2026. SCRIS LOCAL.

Clarificarea utilizatorului cere spațiu între fereastră și marginile ecranului,
păstrând paddingul interior deja implementat. Dialogul Plătitori mobil are acum
margin:10px și lățime/înălțime egală cu viewportul minus 20px. Se folosește dvh
pentru înălțimea disponibilă, cu fallback vh. Paddingul interior de 8px se păstrează.

Fișiere: PYTHON/static/css/adechit.css, ajutorul adechit.html și statusul ADE.
Nu am rulat teste sau verificări vizuale, conform preferinței utilizatorului.
Modificare locală, fără backend/SQL, commit sau publicare. Fișierele statice se
preiau după reîncărcare; utilizatorul publică prin AvacontPush. Următoarea subfelie:
ADE6-14.
