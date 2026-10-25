# ADECHIT — STATUS și plan de portare

Stare produs: confirmarea operatorului din **23.10.2026**; reorganizare documentară D001-04, **24.10.2026**. Sursa unică pentru starea proiectului ADECHIT.
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
- **Pentru numărul principal mai mare de 9, prefixul `ADE` devine `AD`**
  (instrucțiune explicită, 10.10.2026): `SLICE-AD10`, `SLICE-AD11` etc., cu
  subfelii `SLICE-AD11-01`, `SLICE-AD11-02` etc. Regula se aplică și referințelor.
- Numerele alocate sunt permanente. O corecție sau o continuare primește următoarea
  subfelie din aceeași felie; nu redenumim lucrările deja înregistrate.
- Fiecare intervenție are worklog `docs/worklog/SLICE-ADE<n>-<NN>-<descriere>.md`
  pentru n ≤ 9, respectiv `docs/worklog/SLICE-AD<n>-<NN>-<descriere>.md` pentru n > 9:
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
| SLICE-ADE0 | Analiză, plan și mapare Access → web | ADE0-01–09: TESTAT, RULAT, PUBLICAT; ADE0-08 = plan subunități; ADE0-09 = verificare selectivă MDB; subunități implementate | [ADE0](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade0) |
| SLICE-ADE1 | Integrarea proiectului, sistemele comune și preview 5050 | TESTAT, RULAT, PUBLICAT — ecranul Prezență refăcut după Access | [ADE1](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade1) |
| SLICE-ADE2 | DataGrid comun cu editare directă în celule | TESTAT, RULAT, PUBLICAT — editare și regresie read-only probate în browser | [ADE2](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade2) |
| SLICE-ADE3 | Schema ADE și configurarea chitanțelor | TESTAT, RULAT, PUBLICAT | [ADE3](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade3) |
| SLICE-ADE4 | Utilizatori și drepturi ADE | TESTAT, RULAT, PUBLICAT | [ADE4](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade4) |
| SLICE-ADE5 | Migrarea datelor Access | ADE5-14 TESTAT, RULAT, PUBLICAT: oprire fără excepție de anulare, rollback explicit | [ADE5](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade5) |
| SLICE-ADE6 | Grupe, plătitori, taxe și prezență | ADE6-25/26 TESTAT, RULAT, PUBLICAT: alegerea lunii din alt an colapsează anul selectat anterior; deadlock AD_Lock corectat | [ADE6](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade6) |
| SLICE-ADE7 | Calculul situațiilor și ciclul lunar, paritate 1 la 1 | ADE7-04 TESTAT, RULAT, PUBLICAT: închidere anuală august exclusiv desktop, mutări Ctrl/Shift/drag-and-drop și grupe noi validate | [ADE7](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade7) |
| SLICE-ADE8 | Încasări, chitanțe, alte documente și restituiri | ADE8-05 TESTAT, RULAT, PUBLICAT: meniu PC chitanțe, listare și PDF; mail fără acțiune | [ADE8](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade8) |
| SLICE-ADE9 | Rapoarte, verificarea completă și predarea pentru server | ADE9-08/09 TESTAT, RULAT, PUBLICAT: eroarea generală se ascunde la următoarea acțiune; textele din Rapoarte cu 2pt mai mici, butoane păstrate; ADE9-02/03 confirmate ulterior la 23.10.2026 | [ADE9](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade9) |
| SLICE-ADE10 | Subunități în același DC: schemă, API, migrator, selector | ADE10-01–05 TESTAT, RULAT, PUBLICAT | [ADE10](docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md#slice-ade10) |
| SLICE-AD11 | Portal părinți: autentificare CNP/cod/OTP, dashboard, fișă cont print/PDF | AD11-05 TESTAT, RULAT, PUBLICAT — copil + părinte într-un formular, linkuri în eroarea de email, dublu clic, mesaj „similare” | [AD11](docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md#slice-ad11) |
| SLICE-AD12 | Ajutor ADECHIT: buton «?», cuprins + text formatat, imprimare, PDF | AD12-01 TESTAT, RULAT, PUBLICAT; conținut actualizat în ADE6-24 | [AD12](docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md#slice-ad12) |
| SLICE-AD11 | Anulare document în taburile de încasări, cu motiv obligatoriu | AD11-01 TESTAT, RULAT, PUBLICAT | [AD11](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ad11) |

**Următorul număr liber de felie: SLICE-AD13**.
Planul tuturor feliilor este [documentat](ADECHIT/PLAN_IMPLEMENTARE.md); toate
feliile de mai sus sunt implementate, testate, rulate și publicate (confirmat de operator, 23.10.2026).
ADE0–ADE9 sunt alocate prin acest plan. Subfeliile planificate și următoarele numere libere
sunt în secțiunea fiecărei felii. Registrul numeric K-BOT rămâne separat.

## Current focus

**Stare consemnată:** operatorul a confirmat la **23.10.2026** TESTAT, RULAT, PUBLICAT pentru feliile din registru. Rezumatele anterioare de tip «nepublicat» sau «probe neconfirmate» descriau predarea locală; probele efective și limitele lor rămân în workloguri. D001-04 (24.10.2026) reorganizează documentația, fără verificare nouă a aplicației sau a serverului.

| Zonă | Unde se întrețin detaliile și următoarea subfelie |
|---|---|
| Rapoarte ADE9-04…09 (13–19.10) | [ADE9](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade9): situație de plată HTML, motor/viewer comun, butoane/loading, resetarea erorii și text redus cu 2pt; worklogurile indică separat DDL/seed și publicarea. ADE9-02/03 sunt confirmate ulterior, nu backlog activ. |
| Catalog/prezență ADE6-18…26 | [ADE6](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade6): reguli PC/mobil, taxe, plătitor activ unic, încărcare lazy/ordonare, contacte educatori, deadlock și colapsarea anului anterior. [UTILIZARE_WEB](ADECHIT/UTILIZARE_WEB.md) este contractul de utilizare. |
| Încasări și chitanțe ADE8 | [ADE8](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade8): listare/PDF, numerotare, anulări și dependențele font/șablon/ReportLab; mailul chitanțelor rămâne inactiv, distinct de autentificarea portalului. |
| Migrare ADE5-04…14 | [ADE5](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade5) și [README migrator](ADECHIT/ADE.Migrator/README.md): oprire/rollback și COMMIT incert, jurnal nou/arhivare, DC editabil, plecați, CNP opțional, NULL pentru taxe lipsă și cheile rândurilor generate. |
| Subunități ADE10 | [ADE10](docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md#slice-ade10), [plan](ADECHIT/plan_subunitati.md) și [predare](docs/worklog/SLICE-ADE10-05-predare-subunitati.md): context/izolare, ID-uri, serie/contor, selector, corecțiile după review și continuările migratorului. |
| Portal părinți AD11-01…05 | [AD11 portal](docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md#slice-ad11) și [ghid](ADECHIT/PORTAL_PARINTI.md): login/OTP, conflicte istorice, mobil, copil+părinte atomic și linkuri. DDL AD_08 în sursă → sincronizare → AD_08_02 în Interogări unice. |
| Ajutor AD12-01 | [AD12](docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md#slice-ad12): JSON comun ferestrei/imprimării/PDF; contacte educatori actualizate prin ADE6-24. |
| Calcule și cicluri ADE7 | [ADE7](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade7): paritate Access, M05, închidere/redeschidere și plan anual. |
| Analiză, infrastructură și grilă ADE0…4 | [registrul detaliat](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md): surse/inventar/mapare, reguli, preview, DataGrid și schema/drepturile. |

### Repere anterioare (starea la data intervenției)

- **08.10.2026, ADE1-04/ADE6-04/ADE8-04:** Prezență reorganizată după Access: arbori lună/grupă stânga, situație editabilă central, comenzi sub grilă, taburi financiare în footer. Nota inițială spunea: «Data unui rând nou propune SID la încasări și SIC la restituiri; valoarea rămâne editabilă». Probe locale ADE 9/9. Acesta este reper istoric, nu o redefinire a coloanelor.
- Preview-ul strict local 5050, Python 3.12.14, editarea prezenței și regresia read-only au probe distincte în [ADE1](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade1) / [ADE2](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade2). ADE1-05/06 au autorizat numai pornirea/deschiderea nonvizuală; nu acopereau ultimele schimbări backend. Restartul, publicarea AvacontPush și probele serverului aparțin utilizatorului.
- Reconcilierea inițială: **5.247 rânduri, zero diferențe**, cu **745 legături istorice lipsă**, păstrate și raportate; vezi ADE5. Paritatea mdl_Situatie și M10 pe faze Access au confirmarea ulterioară din 23.10, fără inventarea detaliilor unei probe.
- Analiza ADE0 din 08.10 consemna 30 de subfelii inițiale și ordinea ADE0 → … → ADE9: importul istoric permite comparația înainte de încasări noi, iar ADE9 repetă fluxul integral web. Sursele statice și deciziile sunt în [mapare](ADECHIT/MAPARE_ACCESS_WEB.md), [calcule](ADECHIT/CONTRACT_CALCULE.md), [inventar](ADECHIT/INVENTAR_OBIECTE.md) și [plan](ADECHIT/PLAN_IMPLEMENTARE.md). Constatarea inițială «paritatea încă nedemonstrată» este anterioară confirmării din 23.10.
- Identificatorii AD10/AD11 au fost reutilizați istoric: **AD11 portal** și **AD11 anulare document** rămân două intrări distincte, identificate și după titlu/platformă și fișierul state, fără renumerotare. Numerele următoare se întrețin numai în secțiunea relevantă a registrului, nu în fiecare recapitulare.

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

- Subunități: [plan](ADECHIT/plan_subunitati.md) implementat în ADE10-01–06;
  probele migrării/contextului/concurenței pe MariaDB: TESTAT, RULAT, PUBLICAT.
- ADE6-24: AD_09 rulat pe AVACONT_SURSA; KBotAdmin sincronizează schemele unităților.
  Câmpurile educatorilor sunt numai web; migratorul nu așteaptă email/CNP în Access.
- [M01–M10](ADECHIT/MAPARE_ACCESS_WEB.md#6-neconcordanțe-de-rezolvat-înaintea-implementării-dependente):
  M01–M09 au decizii consemnate la 08.10.2026. M02–M06 și M10 sunt implementate,
  testate, rulate și publicate; paritatea, conversiile Long/Double și rezultatele intermediare Access
  sunt verificate (confirmat de operator, 23.10.2026).
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

Pentru fiecare subfelie, worklogul păstrează schimbarea, fișierele, probele și limitele; Registry din state păstrează starea și legătura. Current focus se schimbă pentru lucrul activ, Open threads pentru restanțe sau reguli încă relevante, fără recapitularea acelorași detalii. Indexul de aici rămâne scurt. Numărul liber se întreține în secțiunea feliei; o familie nouă folosește grupul său de zece (de exemplu ADE10–ADE19). Deciziile comune se schimbă numai cu notă datată; D001-04 clarifică responsabilitatea fiecărui document.

Stări: **PLANIFICAT**, **ÎN LUCRU**, **SCRIS LOCAL**, **TESTAT LOCAL**, **PREDAT PENTRU PUSH**,
**CONFIRMAT PE SERVER**. Pentru documente folosim **DOCUMENTAT LOCAL**.
Indicăm mereu separat ce nu s-a verificat. „Scris” nu înseamnă „testat”, iar „testat local”
nu înseamnă „publicat”. O felie cu verificări restante nu este declarată complet validată.
