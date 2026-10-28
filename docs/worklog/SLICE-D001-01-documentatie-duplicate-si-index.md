# Istoricul curățării documentare D001-01…06

Reunire documentară SLICE-D003-23: etapele, bilanțurile, inventarele și limitele sunt cele consemnate la data lor. Rubricile administrative sunt marcate în paragrafe; ancorele fiecărei etape păstrează navigarea.

## SLICE-D001-01 — Documentație: duplicate și index <a id="slice-d001-01"></a>

Data: 24.10.2026. Familia documentară `SLICE-Dnnn-nn`, stabilită de utilizator.

<a id="slice-d001-01-what-changed-and-why"></a><a id="what-changed-and-why"></a>**What changed and why**

- În `implementare_vercon_v2.md`, șase copii integrale ale textelor VC06-18, VC03-07/08/09/10 și VC05-11 sunt înlocuite cu linkuri la worklogurile existente. Titlurile secțiunilor și destinațiile sunt păstrate; worklogurile conțin detaliile, limitele și stările istorice originale. Reducere: 232 de rânduri.

- În jurnalul KE01-02 este eliminată introducerea copiată accidental în fraza despre validarea codului de angajament. Fraza este refăcută cu expresia `^[A-Za-z0-9_./-]{1,60}$` din `PYTHON/routes/admin_cli/admin_cli.py`. Restul jurnalului și limitele sale istorice sunt păstrate. Reducere: 22 de rânduri.

- README-ul generic devine un index de documentație. Cerințele specifice noutăților sunt păstrate, iar formatul este delegat ghidului existent, fără redefinirea lui. Bilanțul lotului: 227 de rânduri mai puțin, incluzând cele două fișiere noi necesare urmăririi familiei D (registrul și acest jurnal).

<a id="slice-d001-01-files-touched"></a><a id="files-touched"></a>**Files touched** — `README.md`; `implementare_vercon_v2.md`; `docs/worklog/SLICE-KE01-02-fisa-angajament.md`; `docs/worklog/KBOT_STATUS.md`; registrul `state/KBOT_STATUS_D000-D009.md`; acest jurnal.

<a id="slice-d001-01-test-results"></a><a id="test-results"></a>**Test results** — Numai analiză de text și de proiectare: cele șase corpuri retrase au fost comparate integral cu destinațiile, cu normalizarea sfârșiturilor de rând. Nu s-au rulat teste, builduri, aplicații, verificatoare de ajutor, comenzi Git sau SQL. Nu se schimbă starea validării vreunei funcționalități.

<a id="slice-d001-01-unverified--deferred"></a><a id="unverified--deferred"></a>**Unverified / deferred**

- Modificările sunt locale, fără commit sau push.

- Reorganizarea completă continuă cu indexurile/statusurile și cu informația unică din jurnalul general VERCON; acest lot nu declară încheiată curățarea proiectului.

- Fișierele AVACONT, documentația activă AV/AVACONT și orice CLAUDE.md sau variantă istorică CLAUDE_*.md sunt excluse. Ajutorul runtime, scripturile și numele worklogurilor existente nu sunt modificate.

- Comasarea jurnalelor dependente de nume și schimbarea regulilor de întreținere rămân pentru loturi distincte.

## SLICE-D001-02 — Indexul de status și rapoartele VERCON <a id="slice-d001-02"></a>

Data: 24.10.2026. Continuarea curățării documentației D001. Bilanț: 41 de rânduri net eliminate, incluzând acest jurnal nou și actualizarea registrului.

<a id="slice-d001-02-what-changed-and-why"></a>**What changed and why**

- Rândurile 0000, 000T, 00EF și KE din indexul global sunt reduse la obiectiv, stare și trimiteri. Detaliile pe subfelii rămân în registrele și worklogurile existente; reperele 00EF-12/16 au fost citite și în jurnalele lor. Rândurile AV/AVACONT nu sunt modificate.
- Reperele Current focus din 28.09, 01.10 și 13.10 sunt condensate și datate explicit, cu păstrarea faptelor, limitelor și linkurilor. Nu sunt tratate ca validări actuale.
- Șase descrieri din jurnalul general VERCON sunt înlocuite cu trimiteri la ADE9-05/06/07 și VC08-01/02/03. Titlurile existente și stările istorice rămân. Textul distinct despre reorganizarea CSS este păstrat în jurnalul general.
- Clauzele care nu erau explicite în destinații sunt transferate: antetul/meniul VERCON în ADE9-05; modulele rapoartelor, legătura KBotAdmin și fișierele extracției Excel în VC08-03. API-ul comun rămâne în `PYTHON/reporting/README.md`.

<a id="slice-d001-02-files-touched"></a>**Files touched** — `docs/worklog/KBOT_STATUS.md`; `implementare_vercon_v2.md`; worklogurile ADE9-05 și VC08-03; registrul `state/KBOT_STATUS_D000-D009.md`; acest jurnal.

<a id="slice-d001-02-test-results"></a>**Test results** — Numai citire și analiză de proiectare a textelor sursă/destinație. Nu s-au rulat teste, builduri, verificatoare de ajutor, aplicații, Git, SQL sau acces MariaDB. Nu se modifică stările de probare/publicare ale funcționalităților.

<a id="slice-d001-02-unverified--deferred"></a>**Unverified / deferred** — Modificările sunt locale, fără commit/push. Restul jurnalului general, statusurile/modulele rămase și comasarea fișierelor se continuă separat. AVACONT, documentația activă AV/AVACONT, fișierele CLAUDE și ajutorul runtime rămân excluse.

## SLICE-D001-03 — Absorbția planurilor redundante și a predărilor istorice <a id="slice-d001-03"></a>

Data: 24.10.2026. Bilanț: 4 fișiere retrase, 1 jurnal nou; net 3 fișiere și 321 de rânduri eliminate. Reorganizare documentară; afirmațiile istorice rămân datate, fără certificare nouă a codului sau a serverului.

<a id="slice-d001-03-what-changed-and-why"></a>**What changed and why**

| Fișier retras | Informația păstrată și destinația |
|---|---|
| `docs/KBOT_Tier1_Plan.md` | Redirecționarea este înlocuită de planul [Tier 1](KBOT_Tier1_Plan.md); nu exista un contract distinct în copie. |
| `docs/PLAN_MainForm_Scaffolding.md` | Constructorul cu cinci parametri și trecerea de la șase la zece vederi sunt consemnate în [planul MainForm](PLAN_MainForm_Scaffolding.md), împreună cu referința la 0008. |
| `docs/HANDOFF_TreeDataApi.md` | Bilanțul Tier1 și capcana config-ului fictiv sunt în planul Tier1; livrarea MainForm este în planul MainForm; contractul compus al arborelui și corectarea afirmațiilor greșite sunt în [0008](SLICE-0008-tree-data-api.md) și [planul său](PLAN_TreeDataApi.md). |
| `status_migrare6.md` | Logica pașilor, abaterile, probele și limitele sunt deja în jurnalele 0020-01…05. Commiturile și reperul rezumatului sunt în [starea 0020](state/KBOT_STATUS_0020-0029.md#slice-0020); condițiile semnării amânate sunt păstrate în [0020-05](SLICE-0020-01-ddf-endpoint.md#slice-0020-05). |

Trimiterile de navigare folosesc acum planurile din worklog. Referințele la căile retrase din tabelul istoric Files touched al lui 0008 sunt păstrate ca evidență a intervenției din 2026, cu o notă explicită că nu sunt destinații curente. Nu se copiază în contractul actual afirmațiile respinse despre Salarii, IDORD, scoping sau CODPARTENER. Sursele originale rămase explică de ce au fost respinse.

<a id="slice-d001-03-files-touched"></a>**Files touched** — Planurile Tier1/MainForm/TreeDataApi și jurnalul 0008; stările 0000–0009 și 0020–0029; jurnalul 0020-05; indexul global și registrul D; acest jurnal. Cele patru fișiere din tabel sunt eliminate după transfer.

<a id="slice-d001-03-test-results"></a>**Test results** — Numai citire, căutarea trimiterilor și analiza documentară sursă/destinație. Nu s-au rulat teste, builduri, verificatoare, aplicații, Git, SQL sau acces MariaDB. Rezultatele de test din documentele absorbite sunt istorice; nu au fost rerulate.

<a id="slice-d001-03-unverified--deferred"></a>**Unverified / deferred** — Modificări locale, fără commit/push. AVACONT, documentația activă AV/AVACONT, fișierele CLAUDE și ajutorul runtime rămân excluse. Navigarea și mecanismele runtime nu au fost executate. Restul consolidării continuă în D001-04.

## SLICE-D001-04 — Consolidare extinsă a documentației <a id="slice-d001-04"></a>

Data: 24.10.2026. Intervenție editorială locală, în mai multe treceri; nu certifică implementarea sau serverul.

<a id="slice-d001-04-what-changed-and-why"></a>**What changed and why** — Lotul cuprinde 21 de treceri: jurnale VERCON/Python, statusuri ADECHIT/VERCON/0048, predări DDF/migrator, contracte și matrici de probe. Fiecare absorbție păstrează informația distinctă, proveniența și starea la data ei; explicațiile repetitive rămân într-un singur loc.

### Prima trecere — jurnalul VERCON și punctul de reluare ADECHIT

- Jurnalul general VERCON păstrează titlurile/ancorele, iar 19 secțiuni din 23.10 trimit la cele 21 de workloguri existente. Detaliile tehnice și stările nu mai au două copii.
- În VC08-11 se păstrează motivul corecției MAGAZIE; în VC06-11 se păstrează limita istorică a corecției criteriilor CUI/data, fără DDL sau probe noi.
- `ADECHIT/STARE_LA_PAUZA.md` este absorbit în [ADE6-24](SLICE-ADE6-24-educatori-email-cnp.md), [ADE6-25](SLICE-ADE6-25-deadlock-lock-row.md) și starea ADE6; fișierul vechi este retras. Ordinea AD_09 → KBotAdmin → publicare/restart → probe, lipsa remigrării contactelor, deadlockul, excepția emailurilor și acceptarea din 23.10 sunt păstrate. Legăturile de navigare sunt actualizate; căile din listele istorice de fișiere atinse rămân ca proveniență.
- `PYTHON/AGENTS.md` cere worklog + status și numai trimitere în indexul istoric, pentru a preveni reapariția descrierilor duplicate. Se păstrează obligația de trasabilitate după fiecare pagină/slice și limitele verificărilor.

### Corespondența secțiunilor VERCON

- [VC08-11](SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md); [VC08-12](SLICE-VC08-12-antet-comun-bonuri.md); [VC08-13](SLICE-VC08-13-corectii-balanta-oi-bonuri-primitori.md); [VC08-14](SLICE-VC08-14-generare-http-500-parametri.md)
- [VC07-06](SLICE-VC07-06-copii-liste-ultima-luna.md); [VC06-09](SLICE-VC06-09-bc-luna-activa-stoc-si-istoric.md); [VC07-07](SLICE-VC07-07-lista-butoane-actiuni.md); VC08-15 / VC06-10 → [VC08-15](SLICE-VC08-15-tooltip-arbori.md); [VC06-10](SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md#slice-vc06-10)
- [VC08-16](SLICE-VC08-16-ordonare-tipuri-si-preturi-istorice.md); [VC08-17](SLICE-VC08-17-fisa-magazie-selectie-multipla.md); [VC06-11](SLICE-VC06-11-efactura-asociere-nir-partiala.md); [VC06-12](SLICE-VC06-12-nir-popup-asociere-si-grupare-furnizori.md)
- VC06-13 / VC07-08 → [VC06-13](SLICE-VC06-11-efactura-asociere-nir-partiala.md#slice-vc06-13); [VC07-08](SLICE-VC07-07-lista-butoane-actiuni.md#slice-vc07-08); [VC06-14](SLICE-VC06-11-efactura-asociere-nir-partiala.md#slice-vc06-14); [VC08-18](SLICE-VC08-18-status-intre-actiuni.md); [VC06-15](SLICE-VC06-15-efactura-informare-inactive.md)
- [VC06-16](SLICE-VC06-11-efactura-asociere-nir-partiala.md#slice-vc06-16); [VC06-17](SLICE-VC06-11-efactura-asociere-nir-partiala.md#slice-vc06-17); [VC08-19](SLICE-VC08-19-mesaje-dialog-comun.md)

### A doua trecere — intervenții din 19 și 21 octombrie

Pentru fiecare intrare de mai jos, detaliile sunt păstrate în worklog; titlul jurnalului general rămâne.

- [ME01-03](SLICE-ME01-03-privilegii-mariadb.md); [VC08-10](SLICE-VC08-10-zoom-meniu-vercon-adechit.md); [VC08-06](SLICE-VC08-06-nir-valoare-factura-numpad.md); [VC08-07](SLICE-VC08-07-antet-comun-rapoarte-fara-dc.md); [VC08-08](SLICE-VC08-08-meniu-aspect-adechit.md); [VC08-09](SLICE-VC08-09-arbori-documente-doua-niveluri-lazy.md)
- [VC09-01](SLICE-VC09-01-ajutor-vercon-si-chat.md); [VC09-02](SLICE-VC09-02-capturi-ajutor.md); [VC09-03](SLICE-VC09-03-simplificare-gestionari-rapoarte-copaci.md); [VC09-04](SLICE-VC09-03-simplificare-gestionari-rapoarte-copaci.md#slice-vc09-04); [VC06-07](SLICE-VC06-07-nir-din-efactura-lista-si-completare.md) / [VC06-08](SLICE-VC06-07-nir-din-efactura-lista-si-completare.md#slice-vc06-08); [VC09-05](SLICE-VC09-05-reglare-registru-oi-si-produs-la-migrare.md)

Confirmările ulterioare din 23.10 pentru VC09-05 și VC06-07/08 sunt transferate în worklogurile lor, separat de probele locale din 21.10; cazurile amânate nu sunt declarate individual rezolvate prin deducție. VC09-04 păstrează excepția ACTUALIZAT LOCAL, NEPUBLICAT. Reperul limitei de ajustare NIR din ajutor este păstrat în VC09-01.

### A treia trecere — unificarea jurnalelor secundare Python

- Nouă fișiere `PYTHON/docs/worklog/SLICE-CHAT-01…09-*.md` sunt absorbite în [PROIECT chat web](../../PYTHON/docs/chat/PROIECT.md): contract existent, completări unice și ancore istorice pentru fiecare identificator; fișierele vechi sunt retrase. Rămân limitele inițiale, conversia emoji manuală, diagnosticul seed-ului, pollingul 20 → 60 s, detaliile gesturilor și compactarea după a doua captură.
- Patru jurnale fără identificator (`ADECHIT-cautare-copii-in-luna`, `UI-tooltip-font`, `UI-tooltip-noduri-doar-text-trunchiat`, `VERCON-meniu-intrari-efactura`) sunt reunite, cu textul tehnic distinct, în [istoricul web Python](../../PYTHON/docs/worklog/INTERVENTII_WEB_2026-10.md). Doar metadatele comune sunt deduplicate; toate căile rămân relative la PYTHON.
- Indexul și starea CHAT, plus jurnalul desktop CHAT-08, indică sursa nouă și disting explicit reutilizarea numerelor CHAT-08/09 web vs desktop. Ajutorul runtime și toolurile care caută jurnalele desktop nu sunt modificate.

### A patra trecere — predarea DDF 0081

`docs/HANDOFF_0081_DDF_Trimitere.md` este absorbit în [PLAN_DDF_Trimitere](../PLAN_DDF_Trimitere.md#handoff-notes-absorbed-in-d001-04) și retras. Cele 15 decizii sunt în plan/fundament; clauzele unice de predare, sursele A1.0.08, restricțiile și secvența inițială sunt păstrate în nota datată. Planul este identificat drept istoric și trimite explicit la corecțiile adoptate din 0081-01 (`StareTrimitere`, `X-Semnatura: -`), fără a transforma presupunerile vechi în contract curent. Navigarea din 0081-01 și status este actualizată; lista surselor citite la 0000-03 rămâne evidență istorică.

### A cincea trecere — familia vizuală 0048-05…09

Cele cinci jurnale de grafic/benzi sunt consolidate în [0048-05](SLICE-0048-05-graficul-evolutiei.md), cu ancore explicite `slice-0048-05`…`slice-0048-09`. Fișierele 0048-06…09 sunt retrase; probele, fișierele atinse și limitele originale rămân în secțiunea intervenției. Numai descrierile tehnice repetitive sunt condensate, cu API-ul comun trimis la documentele Chart/Lane/C1–C9. Rămân deosebite treptele, culorile și excluderea roșului, contrastul, Tag/selectarea arborelui, obiectele-reper separate, domeniul F13/F14/F15/F16, hazardul F30, reversibilitatea, lipsa Undo/plasării în masă și calculul totalului pe plată.

Cele cinci celule Notes din registrul 0048 nu mai repetă jurnalele; stările/probele din coloana Status rămân istorice. HelpCheck citește pentru identificatorii numerici și rândul principal 0048 din index, deci nu cere păstrarea celor patru nume; nu a fost rulat. Nu s-au schimbat tooluri sau ajutor. ReleaseNotes selectează fișiere după modificare: reorganizarea editorială nu este o noutate de produs și nu a fost publicată.

### A șasea trecere — etapele NIR din 16 octombrie

23 secțiuni din jurnalul general trimit la istoricul absorbit în [VC06-06](SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#etapele-din-jurnalul-general-absorbite-prin-d001-04--16102026). Sunt păstrate variantele retrase, metadatele AUTO, reviziile structurii/importului, stagingul cerut atunci și resetul demo manual, corecțiile UI/DGV/ComboTree, lista fișierelor și probele precise din pagina izolată. Aceste probe istorice nu validează NIR pe date reale și nu sunt rerulate acum; oprirea ulterioară a serverului 5052 rămâne explicită.

### A șaptea trecere — indexul ADECHIT

Current focus devine navigare tematică spre registre/workloguri, fără cronologia integrală repetată. Se păstrează reperele unice (9/9 local, Python/preview, 5247/745, cele 30 de subfelii inițiale, limitele autorizării nonvizuale) și separarea AD11 portal/anulare. Confirmarea din 23.10 este datată; eticheta veche ADE9-02/03 planificate este înlocuită prin confirmarea deja existentă în registrul detaliat și punctul de reluare absorbit. Procedura de întreținere cere detalii o singură dată în worklog și numai stare/trimitere în registre.

### A opta trecere — registrul OI/MF și documentele orfane

Unsprezece intrări generale sunt consolidate în [registrul OI/MF](../../VERCON/REGISTRU_OI_MF.md), README VRC, worklogul OI și VC06-04. Diagnosticul păstrează contextul exact și cele trei momente (cauză necunoscută → mecanism demonstrat în rezultatele operatorului → eroare Access confirmată), separat de rezultatul de design nerulat. Criteriile BR/storno NERULATE au fost mutate integral în VC06-04 pentru a evita o trimitere circulară; regulile de lot/versiune/audit, datele brute și limitele reparațiilor istorice sunt păstrate.

### A noua trecere — predarea migrator/online din 18 octombrie

`VERCON/CODEX_SCHIMBARI_MIGRATOR_2026-10-18.md` este retras după absorbția în VC06-04, REGISTRU_OI_MF și README VRC. Rămân exemplul celor trei retururi, metadatele N12, diferența față de noua politică N15, contextul multi-subunitate/ZEC, semnificația conturilor și restricțiile predării. Lista de cerințe nu mai apare ca backlog concurent după implementarea consemnată, iar limitările și probele nerulate sunt păstrate în worklog.

### A zecea trecere — catalogul probelor VERCON

PROBE_BROWSER și PROBE_VC07 sunt absorbite în [PROBE_ACCES](../../VERCON/PROBE_ACCES.md), cu ancore permanente probe-acces/probe-browser/probe-vc07; două fișiere sunt retrase. Rândurile scenariilor și rezultatele sunt păstrate integral, inclusiv accesul PARȚIAL EXECUTAT și matricile PLANIFICAT/NEEXECUTAT, porturile de preview și limitele roluri/MariaDB. Este reducere a fragmentării, nu o pretinsă deduplicare a unor probe diferite. Legăturile active din status, indexurile mockupurilor și workloguri indică secțiunile noi. Nu s-a rulat nicio probă.

### A unsprezecea trecere — analiza NIR

Analiza NIR este păstrată ca fișier distinct pentru dovezile Access: secțiunile cu sursele, formulele, defectele și snapshotul web rămân. Sunt condensate numai propunerile repetitive de validare/repartizare/migrare, păstrând condițiile, excepțiile MF, țintele distincte și proveniența. Formula inversă, exemplul numeric, regulile AUTO, cronologia și TVA derivat rămân în textul original. Macheta separată/NIR per cont sunt marcate istoric respinse și trimit la contractul adoptat.

### A douăsprezecea trecere — planul de înregistrare/provizionare

PLAN_AutoProvisioning păstrează starea confirmată la 23.09 separat de predarea anterioară «nerulat». Nu se inventează probe ale tuturor căilor undo sau ale checklistului: numai confirmările consemnate sunt marcate. Nota repetată de livrare UI devine scurtă și trimite la F2a/D21/D29 și README. Raportul inițial «toate conturile SU» este explicit înlocuit de citirea grants din §13.1, cu procedura de intervenție și dependența SQL SECURITY DEFINER păstrate. SQL-ul și codul nu sunt modificate.

### A treisprezecea trecere — decizii închise și titlurile planurilor

DECIZII_DESCHISE ADECHIT păstrează toate M01–M10/D01–D10, dar confirmarea identică este consemnată o singură dată pentru registru; regulile UI repetate trimit la ghidul existent, cu excepțiile și data adoptării păstrate. Titlul și rubricile indică starea închisă și contextul istoric. Titlurile KE/EV nu mai afirmă «nimic implementat» deasupra propriilor actualizări de implementare; corpul și limitările lor nu sunt schimbate.

### Pasul 14 — Tooltip comun

Cele trei intrări din 20 octombrie trimit la docs/web-tooltip.md. Contractul tehnic exista deja acolo; am păstrat într-un istoric comun fișierele fiecărei etape, trecerea de la antet activ la opțional și normalizarea preferințelor vechi. Extensia de font din 21 octombrie rămâne distinctă, cu propriile limite; confirmarea operatorului nu este extrapolată. Titlurile jurnalului rămân pentru navigare.

### Pasul 15 — Resurse și machete de raport

Șapte intrări din 17 octombrie trimit la README și istoricul VC08-03. Sunt păstrate diagnosticul metadatelor/aliasurilor, fișierele și pașii de livrare, precedența CSS, etapele orizontal → portret și FCS pozitiv → semn negativ numai la afișare, limitele probelor și sursele fonturilor. Nu se transformă implementarea locală în publicare confirmată.

### Pasul 16 — Registrul 0048-03 și completarea

Două celule Notes repetau jurnalele și două Open threads repetau limitele, inclusiv întrebări retrase. Registrul trimite acum la explicații/probe, cu excepțiile încă relevante păstrate. Consecința unică asupra rutei de citire este transferată în completare. Jurnalul inițial primește avertisment datat de evoluție și etichetă explicită pe interogarea F20 retrasă; F13 nu mai poate fi citit drept veto actual. Cele două jurnale rămân separate și toate rezultatele istorice rămân.

### Pasul 17 — Statusul VERCON

Șase descrieri istorice devin stare datată și trimiteri la contracte/workloguri; regulile tehnice nu mai au o copie concurentă în index. VC09-05 reflectă confirmarea din 23.10 deja păstrată în worklog, separat de probele locale; limita globală din 12.10 este etichetată drept istorică. Criteriile VC06-04 indică acum locul efectiv, nu vechiul jurnal general. Confirmarea identică a rândurilor din tabel este definită o singură dată, fără schimbarea excepțiilor DOCUMENTAT/NEPUBLICAT.

### Pasul 18 — Current focus 0048

Cele trei reluări 0048-07/08/09 sunt înlocuite cu navigare la secțiunile jurnalului deja consolidat. Corespondența păstrează acolo toate lecțiile: conținut mutabil versus identitate, contrast WCAG, randarea controlului, DataR editabil și comparația pe zi, retragerea vetoului pe ambele căi, F30 și calculul unic al totalului plății. Stările istorice și probele nu sunt rescrise.

### Pasul 19 — Amendamentele planului FOREXE

D1/D3/D4 exprimă direct contractul adoptat în două faze, cu motivul și excepția ignorat păstrate, în locul premisei urmate de repetarea corecției. Distincția scrieri tranzacționale versus persistență rămâne explicită. Introducerea și exemplul API vechi sunt identificate drept istorice, cu trimiteri la jurnalele implementării; nu sunt inventate corpuri noi de API. Restul planului, sursele și deciziile neatinse rămân.

### Pasul 20 — Confirmările identice ale jurnalului general

Cele 36 de rânduri cu aceeași formulare de confirmare folosesc un marcaj definit o singură dată la început. Domeniul rămâne strict intrarea marcată; nu sunt confirmate alte intervenții sau cazuri individuale. Sunt eliminate și cele șapte separări cu linii goale multiple, fără schimbarea titlurilor/ancorelor.

### Pasul 21 — Convenția formularelor

Secțiunea de theming trimite la C1/C5 pentru controalele custom. Modelul vechi LoginForm/KBotTheme este condensat și identificat istoric, păstrând proprietățile stock, helperii și ordinea apelurilor. Regulile Designer, layout și DPI rămân nemodificate; nu sunt tratate ca redundante doar pentru că există convenții comune.

<a id="slice-d001-04-files-touched"></a>**Files touched** — Inventar al întregului lot; rândurile sunt calculate față de începutul D001-04, incluzând rândurile goale interne. Minus în coloana Diferență înseamnă reducere. Fișierele retrase au destinațiile explicate în trecerile de mai sus; listele istorice de fișiere atinse rămân ca proveniență.

| Fișier | Operație | Diferență rânduri |
|---|---|---:|
| implementare_vercon_v2.md | Actualizat | -1696 |
| docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md | Actualizat | 2 |
| docs/worklog/SLICE-VC06-11-efactura-asociere-nir-partiala.md | Actualizat | 4 |
| ADECHIT/STARE_LA_PAUZA.md | Absorbit și retras | -70 |
| ADECHIT_STATUS.md | Actualizat | -201 |
| docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md | Actualizat | 0 |
| docs/worklog/SLICE-ADE6-24-educatori-email-cnp.md | Actualizat | 8 |
| PYTHON/AGENTS.md | Actualizat | 0 |
| docs/worklog/state/KBOT_STATUS_D000-D009.md | Actualizat | 1 |
| docs/worklog/KBOT_STATUS.md | Actualizat | 0 |
| docs/worklog/SLICE-VC09-05-reglare-registru-oi-si-produs-la-migrare.md | Actualizat | 4 |
| docs/worklog/SLICE-VC06-07-nir-din-efactura-lista-si-completare.md | Actualizat | 4 |
| docs/worklog/SLICE-VC06-08-nir-din-efactura-inchiderea-trimiterii.md | Actualizat | 4 |
| docs/worklog/SLICE-VC09-01-ajutor-vercon-si-chat.md | Actualizat | 2 |
| PYTHON/docs/chat/PROIECT.md | Actualizat | 52 |
| PYTHON/docs/worklog/INTERVENTII_WEB_2026-10.md | Creat | 94 |
| PYTHON/docs/worklog/ADECHIT-cautare-copii-in-luna.md | Absorbit și retras | -27 |
| PYTHON/docs/worklog/SLICE-CHAT-01-model-date.md | Absorbit și retras | -12 |
| PYTHON/docs/worklog/SLICE-CHAT-02-api.md | Absorbit și retras | -16 |
| PYTHON/docs/worklog/SLICE-CHAT-03-componenta.md | Absorbit și retras | -18 |
| PYTHON/docs/worklog/SLICE-CHAT-04-integrare.md | Absorbit și retras | -17 |
| PYTHON/docs/worklog/SLICE-CHAT-05-codare-emoji.md | Absorbit și retras | -9 |
| PYTHON/docs/worklog/SLICE-CHAT-06-diagnostic-503.md | Absorbit și retras | -13 |
| PYTHON/docs/worklog/SLICE-CHAT-07-comportament-si-antet.md | Absorbit și retras | -20 |
| PYTHON/docs/worklog/SLICE-CHAT-08-bula-mobila.md | Absorbit și retras | -23 |
| PYTHON/docs/worklog/SLICE-CHAT-09-mesaje-compacte.md | Absorbit și retras | -14 |
| PYTHON/docs/worklog/UI-tooltip-font.md | Absorbit și retras | -24 |
| PYTHON/docs/worklog/UI-tooltip-noduri-doar-text-trunchiat.md | Absorbit și retras | -27 |
| PYTHON/docs/worklog/VERCON-meniu-intrari-efactura.md | Absorbit și retras | -14 |
| docs/worklog/state/KBOT_STATUS_CHAT.md | Actualizat | 0 |
| docs/worklog/SLICE-CHAT-08-robot-si-balon.md | Actualizat | 0 |
| docs/HANDOFF_0081_DDF_Trimitere.md | Absorbit și retras | -89 |
| docs/PLAN_DDF_Trimitere.md | Actualizat | 10 |
| docs/worklog/SLICE-0081-01-starea-reviziei.md | Actualizat | 0 |
| docs/worklog/state/KBOT_STATUS_0080-0089.md | Actualizat | 0 |
| docs/worklog/SLICE-0048-05-graficul-evolutiei.md | Actualizat | 607 |
| docs/worklog/SLICE-0048-06-culorile-graficului-si-clic-pe-punct.md | Absorbit și retras | -210 |
| docs/worklog/SLICE-0048-07-benzi-si-retragerea-f13.md | Absorbit și retras | -390 |
| docs/worklog/SLICE-0048-08-benzi-lizibile-si-doua-vederi.md | Absorbit și retras | -234 |
| docs/worklog/SLICE-0048-09-total-receptii-pe-reperul-platii.md | Absorbit și retras | -118 |
| docs/worklog/state/KBOT_STATUS_0040-0049.md | Actualizat | -91 |
| docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md | Actualizat | 66 |
| VERCON/REGISTRU_OI_MF.md | Actualizat | 19 |
| docs/worklog/SLICE-VC06-04-br-si-fcs.md | Actualizat | 25 |
| VERCON/VRC.Migrator/README.md | Actualizat | 10 |
| docs/worklog/SLICE-VC07-OI-conturi-documente-rapoarte.md | Actualizat | 2 |
| VERCON/CODEX_SCHIMBARI_MIGRATOR_2026-10-18.md | Absorbit și retras | -116 |
| VERCON/PROBE_ACCES.md | Actualizat | 121 |
| VERCON/PROBE_BROWSER.md | Absorbit și retras | -81 |
| VERCON/PROBE_VC07.md | Absorbit și retras | -26 |
| VERCON/MOCKUPURI/2026-10-12-corectate/INDEX.md | Actualizat | 0 |
| VERCON/MOCKUPURI/2026-10-12-corectate-v2/INDEX.md | Actualizat | 0 |
| VERCON/VERCON_STATUS.md | Actualizat | -36 |
| docs/worklog/SLICE-VC02-05-probe-browser-si-ajutor.md | Actualizat | 0 |
| docs/worklog/SLICE-VC07-05-verificare-end-to-end-pe-tipuri.md | Actualizat | 0 |
| VERCON/ANALIZA_FLUX_NIR_2026-10-16.md | Actualizat | -132 |
| docs/PLAN_AutoProvisioning.md | Actualizat | -40 |
| ADECHIT/DECIZII_DESCHISE.md | Actualizat | -22 |
| docs/KE_PLAN.md | Actualizat | 0 |
| docs/EV_PLAN.md | Actualizat | 0 |
| docs/web-tooltip.md | Actualizat | 10 |
| docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md | Actualizat | 23 |
| docs/worklog/SLICE-0048-03-pasii-3-8-si-contractul-in-doua-faze.md | Actualizat | 4 |
| docs/worklog/SLICE-0048-03-completare.md | Actualizat | 4 |
| docs/PLAN_ForexeIngest.md | Actualizat | -42 |
| docs/kbot-forms-ui-convention.md | Actualizat | -9 |
| docs/worklog/SLICE-D001-04-consolidare-extinsa-documentatie.md | Creat; jurnalul prezent | Inclus în bilanț |

Bilanț D001-04: 67 căi atinse; 20 fișiere și 2539 rânduri mai puține net. Cumulat D001-01…04: 3128 rânduri și 20 fișiere mai puține. Nu este doar mutare într-o arhivă; conținutul duplicat este înlocuit efectiv cu trimiteri.

<a id="slice-d001-04-test-results"></a>**Test results** — Numai lectură, căutarea referințelor și analiză documentară sursă → destinație. Fără teste, builduri, verificatoare, aplicații, SQL, MariaDB sau Git. Confirmările istorice ale operatorului nu reprezintă verificări efectuate acum.

<a id="slice-d001-04-unverified--deferred"></a>**Unverified / deferred** — Lot închis la 9% rămas din limita de 5 ore, conform pragului utilizatorului. Modificări locale, fără commit/push. AVACONT, documentația activă AV/AVACONT, orice CLAUDE.md și variantele sale istorice rămân excluse. Nu s-au modificat ajutorul runtime sau sursele importate; navigarea nu a fost executată. Restul consolidării rămâne pentru SLICE-D001-05; exporturile de sursă, ajutorul funcțional și documentele cu dependențe de nume cer mapare separată înaintea unei eventuale absorbții. Inventarul inițial este o fotografie anterioară modificărilor, nu un contor curent.

Priorități pentru continuare: contractul NIR față de FLUXURI_UI/SCHEMA_VC (fără pierderea cardinalităților), restul jurnalului general VERCON cu informație unică, familiile de worklog închise fără dependențe de nume și convențiile comune de controale. Nu se elimină automat fișierele doar pentru că figurează în inventarul inițial.

## SLICE-D001-05 — Contracte NIR/UI, nomenclatoare, rapoarte și istoricul Adobe <a id="slice-d001-05"></a>

Data: 24–25.10.2026. Continuarea loturilor documentare D001, aplicată local. Nu schimbă comportamentul aplicațiilor sau starea probării lor.

<a id="slice-d001-05-what-changed-and-why"></a>**What changed and why**

1. Comportamentul editorului NIR este întreținut în `VERCON/FACTURI_NIR_IMPLEMENTARE.md`; FLUXURI_UI, SCHEMA_VC, CONTRACT_CALCULE și rezumatul VC06-06 trimit la acesta și la deciziile D01–D11. Lanțul tehnic de cont, antetul NULL/multicont, metadatele facturii, focusul popupului, dependențele/stocul și limitele migrării rămân explicite. N01/N02 din 12.10 este delimitat de egalitatea exactă pentru NIR adoptată la 16.10. Istoricul corecțiilor UI și probele izolate nu devin probe ale întregului editor.
2. Nouă workloguri Adobe: șapte (0078-07/08/09/11/12/13/14) sunt absorbite în `SLICE-0078-05-fereastra-gazduita-ctrl-h.md`, cu ancore `slice-0078-NN`. Corpurile, datele, listele de fișiere, probele și limitele sunt păstrate, cu ajustarea nivelurilor titlurilor. În plus, 0078-06 intră în 0078-04 (semnarea A/B), iar 0078-16 în 0078-03 (diagnostic ActiveX). Registrul 0070–0079 devine navigabil spre secțiuni și nu mai copiază mecanismele.
3. Istoricul distinge «Da» inițial / «Nu» final la închidere, defectul confirmării constatat în 0078-12 / modificarea ulterioară nerulată din 13, monitorizarea <2024 / recomandarea de versiune >=2025. Un titlu duplicat `Slice 0078` este eliminat. Jurnalele semnării și rescrierii ActiveX rămân separate; comentariile sursă care depind de 0078-15 nu sunt afectate.

4. Opt intrări despre Menu devin trimiteri la contractul comun: dimensiuni, animații și cleanup, tranziția de la 180/320 la 420 ms, excepția explicită reduced-motion, minime pe buton și deplasarea între dropdownuri. Banda, chenarul și umbra aveau deja contract în CONTROALE_CUSTOM_THREAD.
5. Douăsprezece etape din 15.10 despre nomenclatoare/Produse/CPV și ActionAddButton sunt consolidate în VC06-01. Rămân succesiunile adoptat/retras (istoricul de prețuri, pagina CPV → modal, salvare pe celulă → rând, padding stânga → dreapta), validările/importul TIP, schema și cache-ul CPV, contextul/stocurile, filtrele și fișierele pe etapă. Contractele DGV/ModalFrame/ActionAddButton sunt referite, nu recopiate.

6. Opt etape MF/perioade din 20.10 sunt în VC08-11: selecții și identități compuse, deduplicarea variantei la amortizare, mișcări/date efective, ani/luni și fallbackul absent versus null, regenerare peste PDF cu păstrarea parametrilor/raportului anterior, minWidth=0. Confirmarea din 23.10 rămâne numai pentru aceste etape.

<a id="slice-d001-05-files-touched"></a>**Files touched** — Inventar complet (rânduri eliminate; valoarea negativă indică adăugare/absorbție):

| Cale | Rânduri |
|---|---|
| `VERCON/FLUXURI_UI.md` | 27 |
| `VERCON/SCHEMA_VC.md` | 17 |
| `VERCON/CONTRACT_CALCULE.md` | 6 |
| `docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md` | 44 |
| `VERCON/FACTURI_NIR_IMPLEMENTARE.md` | -1 |
| `docs/worklog/SLICE-0078-07-adobe-redimensionat-singur.md` | 64 |
| `docs/worklog/SLICE-0078-08-arbore-blocat-cat-se-deschide-pdf.md` | 86 |
| `docs/worklog/SLICE-0078-09-intrebarea-adobe-la-inchidere.md` | 80 |
| `docs/worklog/SLICE-0078-11-procese-adobe-fantoma-la-inchidere.md` | 45 |
| `docs/worklog/SLICE-0078-12-fereastra-gazduita-adobe-vechi.md` | 98 |
| `docs/worklog/SLICE-0078-13-confirmare-suprascriere-si-f8.md` | 30 |
| `docs/worklog/SLICE-0078-14-avertisment-adobe-vechi.md` | 46 |
| `docs/worklog/SLICE-0078-05-fereastra-gazduita-ctrl-h.md` | -490 |
| `docs/worklog/state/KBOT_STATUS_0070-0079.md` | 0 |
| `docs/worklog/SLICE-D001-05-contracte-nir-si-istoric-adobe.md` | -56 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-0078-04-sectiunea-b-peste-semnat.md` | -152 |
| `docs/worklog/SLICE-0078-06-sectiunea-b-angajament-nou.md` | 145 |
| `docs/worklog/SLICE-0078-03-jurnal-activex.md` | -42 |
| `docs/worklog/SLICE-0078-16-lane-jurnal-activex.md` | 35 |
| `VERCON/CONTROALE_CUSTOM_THREAD.md` | -14 |
| `implementare_vercon_v2.md` | 587 |
| `docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md` | -96 |
| `docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md` | -39 |

Bilanț: 25 de căi atinse, 9 fișiere retrase și un jurnal nou; **8 fișiere și 418 rânduri eliminate net** în lot. Cumul D001-01…05: **28 de fișiere și 3546 rânduri eliminate net**.

<a id="slice-d001-05-test-results"></a>**Test results** — Numai citire și analiză documentară sursă/destinație. Nu s-au rulat teste, builduri, verificatoare, aplicații sau SQL. Rezultatele din jurnalele absorbite sunt istorice. Stage-ul cerut de utilizator pentru D001-01…04 conține numai Markdown-ul propriu; fragmentele AVACONT din registrele comune sunt excluse. Fără commit/push.

<a id="slice-d001-05-unverified--deferred"></a>**Unverified / deferred** — Lot închis documentar; consolidarea continuă în D001-06. AVACONT, documentația activă AV și fișierele CLAUDE sunt excluse. Ajutorul runtime, codul și exporturile sursă nu sunt modificate. Nu se certifică funcționarea linkurilor în aplicație sau starea mediului utilizatorului prin reorganizare editorială.

## SLICE-D001-06 — Liste, fișe, rapoarte și import VERCON <a id="slice-d001-06"></a>

Data: 25.10.2026. Lot documentar aplicat local, continuarea D001-05.

<a id="slice-d001-06-what-changed-and-why"></a>**What changed and why**

- Opt etape ale formularului Lista de alimente (17.10) sunt în VC07-01: antet/meniuri, păstrarea D ascuns, allowCustom, sold anterior și recapitulare BigInt, editori FELUL/Explicații și corecțiile de focus/scroll. Stările locale nerulate, publicarea necesară și fișierele sunt păstrate.
- Patru etape ale machetei și proveniența serverului demonstrativ sunt în VC08-03. Matricea separă examinarea PDF-ului fictiv, răspunsul Excel HTTP 200 și versiunile ulterioare ale exportului/multipaginării nerulate. Nu se extinde confirmarea la MariaDB sau producție.
- Titlurile/ancorele jurnalului general sunt păstrate și trimit la sursele de detaliu.

- Cele două revizii numerice/de copiere și popupul HTML VRC (14.10) sunt în VC05-03/09: DECIMAL și precizia EF/VC, sursa păstrată fără recalculare/normalizare de semn, CHECK-urile/SQL manual și caracterul istoric al generatorului, raportul HTML fără recitire și restricțiile lui. Nu se transformă vechea confirmare de compilare a writerului în compilare a noii revizii.

- Șase etape FCS/BC și cache (17.10) sunt în VC06-03/04: disponibil la dată/excludere, numerotare anuală comună NIR/FCS, evitarea duplicatelor, cache în memorie cu setările/invalidările lui, mini fișe și tipografia lor. Regula inițială a disponibilului în consultare este delimitată de corecția ulterioară; contorul FCS separat rămâne numai istoric.

- Șapte etape NIR/TreeView sunt centralizate în VC06-06 și contractul TreeView: fișele informative, geometria și formulele/ordinea lor, factura cumulată readonly versus ținta locală de salvare, liniile externe fără payload și schimbarea contextului cu cereri anulate/lock. Marcajele și datele locale nu sunt reinterpretate.

<a id="slice-d001-06-files-touched"></a>**Files touched** — Inventar complet (rânduri eliminate, minus = informație absorbită/adăugată):

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-VC07-01-lista-alimente.md` | -46 |
| `implementare_vercon_v2.md` | 648 |
| `docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md` | -38 |
| `docs/worklog/SLICE-D001-06-liste-si-rapoarte-vercon.md` | -44 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-VC05-03-writer-atomic-si-maparea-cheilor.md` | -12 |
| `docs/worklog/SLICE-VC05-09-rezultat-colorat-si-ordinea-butoanelor.md` | -6 |
| `docs/worklog/SLICE-VC06-04-br-si-fcs.md` | -27 |
| `docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md` | -15 |
| `docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md` | -26 |
| `VERCON/CONTROALE_CUSTOM_THREAD.md` | -2 |

Bilanț: **12 căi, 430 rânduri eliminate net**, un jurnal nou și niciun fișier retras în acest lot. Cumul D001-01…06: **27 fișiere și 3976 rânduri mai puțin**.

<a id="slice-d001-06-test-results"></a>**Test results** — Numai citire și analiză documentară; fără teste, builduri, verificatoare, aplicații, SQL sau acces MariaDB. Probe și operații de publicare descrise în istoric nu sunt executate acum. Stage selectiv numai pentru Markdown propriu, conform cererii utilizatorului; fără commit/push.

<a id="slice-d001-06-unverified--deferred"></a>**Unverified / deferred** — Lot închis documentar; consolidarea continuă în D001-07. AVACONT, documentația activă AV, fișierele CLAUDE, ajutorul runtime și exporturile sursă sunt excluse. Nu se certifică starea funcțională prin reorganizare.
