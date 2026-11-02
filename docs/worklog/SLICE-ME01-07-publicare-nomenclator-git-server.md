# SLICE-ME01-07 — Nomenclatorul publicării și salvare Git pe server

02.11.2026. Implementat în surse. Fără build, teste, conexiuni sau publicare; verificările nu au fost cerute de operator.

## Ce s-a schimbat și de ce

KBOTADMIN pregătește fișierele într-o zonă temporară și salvează în Git pe server versiunile existente înainte de suprascriere, alegere confirmată de operator. Registrul local fișier/slice/motiv/SHA-256 este obligatoriu; lipsa lui sau conținutul schimbat după înregistrare refuză întregul lot. Înregistrările lotului sunt eliminate numai după confirmarea încărcării și commitului. Înregistrările noi ale altor agenți sunt păstrate. Titlul grupează subslice-urile prin minim/maxim, inclusiv neconsecutive; motivele sunt linii cu `- ` separate prin CRLF. Nu se execută Git local și nu se face push Git.

Pasul de închidere este marcat IMPORTANT numai în instrucțiunile Codex (`AGENTS.md`) și folosește un script comun, disponibil oricărui agent, cu lista explicită a propriilor fișiere. Conform precizării operatorului, nu rămân modificări în `CLAUDE.md` sau în instrucțiunile altor agenți. KBOTADMIN blochează publicarea dacă un agent omite înregistrarea. Niciun mecanism extern de interceptare a ciclului de viață al agenților nu este presupus.

## Fișiere atinse

- `KBotAdmin/Server/DeploymentRegistry.vb`, `DeploymentPublisher.vb`, `PublishServer.py`: citire/validare registru, pregătirea conținutului, mesaj, transfer temporar și salvare/publicare pe server, curățarea înregistrărilor confirmate.
- `KBotAdmin/Server/PushService.vb`: integrarea nomenclatorului la scanare și înlocuirea încărcării directe.
- `KBotAdmin/Views/PushFilesView.vb`, `.Designer.vb`: slice/motiv în grilă, previzualizare commit, mărimi și date sortate după valorile reale.
- `KBotAdmin/Scripts/ScriptCatalog.vb`: blocarea ocolirii prin `Push-Changed` din KBOTADMIN; scriptul extern rămâne neatins.
- `KBotAdmin/KBotAdmin.vbproj`: helper inclus în executabil; FileVersion `1.2.0.1`.
- `tools/Register-DeploymentTask.ps1`, `docs/deployment/README.md`, `docs/deployment/pending/`: înregistrări imuabile, procedură pentru agenți și operator.
- `AGENTS.md`: regula IMPORTANT pentru înregistrare după ultima editare, fără Git local; instrucțiunile celorlalți agenți nu sunt modificate.
- `KBotAdmin/README.md`, indexul și starea ME01: instrucțiuni și starea reală.

## Rezultatele testelor

Nerulate, conform instrucțiunii explicite a utilizatorului. Nu s-au executat compilări, verificări automate, Git local, comenzi SSH, SQL sau publicare.

## Limite și ce rămâne neconfirmat

Nu este confirmată existența/configurarea depozitului Git pe server. Git și identitatea lui trebuie pregătite de operator; aplicația nu face inițializarea automat. Componenta serverului este inclusă în executabil și nu necesită bootstrap în PYTHON.

Înlocuirea este atomică pe fișier; la eroare obișnuită se încearcă restaurarea, dar o oprire brutală poate lăsa lot parțial. Versiunile anterioare rămân în Git și directorul temporar rămâne pentru recuperare. Dacă răspunsul SSH se pierde după succes, registrul rămâne și reluarea poate produce încă un commit. O eroare de curățare locală este raportată separat ca publicare reușită cu nomenclator necurățat. Nu se modifică alte căi externe de publicare sau livrarea pachetelor K-BOT. Nu se șterg fișiere de pe server.

Ajutorul KBOTADMIN este documentat în README și `docs/deployment/README.md`; programul privat nu are motorul de ajutor al clientului K-BOT (conform registrului ME01).
