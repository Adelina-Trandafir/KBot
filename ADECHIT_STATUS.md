# ADECHIT — STATUS și plan de portare

Actualizat: **07.10.2026**. Sursa unică pentru starea proiectului ADECHIT.
Acest fișier spune ce este planificat, ce este scris și ce este verificat.
Dacă realitatea diferă, actualizăm statusul în aceeași intervenție.

Structură după [KBOT_STATUS](docs/worklog/KBOT_STATUS.md) și
[CODE_WORKFLOW](docs/worklog/CODE_WORKFLOW.md), cu numerotarea și regulile
specifice stabilite de utilizator pentru ADECHIT, consemnate mai jos.

## Reguli de lucru și numerotare

- Citim întâi acest fișier, apoi numai secțiunile de stare și sursele necesare feliei.
- **Nu citim și nu modificăm niciun fișier `Claude.md`, indiferent de capitalizare.**
  Nicio trimitere din alte documente nu schimbă această interdicție.
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
| SLICE-ADE0 | Analiză, plan și mapare Access → web | ADE0-01: documentat local; maparea completă neîncepută | [ADE0](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade0) |
| SLICE-ADE1 | Integrarea proiectului, sistemele comune și preview 5050 | PLANIFICAT | [ADE1](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade1) |
| SLICE-ADE2 | DataGrid comun cu editare directă în celule | PLANIFICAT — prioritate critică | [ADE2](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade2) |
| SLICE-ADE3 | Schema ADE și configurarea chitanțelor | PLANIFICAT | [ADE3](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade3) |
| SLICE-ADE4 | Utilizatori și drepturi ADE | PLANIFICAT | [ADE4](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade4) |
| SLICE-ADE5 | Migrarea datelor Access | PLANIFICAT | [ADE5](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade5) |
| SLICE-ADE6 | Grupe, plătitori, taxe și prezență | PLANIFICAT | [ADE6](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade6) |
| SLICE-ADE7 | Calculul situațiilor și ciclul lunar, paritate 1 la 1 | PLANIFICAT — criteriu critic de acceptare | [ADE7](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade7) |
| SLICE-ADE8 | Încasări, chitanțe, alte documente și restituiri | PLANIFICAT | [ADE8](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade8) |
| SLICE-ADE9 | Rapoarte, verificarea completă și predarea pentru server | PLANIFICAT | [ADE9](docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md#slice-ade9) |

**Următorul număr liber de felie: SLICE-ADE10.**
ADE0–ADE9 sunt alocate prin acest plan. Subfeliile planificate și următoarele numere libere
sunt în secțiunea fiecărei felii. Registrul numeric K-BOT rămâne separat.

## Current focus

- **SLICE-ADE0-01:** planul inițial este scris local, numai documentație.
- Următorul pas: **ADE0-02**, maparea exactă a fluxurilor active și a interogărilor din
  `mdl_Situatie`; apoi **ADE1-01** pentru integrare și **ADE2** pentru editarea grilei.
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
3. Refolosim baza deja creată. Tabelele noi de business au prefixul **`ADE_`**.
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
   Semantica numărului curent/următor se stabilește din fluxul activ înainte de import.
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

## Open threads — transversale

- Exportul Access include structuri și cod, nu rândurile necesare migrării și probelor.
  Conținutul din `SQL`, `CFGs` și alte configurări folosite dinamic trebuie inventariat.
- Baza țintă, unitățile, rolurile exacte și eventualele tabele comune accesibile ADE
  se vor stabili înainte de scripturile de instalare finale; nu blocăm preview-ul local.
- `schema_sync` propune ștergerea tabelelor absente din schema sursă. În ADE3 trebuie
  definită integrarea `ADE_` în șablon/filtrare, înainte ca utilizatorul să ruleze sync.
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
