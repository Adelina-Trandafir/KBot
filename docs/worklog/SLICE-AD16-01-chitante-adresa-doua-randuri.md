# SLICE-AD16-01 — Adresa pe două rânduri în chitanțe

30.10.2026. Cerință: ADRESA să ocupe două rânduri și să încapă un text mai lung.

## Ce s-a schimbat și de ce

În generatorul PDF al chitanței, adresa plătitorului are acum un bloc de două rânduri (30 pt), pe întreaga lățime interioară a exemplarului. Textul începe cu mărimea existentă de 10 pt și este micșorat în pași de 0,25 pt numai dacă paragraful depășește cele două rânduri. Adresa salvată se trimite integral paragrafului, fără trunchiere. Spațiul față de suma documentului se păstrează separat.

Blocul se generează identic pentru exemplarul plătitorului și exemplarul unității. Atât Listare, cât și Descarcă PDF folosesc ruta PDF, conform `outputReceipt` din `app.js`. Endpoint-ul HTML alternativ și celelalte câmpuri nu sunt modificate.

Limita existentă a adresei este 255 de caractere, conform contractului local și exportului de schemă `MariaDB_Schema/AVACONT_SURSA.sql`, tabela AD_Platitori_sub. Nu s-a modificat limita, schema sau adresa salvată. Pentru adrese extrem de dense, încadrarea pe două rânduri poate necesita text mai mic; operatorul verifică lizibilitatea.

## Fișiere

De publicat prin AvacontPush:

- `PYTHON/routes/adechit/receipt_output.py`
- `PYTHON/static/help/adechit.json`

Documentație: `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` și acest worklog.

## Probe și limite

**SCRIS LOCAL**, netestat și nepublicat. Conform AGENTS.md nu s-au rulat teste, compilări, generări PDF, randări sau verificări vizuale. S-a făcut numai analiza machetei și a fluxului de listare. Nu s-a creat un artefact PDF sau o captură. Nu s-au executat SQL, commit, publicare sau restart.

Operatorul probează adrese scurte/lungi până la limita existentă, cu diacritice, ambele exemplare, poziția câmpului Suma și lizibilitatea în previzualizare, la imprimare și descărcare. Ajutorul textual este actualizat cu AD16-01. Fără DDL sau seed necesar.
