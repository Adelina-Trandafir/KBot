# SitePreview

A local preview of the PUBLIC site (presentation page, «Cere mai multe detalii», the web area for registered users)
with made-up data, so the site can be looked at, corrected and approved BEFORE anything is uploaded to the real server.
Development only: it is not part of the server and is never deployed.

```powershell
.\tools\SitePreview\Start-SitePreview.ps1            # this computer only:  http://localhost:5050/demo
.\tools\SitePreview\Start-SitePreview.ps1 -Lan       # also a phone on the same Wi-Fi (the address is printed)
.\tools\SitePreview\Start-SitePreview.ps1 -Port 5051 # another port
```

- `/demo` lists the pages and the demo login: any e-mail, password `demo`, code `123456`.
- It serves the REAL files (templates, `content.json`, CSS, JS, the grid, the PDF viewer). HTML / CSS / JS / `content.json`
  changes show on a plain reload; a change to a Python file needs a restart.
- Replaced on purpose: the database, the mail and the form's storage. Nothing is saved, nothing is mailed, the real
  users and units are not touched. The «Cere mai multe detalii» form accepts and drops the request.
- The signed documents shown in «Documente» are the sample PDFs of this repository when they are on this disk (two of them are
  not in git: `Surse/ETAPE ORDONANTARE/Etapa3_Semnata.pdf`, `Surse/CAB+ERRRRRRR/NOTA CAB 23.pdf`).
- `/inregistrare` shows the registration page for its look only; its buttons need the real server.
- `-Lan` makes the preview reachable by anyone on the local network (the data is made up, but it is still a open page): use it
  at home or in the office, not on a public Wi-Fi.
