# SLICE-VC10-01 — separarea conturilor din Materiale

Data: 03.11.2026. Stare: implementare în surse, fără build/teste/import rulate de agent. Înregistrarea pentru publicare nu a fost executată; este necesară confirmarea explicită a Adelinei.

## Cerințe și decizii

- Fila custom VRC.Migrator pentru citirea bazelor încărcate și separarea conturilor în gestiuni noi, sub aceeași subunitate.
- `303 OBIECTE INVENTAR` merge la OI; `302.8.07 PROIECT SCOLAR - OBIECTE INVENTAR` **nu este OI** și merge într-o gestiune separată de Materiale. Nu se clasifică automat după denumire.
- Pentru OI: BC → BP + BC legat + registru deținător; continuitate multianuală fără repetarea reporturilor în magazie.
- Erorile de generare se colectează, cu nume/baze/documente/produse. Diferențele de nume ale deținătorilor se pot asocia din selector; nu se oprește analiza la prima astfel de diferență.
- Nu se scrie în Access și nu se modifică schema MariaDB. Nu se rulează verificări/build/teste fără cerere explicită.

## Context citit

Au fost citite doar copii temporare ale `tz_2022.accdb`–`tz_2026.accdb` din `C:\vercon\Scoala GEP\14`. **`tz.accdb` a fost exclus și nu a fost citit.** Furnizorul ACE încerca un fișier de blocare în folderul sursă, de aceea analiza contextului a folosit copii temporare.

În cele două conturi citite, tabelele OI_Predare/OI_Produse sunt goale. Comparația de context a găsit reporturi anuale egale cu soldurile anului anterior; la sfârșitul bazelor 2022, 2024, 2025, 2026 rămâne o tabletă în magazie, iar 2023 mai are încă două poziții. Nu s-au găsit între ani repetări ale combinației produs–preț–deținător în datele citite inițial. Aceasta este o analiză a surselor, nu un test al codului nou și nici o identificare fizică a obiectelor. Nu se reconstruiește folosința din anii anteriori primei baze selectate.

## Implementare

- `VrcOiData.vb`: snapshot, amprente, proveniență și modele pentru alegeri/asocieri. `VrcOiConversion` aplică reziduurile și sursele generate la planul de bază numai când hash-ul încă se potrivește.
- `VrcOiBuilder.vb`: conversia OI multianuală, reconcilierea deschiderilor, registrul și BC/BP, colectarea diagnosticelor, asocierile deținătorilor și semnalarea suprapunerilor. O eroare de date se colectează la nivelul rândului/documentului/anului; analiza continuă unde datele permit.
- `VrcAccountSeparation.vb`: mai multe destinații, inclusiv Materiale pe ani; reziduurile pentru aceeași sursă se intersectează, ca să nu reintroducă un cont eliminat de altă destinație.
- `VrcOiWorkspace*.vb`: selector de baze/tabele doar pentru citire, conturi cu tip/nume destinație, deținători cu nume sursă și selector destinație, confirmări individuale de predări distincte, raport.
- `VrcPlan.vb`: surse pregătite, surse separate, hash/export cu transformări; permite partițiile suplimentare declarate explicit.
- `VrcRunner.vb` / `VrcRun.vb`: citirea snapshoturilor, verificarea amprentei surselor afectate la transfer și jurnalul `N22`. `N20` rămâne rezervat recuperării prețurilor storno/retur.
- `VrcSettings.vb`: alegeri și asocieri persistente, fără persistarea datelor generate; la reluare este obligatorie regenerarea.
- Versiune în surse: `0.3.18.0`; reguli `VC10-OI-SEPARATION-2026-11-03`.

### Continuarea din 03.11.2026

- `VrcOiHolders.vb` centralizează identificarea și rezolvarea deținătorilor pentru OI și Materiale. O referință lipsă nu se mai poate păstra ca număr vechi care ar putea coincide cu o altă persoană din destinație.
- Grila include deținători lipsă/fără nume și referințe goale/zero, cu data și numărul primului bon și numărul de bonuri implicate. Asocierea lor necesită o alegere explicită către un deținător existent al aceleiași subunități. SourceKey rămâne numai în modelul intern.
- Asocierile sunt aplicate și jurnalizate și pentru destinația Materiale. Fără o asociere rezolvabilă, fiecare bon afectat este diagnosticat, iar datele candidate nu pot fi aplicate planului.
- Încărcarea snapshoturilor colectează erorile tuturor bazelor; nu mai termină citirea la prima eroare de fișier. Încărcarea incompletă invalidează separarea anterioară și impune reîncărcarea înainte de generare.
- Continuarea a folosit numai citirea/revizuirea surselor și editări de cod/documentație. Nici în această continuare nu s-au rulat build, teste sau importuri și nu s-a executat înregistrarea pentru publicare.

## Continuare / limite de verificare

Agentul nu a compilat și nu a rulat aplicația, teste sau importuri. Adelina a raportat dintr-o probă a prototipului blocajul deținătorului cu nume diferite; codul ulterior înlocuiește acest blocaj cu asocieri după numele ales în grilă și diagnostic complet. Modificările ulterioare acestei probe necesită proba ei; nu sunt considerate confirmate pentru publicare.

Revizuirea de proiectare a urmărit mapările, păstrarea explicațiilor documentelor, tipurile coloanelor custom și raportarea destinațiilor separate în planul principal. Copia JSON temporară folosită numai la citirea contextului a fost eliminată din proiect. Nu s-au creat binare de către agent. Următorul pas de utilizare este recitirea bazelor și reîncărcarea conturilor, alegerea tipului OI pentru 303 și MATERIALE pentru 302.8.07, asocierea numelor dorite și regenerarea separării; orice build/probă de către agent necesită cerere explicită.

La continuare se păstrează interdicția de a atinge `tz.accdb`, de a executa DDL, de a rula teste fără cerere și de a înregistra fișierele fără confirmarea explicită. Dacă se cere un model diferit, se pornește de la aceste surse și de la ultimele corecții din chat, fără a relua sau publica automat.

Adelina a cerut oprirea lucrului când limita comună de 5 ore ajunge la **7% rămas**. Se citește `mcp__codex_app__get_usage_limits`, bucket `rateLimitsByLimitId.codex.primary`, `windowDurationMins=300`; procentul rămas este `100-usedPercent`. La `usedPercent >= 93`, se actualizează acest punct de continuare și se oprește lucrul. Nu se consumă credite de resetare și nu se programează automatizări.

## Fișiere proprii pentru înregistrare după confirmarea explicită

1. `VERCON/VRC.Migrator/VRC.Migrator.vbproj`
2. `VERCON/VRC.Migrator/VrcMigratorForm.Designer.vb`
3. `VERCON/VRC.Migrator/VrcMigratorForm.vb`
4. `VERCON/VRC.Migrator/VrcPlan.vb`
5. `VERCON/VRC.Migrator/VrcRun.vb`
6. `VERCON/VRC.Migrator/VrcRunner.vb`
7. `VERCON/VRC.Migrator/VrcSettings.vb`
8. `VERCON/VRC.Migrator/VrcOiData.vb`
9. `VERCON/VRC.Migrator/VrcOiBuilder.vb`
10. `VERCON/VRC.Migrator/VrcOiWorkspace.Designer.vb`
11. `VERCON/VRC.Migrator/VrcOiWorkspace.vb`
12. `VERCON/VRC.Migrator/VrcAccountSeparation.vb`
13. `VERCON/VRC.Migrator/README.md`
14. `docs/worklog/SLICE-VC10-01-separare-materiale-oi.md`
15. `VERCON/VRC.Migrator/VrcOiHolders.vb`

Motiv concret: separarea conturilor din Materiale în gestiuni distincte, conversie OI multianuală fără dublarea reporturilor, diagnostic complet și asocierea deținătorilor după numele ales. Nu se includ binare sau fișiere modificate de alte taskuri.
