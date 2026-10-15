# VERCON pe localhost, cu 000_DEMO prin SSH

Din rădăcina repository-ului:

```powershell
.\VERCON\tools\Start-Localhost.ps1
```

Deschide `http://localhost:5053/vercon`. Folosește subunitatea, gestiunea și anul existente în `000_DEMO`. NIR, storno, bonurile, lista de alimente, nomenclatoarele și rapoartele folosesc codul local actual și API-urile reale. Salvările și anulările se aplică în `000_DEMO`.

Și comanda folosită anterior pornește acum aceeași aplicație reală:

```powershell
.\PYTHON\.venv\Scripts\python.exe .\VERCON\tools\preview.py
```

`preview.py --fixtures` este exclusiv vechea demonstrație cu date fictive; nu se folosește pentru testarea modificărilor pe `000_DEMO`. `Start-Localhost.ps1` confirmă identitatea serverului și baza prin endpointul local de stare înainte să raporteze pornirea reușită. Sonda folosește `127.0.0.1`, fără proxy. Pagina locală primește un marcaj separat pentru autentificarea SSH, astfel încât nu este redirecționată la portal și folosește formularele reale.

Launcherul citește setările SSH din `_Admin/admin_settings.json`, apoi din copia Debug KBotAdmin, sau folosește valorile implicite KBotAdmin. Folosește OpenSSH și cheia din `~/.ssh` / ssh-agent; cheia serverului trebuie să existe în `known_hosts`. Sesiunea deja deschisă în KBotAdmin nu poate fi preluată de alt proces, dar se folosește aceeași configurație SSH.

Conexiunea MariaDB este tunelată prin SSH. Credencialele sunt citite din configurația serverului în memoria procesului, fără afișare și fără salvare. Nu se încarcă fișiere pe server, nu se repornește serviciul public, nu se creează sau modifică schema. Numai baza `000_DEMO` poate fi deschisă de launcher; exportul Excel poate citi lățimile existente din `AVACONT_COMUN`.

Accesul local se bazează pe conexiunea SSH a administratorului și oferă toate operațiile VERCON. Actorul din audit este `localhost-ssh@local.test`; poate fi schimbat cu `-Actor nume@exemplu.ro`. Nu reproduce autentificarea și drepturile unui operator din portal. Blocarea gestiunii folosește mecanismul SQL real.

Machetele și sursele personalizate ale rapoartelor locale se păstrează separat în `VERCON/tools/.local/report-config/000_DEMO`. Inițial sunt folosite machetele implicite din codul local, ca să se poată testa modificările noi.

Fișierele HTML, CSS și JS se recitesc la reîncărcarea paginii. Pentru modificări Python, repornește serverul:

```powershell
.\VERCON\tools\Restart-Localhost.ps1
```

`Restart-Localhost.ps1` oprește serverul care ține portul (pornit cu `Start-Localhost.ps1` sau de mână cu `preview.py`) și îl pornește din nou în fundal. Nu oprește niciodată un program străin care ocupă portul. Separat: `Stop-Localhost.ps1` oprește, `Start-Localhost.ps1` pornește. Eroarea «Portul localhost este deja ocupat» înseamnă că serverul rulează deja: folosește Restart, nu pornește încă unul.

Jurnalele sunt în `VERCON/tools/.local/localhost-5053.out.log` și `.err.log`. Pentru rulare în terminal și erori vizibile imediat: `Start-Localhost.ps1 -Foreground` (Ctrl+C oprește serverul și tunelul).

Dependențele Flask/MariaDB sunt cele din `PYTHON/.venv`; PDF și Excel necesită `PYTHON/requirements-reporting.txt`. Pentru instalarea lor, dacă lipsesc:

```powershell
.\PYTHON\.venv\Scripts\python.exe -m pip install -r .\PYTHON\requirements-reporting.txt
```

Pentru testarea manuală a lucrului curent: Recepție nouă (lățimi, copiere, duplicat, TVA și calcule), salvare/modificare/anulare, arborele documentelor, bon consum, storno, lista de alimente, rapoarte cu selecții multiple, previzualizare PDF, descărcare PDF/Excel și tipărire. Mecanismul de autentificare din portal și migrarea/sincronizarea schemelor nu fac parte din acest launcher.
