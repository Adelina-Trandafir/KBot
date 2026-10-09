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
| SLICE-ADE0 | Analiză, plan și mapare Access → web | ADE0-01–09: DOCUMENTAT LOCAL; ADE0-08 = plan subunități; ADE0-09 = verificare selectivă MDB; implementarea subunităților planificată | [ADE0](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade0) |
| SLICE-ADE1 | Integrarea proiectului, sistemele comune și preview 5050 | TESTAT LOCAL — ecranul Prezență refăcut după Access; server nevalidat | [ADE1](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade1) |
| SLICE-ADE2 | DataGrid comun cu editare directă în celule | TESTAT LOCAL — editare și regresie read-only probate în browser | [ADE2](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade2) |
| SLICE-ADE3 | Schema ADE și configurarea chitanțelor | SCRIS LOCAL — DDL revizuit; nevalidat MariaDB | [ADE3](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade3) |
| SLICE-ADE4 | Utilizatori și drepturi ADE | ÎN LUCRU — guard/matrice parțiale | [ADE4](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade4) |
| SLICE-ADE5 | Migrarea datelor Access | ADE5-10 CONSTRUIT LOCAL: log nou și casetă goală la lansare; opțiunea CNP Copil = CNP Părinte păstrată; server nevalidat | [ADE5](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade5) |
| SLICE-ADE6 | Grupe, plătitori, taxe și prezență | ADE6-23 SCRIS LOCAL: mesaje informative Plătitori eliminate; fără teste noi | [ADE6](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade6) |
| SLICE-ADE7 | Calculul situațiilor și ciclul lunar, paritate 1 la 1 | ADE7-04 SCRIS LOCAL: închidere anuală august exclusiv desktop, mutări Ctrl/Shift/drag-and-drop și grupe noi validate; UI/server nevalidate | [ADE7](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade7) |
| SLICE-ADE8 | Încasări, chitanțe, alte documente și restituiri | ADE8-05 SCRIS LOCAL: meniu PC chitanțe, listare și PDF; mail fără acțiune; funcțional/vizual neprobat | [ADE8](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade8) |
| SLICE-ADE9 | Rapoarte, verificarea completă și predarea pentru server | PLANIFICAT | [ADE9](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade9) |

**Următorul număr liber de felie: SLICE-ADE10.**
Planul tuturor feliilor este [documentat](ADECHIT/PLAN_IMPLEMENTARE.md); stările
PLANIFICAT de mai sus se referă la implementare, nu la lipsa documentației.
ADE0–ADE9 sunt alocate prin acest plan. Subfeliile planificate și următoarele numere libere
sunt în secțiunea fiecărei felii. Registrul numeric K-BOT rămâne separat.

## Current focus

- **ADE0-08:** [planul subunităților](ADECHIT/plan_subunitati.md) documentat la
  09.10.2026, fără cod. Subunitățile împart DC-ul și au serii/contoare de chitanțe
  separate. ADE0-09: baza_40/baza_47 verificate selectiv read-only; constatările
  sunt în secțiunea 12 a planului. Următoarea subfelie liberă de analiză: **ADE0-10**.

- **ADE5-10:** logul anterior este arhivat la lansare și se creează unul nou;
  caseta din formular pornește goală, fără încărcarea automată a logului.
  Build curat; nerulat. [Worklog](docs/worklog/SLICE-ADE5-10-log-nou-pornire.md).
  Următoarea subfelie: ADE5-11.

- **ADE5-09:** opțiune implicit nebifată pentru copierea Platitori.CNP din Access
  în CNP_Platitor la plătitorii asociați. Schimbarea cere reverificarea destinației.
  Compilare: 0 erori/avertismente; fără teste sau migrare executată.
  [Worklog](docs/worklog/SLICE-ADE5-09-cnp-parinte.md). Continuare: ADE5-10.

- **ADE5-08 / ADE6-23 / ADE8-05:** grupă de plecați detectată după `pleca`,
  cu bifa manuală în migrator; mesajele informative Plătitori eliminate;
  meniul 📥 pe PC pentru chitanțe cu Listare/PDF implementate și mail inactiv.
  Migrator construit cu 0 erori/avertismente; sintaxă Python/JS verificată,
  fără teste funcționale sau vizuale. PDF necesită requirements-adechit.txt
  pe server. Următoarele subfelii: ADE5-09, ADE6-24, ADE8-06.
  [Migrare](docs/worklog/SLICE-ADE5-08-grupa-plecati.md),
  [Plătitori](docs/worklog/SLICE-ADE6-23-fara-mesaj-platitori.md),
  [Chitanțe](docs/worklog/SLICE-ADE8-05-meniu-listare-pdf.md).

- **ADE6-22:** ordonare alfabetică în liste/comboboxuri după nume/denumire,
  inclusiv educatori, taxe după explicație și documente după explicație.
  La pornire numai cel mai recent an este deschis, fără lună/grupă selectată
  și fără copii încărcați. [Worklog](docs/worklog/SLICE-ADE6-22-ordonare-ultimul-an.md).
  SCRIS LOCAL, fără teste noi; următoarea subfelie: ADE6-23.

- **ADE6-21:** pornire fără lună/grupă selectate și fără date despre copii;
  ani cu luni la deschidere, grupe după alegerea lunii, situație filtrată pe server
  după grupă. Plătitori încarcă inițial doar grupele, apoi copiii grupei și
  persoanele copilului. [Worklog](docs/worklog/SLICE-ADE6-21-incarcare-lazy.md).
  18 teste API și 3 teste JS trecute; fără probă vizuală/server.
  Următoarea subfelie: ADE6-22. Publicarea/restartul backendului aparțin utilizatorului.

- **ADE5-07:** DC destinație editabil, propus din Access; schimbarea cere Testează
  din nou. Confirmarea și jurnalul disting sursa de destinație. Build curat; nerulat.
  [Worklog](docs/worklog/SLICE-ADE5-07-migrator-dc-editabil.md). Următoarea subfelie: ADE5-08.

- **ADE5-06:** motivele blocării Migrează și jurnalul comun de erori sunt vizibile
  în formular. Ultima verificare din logul existent a eșuat prin Connect Timeout expired;
  cauza de rețea nu este diagnosticată. Build curat; versiunea nouă nerulată.
  [Worklog](docs/worklog/SLICE-ADE5-06-migrator-log-formular.md). Continuare: ADE5-07.

- **ADE5-05:** ADE.Migrator primește oprire controlată, progres pe tabel și jurnal SQL;
  selecția/conexiunea invalidează verificarea anterioară, iar rezultatul COMMIT necunoscut
  este raportat explicit. Build: 0 erori, 0 avertismente; nerulat, fără teste.
  [Worklog](docs/worklog/SLICE-ADE5-05-migrator-oprire-jurnal.md) și
  [utilizare](ADECHIT/ADE.Migrator/README.md). Continuare: ADE5-06.

- **ADE6-20:** Escape în prima celulă activată la adăugare anulează rândul nou;
  taxe cu valoare >0, explicație obligatorie și început nou ulterior celui
  existent, cu dialog Continuă/Anulează. Datele migrate NULL nemodificate sunt
  acceptate. SCRIS LOCAL, fără teste; backendul preview necesită restart.

- **ADE6-19:** formularul Taxe micșorat la 744px, cât DGV-ul plus paddinguri
  și borduri, cu limită 96vw. SCRIS LOCAL, fără teste.

- **SLICE-ADE6-18, 09.10.2026:** documentația threadului consolidată în
  [UTILIZARE_WEB](ADECHIT/UTILIZARE_WEB.md): regulile finale PC/mobil, DGV,
  formulare, CNP, perioadele taxelor și plătitorul activ unic per copil.
  ADE6-06 a fost testat local (16 API și browser); ADE6-07–17 sunt SCRIS LOCAL,
  fără teste ulterioare la cererea utilizatorului. Nu declarăm noua stare testată.
- Mobil: DGV comun, filtrare numai Nume/Grupa, I fără filtru, ➡️ pe Grupe/Copii
  în loc de apăsare lungă; margini exterioare 10px, padding interior 8px,
  rânduri +20%, scroll numai în tabel, acțiuni emoji în footer.
- Taxe: perioade lună/an, Activ checkbox, PanaLa automat; datele NULL din migrare
  nu blochează utilizarea. Necesită [AD_04](sql/AD_04_taxe_perioade.sql) după AD_03.
  Plătitorul nou devine activ și dezactivează ceilalți ai aceluiași copil atomic.
- **ADE1-05/06:** blueprintul și redirecționarea /portal corectate; pornirea și
  deschiderea preview-ului local 5050 au fost testate exclusiv nonvizual.
  Această probă nu acoperă ultimele schimbări backend. Preview-ul trebuie repornit;
  DDL-ul, publicarea prin AvacontPush și testarea serverului aparțin utilizatorului.

### Repere anterioare (starea la data intervenției)

Descrierile istorice de mai jos nu înlocuiesc regulile și starea finală de mai sus.

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
  implementate parțial; M03 rămâne neimplementat, iar M06 are mutarea individuală
  și jurnalul, cu transferul lot și compensarea încă lipsă. ADE9 și toate
  rapoartele/PDF-urile/tipăririle rămân amânate.
- **SLICE-ADE0-02:** maparea statică este documentată local în
  [fluxuri](ADECHIT/MAPARE_ACCESS_WEB.md), [calcule](ADECHIT/CONTRACT_CALCULE.md) și
  [inventarul celor 155 de obiecte](ADECHIT/INVENTAR_OBIECTE.md).
- La reluare: consultați regulile finale din [UTILIZARE_WEB](ADECHIT/UTILIZARE_WEB.md) și firele deschise; integrarea și grila comună sunt deja începute.
  Neconcordanțele M01–M10 sunt consemnate; nu s-a demonstrat încă paritatea în execuție.
- Ordine propusă: ADE0 → ADE1 → ADE2 → ADE3 → ADE4 → ADE5 → ADE6 → ADE7 → ADE8 → ADE9.
  Datele istorice migrate în ADE5 permit compararea calculelor înaintea implementării
  noilor încasări. ADE9 repetă comparația pe operații introduse integral din web.
- Editarea în celule se păstrează la Prezență, Taxe și perioadele educatorilor.
  Pentru Grupe/Copii/Plătitori utilizatorul a cerut liste de selecție și editori
  modali după machete (ADE6-06), înlocuind propunerea inițială de editare directă.

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

13. Regulile finale PC/mobil, filtrele numai Nume/Grupa, I inversat pentru Activ,
    perioadele taxelor nullable la migrare și activarea unică per copil sunt
    consemnate în [UTILIZARE_WEB](ADECHIT/UTILIZARE_WEB.md). Testarea UI aparține
    utilizatorului; excepția autorizată a fost numai pornirea nonvizuală ADE1-06.

## Open threads — transversale

Registrul complet al deciziilor/probelor: [DECIZII_DESCHISE](ADECHIT/DECIZII_DESCHISE.md).

- Subunități: [plan documentat](ADECHIT/plan_subunitati.md), neimplementat.
  Sunt necesare corespondența MDB/subunitate, exporturile țintelor și analiza
  echivalenței regulilor temporale bazate pe IDL înaintea remapării ID-urilor.
- [M01–M10](ADECHIT/MAPARE_ACCESS_WEB.md#6-neconcordanțe-de-rezolvat-înaintea-implementării-dependente):
  M01–M09 au decizii consemnate la 08.10.2026. M02, M04 și M05 sunt implementate
  local; M03 este decis, dar neimplementat, iar M06 este implementat parțial
  (jurnal, perioade educatori, mutare individuală/Plecat). M10 și paritatea necesită probe.
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
