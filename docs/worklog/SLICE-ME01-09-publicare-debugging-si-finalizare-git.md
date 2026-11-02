# SLICE-ME01-09 — Publicare debugging și finalizare numai Git

02.11.2026. Implementat în surse; fără build, teste, comenzi SSH, SQL sau publicare. Lucrarea nu este înregistrată pentru Git înainte de confirmarea explicită a Adelinei.

## Ce s-a schimbat și de ce

Publicarea permite fișiere fără informații Git: sunt trimise fără Git. Bifa «Publicare debugging (fără Git)» omite Git pentru întregul lot, chiar dacă există informații Git, păstrându-le. Numai fișierele cu informații Git valide participă la commitul normal, care rămâne înainte de suprascriere, conform alegerii inițiale a operatorului. Un fișier editat după înregistrare cere o nouă confirmare sau alegerea explicită a modului debugging.

Serverul reține persistent fiecare cale publicată fără Git și SHA-256 versiunii instalate, în directorul privat `RemoteRoot/.kbotadmin-deployment`. Scanarea citește această evidență și arată starea DEBUGGING chiar dacă timpii corespund sau sursa locală nu mai există. Lipsa depozitului/identității Git nu împiedică publicarea debugging; pe această ramură nu se execută nicio comandă Git.

«Finalizează debugging» preia numai fișierele debugging bifate. Un dialog cere slice și motiv pentru grupul afișat, iar «Confirm testul și fac Git» aprobă explicit versiunile de pe server. Metadatele se înregistrează după acest pas. Serverul refuză orice versiune care diferă de amprenta publicării debugging și face numai commitul celor selectate, fără reîncărcarea fișierelor aplicației, fără schimbarea timpilor lor și fără repornirea serviciului. Nu se adaugă spații sau comentarii pentru a forța detectarea.

După succes se curăță numai informațiile Git incluse și intrările debugging selectate. Publicarea debugging nu curăță nomenclatorul local. Înregistrările altor amprente rămân la finalizarea debugging. Scripturile externe și AvacontPush nu sunt modificate. Instrucțiunile noi sunt numai în `AGENTS.md`; `CLAUDE.md` și `CODE_WORKFLOW.md` nu sunt modificate.

## Fișiere atinse

- `KBotAdmin/Server/DeploymentRegistry.vb`: planuri fără Git/mixte, confirmarea metadatelor pentru amprentele debugging, mesaj gol când nu se cere Git.
- `KBotAdmin/Server/DeploymentPublisher.vb`: manifest cu mod și selecție per fișier, listare debugging și finalizare fără transferul conținutului aplicației, curățare după confirmarea succesului.
- `KBotAdmin/Server/PublishServer.py`: lacăt comun, evidență debugging atomică, ramură fără Git, commit numai pentru fișierele marcate, finalizare cu amprente validate și fără înlocuire.
- `KBotAdmin/Server/PushService.vb`: citirea evidenței debugging și rânduri pentru versiunile serverului care nu mai au sursă locală.
- `KBotAdmin/Views/PushFilesView.vb` și `.Designer.vb`: bifa debugging, butonul de finalizare, etichete și numărători separate pentru debugging.
- `KBotAdmin/DebugFinalizeForm.vb` și `.Designer.vb`: lista sortată în română, slice/motiv și confirmarea explicită.
- `KBotAdmin/Scripts/ScriptCatalog.vb`: explicația actualizată pentru blocarea lansatorului extern fără evidență debugging.
- `KBotAdmin/KBotAdmin.vbproj`: FileVersion `1.2.0.2`.
- `AGENTS.md`, `KBotAdmin/README.md`, `docs/deployment/README.md`, indexul și starea ME01: reguli, procedură și starea reală.

## Rezultatele testelor

Nu au fost cerute și nu au fost rulate compilări, teste sau alte verificări automate. Nu s-au executat comenzi pe server, operații Git locale, SQL sau publicare. Codul a fost analizat ca design.

## Limite și ce rămâne neconfirmat

Interfața nu a fost văzută pe ecran, transportul și fluxurile nu au fost încercate. Git necesită depozit și identitate configurate manual. Actualizarea fișierelor rămâne atomică individual; o eroare obișnuită încearcă restaurarea, dar o oprire brutală poate lăsa un lot parțial sau evidența neactualizată. Debugging înseamnă explicit că versiunile anterioare nu sunt garantate în Git. Dacă răspunsul SSH se pierde după commit/finalizare, serverul poate fi deja finalizat în timp ce metadatele locale rămân; starea trebuie clarificată înainte de curățare. Fișierele publicate pe alte căi nu sunt adoptate automat. Ajutorul programului privat este actualizat în README și procedura de publicare.
