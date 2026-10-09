# ADECHIT — decizii și probe încă necesare

Actualizat: 08.10.2026, SLICE-ADE0-03. Registru asociat [planului](PLAN_IMPLEMENTARE.md)
și [mapării](MAPARE_ACCESS_WEB.md). Nu este o listă de întrebări care oprește documentarea.
La rezolvare se adaugă data, sursa răspunsului/probei și efectul asupra feliei, fără
ștergerea constatării inițiale. Nicio opțiune de mai jos nu este considerată aprobată implicit.

## Neconcordanțe din Access

| Cod | Constatare / dovadă necesară | Felii afectate | Când trebuie rezolvat |
|---|---|---|---|
| M01 | **DECIS 08.10.2026:** bonurile fiscale ies complet din flux. Tipul documentului in Plati: Chitanta = 2, Alte documente = 1. Ramurile de bon fiscal din interogari/formulare nu se portează. | ADE5-01, ADE8-01 | Decis; de verificat in cod si la import ca nicio ramura nu mai presupune 0/1 = bon |
| M02 | **DECIS 08.10.2026:** comportamentul VBA era greșit. Orice modificare a documentelor unei luni închise, inclusiv anularea, rescrie valorile situației salvate pentru copilul respectiv; prezența rămâne blocată. | ADE7-03, ADE8-01/03 | Implementat și testat local; comparația Access rămâne |
| M03 | **DECIS 08.10.2026:** motivul anularii unei restituiri se salveaza. Anularea se face printr-o fereastra popup care cere OBLIGATORIU motivul; fara motiv anularea nu se executa. Importul nu inventeaza istoric. | ADE8-01 | De implementat (campul de persistare si popup-ul); neimplementat |
| M04 | **DECIS 08.10.2026:** se păstrează condiția din codul Access; data este refuzată numai dacă diferă și luna, și anul. | ADE8-01 | Implementat și testat local |
| M05 | **DECIS 08.10.2026:** numai ultima lună închisă poate fi redeschisă; luna deschisă următoare se elimină. Mișcările ei rămân orfane, cu luna/anul de proveniență păstrate separat de data documentului, pentru reatașare la recreare. O setare per bază poate bloca operația când există mișcări, verificând atât luna redeschisă, cât și luna eliminată. | ADE7-01/02 | Implementat și testat local; MariaDB nevalidat |
| M06 | **DECIS 08.10.2026:** istoricul MutaCopil din Access NU se reproduce (logica veche era gresita). In loc: (1) jurnal automat si append-only per copil (AD_Platitori_Istoric: INTRARE/MUTARE/PLECARE/REVENIRE/IESIRE_DEFINITIVA/COMPENSARE, data, grupa veche/noua, utilizator), scris in aceeasi tranzactie cu schimbarea; (2) istoric al educatorilor per grupa: AD_Grupe_Educator (IDG, educator, de la/pana la); nivelul mica/mijlocie/mare NU se pastreaza (decis 08.10.2026, nu e relevant), deci nu exista AD_Grupe_An; la migrare istoricul se reconstruieste din AD_SS_Buget (Educator/Grupa/IDG pe luni inchise) si migrarea AFISEAZA lista educatorilor gasiti pe fiecare grupa pentru unirea variantelor INAINTE de scriere; aceeasi grupa continua de la un an la altul, nivelul nu urmeaza automat; o grupa poate fi inchisa (InchisaDinAn), ramane vizibila pentru perioadele anterioare, fara stergere fizica; (3) copil in prezenta unei singure grupe pe luna: cea in care se afla la inchiderea lunii, mutarea dupa inchidere se aplica de la luna deschisa urmatoare; (4) plecati: sold 0 iese din situatia lunii urmatoare, cu debit ramane in grupa speciala «Copii plecati» pana la plata (iese din luna urmatoare platii), cu credit ramane pana la compensare (AD_Compensare, hotarata de consiliu). Educatorul poate lipsi (NULL), fara date inventate. MutaCopil din Access este un tabel mort (gol): nu se importa nimic din el; jurnalul porneste din INTRARE/PLECARE (DataIntrare/DataIesire) si, pentru mutarile vechi, din schimbarile de grupa deduse din lunile consecutive AD_Prezenta, marcate «dedus din prezenta» (fara zi exacta). | ADE6-01/03 | Decis; de implementat (schema + comenzi explicite) |
| M07 | **DECIS 08.10.2026:** celelalte variabile din sabloane nu intereseaza in etapa curenta; ramane doar [LA]. Ramurile de bon fiscal dispar (M01). | ADE3-02, ADE8-02 | Amanat; reluat cand se portează explicatiile/mailul |
| M08 | **DECIS 08.10.2026:** rapoartele nu se fac acum; DocumenteAnulate amanat odata cu ADE9-01. | ADE9-01 | Amanat |
| M09 | **CLARIFICAT 08.10.2026:** nu se cere nicio clarificare de la operator. Chitante/Plati_fact/FTP/Situatii salvate sunt ramuri vechi sau nexportate; raman in afara fluxului. Plati_fact/FTP tin de facturare, iar Situatii salvate de rpt_SitFin (rapoarte amanate). | ADE0, ADE9-01 | Inchis pentru etapa curenta |
| M10 | Structurile linked nu conțin rândurile backendului | ADE3, ADE5, ADE7-03 | Înaintea modelului final/importului și probelor de paritate |

Decizia utilizatorului din 07.10.2026 confirmă **încasarea după închidere cu actualizarea
situației salvate și prezență blocată**. La 08.10.2026, M02 a fost extins explicit și la
anulare și la orice altă modificare a documentelor lunii închise pentru copilul respectiv.

## Date, infrastructură și întinderea funcțiilor

| Cod | Ce lipsește / ce se stabilește | Responsabilitate și moment |
|---|---|---|
| D01 | Baza existentă țintă, unitățile/DC, DDL real și capitalizarea numelui bazei comune | Utilizatorul furnizează/confirmă; ADE3 înainte de SQL final |
| D02 | Rolurile ADE, crearea conturilor și accesul pe tabele comune/non-ADE | Matrice pregătită în ADE4; utilizatorul decide drepturile, fără presupuneri |
| D03 | Extract real, NULL/precizie, chei și rezultatele Access intermediare | ADE5-01 și ADE7-03; nu sunt înlocuite cu exemple calculate numai în Python |
| D04 | Conversiile ACE Long/Double/CInt, evaluarea SFD și comportamentul calendarului | Probe Access; ADE3/ADE6/ADE7 înaintea parității finale |
| D05 | Politica schema_sync pentru ADE și schema comună | ADE3-03 pe cod/diff; utilizatorul verifică și execută pe țintele reale |
| D06 | Configurația efectivă AvacontPush și manifestul publicabil | ADE1-01/ADE9-03; nu presupunem LocalRoot după locația proiectului |
| D07 | Sesiuni portal vs desktop și contextul de log/audit | ADE1-02/ADE4; extensie comună, nu autentificare/logger ADE separat |
| D08 | Calendarul detaliat Prezenta_sub, import SIIR, mail de business, exporturi, COMP manual, rapoarte FisaDebitor | Funcții condiționate; stabilire înaintea implementării dependente, fără includere/excludere tacită a datelor |
| D09 | Domeniul unicității numărului, serii reale, schimbare serie și reproducerea documentului emis | ADE3-02/ADE8-02; nu presupunem resetare anuală sau valori exemplu |
| D10 | Rezultatele de pe Linux/MariaDB și printul fizic | Utilizatorul execută după predare; ADE9 consemnează separat de localhost:5050 |

## Ce poate continua fără aceste răspunsuri

Scheletul, inventarul reutilizărilor, preview-ul și editorul de grilă pot fi construite
cu date fictive. Validatorul de import și comparatorul pot fi pregătite cu fixture-uri.
SQL-ul poate fi proiectat cu ținte declarate ca neconfirmate, fără a-l prezenta drept
gata de executat. Funcțiile afectate de M01–M10 nu se declară conforme înaintea probei/deciziei.

În intervenția ADE0-03 s-a documentat planul și acest registru. Nu s-au cerut sau acordat
drepturi pe server și nu s-a rezolvat implicit niciuna dintre necunoscutele de mai sus.
