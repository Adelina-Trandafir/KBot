# SLICE-ME01-38 - Publicare: fisierele sterse local se pot sterge si de pe server (18.11.2026)

Cererea operatorului: in KBotAdmin, Publicare > Fisiere, fisierele sterse local dar care exista pe server trebuie sincronizate prin eliminarea lor de pe server; altfel raman in lista.

## What changed and why
Un fisier publicat debugging (evidenta `debug.json` de pe server) si apoi sters local ramanea pe server si in lista, ca rand «DEBUGGING» fara fisier local. ME01-30 scotea din evidenta doar intrarile disparute si de pe server, deci acestea nu dispareau niciodata.

- **Server** (`KBotAdmin/Server/PublishServer.py`): actiune noua `delete_debug`. Sub acelasi lock ca restul, pentru fiecare cale primita care se afla in evidenta: sterge fisierul de pe server (daca mai exista) si intrarea din `debug.json`. O cale care NU e in evidenta e ignorata; `config.py` e refuzat (regula 00) inainte de orice stergere. Nu atinge Git.
- **Client** (`DeploymentPublisher.DeleteMissing`): trimite caile si intoarce ce a sters serverul.
- **Scanare** (`PushService.Scan`): stare noua `PushState.DeletedLocally` pentru intrarile din evidenta care lipsesc local (`File.Exists` fals) dar exista pe server (`current_sha256` nenul). Randul apare «ȘTERS LOCAL» (culoare eroare), bifat in coloana Trimite si nebifabil la Debugging. Nu mai intra in numaratoarea «debugging de finalizat»; sumarul scanarii spune cate sunt «șterse local».
- **Pagina** (`PushFilesView`): buton nou «Șterge de pe server» (in bara de debugging, in designer). Ia randurile «ȘTERS LOCAL» bifate, reverifica pe disc ca fisierul tot lipseste, cere confirmare cu lista (primele 15) «Nu se poate anula», trimite, apoi rescaneaza. «Trimite bifatele» si «Finalizeaza debugging» sar peste aceste randuri.
- **.pushignore are ultimul cuvant (cerinta operatorului, 18.11.2026):** `PushService.IsIgnored` (regula din `.pushignore`; daca nu are regula pentru cale, lista fixa de ignorare). Scanarea nu da starea «ȘTERS LOCAL» unui fisier ignorat (ramane «DEBUGGING» ca inainte), iar butonul reciteste `.pushignore` la apasare si scoate din lista de sters orice fisier ignorat (avertisment in jurnal). Doar o linie `!` explicita din `.pushignore` (fisier inclus) lasa fisierul sa poata fi sters.
- KBotAdmin 1.2.0.19.

## Files touched
`KBotAdmin/Server/PublishServer.py`, `KBotAdmin/Server/DeploymentPublisher.vb`, `KBotAdmin/Server/PushService.vb`, `KBotAdmin/Views/PushFilesView.vb`, `KBotAdmin/Views/PushFilesView.Designer.vb`, `KBotAdmin/KBotAdmin.vbproj`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build KBotAdmin/KBotAdmin.vbproj`: 0 erori, 0 avertismente. `PublishServer.py`: `py_compile` curat. Nimic rulat pe serverul real, nimic vazut pe ecran.

## Unverified / deferred
- Nerulat pe server: actiunea `delete_debug` si stergerea efectiva.
- Doar fisierele din evidenta debugging se pot sterge; un fisier sters local care nu a fost publicat debugging de aici (ex. deja trecut prin Git) nu apare si nu se sterge (alegere deliberata: nu se compara tot arborele serverului cu cel local).
- Dupa stergere, arborele Git de pe server arata fisierul ca sters, necomis; nu e atins.
- Inregistrarile locale din `docs/deployment/pending` pentru un fisier sters nu sunt atinse.
- Ajutor: niciunul (KBotAdmin nu e pentru clienti).
