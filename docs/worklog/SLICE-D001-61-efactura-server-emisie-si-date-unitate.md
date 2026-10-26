# SLICE-D001-61…86 — Istoricul curățării documentației (26.10.2026)

Reunire documentară D001-95. Fiecare lot păstrează titlul, data, rezultatul, inventarul complet, bilanțul/cumulul și continuarea istorică. Căile din inventare sunt proveniență, inclusiv când documentul a fost retras ulterior; numărătorile sunt cele consemnate atunci.

Declarațiile identice ale tuturor loturilor de mai jos sunt centralizate aici: data 26.10.2026; numai documentație, aplicat local.

- Test results: Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, ajutor runtime, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.
- Unverified / deferred: Confirmările tehnice/vizuale nu sunt extinse prin consolidare. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse.
- Tabelele indică rânduri eliminate; minus înseamnă rânduri adăugate.

Continuările fiecărui lot sunt repere ale cronologiei; continuarea curentă este în registrul D001.

## SLICE-D001-61 — Serverul, emiterea și datele unității E-Factura <a id="slice-d001-61"></a>

### What changed and why

Opt etape sunt reunite în trei istorice: server 06/07, ecran 08/09/16, datele unității 12/13/14. Corpurile și limitele sunt păstrate; corecția prin trimite, preluarea ANAF repetabilă, conturile comune, atașamentul și modificările ulterioare din 0114 au repere explicite. Planul și ajutorul conturilor trimit la ancore. Șase rânduri existente sunt scurtate; lipsa rândurilor 12/14 nu produce confirmări inventate. Probele mediului 28 nu sunt extinse la facturi reale.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-00EF-06-server-facturi-xml.md` | -26 |
| `docs/worklog/SLICE-00EF-08-ecran-facturi-emise.md` | -49 |
| `docs/worklog/SLICE-00EF-13-date-unitate.md` | -53 |
| `docs/worklog/SLICE-00EF-07-trimitere-anaf.md` | 65 |
| `docs/worklog/SLICE-00EF-09-arbore-meniu-trimitere.md` | 47 |
| `docs/worklog/SLICE-00EF-12-conturi-unitate.md` | 27 |
| `docs/worklog/SLICE-00EF-14-date-unitate-anaf.md` | 31 |
| `docs/worklog/SLICE-00EF-16-corectie-luni-atasament.md` | 46 |
| `docs/worklog/PLAN_00EF_EFactura.md` | 0 |
| `docs/worklog/SLICE-0000-56-ajutor-conturi.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-61-efactura-server-emisie-si-date-unitate.md` | -38 |

Bilanț: **14 căi; net 4 fișiere mai puțin; 49 rânduri mai puțin**. Cumul D001-01…61: **169 fișiere și 7595 rânduri mai puțin**.

Continuare D001-62.

## SLICE-D001-62 — Facturi primite: server, arhive și client <a id="slice-d001-62"></a>

### What changed and why

Șapte jurnale 17…23 sunt reunite în două istorice. Succesiunea parser → probă reală de mesaje și XML → zip rămâne delimitată de sincronizarea MariaDB neîncercată. Clientul păstrează controlul/fereastra/selectorul și înlocuirea celor două cereri cu ArePrimite. Planul trimite la 18/19 implementate, iar registrul păstrează statusurile inițiale cu amendamentele necesare. 28 distinge proba mesajelor primite de emiterea unei facturi reale neconfirmate. Nu se citește registrul Windows sau datele Access.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-00EF-17-primite-server.md` | -16 |
| `docs/worklog/SLICE-00EF-18-primite-client.md` | -59 |
| `docs/worklog/SLICE-00EF-19-primite-ecran.md` | 20 |
| `docs/worklog/SLICE-00EF-20-selector-ddf.md` | 16 |
| `docs/worklog/SLICE-00EF-21-fila-doar-cu-facturi.md` | 12 |
| `docs/worklog/SLICE-00EF-22-fila-din-arbore.md` | 12 |
| `docs/worklog/SLICE-00EF-23-arhive-anaf-zip.md` | 23 |
| `docs/worklog/PLAN_00EF_EFactura.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/SLICE-00EF-28-mediu-vps.md` | -4 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-62-efactura-primite-server-si-client.md` | -37 |

Bilanț: **13 căi; net 4 fișiere mai puțin; 34 rânduri adăugate**. Cumul D001-01…62: **173 fișiere și 7561 rânduri mai puțin**.

Continuare D001-63.

## SLICE-D001-63 — Facturi primite: evoluția ecranelor și popup-ul comun <a id="slice-d001-63"></a>

### What changed and why

Zece etape 27/29…37 sunt grupate în istoricele existente: server/import, ecranele facturilor primite și popup-ul comun 34/37. Corpurile, regulile CPV NULL versus gol, conversia clasificației, salvarea PDF, rezultatul sincronizării, textele de subsol, XmlViz și raza editorilor sunt păstrate. Checkpointurile înlocuite au amendamente explicite. Patru jurnale cu trimiteri și 11 rânduri comune sunt actualizate selectiv; numai legătura 0000-75 se schimbă pe rândul de ajutor. Fără modificarea controalelor, runtime help sau testelor.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-00EF-18-primite-client.md` | -200 |
| `docs/worklog/SLICE-00EF-17-primite-server.md` | -31 |
| `docs/worklog/SLICE-00EF-34-customPopup-combo-casuta.md` | -9 |
| `docs/worklog/SLICE-00EF-27-primite-cpv-continut-operatii.md` | 56 |
| `docs/worklog/SLICE-00EF-29-primite-arbore-furnizori.md` | 76 |
| `docs/worklog/SLICE-00EF-30-primite-sincronizare-din-ribbon.md` | 33 |
| `docs/worklog/SLICE-00EF-31-primite-fara-file-salvare-pdf.md` | 32 |
| `docs/worklog/SLICE-00EF-32-primite-rezultat-sincronizare.md` | 34 |
| `docs/worklog/SLICE-00EF-33-primite-actualizare-in-subsol.md` | 37 |
| `docs/worklog/SLICE-00EF-35-primite-optiuni-arbore.md` | 29 |
| `docs/worklog/SLICE-00EF-36-primite-xml-in-editor.md` | 47 |
| `docs/worklog/SLICE-00EF-37-primite-meniu-actualizare-server.md` | 45 |
| `docs/worklog/SLICE-0000-73-ajutor-efactura-ribbon.md` | 0 |
| `docs/worklog/SLICE-0000-75-ajutor-primite-arbore.md` | 0 |
| `docs/worklog/SLICE-0114-ribbon.md` | 0 |
| `docs/worklog/SLICE-0119-efactura-ribbon.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-63-efactura-primite-evolutie-si-popup.md` | -44 |

Bilanț: **20 căi; net 8 fișiere mai puțin; 104 rânduri mai puțin**. Cumul D001-01…63: **181 fișiere și 7665 rânduri mai puțin**.

Continuare D001-64.

## SLICE-D001-64 — Ajutor E-Factura: emitere și primite <a id="slice-d001-64"></a>

### What changed and why

20 de jurnale de ajutor sunt reunite în două istorice existente: șase etape emitere/token/unitate și 14 etape facturi primite. Textele, etichetele, capturile, rezultatele verificatorului și limitele sunt păstrate; numai limita identică a etapelor 61/62/63 are o definiție comună. Registrele trimit la ancore, iar jurnalele emiterii/unității au legături actualizate. Coliziunea veche 0000-61 cu ajutorul grupelor este consemnată; rândul grupelor rămâne intact. Nu se modifică fișierele de ajutor runtime sau capturile.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0000-54-ajutor-efactura.md` | -105 |
| `docs/worklog/SLICE-0000-61-ajutor-facturi-primite.md` | -148 |
| `docs/worklog/SLICE-0000-55-ajutor-facturi-emise.md` | 28 |
| `docs/worklog/SLICE-0000-56-ajutor-conturi.md` | 20 |
| `docs/worklog/SLICE-0000-57-ajutor-arbore-facturi.md` | 27 |
| `docs/worklog/SLICE-0000-58-ajutor-date-unitate.md` | 21 |
| `docs/worklog/SLICE-0000-60-ajutor-corectie-luni-atasament.md` | 7 |
| `docs/worklog/SLICE-0000-62-ajutor-fereastra-primite.md` | 7 |
| `docs/worklog/SLICE-0000-63-ajutor-leaga-ddf.md` | 7 |
| `docs/worklog/SLICE-0000-64-ajutor-fila-doar-cu-facturi.md` | 4 |
| `docs/worklog/SLICE-0000-65-ajutor-fila-din-arbore.md` | 3 |
| `docs/worklog/SLICE-0000-66-ajutor-arhiva-zip.md` | 3 |
| `docs/worklog/SLICE-0000-75-ajutor-primite-arbore.md` | 25 |
| `docs/worklog/SLICE-0000-76-ajutor-ribbon-sincronizare.md` | 18 |
| `docs/worklog/SLICE-0000-77-ajutor-primite-fara-file.md` | 12 |
| `docs/worklog/SLICE-0000-78-ajutor-rezultat-sincronizare.md` | 11 |
| `docs/worklog/SLICE-0000-79-ajutor-actualizare-in-subsol.md` | 13 |
| `docs/worklog/SLICE-0000-80-ajutor-optiuni-arbore.md` | 11 |
| `docs/worklog/SLICE-0000-81-ajutor-xml-factura.md` | 11 |
| `docs/worklog/SLICE-0000-82-ajutor-meniu-actualizare.md` | 11 |
| `docs/worklog/SLICE-00EF-08-ecran-facturi-emise.md` | 0 |
| `docs/worklog/SLICE-00EF-13-date-unitate.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-64-ajutor-efactura-emise-si-primite.md` | -50 |

Bilanț: **26 căi; net 17 fișiere mai puțin; 65 rânduri adăugate**. Cumul D001-01…64: **198 fișiere și 7600 rânduri mai puțin**.

Continuare D001-65.

## SLICE-D001-65 — Lista facturilor primite, transferul Access și trimiterea către VERCON <a id="slice-d001-65"></a>

### What changed and why

Istoricele 40, 42, 44 și 47…49 sunt integrate în documentele funcționale, cu ancore și toate corpurile etapelor. 38 (legarea certă la migrare), 39 (clasificațiile înainte de legare) și 46 (controlul combo) rămân documente distincte, compactate numai la împachetarea textului. Planul EV păstrează D1…D12 și primește legătura către evoluția 49. Referințele existente la fișierele retrase au fost mutate, fără schimbarea stărilor din registru. Nu s-au citit documente Tests; excluderile AVACONT/CLAUDE rămân.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-00EF-18-primite-client.md` | -17 |
| `docs/worklog/SLICE-00EF-41-primite-lista-toate-facturile.md` | -28 |
| `docs/worklog/SLICE-00EF-43-primite-access-trimite-preia.md` | -33 |
| `docs/worklog/SLICE-00EF-45-trimitere-la-vercon-serverul.md` | -86 |
| `docs/worklog/SLICE-00EF-38-migrator-legare-automata-receptii.md` | 18 |
| `docs/worklog/SLICE-00EF-39-clasificatii-in-fereastra-de-legare.md` | 0 |
| `docs/worklog/SLICE-00EF-46-combo-cu-grupe-pliabile.md` | 7 |
| `docs/worklog/SLICE-00EF-40-arbore-primite-fara-contor-si-an.md` | 15 |
| `docs/worklog/SLICE-00EF-42-lista-facturi-receptia-asociata.md` | 20 |
| `docs/worklog/SLICE-00EF-44-preia-access-jurnal-mariadb.md` | 26 |
| `docs/worklog/SLICE-00EF-47-trimite-la-vercon-fereastra.md` | 29 |
| `docs/worklog/SLICE-00EF-48-pagina-trimite-la-vercon-in-ribbon.md` | 32 |
| `docs/worklog/SLICE-00EF-49-vercon-cont-pentru-toate-liniile-un-furnizor.md` | 20 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/SLICE-0000-122-ajutor-vercon-cont-pentru-toate-liniile.md` | 0 |
| `docs/EV_PLAN.md` | 56 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-65-efactura-lista-access-si-trimitere-vercon.md` | -43 |

Bilanț: **19 căi; net 5 fișiere mai puțin; 15 rânduri mai puțin**. Cumul D001-01…65: **203 fișiere și 7615 rânduri mai puțin**.

Continuare D001-66.

## SLICE-D001-66 — Ajutorul E-Factura: lista primitelor și trimiterea către VERCON <a id="slice-d001-66"></a>

### What changed and why

Amendamentele 93/94/96…98 sunt integrate în istoricele ajutorului E-Factura emis/primit, iar 116/117/118/122 într-un singur istoric pentru VERCON. Se păstrează integral conținutul, rezultatele istorice HelpCheck/build, versiunile, lipsa capturilor și tutorialul nerulat. Textele vechi despre lunile bifabile și fereastra modală sunt încadrate cronologic, fără modificarea ajutorului din aplicație. Referințele din registru urmează ancorele noi; căile din inventarele loturilor trecute rămân dovezi istorice.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0000-61-ajutor-facturi-primite.md` | -62 |
| `docs/worklog/SLICE-0000-54-ajutor-efactura.md` | -17 |
| `docs/worklog/SLICE-0000-116-ajutor-trimitere-la-vercon.md` | -56 |
| `docs/worklog/SLICE-0000-93-ajutor-efactura-vedere-fara-furnizor.md` | 13 |
| `docs/worklog/SLICE-0000-94-ajutor-efactura-atasamente-in-continut.md` | 13 |
| `docs/worklog/SLICE-0000-96-ajutor-arbore-primite-fara-contor-si-an.md` | 13 |
| `docs/worklog/SLICE-0000-97-ajutor-primite-lista-toate-facturile.md` | 13 |
| `docs/worklog/SLICE-0000-98-ajutor-lista-facturi-receptia-asociata.md` | 13 |
| `docs/worklog/SLICE-0000-117-ajutor-pagina-vercon-din-ribbon.md` | 13 |
| `docs/worklog/SLICE-0000-118-tutorial-trimite-la-vercon.md` | 15 |
| `docs/worklog/SLICE-0000-122-ajutor-vercon-cont-pentru-toate-liniile.md` | 16 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-66-ajutor-efactura-lista-si-vercon.md` | -39 |

Bilanț: **15 căi; net 7 fișiere mai puțin; 66 rânduri adăugate**. Cumul D001-01…66: **210 fișiere și 7549 rânduri mai puțin**.

Continuare D001-67.

## SLICE-D001-67 — Recepții: model, clasificații, legare și robot <a id="slice-d001-67"></a>

### What changed and why

Șase fișiere istorice sunt integrate în documentele funcționale KE02, KE04 și KE05. Checkpointul identic de validare/prerechizite KE01…KE07 este definit o singură dată în KE01 și referit cu data și domeniul lui. Diagnosticul KE01-02 rămâne separat. Sunt păstrate corecția de angajament observată pe ecran, ordinea salvării clasificațiilor, limitele marcajului, eșecul separat al legăturii, ambiguitatea fără marcaj și incidentul CAB-0031 cu cauza nedovedită. Planul și registrul disting planul inițial de etapele ulterioare; fără execuții sau documentație Tests.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-KE01-modelul-legaturii-si-calculele.md` | -2 |
| `docs/worklog/SLICE-KE02-angajament-si-clasificatii-pe-linii.md` | -53 |
| `docs/worklog/SLICE-KE04-legare-de-receptie-existenta.md` | -71 |
| `docs/worklog/SLICE-KE05-robotul-si-receptie-noua-in-forexe.md` | -32 |
| `docs/worklog/SLICE-KE01-02-fisa-angajament.md` | 12 |
| `docs/worklog/SLICE-KE03-vederea-receptii.md` | 19 |
| `docs/worklog/SLICE-KE06-receptie-noua-din-factura.md` | 21 |
| `docs/worklog/SLICE-KE07-modificarea-unei-receptii.md` | 19 |
| `docs/worklog/SLICE-KE09-receptie-noua-format-site-si-eroare.md` | 35 |
| `docs/worklog/SLICE-KE10-o-receptie-noua-pentru-mai-multe-facturi.md` | 22 |
| `docs/worklog/SLICE-00EF-39-clasificatii-in-fereastra-de-legare.md` | 24 |
| `docs/worklog/state/KBOT_STATUS_KE.md` | 6 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/KE_PLAN.md` | 81 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-67-receptii-model-clasificatii-legare-si-robot.md` | -41 |

Bilanț: **17 căi; net 5 fișiere mai puțin; 39 rânduri mai puțin**. Cumul D001-01…67: **215 fișiere și 7588 rânduri mai puțin**.

Continuare D001-68.

## SLICE-D001-68 — Recepții: evoluția paginii și a propunerilor <a id="slice-d001-68"></a>

### What changed and why

KE11/12/12-02/12-04/13/14/15 sunt reunite în istoricul paginii, KE16/17 în istoricul propunerilor. Controlul general CheckBoxes rămâne separat. Sunt păstrate toate rundele, alegerile de implementare încă neconfirmate, limitele de rol, semn, buget pe zi și alocare. Registrul păstrează stările; numele și notele care repetau corpurile citite au fost scurtate. Combo-ul pe grup și diagnosticul KE15 sunt încadrate după actualizările din același jurnal; planul indică implementarea ulterioară KE12-06 a unicului indicator. Interfața inițială și propunerile 1:1 nu devin reguli concurente cu etapele ce le-au înlocuit.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-KE12-receptii-pagina-in-ribbon.md` | -210 |
| `docs/worklog/SLICE-KE12-03-dgv-casete-de-bifat.md` | 9 |
| `docs/worklog/SLICE-KE16-propuneri-legare-in-bloc.md` | -43 |
| `docs/worklog/SLICE-KE11-fereastra-receptiei-dupa-macheta.md` | 73 |
| `docs/worklog/SLICE-KE12-02-copac-cautare-si-ordine.md` | 52 |
| `docs/worklog/SLICE-KE12-04-formularul-receptiei-in-vederi.md` | 32 |
| `docs/worklog/SLICE-KE13-arborele-receptiilor-toate-facturile.md` | 37 |
| `docs/worklog/SLICE-KE14-factura-rezolvata-in-pagina-receptii.md` | 28 |
| `docs/worklog/SLICE-KE15-sume-in-formatul-ferestrei-si-rest-receptie.md` | 28 |
| `docs/worklog/SLICE-KE17-propuneri-angajament-suma-si-grupuri.md` | 35 |
| `docs/worklog/state/KBOT_STATUS_KE.md` | 0 |
| `docs/worklog/SLICE-KE02-angajament-si-clasificatii-pe-linii.md` | 0 |
| `docs/worklog/SLICE-KE04-legare-de-receptie-existenta.md` | 0 |
| `docs/KE_PLAN.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-68-receptii-pagina-propuneri-si-checkpointuri.md` | -41 |

Bilanț: **17 căi; net 6 fișiere mai puțin; 1 rânduri adăugate**. Cumul D001-01…68: **221 fișiere și 7587 rânduri mai puțin**.

Continuare D001-69.

## SLICE-D001-69 — Ajutorul recepțiilor: vedere, pagină și propuneri <a id="slice-d001-69"></a>

### What changed and why

Paisprezece jurnale ale aceleiași secțiuni sunt reunite în 0000-83, cu ancore pe fiecare ID și corpul fiecărei etape. Calea comună și limita UI nevăzută sunt definite o singură dată; fișierele suplimentare, capturile și rezultatele/absența HelpCheck rămân în etapa lor. Registrul elimină descrierile duplicate și păstrează coloana Status. Datele și amendamentele 95 → 101 → 102/105 → 108/109 → 114/115 arată explicit evoluția, fără modificarea ajutorului din aplicație.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0000-83-ajutor-receptiile-facturilor-primite.md` | -148 |
| `docs/worklog/SLICE-0000-95-ajutor-clasificatii-in-fereastra-de-legare.md` | 13 |
| `docs/worklog/SLICE-0000-99-ajutor-o-receptie-pentru-mai-multe-facturi.md` | 14 |
| `docs/worklog/SLICE-0000-101-ajutor-fereastra-receptiei.md` | 16 |
| `docs/worklog/SLICE-0000-102-ajutor-pagina-receptii.md` | 16 |
| `docs/worklog/SLICE-0000-103-ajutor-copac-receptii.md` | 14 |
| `docs/worklog/SLICE-0000-104-ajutor-casete-de-bifat.md` | 14 |
| `docs/worklog/SLICE-0000-105-ajutor-formularul-receptiei-in-vederi.md` | 14 |
| `docs/worklog/SLICE-0000-108-ajutor-arborele-receptiilor.md` | 14 |
| `docs/worklog/SLICE-0000-109-ajutor-factura-rezolvata.md` | 14 |
| `docs/worklog/SLICE-0000-111-ajutor-sume-si-rest-receptie.md` | 5 |
| `docs/worklog/SLICE-0000-112-ajutor-legare-in-bloc-propuneri.md` | 5 |
| `docs/worklog/SLICE-0000-114-ajutor-pagina-receptii-angajamente-ale-furnizorului.md` | 13 |
| `docs/worklog/SLICE-0000-115-ajutor-propuneri-angajament-suma-grupuri.md` | 5 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-69-ajutor-receptii-pagina-si-propuneri.md` | -42 |

Bilanț: **18 căi; net 12 fișiere mai puțin; 34 rânduri adăugate**. Cumul D001-01…69: **233 fișiere și 7553 rânduri mai puțin**.

Continuare D001-70.

## SLICE-D001-70 — Repararea rezervărilor și ajutorul corecțiilor DDF <a id="slice-d001-70"></a>

### What changed and why

Istoricul 0114-26 este integrat în 0114-25 cu limite distincte: confirmarea operatorului privește previzualizarea din 25, nu noua reparație/poartă din 26. Programul DDF/ORD (27) rămâne separat și indică amendamentul detectorului pe date salvate. Jurnalele de ajutor 120/121 sunt integrate în 110; 74, cu verificarea finală și numărătorile ei, în istoricul 69. Rândurile 0114-25…27 aveau toate coloanele comprimate în Nume; sunt așezate în cele patru coloane ale registrului, cu stările istorice păstrate. Notele duplicate ale ajutorului sunt trimise la corp. Nicio execuție, niciun fișier Tests deschis.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0114-25-repara-rezervari-fereastra-previzualizare.md` | -50 |
| `docs/worklog/SLICE-0114-26-rezervari-legate-gresit.md` | 45 |
| `docs/worklog/SLICE-0114-27-program-ddf-dupa-ss-indicatori.md` | 0 |
| `docs/worklog/SLICE-0120-ddf-randul-de-jos.md` | 6 |
| `docs/worklog/SLICE-0000-110-ajutor-repara-rezervari-fereastra.md` | -41 |
| `docs/worklog/SLICE-0000-120-ajutor-rezervari-legate-gresit.md` | 15 |
| `docs/worklog/SLICE-0000-121-ajutor-program-ddf.md` | 16 |
| `docs/worklog/SLICE-0000-69-ajutor-machete-ca-pagini.md` | -29 |
| `docs/worklog/SLICE-0000-74-ajutor-ddf-randul-de-jos.md` | 34 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_0110-0119.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-70-reparare-rezervari-program-si-ajutor.md` | -38 |

Bilanț: **14 căi; net 3 fișiere mai puțin; 43 rânduri adăugate**. Cumul D001-01…70: **236 fișiere și 7510 rânduri mai puțin**.

Continuare D001-71.

## SLICE-D001-71 — Adobe: compactarea istoricelor bancului și gazdei comune <a id="slice-d001-71"></a>

### What changed and why

Ambele istorice au fost citite integral. Paragrafele și continuările listelor sunt reunite fără schimbarea cuvintelor, codului, tabelelor, ancorelor sau ruperilor explicite de rând; se adaugă numai cuprinsul etapelor. Rămân distincțiile dintre randare și Adobe real, captură după titlu și preferința PID, ascundere și colapsare, baza contaminată și verdictul corectat, geometriile măsurate și necunoscutele. Nu s-au deschis fișiere din Tests și nu s-au executat comenzile istorice; numele și numărătorile testelor din aceste workloguri rămân dovezi ale etapelor.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0023-adobe-rhp-harness.md` | 334 |
| `docs/worklog/SLICE-0024-01-adobe-shared-host.md` | 303 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-71-adobe-istorice-compactare-paragrafe.md` | -29 |

Bilanț: **5 căi; net -1 fișiere mai puțin; 607 rânduri mai puțin**. Cumul D001-01…71: **235 fișiere și 8117 rânduri mai puțin**.

Continuare D001-72.

## SLICE-D001-72 — Designer: NavList, fonturi și arbori <a id="slice-d001-72"></a>

### What changed and why

Ambele istorice au fost citite integral și compactate la nivel de paragrafe/continuări de listă. Separatoarele orizontale redundante dintre secțiuni au fost eliminate; titlurile, ancorele, codul și tabelele rămân. Nu se rescriu ciclul inițial în trei stări, alegerea ulterioară Expanded ↔ Icons, sensul valorii zero, proprietatea adăugată CollapseButtonSize ori contractul gazdei Dock/CollapsedChanged. Fontul unic, serializarea valorilor auto și probele vizuale parțiale rămân în etapele lor. Rezultatele istorice ale testelor au rămas în workloguri; documentația proiectelor Tests nu a fost accesată.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0025-designer-surfaces.md` | 512 |
| `docs/worklog/SLICE-0027-tree-designer-surface.md` | 299 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-72-designer-navlist-si-arbore-compactare.md` | -29 |

Bilanț: **5 căi; net -1 fișiere mai puțin; 781 rânduri mai puțin**. Cumul D001-01…72: **234 fișiere și 8898 rânduri mai puțin**.

Continuare D001-73.

## SLICE-D001-73 — Migrare: istoricul HTTP și trecerea la citirea directă <a id="slice-d001-73"></a>

### What changed and why

Ambele istorice au fost citite integral. S-au compactat paragrafele și continuările listelor și s-au eliminat separatoarele de secțiune redundante; cuprinsul folosește ancorele deja existente. Rămân integral diferențele dintre rutare/selecție, insert-if-absent/upsert, validare/conversie, jurnal reconstructiv/tranzacție și coloana absentă/NULL. Primele probe Access nu devin probe de scriere MariaDB; descoperirea, revizia pe DDL și corecția unității rândului rămân cronologice. Exemplele de schema_sync/DDL/SSH sunt istoric citit, fără autorizare sau execuție în această lucrare. Nu s-au citit AVACONT, dumpuri ori documente Tests.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0044-migrare-accdb-impins.md` | 477 |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | 484 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-73-migrare-http-si-directa-compactare.md` | -29 |

Bilanț: **5 căi; net -1 fișiere mai puțin; 931 rânduri mai puțin**. Cumul D001-01…73: **233 fișiere și 9829 rânduri mai puțin**.

Continuare D001-74.

## SLICE-D001-74 — Recorder: gazdele browserului și urmărirea paginii <a id="slice-d001-74"></a>

### What changed and why

Istoricul gazdelor 0070/0074 este integrat în 0053, păstrând ancorele originale și etapa tastaturii. Urmărirea/stilurile/marcajele 0073/01/76 rămân separat și indică restrângerea ulterioară 0114-26 a legării IDREV. Paragrafele/listările sunt compactate; codul, numărătorile, limitele, ordinea suspendării și arbitrajul gazdelor sunt păstrate. Win32 „arătat off-screen” rămâne distinct de SW_HIDE; probele cu runner fals/DrawToBitmap nu devin browser real. Registrul schimbă numai destinația referințelor. Fără acces la Tests sau execuții.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0053-recorder.md` | 111 |
| `docs/worklog/SLICE-0070-browserul-doar-andocat.md` | 267 |
| `docs/worklog/SLICE-0073-recorder-meniu-plutitor-urmarire.md` | 186 |
| `docs/worklog/state/KBOT_STATUS_0070-0079.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-74-recorder-gazde-browser-si-urmarire.md` | -31 |

Bilanț: **7 căi; net 0 fișiere mai puțin; 532 rânduri mai puțin**. Cumul D001-01…74: **233 fișiere și 10361 rânduri mai puțin**.

Continuare D001-75.

## SLICE-D001-75 — DataView: compactarea bazei și a istoricului filtrării <a id="slice-d001-75"></a>

### What changed and why

Ambele istorice au fost citite integral. Numai compactarea paragrafelor și continuărilor listelor, eliminarea separatoarelor redundante și navigarea între etape. Semantica IsDirty, veto-ul ce blochează mutarea, thumb track/release, tipul real versus textul afișat, valorile NULL/goale, agregatele vizibile, lățimea autorată, amânarea validării în BeginInit și excepțiile temei întunecate sunt păstrate în text. Limitele v1 sunt legate de evoluția 0028; numărătorile contradictorii și probele parțiale rămân consemnate. Niciun test, build sau document din Tests accesat/rulat în această lucrare.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0028-dataview-footer-collapse-tooltip.md` | 486 |
| `docs/worklog/SLICE-0010-01-kbotdataview-skeleton.md` | 215 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-75-dataview-contracte-si-filtrare-compactare.md` | -29 |

Bilanț: **5 căi; net -1 fișiere mai puțin; 671 rânduri mai puțin**. Cumul D001-01…75: **232 fișiere și 11032 rânduri mai puțin**.

Continuare D001-76.

## SLICE-D001-76 — ADECHIT: migratorul, reconcilierea și amendamentele <a id="slice-d001-76"></a>

### What changed and why

Cincisprezece jurnale ADE5-01…14/ADE10-07 sunt reunite în două istorice: pipeline-ul inițial și utilitarul direct. Corpurile, probele, cifrele, ordinea conversiilor, regulile NULL, rezultatul necunoscut al COMMIT, oprirea după rollback și corecția chitanțelor rămân pe etapă. Nota identică Rezolvat din cinci etape este definită o singură dată cu domeniul și data confirmării operatorului. Planul păstrează titlurile/ancorele 05…14, trimite detaliile duplicate la jurnal și păstrează utilizarea/numărul următor; nu schimbă celelalte familii ADE. Registrele păstrează stările și destinațiile noi. Nu s-au modificat surse, schema, runtime, documentație Tests sau fișiere AVACONT/CLAUDE.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ADE5-01-extract-read-only.md` | -15 |
| `docs/worklog/SLICE-ADE5-04-ade-migrator.md` | -225 |
| `docs/worklog/SLICE-ADE5-02-import-repetabil.md` | 5 |
| `docs/worklog/SLICE-ADE5-03-reconciliere.md` | 5 |
| `docs/worklog/SLICE-ADE5-05-migrator-oprire-jurnal.md` | 59 |
| `docs/worklog/SLICE-ADE5-06-migrator-log-formular.md` | 47 |
| `docs/worklog/SLICE-ADE5-07-migrator-dc-editabil.md` | 34 |
| `docs/worklog/SLICE-ADE5-08-grupa-plecati.md` | 19 |
| `docs/worklog/SLICE-ADE5-09-cnp-parinte.md` | 27 |
| `docs/worklog/SLICE-ADE5-10-log-nou-pornire.md` | 26 |
| `docs/worklog/SLICE-ADE5-11-cnp-lipsa-null.md` | 28 |
| `docs/worklog/SLICE-ADE5-12-taxe-lipsa-null.md` | 24 |
| `docs/worklog/SLICE-ADE5-13-chei-randuri-generate.md` | 21 |
| `docs/worklog/SLICE-ADE5-14-oprire-fara-exceptie.md` | 26 |
| `docs/worklog/SLICE-ADE10-07-migrator-chitante-dublate-si-platitori-reconstituiti.md` | 47 |
| `ADECHIT/PLAN_IMPLEMENTARE.md` | 35 |
| `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md` | 0 |
| `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-76-adechit-migrator-istoric-si-plan.md` | -45 |

Bilanț: **21 căi; net 12 fișiere mai puțin; 117 rânduri mai puțin**. Cumul D001-01…76: **244 fișiere și 11149 rânduri mai puțin**.

Continuare D001-77.

## SLICE-D001-77 — ADECHIT: cataloage, prezență, taxe și amendamente UI <a id="slice-d001-77"></a>

### What changed and why

Cele 25 jurnale ADE6 existente sunt citite integral și organizate în cinci documente: cataloage/editori, prezență/context, taxe/validare, documentarea threadului și diagnosticul deadlockului. Douăzeci de fișiere sunt retrase, cu ancore pe fiecare ID, toate corpurile/probele și nota comună de închidere păstrate pe domeniu. Registrul separă legătura de Stare și scurtează recapitulările Current focus; confirmarea M06 din Open threads este păstrată și în istoricul catalogului. Nu există jurnal ADE6-03 separat: rândul său trimite la implementarea deja consemnată în ADE6-06, fără jurnal inventat. Ghidul final a fost citit și rămâne contractul operativ. Nicio intervenție asupra runtime, Tests, DDL sau surselor Access.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ADE6-01-cataloage.md` | -268 |
| `docs/worklog/SLICE-ADE6-05-ferestre-platitori-taxe.md` | 55 |
| `docs/worklog/SLICE-ADE6-06-machete-platitori.md` | 85 |
| `docs/worklog/SLICE-ADE6-07-editare-dgv-formulare.md` | 41 |
| `docs/worklog/SLICE-ADE6-10-platitori-mobil.md` | 41 |
| `docs/worklog/SLICE-ADE6-11-coloane-inchis.md` | 17 |
| `docs/worklog/SLICE-ADE6-12-butoane-deschidere-mobil.md` | 32 |
| `docs/worklog/SLICE-ADE6-13-margine-fereastra-mobil.md` | 14 |
| `docs/worklog/SLICE-ADE6-14-filtre-culori-dgv.md` | 19 |
| `docs/worklog/SLICE-ADE6-16-latimi-scroll-dgv.md` | 21 |
| `docs/worklog/SLICE-ADE6-17-platitor-activ-unic.md` | 22 |
| `docs/worklog/SLICE-ADE6-23-fara-mesaj-platitori.md` | 14 |
| `docs/worklog/SLICE-ADE6-24-educatori-email-cnp.md` | 70 |
| `docs/worklog/SLICE-ADE6-02-prezenta.md` | -141 |
| `docs/worklog/SLICE-ADE6-04-workspace-prezenta.md` | 25 |
| `docs/worklog/SLICE-ADE6-08-bara-selectie-mobil.md` | 24 |
| `docs/worklog/SLICE-ADE6-09-mobil-fara-mesaj.md` | 12 |
| `docs/worklog/SLICE-ADE6-21-incarcare-lazy.md` | 56 |
| `docs/worklog/SLICE-ADE6-22-ordonare-ultimul-an.md` | 35 |
| `docs/worklog/SLICE-ADE6-26-colapsare-an-anterior.md` | 36 |
| `docs/worklog/SLICE-ADE6-15-taxe-perioade.md` | -6 |
| `docs/worklog/SLICE-ADE6-19-taxe-formular-compact.md` | 13 |
| `docs/worklog/SLICE-ADE6-20-rand-nou-escape-validare-taxe.md` | 32 |
| `docs/worklog/SLICE-ADE6-18-documentatie-thread.md` | 13 |
| `docs/worklog/SLICE-ADE6-25-deadlock-lock-row.md` | 28 |
| `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md` | 40 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-77-adechit-cataloage-prezenta-si-taxe.md` | -53 |

Bilanț: **29 căi; net 19 fișiere mai puțin; 276 rânduri mai puțin**. Cumul D001-01…77: **263 fișiere și 11425 rânduri mai puțin**.

Continuare D001-78.

## SLICE-D001-78 — ADECHIT: ciclul financiar, încasări și rapoarte <a id="slice-d001-78"></a>

### What changed and why

18 jurnale sunt citite integral și grupate în patru documente: ciclul ADE7, încasări/anulări ADE8, rapoarte ADE9 și notificarea generală ADE9-08. Corpurile, cifrele, confirmările și limitele fiecărei etape sunt păstrate; nota identică Rezolvat este centralizată pe domeniu. AD11-01 anulare rămâne distinct de portalul cu număr reutilizat, fără renumerotare; vechea setare AllowCancelDocuments este încadrată istoric față de dreptul AD_ANULARE din ghid. Ancorele comune d001-12-antet-export sunt păstrate și referințele VERCON mutate fără fragment dublu. Jurnalul ADE9-10 exista fără rând în registru: primește legătură și starea documentată din 21.10, fără confirmare de publicare inventată; următorul număr devine 11. ADE9-02/03 păstrează stările existente, fără jurnale reconstruite. Fără teste sau runtime.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ADE7-01-faze-calcul.md` | -54 |
| `docs/worklog/SLICE-ADE7-02-ciclu-m05.md` | 5 |
| `docs/worklog/SLICE-ADE7-03-fixtures-comparator.md` | 5 |
| `docs/worklog/SLICE-ADE7-04-inchidere-anuala.md` | 66 |
| `docs/worklog/SLICE-ADE8-01-documente.md` | -86 |
| `docs/worklog/SLICE-ADE8-02-chitante.md` | 4 |
| `docs/worklog/SLICE-ADE8-03-idempotency.md` | 4 |
| `docs/worklog/SLICE-ADE8-04-taburi-incasari.md` | 27 |
| `docs/worklog/SLICE-ADE8-05-meniu-listare-pdf.md` | 38 |
| `docs/worklog/SLICE-AD11-01-anulare-document.md` | 36 |
| `docs/worklog/SLICE-ADE9-01-rapoarte.md` | -195 |
| `docs/worklog/SLICE-ADE9-04-situatie-plata-html.md` | 67 |
| `docs/worklog/SLICE-ADE9-05-sistem-rapoarte-comun-vercon.md` | 94 |
| `docs/worklog/SLICE-ADE9-06-butoane-stil-vercon.md` | 41 |
| `docs/worklog/SLICE-ADE9-07-rapoarte-loading-blocare.md` | 93 |
| `docs/worklog/SLICE-ADE9-09-text-rapoarte-minus-2pt.md` | 23 |
| `docs/worklog/SLICE-ADE9-10-help-capturi-extrase.md` | 13 |
| `docs/worklog/SLICE-ADE9-08-resetare-eroare-la-actiune.md` | 13 |
| `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md` | -1 |
| `implementare_vercon_v2.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-78-adechit-ciclu-incasari-si-rapoarte.md` | -47 |

Bilanț: **23 căi; net 13 fișiere mai puțin; 145 rânduri mai puțin**. Cumul D001-01…78: **276 fișiere și 11570 rânduri mai puțin**.

Continuare D001-79.

## SLICE-D001-79 — Portal părinți: implementare și corecturi <a id="slice-d001-79"></a>

### What changed and why

AD11-02…05 integrate cronologic în AD11-01, cu ancore și confirmarea comună din 23.10.2026 delimitată de probele istorice. Păstrate autentificarea, pașii de predare, limitele SQL, conflictul tolerat la import, corecturile mobile și salvarea atomică. Ghidul separă validarea la salvare (copii activi/subunitatea curentă) de identitatea portalului (întregul DC); checkpointul inițial ADE9-07 este datat față de confirmarea ulterioară. ID-ul AD11-01 al anulării rămâne în istoricul ADE8. Documentația din teste nu a fost analizată.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-AD11-01-portal-parinti.md` | -74 |
| `docs/worklog/SLICE-AD11-02-font-totaluri-scroll.md` | 27 |
| `docs/worklog/SLICE-AD11-03-conflicte-query-avacontpush.md` | 35 |
| `docs/worklog/SLICE-AD11-04-latimi-mobil.md` | 17 |
| `docs/worklog/SLICE-AD11-05-copil-parinte-linkuri.md` | 68 |
| `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` | 0 |
| `ADECHIT/PORTAL_PARINTI.md` | 59 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-79-portal-parinti.md` | -34 |

Bilanț: **10 căi; net 3 fișiere mai puțin; 97 rânduri mai puțin**. Cumul D001-01…79: **279 fișiere și 11667 rânduri mai puțin**.

Continuare D001-80.

## SLICE-D001-80 — ADECHIT: pagina, preview și editare DataGrid <a id="slice-d001-80"></a>

### What changed and why

Etapele ADE1-02…06 sunt într-un jurnal, iar ADE2-01…03 într-altul, cu ancore per subfelie și legături din registre. Checkpointul parțial ADE1–ADE8 rămâne separat și este încadrat istoric; sunt păstrate deciziile M02/M04/M05, necunoscutele inițiale M03/M06, cifrele și limitele probelor. Păstrate separarea preview/runtime, confirmarea din 23.10.2026 și diferențele dintre probele inițiale și acceptarea ulterioară. Reparată referința veche la pagina-si-preview, absentă din inventarul curent. Nicio documentație din teste nu a fost deschisă.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ADE1-01-implementare-partiala.md` | 25 |
| `docs/worklog/SLICE-ADE1-02-pagina-aplicatie.md` | -83 |
| `docs/worklog/SLICE-ADE1-03-preview-local.md` | 5 |
| `docs/worklog/SLICE-ADE1-04-ecran-prezenta-access.md` | 28 |
| `docs/worklog/SLICE-ADE1-05-initializare-blueprint.md` | 22 |
| `docs/worklog/SLICE-ADE1-06-pornire-preview.md` | 48 |
| `docs/worklog/SLICE-ADE2-01-editori-datagrid.md` | -15 |
| `docs/worklog/SLICE-ADE2-02-salvare-validare.md` | 4 |
| `docs/worklog/SLICE-ADE2-03-proba-browser.md` | 5 |
| `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-80-pagina-preview-editare.md` | -37 |

Bilanț: **13 căi; net 5 fișiere mai puțin; 1 rânduri mai puțin**. Cumul D001-01…80: **284 fișiere și 11668 rânduri mai puțin**.

Continuare D001-81.

## SLICE-D001-81 — ADECHIT: subunități de la schemă la predare <a id="slice-d001-81"></a>

### What changed and why

ADE10-01…05 și completările ADE10-06 reunite în jurnalul AD10-01-schema-subunitati, cu ancore per etapă. Păstrate izolarea DC/subunitate, 17 tabele/29 relații/46 constrângeri, remaparea ID-urilor, regulile de numerotare, snapshot/lock și limitele reconcilierii; scenariile și ordinea predării rămân complete. Nota identică din cinci rubrici de nevalidat este definită o dată, cu domeniu și data 23.10.2026; obligațiile inițiale și confirmările ulterioare nu sunt confundate. Registrul și cele două trimiteri din plan/status folosesc acum fișierul real AD10 în locul variantelor ADE10 inexistente. Istoricul AD10-01-drepturi-pe-sectiuni și etapa AD10-11 rămân distincte, neatinse. Documentația din teste exclusă.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-AD10-01-schema-subunitati.md` | -131 |
| `docs/worklog/SLICE-AD10-02-api-subunitati.md` | 71 |
| `docs/worklog/SLICE-AD10-03-migrator-subunitati.md` | 42 |
| `docs/worklog/SLICE-AD10-04-interfata-subunitati.md` | 57 |
| `docs/worklog/SLICE-AD10-05-predare-subunitati.md` | 59 |
| `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` | 0 |
| `ADECHIT/plan_subunitati.md` | 0 |
| `ADECHIT_STATUS.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-81-subunitati-schema-api-predare.md` | -35 |

Bilanț: **11 căi; net 3 fișiere mai puțin; 62 rânduri mai puțin**. Cumul D001-01…81: **287 fișiere și 11730 rânduri mai puțin**.

Continuare D001-82.

## SLICE-D001-82 — KBotAdmin: interfață, registru și limitele probelor <a id="slice-d001-82"></a>

### What changed and why

ME01-02/05/06 reunite cronologic cu ME01, inclusiv diagnosticul tray și cauza demonstrată de jurnal la 21.10.2026. ME01-03 (privilegii) și ME01-04 (clonare DEMO) rămân distincte, cu paragrafe compacte; nu se schimbă politica, atomicitatea/limitele sau obligațiile operatorului. Registrul are trimiteri per etapă și stări datate în locul rezumatelor repetate în Open threads; numele verbelor demo-clone-plan/demo-clone, unice în rezumatul vechi, sunt transferate în ME01-04. README indică jurnalul consolidat. Utilizarea inițială nu este extinsă la etapele ulterioare nerulate. Fără acces la server, fără documentație din teste.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ME01-admin-desktop.md` | -53 |
| `docs/worklog/SLICE-ME01-02-drepturi-adechit-vercon-macheta.md` | 16 |
| `docs/worklog/SLICE-ME01-05-ribbon-si-ajustari-vizuale.md` | 52 |
| `docs/worklog/SLICE-ME01-06-arbore-cu-bife-si-moduri.md` | 33 |
| `docs/worklog/SLICE-ME01-03-privilegii-mariadb.md` | 58 |
| `docs/worklog/SLICE-ME01-04-clonare-demo.md` | 7 |
| `docs/worklog/state/KBOT_STATUS_ME01.md` | 11 |
| `KBotAdmin/README.md` | 34 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-82-kbotadmin-interfata-si-stari.md` | -35 |

Bilanț: **11 căi; net 2 fișiere mai puțin; 122 rânduri mai puțin**. Cumul D001-01…82: **289 fișiere și 11852 rânduri mai puțin**.

Continuare D001-83.

## SLICE-D001-83 — ADECHIT: planul subunităților și continuările distincte <a id="slice-d001-83"></a>

### What changed and why

Lectură integrală și compactare de paragrafe/liste în plan_subunitati, autentificare/drepturi AD10-01/02, zile lucrătoare AD10-11 și ajutor AD12-01. Păstrate toate tabelele cu numărători, criteriile, formulele, pașii SQL și limitele probelor; planul din 09.10 este încadrat față de implementarea ulterioară. Confirmarea identică din AD10-11 este definită o singură dată pentru implementare și corecturi, cu limitele HTTP păstrate independent. Registrul ADE10 include acum jurnalul AD10-11 existent, fără renumerotarea familiei distincte AD10; drepturile sunt trimise la continuarea KBotAdmin. Fără acces la date brute/server sau documentație din teste.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-AD10-01-drepturi-pe-sectiuni.md` | 29 |
| `docs/worklog/SLICE-AD10-11-zile-lucratoare-subunitate-rapoarte.md` | 63 |
| `docs/worklog/SLICE-AD12-01-ajutor-adechit.md` | 15 |
| `ADECHIT/plan_subunitati.md` | 120 |
| `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` | 1 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-83-adechit-plan-si-continuari.md` | -32 |

Bilanț: **8 căi; net -1 fișiere mai puțin; 195 rânduri mai puțin**. Cumul D001-01…83: **288 fișiere și 12047 rânduri mai puțin**.

Continuare D001-84.

## SLICE-D001-84 — ADECHIT: planificare și decizii succesive <a id="slice-d001-84"></a>

### What changed and why

Nouă jurnale ADE0 reunite în trei: 01…05 (planificare/mapare/reguli), 06…07 (schema și deciziile care închid M06 și completează M03), 08…09 (subunități și analiza selectivă MDB). Rubricile identice de confirmare sunt definite per grup cu data 23.10.2026; toate tabelele deciziilor, constatările, cifrele și comenzile nereușite rămân. Registrul și planul trimit la ancorele etapelor. Formulările de disponibilitate a vechiului MDB și de implementare a subunităților sunt încadrate după datele checkpointurilor, fără presupuneri despre starea live. CLAUDE.md și sursele brute nu au fost deschise.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ADE0-01-plan-initial.md` | -107 |
| `docs/worklog/SLICE-ADE0-02-mapare-access-web.md` | 50 |
| `docs/worklog/SLICE-ADE0-03-documentare-toate-feliile.md` | 45 |
| `docs/worklog/SLICE-ADE0-04-reguli-proiect.md` | 31 |
| `docs/worklog/SLICE-ADE0-05-memorie-access.md` | 32 |
| `docs/worklog/SLICE-ADE0-06-decizii-schema-ad.md` | -33 |
| `docs/worklog/SLICE-ADE0-07-decizii-grupe-istoric-ddl-final.md` | 48 |
| `docs/worklog/SLICE-ADE0-08-plan-subunitati.md` | -17 |
| `docs/worklog/SLICE-ADE0-09-verificare-mdb-subunitati.md` | 39 |
| `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md` | 1 |
| `ADECHIT/PLAN_IMPLEMENTARE.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-84-adechit-analiza-si-decizii.md` | -38 |

Bilanț: **14 căi; net 5 fișiere mai puțin; 50 rânduri mai puțin**. Cumul D001-01…84: **293 fișiere și 12097 rânduri mai puțin**.

Continuare D001-85.

## SLICE-D001-85 — ADECHIT: contracte și inventar fără tratamente repetitive <a id="slice-d001-85"></a>

### What changed and why

Lectură integrală a contractului calculelor, registrului M/D, mapării, regulilor și inventarului. Contractele rămân distincte; paragrafele și listele sunt compactate fără schimbarea formulelor/fazelor, blocurilor de cod, deciziilor, regulilor sau tabelelor de probe. În inventar, obiectele cu același text exact de tratament sunt grupate per categorie: tables — 66 — 66 obiecte / 14 grupe; Forms — 44 — 44 obiecte / 11 grupe; queries — 27 — 27 obiecte / 11 grupe; Reports — 18 — 18 obiecte / 4 grupe. Fiecare nume, link și încadrare rămâne explicit. Nicio sursă Access/SQL și nicio documentație din teste nu a fost deschisă.

### Files touched

| Cale | Rânduri |
|---|---|
| `ADECHIT/CONTRACT_CALCULE.md` | 61 |
| `ADECHIT/DECIZII_DESCHISE.md` | 9 |
| `ADECHIT/MAPARE_ACCESS_WEB.md` | 63 |
| `ADECHIT/REGULI_PROIECT.md` | 53 |
| `ADECHIT/INVENTAR_OBIECTE.md` | 117 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-85-adechit-contracte-si-inventar.md` | -32 |

Bilanț: **8 căi; net -1 fișiere mai puțin; 270 rânduri mai puțin**. Cumul D001-01…85: **292 fișiere și 12367 rânduri mai puțin**.

Continuare D001-86.

## SLICE-D001-86 — ME02: copii locale și limitele confirmării operatorului <a id="slice-d001-86"></a>

### What changed and why

Cele trei jurnale rămase ME02 și registrul au fost citite integral și compactate prin reunirea rândurilor în paragrafe/liste. Metadatele SQL, dumpul server și motorul local rămân distincte; sunt păstrate excluderile de baze, politica datelor NULL pentru rândurile vechi, motivul obligatoriu, limitele --hex-blob/max_allowed_packet, criptografia, intervalele paginilor și operațiile offline. Hotărârea finală fără programare automată și retenția nelimitată a copiilor manuale rămân delimitate de propunerea inițială. Confirmarea din 21.10 privește exact primele două copii după corecția SFTP; nu este extinsă la verificare/export/restaurare/istoric/curățare/ștergere sau DPAPI. Fără documentație din teste, server sau SQL executat.

### Files touched

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ME02-01-date-creare-modificare.md` | 15 |
| `docs/worklog/SLICE-ME02-02-copie-manuala-toate-bazele.md` | 9 |
| `docs/worklog/SLICE-ME02-03-format-si-motor.md` | 27 |
| `docs/worklog/state/KBOT_STATUS_ME02.md` | 8 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-86-copii-locale-istorice-compacte.md` | -31 |

Bilanț: **7 căi; net -1 fișiere mai puțin; 27 rânduri mai puțin**. Cumul D001-01…86: **291 fișiere și 12394 rânduri mai puțin**.

Continuare D001-87.
