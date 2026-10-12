# SLICE-VC01-03 — sesiune, log, conexiuni

Stare: **IMPLEMENTAT_LOCAL**. Nimic rulat, testat sau previzualizat (regula: fara teste/randari decat la cerere); nimic validat pe server.

## Facut
- `routes/vercon/diagnostics.py`: id de cerere (`X-Request-Id`, ecou in raspuns), email mascat, linii `VR op= outcome= unit= user= rid=`.
- `routes/vercon/connection.py`: o singura cale de conexiune (`utils.database`), inchidere garantata, fara pool/sesiune/auth paralele, fara reincercare la scriere.
- Garda: unitate (`X-Vercon-Unit`) + drept `read` per ruta; refuzurile si erorile se logheaza fara parole sau date personale complete.

## Limite
Operatiile VR din roluri se definesc la VC04-01; pana atunci nimeni nu are acces in productie (inchis, nu deschis).
