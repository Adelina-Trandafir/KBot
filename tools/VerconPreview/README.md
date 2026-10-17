# VERCON — server local de probă

Server separat, numai pe `127.0.0.1:5053`, cu date inventate în memorie.
Nu importă `main.py`, nu înregistrează rutele SQL și nu deschide conexiuni MariaDB.
Nu cere credențiale sau e-mailuri reale. Nu se publică pe server.

```powershell
.\tools\VerconPreview\Start-VerconPreview.ps1
# sau, fără fereastră suplimentară:
.\tools\VerconPreview\Start-VerconPreview.ps1 -Background
.\tools\VerconPreview\Stop-VerconPreview.ps1
```

Deschide [pagina de pornire](http://localhost:5053/) sau
[VERCON](http://localhost:5053/vercon). Portul se poate schimba prin `-Port 5054`.

Preview-ul folosește HTML/CSS/JS reale, formularul Lista de alimente și
`report_service.build`, `report_food` și renderer-ele PDF/Excel reale.
Numai sursa datelor este demonstrativă (`DemoSource`, injectată explicit).
Snapshot-urile rapoartelor și salvările sunt în memorie, izolate de `instance/`.
Nicio configurare de machetă nu este salvată sau suprascrisă.

- IEȘIRI → Lista Alimente: consultare, adăugare, modificare și anulare demonstrativă.
- Rapoarte: ListaAlimente2015, selecție document/perioadă, PDF și Excel.
- [PDF direct, lista 130](http://localhost:5053/preview/report/130.pdf).
- [Excel direct, lista 130](http://localhost:5053/preview/report/130.xlsx).

Deschide luna octombrie în arborele listelor, apoi lista dorită.
Pentru Rapoarte, bifează un singur document și apasă Listare.
După HTML/CSS/JS: refresh. După Python: oprire/pornire și regenerare raport.
Repornirea reface datele demo. Nu există proxy către API-ul real.

Limitări: celelalte module Vercon nu au încă date demo. Salvarea în memorie
este pentru interfață; nu reproduce validarea stocurilor, tranzacțiile,
drepturile sau blocarea concurentă a bazei reale. Acestea cer un mediu MariaDB
de test pregătit separat de utilizator, conform regulilor proiectului.

Mediul necesar este `PYTHON/.venv` (Python 3.12), cu Flask și mysql-connector-python
pentru importurile aplicației, plus pachetele din `PYTHON/requirements-reporting.txt`.
Launcher-ul nu instalează și nu modifică automat dependențe.
