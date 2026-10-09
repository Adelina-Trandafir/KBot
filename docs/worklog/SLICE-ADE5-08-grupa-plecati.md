# ADE5-08 — Grupa de plecați în migrator

La citirea Access, migratorul afișează grupele alfabetic și bifează Plecați dacă
denumirea conține «pleca», fără diferență între litere mari/mici. Bifa poate fi
schimbată manual, inclusiv debifarea unei grupe detectate automat. Recitirea sursei
reface detectarea. Modificarea alegerii invalidează verificarea anterioară.

Planul primește opțiunile înainte de construirea istoricului și scrie tipul
PLECATI/NORMALA corespunzător. Mai multe grupe de plecați blochează planul,
deoarece serviciul lunar acceptă cel mult una. Lipsa grupei produce doar o notă.

Fișiere: AdePlan.vb, AdeMigratorForm.vb, designerul formularului, README și
documentele de stare/plan. Nu s-au modificat bazele de date sau serviciile remote.

Compilare locală: `dotnet build ADECHIT/ADE.Migrator/ADE.Migrator.vbproj --no-restore -p:BuildProjectReferences=false -p:UseSharedCompilation=false -m:1 -nr:false -v:minimal`.
Rezultat: 0 avertismente, 0 erori. Nu s-au executat teste automate sau verificări
funcționale/vizuale; acestea revin utilizatorului.

Următoarea subfelie liberă: ADE5-09.
