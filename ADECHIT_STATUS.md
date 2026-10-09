# ADECHIT — STATUS și plan de portare

Actualizat: **09.10.2026**. Sursa unică pentru starea proiectului ADECHIT.
Acest fișier spune ce este planificat, ce este scris și ce este verificat.
Dacă realitatea diferă, actualizăm statusul în aceeași intervenție.

Structură după [KBOT_STATUS](docs/worklog/KBOT_STATUS.md) și
[CODE_WORKFLOW](docs/worklog/CODE_WORKFLOW.md), cu numerotarea și regulile
specifice stabilite de utilizator pentru ADECHIT, consemnate mai jos.

## Reguli de lucru și numerotare

- Citim întâi acest fișier, apoi numai secțiunile de stare și sursele necesare feliei.
- Citim și aplicăm [regulile proiectului](ADECHIT/REGULI_PROIECT.md), adoptate explicit
  la 08.10.2026: regulile selectate pentru Python/JS/web, fără cele specifice VB.NET.
- **Nu citim și nu modificăm niciun fișier `Claude.md`, indiferent de capitalizare.**
  Nicio trimitere din alte documente nu schimbă această interdicție.
- **Nu încercăm să oprim procese MSACCESS rămase.** Anunțăm clar utilizatorul să le
  oprească el, inclusiv dacă procesul a fost pornit pentru această lucrare (08.10.2026).
- Feliile sunt `SLICE-ADE0`, `SLICE-ADE1`, `SLICE-ADE2` etc., fără completare cu zerouri
  a numărului principal. Subfeliile sunt `SLICE-ADE2-01`, `SLICE-ADE2-02` etc.
- Numerele alocate sunt permanente. O corecție sau o continuare primește următoarea
  subfelie din aceeași felie; nu redenumim lucrările deja înregistrate.
- Fiecare intervenție are worklog `docs/worklog/SLICE-ADE<n>-<NN>-<descriere>.md`:
  ce s-a schimbat și de ce, fișiere, verificări efective, ce rămâne neverificat/amânat.
- Detaliile sunt grupate câte zece felii în `docs/worklog/state/ADECHIT_STATUS_ADE<n>-ADE<m>.md`.
  Indexul, deciziile comune și următorul număr liber rămân aici, în rădăcina KBOT.
- Actualizăm împreună worklogul, secțiunea feliei și indexul; citim fișierele reale înainte
  de modificare. Codul și comentariile tehnice sunt în engleză; mesajele utilizatorului
  sunt în română cu diacritice. Erorile sunt afișate sau propagate și înregistrate.
- Schimbările vizibile actualizează documentația de utilizare relevantă cu codul feliei;
  dacă rămâne documentație de actualizat, o numim explicit în worklog și în Open threads.
- **Codex lucrează local, fără acces direct la Linux sau MariaDB. Utilizatorul face push
  prin proiectul `AvacontPush` și testează pe server.** Nu executăm SSH, SQL remote,
  publicare sau restart de serviciu din acest chat.
- Pentru probe locale folosim **`http://localhost:5050`**, cu date de probă și adaptoare
  locale pentru serviciile indisponibile. Proba locală nu dovedește funcționarea pe MariaDB.
- O predare precizează fișierele de publicat, ordinea scripturilor SQL, verificările locale
  și pașii de probat pe server. Commitul, pushul, DDL-ul și validarea pe server se raportează
  separat; nu sunt presupuse executate. Regula de publicare de mai sus prevalează asupra
  cerinței generice de push din CODE_WORKFLOW.

## Slice registry

| Felie | Obiectiv | Stare | Detalii |
|---|---|---|---|
| SLICE-ADE0 | Analiză, plan și mapare Access → web | ADE0-01–07: DOCUMENTAT LOCAL; sursa reală și regula MSACCESS salvate; ADE0-06 = deciziile despre schema AD_; ADE0-07 = grupe/istoric/plecați + DDL final | [ADE0](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade0) |
| SLICE-ADE1 | Integrarea proiectului, sistemele comune și preview 5050 | TESTAT LOCAL — ecranul Prezență refăcut după Access; server nevalidat | [ADE1](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade1) |
| SLICE-ADE2 | DataGrid comun cu editare directă în celule | TESTAT LOCAL — editare și regresie read-only probate în browser | [ADE2](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade2) |
| SLICE-ADE3 | Schema ADE și configurarea chitanțelor | SCRIS LOCAL — DDL revizuit; nevalidat MariaDB | [ADE3](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade3) |
| SLICE-ADE4 | Utilizatori și drepturi ADE | ÎN LUCRU — guard/matrice parțiale | [ADE4](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade4) |
| SLICE-ADE5 | Migrarea datelor Access | TESTAT LOCAL — 5.247 rânduri importate și reconciliate fără diferențe | [ADE5](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade5) |
| SLICE-ADE6 | Grupe, plătitori, taxe și prezență | TESTAT LOCAL — trei liste Grupe/Copii/Plătitori și formulare modale după machete; CNP validat; server nevalidat | [ADE6](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade6) |
| SLICE-ADE7 | Calculul situațiilor și ciclul lunar, paritate 1 la 1 | TESTAT LOCAL PARȚIAL — M05 implementat; rezultate Access lipsă | [ADE7](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade7) |
| SLICE-ADE8 | Încasări, chitanțe, alte documente și restituiri | TESTAT LOCAL PARȚIAL — taburi/rând nou SID-SIC; anularea restituirii blocată de M03 | [ADE8](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade8) |
| SLICE-ADE9 | Rapoarte, verificarea completă și predarea pentru server | PLANIFICAT | [ADE9](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade9) |

**Următorul număr liber de felie: SLICE-ADE10.**
Planul tuturor feliilor este [documentat](ADECHIT/PLAN_IMPLEMENTARE.md); stările
PLANIFICAT de mai sus se referă la implementare, nu la lipsa documentației.
ADE0–ADE9 sunt alocate prin acest plan. Subfeliile planificate și următoarele numere libere
sunt în secțiunea fiecărei felii. Registrul numeric K-BOT rămâne separat.

## Current focus

- **SLICE-ADE6-14, 09.10.2026:** filtrare exclusiv pentru Nume/Grupa în toate
  DGV ADE, pe PC și mobil. Antetele și footerele listelor folosesc culorile comune
  ale DGV-ului principal. SCRIS LOCAL, fără teste.

- **SLICE-ADE6-13, 09.10.2026:** fereastra Plătitori pe mobil are margine exterioară
  de 10px pe toate laturile; paddingul interior din ADE6-12 se păstrează.
  SCRIS LOCAL, fără teste.

- **SLICE-ADE6-12, 09.10.2026:** padding mobil de 8px pentru Plătitori, buton ➡️
  pe fiecare rând Grupe/Copii în locul apăsării lungi. Coloana de 40px se scade
  exclusiv din denumire; CNP/I își păstrează lățimile. Rândurile tuturor DGV ADE
  sunt cu 20% mai înalte pe mobil. SCRIS LOCAL, fără teste.

- **SLICE-ADE6-11, 09.10.2026:** toate coloanele de stare din liste au antetul I
  (închis), fără filtru. La plătitori I este inversul lui Activ numai pentru afișare;
  datele și editorul păstrează Activ. SCRIS LOCAL, fără teste.

- **SLICE-ADE6-10, 09.10.2026:** Plătitori pe mobil ocupă întregul ecran, cu
  un singur tabel vizibil și scroll numai în tabel. Apăsare lungă Grupe → Copii →
  Plătitori, înapoi în antet, acțiuni emoji în footer și coloane proporționale.
  SCRIS LOCAL, fără teste; verificarea vizuală aparține utilizatorului.

- **SLICE-ADE6-09, 09.10.2026:** rândul de mesaje și spațiul rezervat lui sunt
  eliminate pe mobil; zona de lucru începe cu comboboxurile. SCRIS LOCAL, fără teste.

- **SLICE-ADE6-08, 09.10.2026:** bara lună/grupă ascunsă complet pe PC, fără rând
  rezervat; tabelul începe la nivelul panoului LUNA / ANUL. Pe mobil rămân doar
  comboboxurile, fără text explicativ. SCRIS LOCAL, fără teste vizuale.

- **SLICE-ADE1-06, 09.10.2026:** pornirea preview-ului și deschiderea aplicației
  verificate nonvizual la cererea utilizatorului. Pagina locală este marcată explicit,
  gate-ul nu o mai redirecționează spre autentificare, iar / și /portal revin la /adechit.
  HTTP 200 pentru context/situație/catalog/configurarea chitanțelor; browser fără token,
  fără erori JS/HTTP. Server local pornit pe 5050; nu s-a verificat aspectul.

- **SLICE-ADE1-05, 09.10.2026:** corectat NameError la pornire: fabrica de blueprint
  creează acum `bp` înaintea decoratorilor rutelor. SCRIS LOCAL; fără pornire/teste,
  conform cerinței utilizatorului.

- **SLICE-ADE6-07, 09.10.2026:** Enter trece la următoarea celulă editabilă,
  clic unic pe PC, calendar comun în DGV; antete modale ca antetul principal,
  ANI aliniat cu tabelul educatorilor, + Adaugă în footer, formulare copil/plătitor
  la jumătate din lățime și spațiere redusă. SCRIS LOCAL, fără teste la cererea
  utilizatorului; verificarea funcțională îi aparține. Server nepublicat.

- **SLICE-ADE6-06, 09.10.2026:** Plătitori urmează cele patru machete: trei liste
  alăturate, Adaugă/Modifică, formulare uniforme pentru grupe/copii/plătitori,
  combobox, calendare comune și validare CNP în JS/API. Salvare atomică pentru
  grupa + educatorii săi; mutarea copilului păstrează lunile închise și scrie jurnalul.
  API: 16/16; browser local 5050, inclusiv telefon. Server/schema live nevalidate.
  PL este checkbox doar pentru afișare în liste, editabil exclusiv din editorul elementului;
  la grupe înseamnă Grupă închisă, la copii Plecat (confirmat de utilizator).

- **SLICE-ADE6-05, 08.10.2026:** Plătitori și Taxe sunt ferestre modale din antet,
  cu fundal blocat și salvare explicită atomică. Erorile păstrează editările.
  API ADE: 11/11; probe browser locale pe 5050. Server nevalidat.

- **SLICE-ADE1-04/ADE6-04/ADE8-04, 08.10.2026:** pagina Prezență urmează acum
  organizarea Access: arbori lună/grupă în stânga, situație editabilă în centru, comenzi sub
  grilă și taburi Chitanțe/Alte plăți/Restituiri în footer. Data unui rând nou propune SID
  la încasări și SIC la restituiri; valoarea rămâne editabilă. Testele locale ADE sunt 9/9.
- **Continuare 08.10.2026:** mediul local rulează Python 3.12.14; pagina are
  `static/js/adechit/app.js`, iar preview-ul strict local a fost probat în browser pe 5050.
  Editarea directă a prezenței și regresia situației read-only au trecut. Importul real din
  MDB a reconciliat 5.247 de rânduri fără diferențe; 745 legături istorice lipsă sunt raportate.
  M05 este implementat și testat. Paritatea `mdl_Situatie` nu este declarată: lipsesc încă
  rezultatele intermediare produse de Access.
- **SLICE-ADE5-04, 08.10.2026:** `ADECHIT/ADE.Migrator` — utilitar nou care citește direct `.mdb`-ul (parola comună, DC din
  `Unitati`) și scrie schema AD_03 în MariaDB, cu educatorii despărțiți și corectabili și istoricul copiilor reconstruit.
  Build curat; nerulat. Detalii în [worklog](docs/worklog/SLICE-ADE5-04-ade-migrator.md).
- **SLICE-ADE0-07, 08.10.2026:** M06 decis: fără MutaCopil (tabel mort); jurnal automat per copil, educatori pe perioade
  per grupă (fără nivel mica/mare), grupe închise dar vizibile în trecut, copil în prezența grupei din momentul
  închiderii lunii, copii plecați + compensare, motiv de anulare la restituiri (M03). DDL final în
  [sql/AD_03_schema_finala.sql](sql/AD_03_schema_finala.sql), nevalidat pe MariaDB; codul nu e adaptat încă.
  Detalii în [worklog](docs/worklog/SLICE-ADE0-07-decizii-grupe-istoric-ddl-final.md).
- **SLICE-ADE0-06, 08.10.2026:** prefixul tabelelor este `AD_`, schema are chei străine și este destinată
  AVACONT_SURSA; `Delegati`, `Prezenta_sub` și `MutaCopil` nu se mai portează; multe coloane sunt scoase;
  `LunaD` primește `ZileLuna`. Detalii în [worklog](docs/worklog/SLICE-ADE0-06-decizii-schema-ad.md).
  Importul ADE5 trebuie repetat pe schema nouă; M03 (motivul restituirii) rămâne fără loc de salvare.
- **SLICE-ADE0-05, 08.10.2026:** datele reale sunt în
  `ADECHIT/ACCESS_SOURCES/baza2020_PP.mdb`; citirea locală doar-citire a reușit.
  Această disponibilitate actualizează notele anterioare despre date încă neprimite;
  reconcilierea completă și rezultatele de calcul Access rămân de verificat.
  Regula de neoprire MSACCESS este salvată în regulile proiectului.
- **SLICE-ADE0-04, 08.10.2026:** cele 15 reguli selectate sunt adoptate în
  [REGULI_PROIECT](ADECHIT/REGULI_PROIECT.md). Această intervenție nu pornește implementarea.
- **SLICE-ADE0-03, 08.10.2026:** documentarea tuturor feliilor este terminată local.
  [Planul detaliat](ADECHIT/PLAN_IMPLEMENTARE.md) acoperă ADE0–ADE9 și toate cele
  30 de subfelii alocate; [deciziile deschise](ADECHIT/DECIZII_DESCHISE.md) au dependențe explicite.
- ADE1 și ADE2 sunt testate local. ADE5 este testat local pe extractul real. ADE6/ADE8 sunt
  finalizate pentru operațiile neblocate; M06 și M03 rămân refuzuri explicite. ADE9 și toate
  rapoartele/PDF-urile/tipăririle rămân amânate.
- **SLICE-ADE0-02:** maparea statică este documentată local în
  [fluxuri](ADECHIT/MAPARE_ACCESS_WEB.md), [calcule](ADECHIT/CONTRACT_CALCULE.md) și
  [inventarul celor 155 de obiecte](ADECHIT/INVENTAR_OBIECTE.md).
- La reluarea implementării: **ADE1-01** pentru integrare; **ADE2** pentru editarea grilei.
  Neconcordanțele M01–M10 sunt consemnate; nu s-a demonstrat încă paritatea în execuție.
- Ordine propusă: ADE0 → ADE1 → ADE2 → ADE3 → ADE4 → ADE5 → ADE6 → ADE7 → ADE8 → ADE9.
  Datele istorice migrate în ADE5 permit compararea calculelor înaintea implementării
  noilor încasări. ADE9 repetă comparația pe operații introduse integral din web.
- Primele ecrane de lucru vor avea editare în celule. Aceasta nu este amânată pentru
  o versiune ulterioară și nu este înlocuită cu formulare separate.

## Locked decisions

1. **Reutilizăm TOATE sistemele comune existente:** Linux/nginx/Gunicorn/Flask,
   conectarea la baza existentă, autentificarea, sesiunile și monitorizarea lor,
   logarea și auditul, instrumentarea timpilor, EventBus și diagnosticul lui,
   ListenerTracker, registrul instanțelor, controalele JS, temele, preferințele,
   aspectul portalului, vizualizarea PDF și fluxul de publicare AvacontPush.
   Extindem implementările comune unde este nevoie; nu creăm copii independente.
2. Proiectul ADECHIT are rădăcina `ADECHIT/`, sursele de referință sunt în
   `ADECHIT/ACCESS_SOURCES/`. Integrarea cu `PYTHON/` și pachetul trimis prin AvacontPush
   se definește în ADE1. Sursele Access rămân referință, nu cod de servit public.
3. Refolosim baza deja creată. Tabelele noi de business au prefixul **`AD_`**.
   Excepție cerută explicit: **`AVACONT_COMUN.Unitati_Chitante`**.
   Nu presupunem numele bazei țintă sau acorduri de acces pe alte tabele.
4. **Nu portăm tabelele `_L`:** `Delegati_L`, `Grupe_L`, `LunaD_L`, `Platitori_L`,
   `Platitori_sub_L`, `Prezenta_L`, `Prezenta_sub_L`. Păstrăm regulile de business
   din codul care le utiliza. Celelalte tabele auxiliare se clasifică individual.
5. **Fără facturi și fără bonuri fiscale în ADECHIT.** Nu portăm ecranele, emiterea
   sau integrarea lor. Posibilele contribuții ale datelor istorice la solduri se
   inventariază înainte de import; nu eliminăm sume care ar schimba calculele cerute.
6. **Calculele din `mdl_Situatie` și interogările apelate se respectă 1 la 1.**
   Fără reinterpretare a formulelor, filtrelor, anulărilor, semnelor, perioadelor,
   conversiilor, rotunjirilor sau ordinii operațiilor. Nu portăm reducerile pentru
   absențe consecutive ori frați. Diferențele/ambiguitățile se consemnează, nu se
   „corectează” unilateral. Validarea compară web cu rezultatele Access.
7. Datele unității se refolosesc din sistemul existent: formularul
   `src/KBot.EFactura/Forms/DateUnitateForm.vb` scrie, prin API, în
   `AVACONT_COMUN.Unitati_Detalii`, pe `DC`; conturile sunt în `Unitati_Conturi`.
   **Momentan nu construim în ADECHIT pagină/rută pentru editarea datelor unității.**
8. `Unitati_Chitante` va primi configurația Access `CFGs`, `f='CH'`:
   `SERIE`, `NUMAR`, `Explicatie`, asociate unității. **`GR40` și `783` sunt DOAR exemple**,
   nu valori implicite, seed-uri sau constante. Explicația se preia din date;
   șablonul ilustrat este `C/Val. luna [LA] conf. contract`, cu substituția `[LA]`.
   Precizare 07.10.2026, ADE0-02: Plati_chitante citește numărul de utilizat și salvează
   NUMAR+1; configurația reprezintă următorul număr pe acest flux. Verificăm datele la import.
9. Utilizatorii noi au acces numai la ADE și la alte tabele aprobate explicit.
   Verificarea drepturilor se face în API și în configurarea SQL, nu doar în meniu.
   API-ul existent folosește cont de serviciu; drepturile loginului MariaDB singure
   nu limitează cererile executate prin acel cont.
10. DataGrid primește editare ca funcționalitate comună, configurabilă pe coloană și
    operație. Utilizările existente rămân implicit doar-citire. Refolosim controalele
    custom pentru editori și sistemele de evenimente, urmărire și curățare.
11. Adaptăm tranzacțiile, numerotarea și concurența pentru web fără schimbarea
    rezultatelor de business. Nicio golire globală a tabelelor de lucru între utilizatori;
    lunile închise și drepturile se verifică pe server la scriere.
12. **Confirmat de utilizator la 07.10.2026:** păstrăm încasările în luna închisă,
    cu actualizarea SS_Buget; prezența rămâne blocată. Păstrăm condiția temporală Access
    pentru inserare/anulare: refuz dacă IDL selectat + 1 < MAX(Prezenta.IDL).
    La 08.10.2026 utilizatorul a decis că orice modificare a documentelor unei luni închise,
    inclusiv anularea, rescrie situația salvată pentru copilul respectiv.

## Open threads — transversale

Registrul complet al deciziilor/probelor: [DECIZII_DESCHISE](ADECHIT/DECIZII_DESCHISE.md).

- [M01–M10](ADECHIT/MAPARE_ACCESS_WEB.md#6-neconcordanțe-de-rezolvat-înaintea-implementării-dependente):
  M02, M04 și M05 sunt implementate local. M03 și M06 rămân
  fără răspuns, iar TIP legacy, șabloanele și sursele lipsă necesită probe.
  Conversiile Long/Double și rezultatele intermediare necesită probe Access; nu rescriem
  formulele prin presupuneri. Aceste puncte nu blochează scheletul comun sau grila.
- MDB-ul real a fost extras selectiv read-only. Rezultatele pe faze ale interogărilor Access
  nu au fost încă produse; fixture-urile web și comparatorul raportează această lipsă.
- Baza țintă, unitățile, rolurile exacte și eventualele tabele comune accesibile ADE
  se vor stabili înainte de scripturile de instalare finale; nu blocăm preview-ul local.
- `schema_sync` propune ștergerea tabelelor absente din schema sursă. În ADE3 trebuie
  definită integrarea `AD_` în șablon/filtrare, înainte ca utilizatorul să ruleze sync.
- Documentația veche despre ținta `schema_sync` diferă de codul citit: în prezent,
  `schema_common.connect` folosește `DB_CONFIG_NEW`. Verificăm codul și configurația
  furnizată de utilizator la predare, fără conexiune directă la server.
- Loggerul corelează prin `g.session`/`g.session_token`, portalul folosește `g.portal`.
  ADE1/ADE4 vor adapta contextul comun pentru a păstra utilizatorul/unitatea în jurnal.
- Starea live a serviciilor, schemei și deploy-urilor nu este verificată de Codex.
  Nu schimbăm numărul de workeri sau backendul de sesiuni în baza unor presupuneri.

## Cum actualizăm statusul

Pentru fiecare subfelie: actualizăm Registry, Current focus și Open threads din fișierul
de stare, worklogul și rândul scurt de aici. La o felie nouă actualizăm numărul liber;
la ADE10 creăm grupul ADE10–ADE19. Deciziile comune se schimbă numai cu o notă datată.

Stări: **PLANIFICAT**, **ÎN LUCRU**, **SCRIS LOCAL**, **TESTAT LOCAL**, **PREDAT PENTRU PUSH**,
**CONFIRMAT PE SERVER**. Pentru documente folosim **DOCUMENTAT LOCAL**.
Indicăm mereu separat ce nu s-a verificat. „Scris” nu înseamnă „testat”, iar „testat local”
nu înseamnă „publicat”. O felie cu verificări restante nu este declarată complet validată.
