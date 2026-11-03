# SLICE-ME01-19 — Selecție Debugging separată de Trimite

03.11.2026. Implementat în surse, fără compilare, teste, SSH sau publicare. Înregistrarea acestei corecții așteaptă confirmarea explicită a Adelinei. Înregistrările deja create pentru ME01-18 sunt păstrate.

«Finalizează debugging» citea coloana «Trimite», deși coloana «Debugging» arăta deja fișierele publicate fără Git. Finalizarea citește acum exclusiv bifele «Debugging», păstrând condiția existenței unei amprente debugging pentru fiecare fișier.

Coloana «Debugging» este editabilă pentru fișierele din evidența serverului. La scanare, acestea rămân bifate în această coloană, ca înainte; operatorul le poate debifa individual înainte de finalizare. Pentru fișierele fără evidență debugging, bifa este inactivă. Coloana «Trimite» rămâne independentă: fișierele debugging identice cu serverul sunt nebifate pentru încărcare, conform ME01-18.

«Trimite bifatele», «Bifează tot» și «Debifează tot» continuă să folosească numai coloana «Trimite». Finalizarea păstrează informațiile Git existente, controlul amprentelor și operația numai Git pe server, fără reîncărcare sau repornire. Eticheta de lângă buton, tooltipurile și mesajul pentru selecție goală indică separat cele două coloane.

## Fișiere proprii create/modificate

- `KBotAdmin/Views/PushFilesView.vb`
- `KBotAdmin/Views/PushFilesView.Designer.vb`
- `KBotAdmin/KBotAdmin.vbproj` — FileVersion `1.2.0.8`.
- `KBotAdmin/README.md`
- `docs/deployment/README.md`
- `docs/worklog/state/KBOT_STATUS_ME01.md`
- `docs/worklog/SLICE-ME01-19-selectie-debug-separata-de-trimitere.md`

Analiză de design prin citirea codului, fără rularea aplicației sau a verificărilor, conform instrucțiunilor proiectului. Executabilul necesită reconstruire pentru a include corecția.
