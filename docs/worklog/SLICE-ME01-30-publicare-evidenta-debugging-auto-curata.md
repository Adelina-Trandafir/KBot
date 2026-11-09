# SLICE-ME01-30 - Publicare: evidenta debugging a fisierelor disparute se curata singura la scanare (09.11.2026)

Cererea operatorului: in KBotAdmin, pagina Publicare > Fisiere, randurile (stare «Asteapta Git», coloana Debugging) pentru fisiere care nu mai exista nici local, nici pe server trebuie sa se curete singure din fisier.

## What changed and why
Evidenta debugging sta pe server in `<RemoteRoot>/.kbotadmin-deployment/debug.json`. Scanarea adauga la lista orice intrare din ea al carei fisier nu e gasit local, ca sa poata fi finalizata. Daca fisierul lipsea si de pe server, intrarea ramanea pe veci: nu se mai putea finaliza (serverul cere fisierul cu amprenta ei) si nici scoate din lista.

- **Server** (`KBotAdmin/Server/PublishServer.py`, incorporat in exe si urcat la fiecare rulare, deci nimic de pus separat pe server): actiune noua `forget_debug`. Pentru fiecare cale primita sterge intrarea din `debug.json` DOAR daca fisierul tinta nu mai exista pe server (verificat din nou acolo); un fisier existent nu e atins. Fara director de evidenta intoarce o lista goala.
- **Client** (`DeploymentPublisher.ForgetMissing`, `Execute` primeste calea/caile in manifest): trimite caile si intoarce ce a uitat serverul.
- **Scanare** (`PushService.Scan`): o intrare e «disparuta» cand serverul raporteaza `current_sha256` gol (fisier lipsa pe server) SI `File.Exists` local e fals. Acele intrari se sterg din evidenta si nu mai apar in lista; jurnalul scanarii spune cate. Daca stergerea esueaza, eroarea merge in `GlobalErrorLog` si in jurnalul scanarii, iar randurile raman vizibile ca inainte.
- KBotAdmin 1.2.0.18.

## Files touched
`KBotAdmin/Server/PublishServer.py`, `KBotAdmin/Server/DeploymentPublisher.vb`, `KBotAdmin/Server/PushService.vb`, `KBotAdmin/KBotAdmin.vbproj`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build KBotAdmin/KBotAdmin.vbproj`: 0 erori, 0 avertismente. Scanarea nu a fost rulata pe serverul real; `PublishServer.py` nu a fost rulat.

## Unverified / deferred
- Nerulat pe server: actiunea `forget_debug` si curatarea efectiva a evidentei nu sunt probate.
- Inregistrarile locale din `docs/deployment/pending` care ar mai pomeni un astfel de fisier nu sunt atinse.
- Ajutor: niciunul (KBotAdmin nu e pentru clienti).
- Aceeasi sesiune a mai schimbat asezarea paginii Fisiere (panou lateral mai ingust, butoanele sub tabele, coloane fixe cu scroll orizontal in tabelul de fisiere): ajustari de aspect, necontate in worklog.
