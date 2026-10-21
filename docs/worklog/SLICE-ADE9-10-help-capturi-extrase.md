# ADECHIT — help cu capturi și încasări din extrase

Data: 21 octombrie 2026

Am extins helpul existent din `PYTHON/static/help/adechit.json` pe structura lui actuală. Acoperă ecranul principal, tabelul de prezență, grupurile și plătitorii, taxele, zilele lucrătoare, chitanțele, alte plăți, restituirile, rapoartele, exportul, drepturile, DISCUȚII și durata sesiunii. Am descris și cele șase comenzi din Meniu de sus.

Secțiunea nouă despre extrase se aplică numai unităților care încasează contribuțiile prin bancă. Explică selectarea încasărilor neasociate, asocierea manuală, previzualizarea automată, potrivirea după cod fiscal și cont, verificarea și generarea documentelor în Alte plăți, cazurile ambigue, accesul necesar și tratarea dublelor asocieri.

Cele 23 de capturi publicabile din `PYTHON/static/help/images/adechit/` provin din interfața DEMO. Captura tabelului principal are ambele panouri laterale restrânse. Valorile personale, financiare, de prezență și conținutul discuțiilor sunt mascate cu blur puternic. Inventarul public se află în `manifest.json`. Originalele nemascate au fost șterse din spațiul temporar după prelucrare. Captura pregătită inițial pentru filtrare a fost exclusă deoarece nu arăta meniul de filtre; instrucțiunile pentru filtrare rămân în help.

Am adăugat în `AGENTS.md` regula de blur puternic pentru viitoarele capturi de help. Capturile VERCON rămân în forma deja acceptată.

Nu au fost modificate datele din ADECHIT sau structura bazei de date. Nu au fost rulate teste, conform instrucțiunilor proiectului. Publicarea fișierului de help și a imaginilor rămâne pasul de deployment.
