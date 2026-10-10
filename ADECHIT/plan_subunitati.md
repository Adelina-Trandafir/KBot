# ADECHIT — plan pentru subunități în același DC

Data: **09.10.2026**. Felie de analiză: **SLICE-ADE0-08**.
Stare: **IMPLEMENTAT LOCAL (SLICE-ADE10-01–05, 09.10.2026) — netestat, DDL nerulat, nepublicat**. Predarea: [ADE10-05](../docs/worklog/SLICE-ADE10-05-predare-subunitati.md).

## 1. Obiectiv și decizii confirmate

Mai multe evidențe Access trebuie să coexiste în aceeași bază MariaDB, identificată
prin DC, deoarece aparțin aceleiași unități părinte. Utilizatorul trebuie să poată
alege evidența în care lucrează, fără amestecarea datelor.

Decizii confirmate de utilizator:

- Nivelul nou se numește **Subunitate**, inclusiv în interfață și documentație.
- Fiecare MDB care reprezintă o evidență independentă se importă într-o subunitate.
- Subunitățile coexistă în același DC; nu se creează câte un DC pentru fiecare MDB.
- Fiecare subunitate are **propria serie și propriul contor de chitanțe**, importate
  din sursa sa. Numerotarea nu este comună unității părinte.
- Relațiile, calculele și operațiile rămân în interiorul subunității selectate.
- Această intervenție produce numai planul; nu modifică aplicația, MDB-urile sau serverul.

Modelul funcțional:

```text
DC — unitatea părinte
  Subunitate A — evidența din MDB A, serie și contor A
  Subunitate B — evidența din MDB B, serie și contor B
```

Subunitatea este o entitate permanentă, cu identificator stabil și denumire pentru
utilizator. Numele fișierului și hash-ul MDB identifică proveniența unui import;
nu reprezintă identitatea subunității. Redenumirea MDB-ului sau a subunității nu
schimbă apartenența datelor.

Arhivele pe ani ale aceleiași evidențe nu devin automat subunități noi. Importul
mai multor arhive într-o subunitate populată necesită o etapă distinctă de reconciliere.

## 2. Situația actuală și sursele analizei

Analiza este statică și se bazează pe:

- `MariaDB_Schema/000_DEMO.sql`, export datat 09.10.2026, pentru schema reală de referință;
- `MariaDB_Schema/AVACONT_COMUN.sql`, pentru infrastructura comună disponibilă în export;
- `PYTHON/routes/adechit/repository.py`, `__init__.py`, `service.py`;
- `PYTHON/static/js/adechit/app.js`;
- `ADECHIT/ADE.Migrator/AdePlan.vb`, `AdeWriter.vb` și `README.md`;
- `ADECHIT_STATUS.md` și `ADECHIT/REGULI_PROIECT.md`.

Astăzi contextul ADE este DC-ul. Repository-ul nu primește o subunitate,
verificarea contextului folosește `X-Ade-Unit`, iar blocarea tranzacțiilor folosește
un rând comun din `AD_Lock`. Codul configurației chitanțelor caută după DC în
`AVACONT_COMUN.Unitati_Chitante`. Migratorul cere tabele de destinație goale.

În exportul `000_DEMO`, cheile tehnice sunt generate automat, unicitățile lună/an
și serie/număr sunt la nivelul întregului DC, iar `AD_Imports` și `AD_Operations`
nu disting subunități. Aceste constatări nu confirmă schema tuturor DC-urilor.
Înaintea DDL-ului se verifică exporturile țintelor efective și configurația reală
a chitanțelor; nu se accesează direct serverul pentru actualizarea exporturilor.

## 3. Modelul de date propus

Introducem un catalog local `AD_Subunits`, cu identificator tehnic `SubunitId`,
denumire, stare activă și informațiile administrative necesare. Numele tehnice
sunt propuneri; contractul final se fixează înaintea implementării.

Adăugăm apartenența obligatorie la subunitate pentru datele ADE, inclusiv:

- grupe, educatori și perioadele lor;
- copii, plătitori asociați și istoricul mutărilor;
- taxe, luni și prezență;
- plăți, chitanțe, alte documente, restituiri și compensări;
- situații salvate și orice date auxiliare persistente ale calculelor;
- setări, importuri, mapări de ID-uri și evidența operațiilor.

Inventarul complet al tabelelor și interogărilor precedă modificarea schemei.
Infrastructura comună a unității părinte rămâne la nivel de DC; apartenența ADE
nu trebuie extinsă automat la modulele KB sau la alte secțiuni.

### Chei și relații

Păstrăm cheile tehnice unice în fiecare tabel al DC-ului. Separarea funcțională
se face prin `SubunitId`, nu prin intervale numerice rezervate fiecărui MDB.

Pentru relațiile dintre două tabele ADE, baza trebuie să verifice și apartenența
la aceeași subunitate, prin chei candidate și referințe compuse de forma
`(SubunitId, ParentId)`. API-ul verifică aceeași condiție înaintea scrierii.
Aceasta acoperă inclusiv compensările între doi copii și ambele capete ale istoricului.

Se revizuiesc individual relațiile nullable și acțiunile `ON DELETE SET NULL`:
nu se poate permite ca anularea unei legături să golească apartenența obligatorie
la subunitate. Strategia de ștergere se stabilește pentru fiecare relație.

Unicitățile de business și indexurile se adaptează la noul context, de exemplu:

- lună/an: `(SubunitId, Luna, Anul)`;
- chitanță: `(SubunitId, Serie, Numar)`;
- setare: `(SubunitId, SettingKey)`;
- cerere reîncercată: `(SubunitId, RequestKey)`;
- sursă importată: subunitate și hash sursă, cu identitatea importului păstrată.

Nu se unifică automat copii sau plătitori pe baza CNP-ului ori a numelui între MDB-uri.

## 4. Serii și numere de chitanțe

Fiecare subunitate primește configurația proprie: serie, contor, explicație și
versiunea configurației. Recomandarea este o configurație ADE locală, legată de
`AD_Subunits`, pentru a nu modifica implicit contractul comun al altor module.
Amplasarea finală se confirmă după inventarierea consumatorilor configurației existente.

La import:

- seria și numărul fiecărei chitanțe istorice se păstrează;
- configurația se citește din MDB-ul subunității respective;
- contorul se confruntă cu documentele istorice și regulile existente de emitere;
- o neconcordanță sau o configurație incompletă este raportată explicit;
- configurația altei subunități nu este folosită și nu este suprascrisă.

Emiterea blochează și incrementează numai contorul subunității curente, în aceeași
tranzacție cu documentul. Reîncercarea aceleiași operații nu consumă un număr nou.
Anularea păstrează documentul și regula existentă de numerotare; nu introduce
reutilizarea numerelor. PDF-ul și listarea folosesc configurația și identitatea
subunității documentului, împreună cu datele unității părinte aplicabile formularului.

## 5. ID-uri și păstrarea regulilor temporale

Migratorul construiește o mapare explicită pentru fiecare tabel:

```text
import + tabel + ID Access -> ID MariaDB
```

ID-urile destinație sunt alocate de server. Toate referințele sunt rescrise prin
mapare, inclusiv referințele multiple și rândurile derivate de migrator. Maparea
se păstrează pentru audit și diagnostic; nu este calculată printr-un offset fix.

**Dependență critică:** regula actuală pentru inserarea/anularea documentelor
compară `IDL + 1` cu ultimul ID de lună din prezență. După remapare, ID-urile globale
nu mai exprimă distanța dintre lunile unei subunități.

Înaintea importului cu ID-uri remapate, trebuie separate identitatea tehnică a lunii
și ordinea locală folosită de regulile Access. Se proiectează o ordine locală care
reproduce comportamentul sursei, inclusiv eventualele goluri și situații istorice.
Nu se înlocuiește automat regula cu o comparație calendaristică fără demonstrarea
echivalenței. Aceeași analiză se aplică tuturor comparațiilor, operațiilor aritmetice,
sortărilor și calculelor `MIN/MAX` bazate pe ID-uri. Lipsa echivalenței blochează
implementarea dependentă; formulele de business nu se schimbă prin presupuneri.

## 6. Context API, drepturi și concurență

Contextul ADE devine **DC + SubunitId**. Repository-ul primește obligatoriu
subunitatea autorizată. Serverul verifică existența, apartenența la DC, starea
și drepturile la fiecare cerere; nu acceptă un context doar pentru că a fost trimis de client.

Toate accesările sunt limitate la context, inclusiv SQL-ul direct, subinterogările,
listele de ani, citirea după ID, actualizările, ștergerile, calculele, snapshot-urile,
închiderile lunare/anuale, exporturile, listările și PDF-urile. Un ID din altă
subunitate nu trebuie să permită nici citirea, nici modificarea înregistrării.

Clientul transmite ambele componente ale contextului. O nepotrivire este refuzată
explicit, înaintea oricărei scrieri. Evidența operațiilor include subunitatea în
identitate și amprentă, pentru ca o reîncercare în B să nu returneze rezultatul din A.

Drepturile existente pe DC sunt baza autorizării. În prima versiune utilizatorul
cu drepturi ADE pe DC poate accesa subunitățile active ale acelui DC. Restricțiile
individuale pe subunitate sunt o extensie ulterioară, dacă sunt solicitate.

Blocarea mutațiilor se mută la nivel de subunitate, cu aceeași ordine de achiziție
a blocărilor pentru toate fluxurile. Două operații în aceeași subunitate rămân
serializate; subunitățile diferite nu folosesc implicit același rând de blocare.
Importul și aplicația trebuie să folosească mecanisme compatibile de blocare.

## 7. Selectorul „Subunitate” și comutarea

Adăugăm selectorul **Subunitate** lângă unitatea părinte, folosind componentele
și tema comună. Denumirea selectată rămâne vizibilă în timpul lucrului.

- Cu o singură subunitate disponibilă, selecția poate fi automată.
- Cu mai multe, se restaurează ultima selecție validă din fila curentă sau se cere alegerea.
- Fără subunitate validă, nu se execută operații asupra datelor ADE.
- La schimbarea DC-ului se invalidează selecția subunității precedente.

Selecția este independentă în fiecare filă de browser și este validată pe server
pentru fiecare cerere. Două file pot lucra simultan în subunități diferite.

Înaintea comutării se rezolvă modificările nesalvate prin salvare, renunțare sau
anularea comutării. O scriere în curs nu este redirecționată către noua subunitate.
După comutare se resetează anii/lunile/grupele selectate, grilele, cataloagele,
dialogurile și cache-urile. Cererile și răspunsurile întârziate ale contextului
anterior nu pot actualiza noul ecran. Cheile cache-urilor includ DC și subunitate.

## 8. Migratorul și datele existente

Migratorul cere DC destinație și subunitate nouă sau existentă. La crearea unei
subunități cere denumirea; nu deduce identitatea numai din calea MDB-ului.
Planul și confirmarea arată DC sursă, DC destinație, subunitate, numărători,
configurația chitanțelor și conversiile necesare.

Prima versiune permite importul inițial într-o subunitate goală. Verificarea actuală
a tabelelor goale la nivelul întregului DC este înlocuită cu verificarea subunității.
Se poate importa B după A, fără modificarea lui A. Importul repetat accidental este
blocat prin context, hash și starea destinației. Completarea ori înlocuirea unei
subunități populate rămâne în afara primei versiuni.

Importul păstrează verificarea hash-ului sursei și reverificarea destinației.
Datele, mapările, configurația și înregistrarea importului se confirmă împreună;
la eroare se anulează împreună. Jurnalul identifică DC-ul, subunitatea și importul.
Un rezultat de COMMIT necunoscut se verifică prin identitatea importului înaintea reluării.

Pentru DC-urile deja populate, conversia creează o subunitate inițială și îi atribuie
toate datele ADE, setările și configurația existentă. ID-urile existente se păstrează
unde este posibil. Dacă datele sunt deja amestecate din mai multe surse, se oprește
atribuirea automată și se cere o reconciliere bazată pe proveniență.

## 9. Etapele implementării

| Etapă | Livrabil | Condiție de încheiere |
|---|---|---|
| 1. Inventar și contract | Matrice tabel/interogare/flux, relații, consumatori chitanțe, dependențe de ID-uri și context | Toate accesările ADE sunt clasificate; ordinea lunilor are contract de echivalență |
| 2. Schema și conversia | Catalog subunități, apartenență, indexuri, relații, configurații, mapări, conversia datelor existente | Fiecare rând are apartenență validă; conversia nu pierde date sau configurații |
| 3. API și operații | Repository cu context obligatoriu, autorizare, SQL limitat, blocări și reîncercări separate | Niciun flux nu poate traversa subunitățile |
| 4. Migrator | Alegere/creare subunitate, maparea ID-urilor, import tranzacțional și jurnal | A și B se importă succesiv în același DC, cu serii și contoare independente |
| 5. Interfață | Selector Subunitate, comutare, cache-uri și cereri legate de context | Schimbarea contextului și lucrul în două file păstrează datele corecte |
| 6. Verificare și predare | Scenarii de acceptare, documentație, ordine SQL și lista fișierelor de publicat | Utilizatorul confirmă probele autorizate și cele de pe server |

Implementarea primește subfelii distincte în registrele ADE existente. Planul nu
rezervă anticipat numerele următoare ale lucrărilor de cod.

Conversia se pregătește cu copie de siguranță și verificări de numărători, legături,
solduri și configurații. Schema și aplicația trebuie puse în funcțiune coordonat,
cu scrierile suspendate în intervalul de conversie. După importarea celei de-a doua
subunități, aplicația veche nu mai este compatibilă cu datele; revenirea necesită
restaurarea coordonată a bazei și aplicației, nu doar înlocuirea fișierelor de cod.

Codex pregătește local livrabilele. Utilizatorul publică prin AvacontPush, execută
SQL-ul și probează serverul, conform regulilor proiectului.

## 10. Scenarii de acceptare pentru utilizator

1. Două MDB-uri cu aceleași ID-uri Access și aceleași luni/ani sunt importate în
   subunități diferite ale aceluiași DC; toate legăturile rămân în sursa lor.
2. Schimbarea subunității schimbă anii, lunile, grupele, copiii, taxele și soldurile;
   niciun cache sau răspuns întârziat nu readuce datele celeilalte subunități.
3. Citirea și modificarea directă folosind un ID din cealaltă subunitate sunt refuzate.
   O relație între subunități este refuzată și de baza de date.
4. Emiterea în A folosește seria/contorul A; emiterea în B folosește seria/contorul B.
   Incrementarea, anularea și reîncercarea în A nu schimbă contorul B.
5. Emiterea concurentă în aceeași subunitate nu dublează numerele. Aceeași cheie
   de reîncercare în subunități diferite nu reutilizează un rezultat străin.
6. Închiderea lunară/anuală, compensările și recalcularea unei luni închise afectează
   numai subunitatea curentă; soldurile rămân echivalente cu sursa.
7. Regula temporală pentru documente produce aceleași acceptări/refuzuri înainte
   și după remaparea ID-urilor, inclusiv pentru surse cu goluri în ID-urile lunilor.
8. Două file deschise în A și B pot lucra independent; schimbarea DC-ului invalidează
   contextul vechi. Comutarea cu modificări nesalvate nu pierde date în tăcere.
9. O eroare de import nu lasă date sau contoare parțiale și nu modifică subunitatea
   deja importată. Reluarea după rezultat necunoscut nu dublează importul.
10. Conversia unui DC cu o singură evidență păstrează datele, documentele, soldurile
    și configurația chitanțelor; PDF-urile identifică subunitatea documentului.

Scenariile sunt planificate. În această intervenție nu s-au rulat teste, migrări,
verificări vizuale, DDL sau operații pe server.

## 11. Puncte de clarificat înaintea implementării dependente

- Denumirile concrete ale subunităților și corespondența MDB → subunitate pentru fiecare DC.
- Exporturile actuale ale țintelor și configurației comune de chitanțe.
- Ordinea locală a lunilor și echivalența regulilor care folosesc ID-uri numeric.
- Eventuale câmpuri proprii subunității în antetul chitanțelor și rapoartelor.
- Necesitatea unor importuri istorice suplimentare într-o subunitate populată,
  a restricțiilor de acces pe subunitate ori a rapoartelor consolidate; acestea
  necesită cerințe separate și nu sunt incluse implicit în prima versiune.

Terminologia **Subunitate** și seriile/numerele separate sunt deja decise;
nu sunt puncte deschise.

## 12. Verificare selectivă a MDB-urilor furnizate

Actualizare: **09.10.2026**, **SLICE-ADE0-09 — ANALIZAT LOCAL**.
Utilizatorul a indicat `ADECHIT/ACCESS_SOURCES` ca sursă a bazelor distincte și a
cerut să nu fie citite integral. În dosar sunt disponibile `baza_40.mdb` și
`baza_47.mdb`; vechiul fișier `baza2020_PP.mdb` menționat în documentație nu este
prezent în inventarul curent. Documentele exportate anterior din dosar nu au fost
considerate automat schema acestor două MDB-uri.

Citirea s-a făcut prin ACE OLE DB, cu `Mode=Read`, fără pornirea MSACCESS.
Au fost folosite metadate, `COUNT/MIN/MAX`, agregări pe serie, verificări de referințe
și eșantioane limitate prin `TOP`. Nu s-au extras integral tabele și nu s-au citit
nume de copii, CNP-uri, adrese ori detalii de plăți. Agregările pot parcurge intern
rândurile, dar returnează numai numărători sau rezultate limitate.

### Identificarea surselor

| Caracteristică | baza_40.mdb | baza_47.mdb |
|---|---|---|
| Dimensiune | 2.138.112 octeți | 11.243.520 octeți |
| DC în sursă | 051_GR40 | 047_GR47 |
| Denumire în Unitati | Grădinița cu P.P. Nr. 40 | Grădinița cu P.P. Nr. 47 |
| Serie configurată în CFGs, familia CH | GR40 | G.R. |
| Valoarea NUMAR din aceeași configurație | 6715 | 10199 |
| Luni disponibile în LunaD | septembrie și octombrie 2026 | septembrie și octombrie 2026 |
| IDL pentru aceste luni | 70, 71 | 72, 73 |
| Chitanțe existente | 0 | 1: seria G.R., numărul 10198 |

DC-urile sursă sunt diferite. DC-ul părinte de destinație nu poate fi dedus din
aceste fișiere și trebuie ales explicit. Denumirile de mai sus pot fi propuse
pentru subunități, dar nu constituie o atribuire confirmată de utilizator.

Valoarea `NUMAR` se păstrează ca valoare brută a configurației. În baza 47 este
cu unu mai mare decât numărul singurei chitanțe păstrate, însă aceasta nu dovedește
singură regula completă de emitere. Baza 40 demonstrează că un contor valid nu
poate fi reconstruit numai din `MAX(Chitante.Numar)`: tabelul este gol, iar contorul
configurat este 6715.

### Numărători și intervale de ID-uri

| Tabel | baza_40: rânduri; interval ID | baza_47: rânduri; interval ID |
|---|---|---|
| Grupe | 13; 1–14 | 17; 1–17 |
| ValoriTaxe | 6; 1–6 | 8; 1–8 |
| Platitori | 1.293; 1–2.215 | 1.032; 320–2.175 |
| Platitori_sub | 1.296; 1–2.218 | 1.032; 320–2.177 |
| LunaD | 2; 70–71 | 2; 72–73 |
| Prezenta | 1.686; 2.185.889–2.187.574 | 925; 993.274–994.198 |
| Plati | 171; 8.585–8.755 | 174; 8.597–8.770 |
| Chitante | 0; fără interval | 1; 4.540 |
| AlteDoc | 171; 1.874–2.044 | 173; 4.058–4.230 |
| Retur | 297; 1–297 | 1; 403 |
| SS_Buget | 948; 69.125–70.072 | 36.297; 1–36.749 |

ID-ul 1 există în ambele surse pentru grupe și taxe; intervalele plăților se
suprapun. Alte intervale se intersectează fără ca această analiză să inventarieze
fiecare ID comun. Planul de remapare rămâne necesar. Lunile calendaristice coincid,
chiar dacă IDL-urile sursă sunt distincte; unicitatea lună/an trebuie limitată la subunitate.

### Schema și istoricul incomplet

Au fost comparate numele de coloane, tipurile OLE DB, lungimile text și
nullable pentru 13 tabele relevante: Unitati, CFGs și cele 11 tabele din tabelul
de numărători. Cele 151 de coloane comparate nu au diferențe între aceste două
MDB-uri. Această verificare nu compară toate tabelele, indexurile, relațiile,
interogările sau regulile de validare ale bazelor.

În ambele surse, Chitante și AlteDoc nu au coloana IDL. Migratorul trebuie să
păstreze derivarea lunii prin legăturile existente, iar remaparea să fie aplicată
coerent înaintea sau în timpul construirii rândurilor derivate.

Referințele nenule la luni care nu mai există în LunaD au fost numărate astfel:

- baza_40: 295 din 297 rânduri Retur au IDL fără corespondent;
- baza_47: 35.834 din 36.297 rânduri SS_Buget au IDL fără corespondent;
- pentru Prezenta și Plati rezultatul este zero în ambele surse;
- pentru SS_Buget din baza_40 și Retur din baza_47 rezultatul este zero.

SS_Buget din baza_47 conține ani între 2020 și 2026, deși LunaD păstrează numai
două luni din 2026. Nu se elimină aceste rânduri și nu se inventează lunile lipsă.
Se păstrează tratamentul aprobat al referințelor istorice lipsă, proveniența și
apartenența la subunitate. Impactul asupra soldurilor și regulii temporale se
verifică în etapa de paritate; numărătorile nu demonstrează echivalența calculelor.

Nu s-au găsit luni calendaristice duplicate sau goluri între IDL-urile existente
în fiecare fișier. Rezultatul privește numai cele două luni păstrate; nu validează
ordinea întregului istoric și nu rezolvă dependența `IDL + 1` din secțiunea 5.

### Completări ale criteriilor de acceptare

- Se probează importul unei subunități cu zero chitanțe și contor nenul: baza_40.
- Se probează păstrarea seriei și contorului din baza_47 independent de baza_40.
- Se probează istoricul SS_Buget și Retur cu referințe la luni lipsă, fără pierderi
  de rânduri sau asociere accidentală la lunile celeilalte subunități.
- Se probează păstrarea/derivarea IDL pentru Chitante și AlteDoc după remapare.
- Se confirmă DC-ul părinte destinație și corespondența celor două surse cu subunitățile.

Această analiză nu reprezintă o migrare, un test al aplicației sau o validare pe server.
