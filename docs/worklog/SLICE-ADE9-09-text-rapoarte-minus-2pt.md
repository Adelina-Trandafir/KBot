# SLICE-ADE9-09 — textul din Rapoarte redus cu 2pt

19.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

În panoul ADECHIT Rapoarte, titlul, etichetele bifelor, etichetele datelor și
valorile din câmpurile calendarului folosesc dimensiunea anterioară minus
2pt. Reducerea este exprimată în puncte, nu în pixeli. Textul îngroșat
„Trimite Mail?” moștenește aceeași reducere ca eticheta sa.

Textul comenzilor de raport, butonul de restrângere și butonul calendarului
își păstrează dimensiunile. Pentru calendar, dimensiunea butonului este
exprimată explicit față de tokenul original, evitând micșorarea prin
moștenirea noii dimensiuni a etichetei. Căsuțele de bifare și geometria
butoanelor nu sunt modificate.

De publicat prin AvacontPush:

- `PYTHON/static/css/adechit-reports.css`;
- `PYTHON/static/help/adechit.json`.

Documentație: `ADECHIT/UTILIZARE_WEB.md`, statusul principal și starea ADE0–ADE9.
Aspectul în teme luminoasă/întunecată, panoul extins/restrâns și butoanele
calendarului: TESTAT, RULAT, PUBLICAT. Următoarea subfelie de rapoarte: **ADE9-10**.
