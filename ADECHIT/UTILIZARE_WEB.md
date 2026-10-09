# ADECHIT — interfața și regulile actuale

Actualizat: 09.10.2026, SLICE-ADE6-23 / ADE8-05. Acest document consolidează rezultatul
threadului ADE1-05/06 și ADE6-05–17. Regulile de mai jos înlocuiesc descrierile
intermediare ale interfeței din workloguri; acestea rămân dovezi istorice.
Starea implementării este în [status](../ADECHIT_STATUS.md).

## Pagina principală și controalele comune

<!-- slice: ADE6-22 -->
La deschidere nu este selectată nicio lună sau grupă, iar tabelul copiilor este
gol. Cel mai recent an din arbore este deschis automat și numai lunile lui
se încarcă la pornire; ceilalți ani sunt închiși și își încarcă lunile la deschidere.
Alegerea lunii încarcă grupele, fără opțiunea Toate. Alegerea grupei încarcă
numai situația copiilor acelei grupe, cu istoricul necesar calculării soldurilor.
Reîncărcarea și preluarea/adăugarea copiilor sunt disponibile după alegerea
lunii și grupei; închiderea/redeschiderea necesită alegerea lunii.

Grupele din arbore și combobox, copiii, plătitorii și educatorii sunt ordonați
alfabetic după nume/denumire, cu regulile limbii române. Taxele se ordonează
după Explicație, iar documentele după Explicație, cu rândul nou la sfârșit.
Comboboxul lunilor folosește denumirea lunii. Arborele LUNA / ANUL păstrează
ordinea cronologică descrescătoare pentru ani și luni. Ordonarea inițială a
grilelor se poate schimba prin apăsarea antetului unei coloane.

Pe PC, bara cu luna/grupa și explicația ei este ascunsă; tabelul începe la același
Y cu panoul LUNA / ANUL. Pe mobil, primul rând conține comboboxurile Anul, Luna și Grupa;
mesajul de încărcare și spațiul rezervat lui sunt eliminate.

Se păstrează DataGrid-ul comun în toate modurile, inclusiv pe mobil. Filtrarea
există exclusiv la Nume și la Grupa (denumirea grupei), peste tot în ADECHIT.
CNP, Educatori, date, valori, I și coloanele de acțiuni nu au filtre.
Antetele și footerele DGV folosesc culorile DGV-ului principal. Antetele tuturor
ferestrelor modale folosesc culoarea antetului paginii principale; formularele
au fundal alb și același design al butoanelor, adaptat temei comune.

În grilele editabile, pe PC un clic intră în editare. Pe mobil un clic selectează;
editarea începe cu dublu clic sau Enter. Enter confirmă și trece la următoarea
celulă editabilă disponibilă, inclusiv pe rândul următor. Tab navighează la fel,
iar Shift inversează direcția. Celulele needitabile sunt sărite. O eroare de
validare păstrează editorul. Escape anulează numai editarea celulei, fără să
închidă fereastra, inclusiv când editorul are combobox/calendar.

Datele calendaristice folosesc calendarul custom comun, ca la Plăți. Excepția
este perioada taxelor: lună/an, fără calendar. Checkboxurile sunt editabile numai
în coloanele declarate explicit astfel. Rândurile DGV ADE sunt cu 20% mai înalte
pe mobil. Calculul lățimilor ține cont de borduri, scrollbarul vertical și
rotunjiri; scrollul orizontal necesar unor grile late rămâne posibil pe PC.

## Plătitori pe PC și editorii

<!-- slice: ADE6-23 -->
Mesajul introductiv „Selectați grupa, copilul și plătitorul. Folosiți Adaugă
sau Modifică.” este eliminat. Fereastra nu afișează nici mesajele informative
de încărcare/salvare; zona de mesaje apare numai pentru erori.

<!-- slice: ADE6-21 -->
La fiecare deschidere se încarcă numai grupele, fără selecție implicită.
Copiii se descarcă după alegerea grupei, iar persoanele asociate după alegerea
copilului. Schimbarea grupei golește imediat copiii și plătitorii precedenți;
un răspuns întârziat pentru o selecție veche este ignorat. Anii și istoricul
educatorilor se încarcă numai când se deschide editorul grupei.

Fereastra conține trei liste dependente, Grupe → Copii → Plătitori, cu Adaugă și
Modifică. Listele sunt doar pentru selecție și afișare; modificările se fac în
editorul elementului. Toate fostele coloane PL au antetul **I**, fără filtru:

| Listă | Semnificația bifei I |
|---|---|
| Grupe | Grupă închisă |
| Copii | Plecat |
| Plătitori / Platitori_sub | Închis: inversul lui Activ, numai la afișare |

Activ rămâne cu sensul normal în date și în editorul plătitorului. Închiderea
grupei folosește InchisaDinAn: anul existent se păstrează, altfel se folosește
anul curent; formularul nu cere separat anul.

Editorul grupei conține denumirea, bifa de închidere, ANI și tabelul perioadelor
educatorilor. ANI și tabelul au înălțimi, antete, culori și rânduri aliniate.
Coloanele umplu lățimea disponibilă; «+ Adaugă» se află în footerul tabelului.
Lățimea maximă a editorului este 760px.

Editorul copilului conține nume, CNP, combobox grupă, data intrării, Plecat și
data ieșirii. Editorul plătitorului conține nume, adresă, CNP, cod fiscal, cont,
bancă, telefon, email și Activ. Ambele au lățime maximă 460px și distanță de 10px
între rânduri, jumătate din dimensiunile anterioare.

Ferestrele sunt modale și blochează fundalul. Clicul în afară și Escape nu le
închid. Salvarea închide editorul numai după succes; eroarea/conflictul păstrează
datele nesalvate. Renunță abandonează explicit editările, conform machetelor.

CNP se verifică în JS și API după funcțiile VBA furnizate: 13 cifre, lună ≤12,
zi ≤31, județ ≤52 și cifra de control cu ponderile 279146358279, modulo 11
(restul 10 devine 1). Codurile sunt 0/2/3/4/5 pentru erori și -1 pentru valid.
CNP gol este permis pentru date istorice/persoane juridice; verificarea nu
introduce suplimentar validarea completă a unei date calendaristice.

La salvarea unui plătitor nou, acesta este activ automat și ceilalți plătitori
ai **aceluiași copil** devin inactivi în aceeași tranzacție. Activarea unui
plătitor existent aplică aceeași regulă. În editorul nou, Activ este bifat și
blocat. Un plătitor existent poate fi dezactivat explicit: pot exista zero activi,
dar nu mai mulți activi în urma acestor salvări. Importul nu curăță global
eventualele dubluri istorice; persoanele asociate altor copii nu sunt afectate.

Salvarea grupei și a perioadelor educatorilor este atomică. Istoricul educatorilor
este sursa principală, cu fallback legacy numai când istoricul lipsește complet.
Mutarea individuală/Plecat scrie jurnalul copilului și păstrează lunile închise;
schimbarea grupei afectează prezența deschisă. Copiii plecați cu debit/credit
rămân în grupa specială în luna următoare, cei cu sold zero ies. Transferul lot,
compensarea și înlocuirea endpointului legacy /transfer rămân de implementat.

## Plătitori pe mobil

La lățimi ≤980px se vede un singur DGV, pe nivelul curent. Fereastra păstrează
10px margine exterioară pe toate laturile și padding interior de 8px. Tabelul
ocupă înălțimea disponibilă între antet/footer; scrollul vertical este numai în
tabel, fără scroll exterior sau orizontal. Mesajul introductiv este eliminat.

Grupele și copiii au pe rând un buton ➡️ într-o coloană fără nume, filtru sau
sortare, care deschide nivelul următor. Apăsarea lungă a fost înlocuită.
Platitori_sub nu are această coloană. Butonul ↩️ din dreapta antetului revine
la nivelul anterior; la Grupe închide fereastra listei. Footerul paginii are
numai ➕ și ✏️ pentru nivelul curent.

| Listă | Lățimi |
|---|---|
| Grupe | Denumire 85% minus 40px; I 15%; ➡️ 40px; Educatori ascuns |
| Copii | Nume 50% minus 40px; CNP 35%; I 15%; ➡️ 40px |
| Plătitori | Nume 50%; CNP 35%; I 15% |

Lățimea butonului se scade numai din denumire/nume. Filtrarea denumirii/numelelor
rămâne disponibilă și pe mobil.

## Chitanțe pe PC

<!-- slice: ADE8-05 -->
Pe PC, la Chitanțe, butonul 📥 de lângă 💾 deschide meniul ultimei chitanțe
salvate pentru copilul curent. Până la prima salvare este inactiv. Fiecare
chitanță existentă are și propriul buton 📥. Meniul are stil Windows,
pictograme în stânga, navigare cu săgeți și închidere prin Escape sau clic în afară:
🖨️ Listare, ✉️ Trimitere pe mail, 📄 Descarcă PDF. Mailul apare numai dacă
plătitorul asociat chitanței are EMail completat și rămâne inactiv.

Listare deschide o fereastră cu două exemplare și dialogul browserului;
butonul Listare din acea fereastră permite repetarea comenzii. Descarcă PDF
generează documentul aceleiași chitanțe. Datele/suma/seria/numărul provin din
chitanța și plata salvate, persoana asociată și datele comune ale unității.
Chitanțele anulate sunt marcate ANULATĂ. Listarea și PDF-ul nu emit numere noi
și nu salvează rândul nou. Meniul este ascuns pe mobil.

Publicarea include fontul DejaVuSans.ttf și licența sa, șablonul receipt.html,
modulele API/JS și CSS. PDF-ul necesită instalarea în mediul Python al serverului
a `requirements-adechit.txt`, apoi restartul backendului. Fără DDL suplimentar.
Nu s-au executat probe funcționale/vizuale; verificarea revine utilizatorului.

## Taxe și perioade

ADE6-20: la adăugarea unui rând în DGV, Escape în celula activată automat
anulează întregul rând nou. După confirmarea primei celule, Escape anulează
numai editorul curent. Aplicat la Taxe și perioadele educatorilor.

Taxele necesită valoare >0 și explicație nevidă la salvare. Pentru un rând nou,
începutul este obligatoriu și după ultimul început; aceeași comparație se aplică
la modificarea începutului unei taxe existente. Excepția migrării păstrează
începutul NULL existent, nemodificat. La eroare se afișează un dialog cu
«Continuă editarea» / «Anulează înregistrarea», inclusiv la ieșirea din rând
din taste și la salvare. Anularea unui rând existent abandonează modificările
lui; anularea unei taxe noi o elimină și reface taxa anterior activă.
Schimbare scrisă local, fără teste.

ADE6-19: formularul are lățime maximă 744px (aproximativ 34% mai mic decât
1120px), cât coloanele DGV de 710px plus paddinguri și borduri; limita este 96vw.
Schimbare scrisă local, fără teste.

Instrucțiunile introductive sunt eliminate în toate modurile; erorile rămân
afișabile. Grila conține TaxaZilnica, Expl, DeLa, PanaLa și Activ (checkbox).
DeLa se editează ca lună/an, afișat ll.aaaa și stocat YYYY-MM. PanaLa este numai
pentru afișare și se completează automat, fără casetă la adăugare.

La adăugare, taxa nouă este activă implicit. Începutul propus este luna curentă
sau luna de după ultima perioadă cunoscută. Noua perioadă trebuie să înceapă după
toate începuturile cunoscute. Taxa anterior activă devine inactivă și primește
ca sfârșit luna anterioară noului început; modificările se salvează atomic.

**Taxele migrate pot avea DeLa/PanaLa NULL.** Lipsa datelor nu blochează citirea,
editarea, utilizarea sau importul lor și nu se inventează perioade istorice.
Numai taxele noi create prin catalog necesită DeLa. IDV istoric rămâne păstrat.

## Închidere anuală — ADE7-04

<!-- slice: ADE7-04 -->

La închiderea unei luni august deschise, pe desktop apare fereastra de închidere
anuală. Pe mobil (cel mult 980px) fereastra nu apare, iar închiderea lui august
este dezactivată. Celelalte luni păstrează fluxul obișnuit.

În stânga se află arborele custom cu grupele; în dreapta, DGV-ul custom cu copiii
grupei selectate. Prima coloană bifează selecția. Ctrl adaugă/elimină copii din
selecție, iar Shift bifează intervalul în ordinea afișată. Copiii selectați se
trag peste grupa destinație din arbore. Alternativ, se alege destinația din
comboboxul custom și se apasă „Mută copiii selectați”. Mutarea poate cuprinde un
copil, mai mulți sau toată grupa. Coloana „Grupa din august” păstrează originea.

„Plecat” este o bifă separată de selecție. Copiii astfel marcați ies la 31 august;
cei cu sold sunt preluați în grupa specială pentru copiii plecați în septembrie.
Sub tabelul copiilor este DGV-ul custom al educatorilor noului an. Numele grupei
poate fi schimbat, iar educatorii pot fi adăugați, editați sau eliminați din plan.

„Adaugă grupă”, sub arbore, creează o grupă în planul noului an. Salvarea este
refuzată dacă numele nu este unic, lipsește un educator sau lipsește un copil
care rămâne. O grupă nouă fără copii poate fi eliminată din plan.

„Renunță” abandonează planul. „Închide august și deschide septembrie” salvează
atomic situația din august cu datele vechi, aplică planul și creează septembrie.
Mutările și plecările sunt jurnalizate; perioadele educatorilor anteriori se
păstrează. Datele modificate între timp de alt utilizator impun reîncărcarea
ferestrei. Operația necesită drepturile de închidere, catalog și transfer.

Implementare locală. Aspectul, interacțiunea drag-and-drop în browser și
funcționarea pe MariaDB rămân de verificat de utilizator.

## Predare și verificări efective

DDL-ul suplimentar este [AD_04_taxe_perioade.sql](../sql/AD_04_taxe_perioade.sql):
adaugă DeLa/PanaLa VARCHAR(7) NULL în AD_ValoriTaxe. Se aplică o singură dată,
după AD_03, în baza unității. Nu a fost executat pe MariaDB din acest chat.
Utilizatorul publică prin AvacontPush; preview-ul local trebuie repornit pentru
ultimele schimbări backend/schema. Nu presupunem că restartul sau DDL-ul au avut loc.

ADE6-06: 16 teste API și probe browser locale, inclusiv mobil, pentru starea de
atunci. ADE6-07–17: scris local, **fără teste ulterioare** la cererea utilizatorului.
ADE1-06: doar pornirea/deschiderea aplicației pe localhost:5050 a fost verificată
nonvizual, după corectarea blueprintului și redirecționării /portal. Marcajul de
preview nu ocolește autentificarea producției; launcherul rămâne strict local.
Această probă nu validează aspectul sau schimbările backend ulterioare.
Nu există validare pe server, paritate completă cu Access sau publicare din chat.
