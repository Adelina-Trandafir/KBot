# SLICE-ME01-18 — Fișiere debugging neschimbate, nebifate la scanare

03.11.2026. Implementat în surse, fără compilare, teste, conexiune SSH sau publicare efectuate de agent. Adelina a confirmat explicit «da. e ok. inchide si inregistreaza fisierele». Task închis prin înregistrarea celor 10 fișiere de mai jos în nomenclatorul local pentru Git pe server; înregistrarea nu reprezintă publicare sau commit executat pe server.

La scanarea din KBOTADMIN, fișierele deja încărcate fără Git nu trebuie trimise din nou numai fiindcă așteaptă mesajul de commit. Rămân vizibile pentru finalizarea debugging, dar se bifează automat numai dacă au conținut diferit ori lipsesc de pe server.

## Implementare

- Helperul încorporat returnează, pentru fiecare fișier din evidența debugging, și SHA-256 al conținutului actual de pe server. Nu schimbă evidența și nu execută Git pentru această comparație.
- Scanarea compară SHA-256 local cu amprenta actuală a serverului. Conținutul identic devine «DEBUGGING», indiferent de timpi; conținutul diferit devine «MODIFICAT», inclusiv când timpii și dimensiunile coincid. Dacă fișierul lipsește de pe server, devine «NOU».
- Numai «NOU» și «MODIFICAT» primesc bifa automată și intră în totalurile de trimis pe extensie. Înregistrările Git existente nu forțează retrimiterea unui fișier debugging identic.
- Fișierele debugging fără sursă locală ori excluse rămân vizibile, nebifate. Pentru finalizare se bifează manual versiunile testate. Bifarea manuală și «Bifează tot» permit retrimiterea explicită.
- Finalizarea Git folosește în continuare amprenta versiunii publicate și informațiile deja aprobate; noua amprentă de scanare nu aprobă și nu adoptă o versiune schimbată separat pe server.
- Comparația fișierelor care nu apar în evidența debugging rămâne în fluxul existent. Helperul este încorporat în KBOTADMIN; nu există un fișier Python de publicat separat în aplicația server.

## Fișiere proprii create/modificate

- `KBotAdmin/Server/PublishServer.py`
- `KBotAdmin/Server/DeploymentRegistry.vb`
- `KBotAdmin/Server/PushService.vb`
- `KBotAdmin/Views/PushFilesView.vb`
- `KBotAdmin/Views/PushFilesView.Designer.vb`
- `KBotAdmin/KBotAdmin.vbproj` — FileVersion `1.2.0.7`.
- `KBotAdmin/README.md`
- `docs/deployment/README.md`
- `docs/worklog/state/KBOT_STATUS_ME01.md`
- `docs/worklog/SLICE-ME01-18-debugging-neschimbat-nebifat.md`

Analiză de design prin citirea codului. Nu au fost rulate verificări automate sau manuale, conform instrucțiunilor proiectului. Schimbarea necesită reconstruirea KBOTADMIN înainte de probă. Amprentele descriu conținutul din momentul scanării; modificările ulterioare necesită o nouă scanare, iar finalizarea își păstrează verificările pe server.
