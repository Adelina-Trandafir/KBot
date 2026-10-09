# ADECHIT — interfața și regulile actuale

Actualizat: 09.10.2026, SLICE-ADE6-18. Acest document consolidează rezultatul
threadului ADE1-05/06 și ADE6-05–17. Regulile de mai jos înlocuiesc descrierile
intermediare ale interfeței din workloguri; acestea rămân dovezi istorice.
Starea implementării este în [status](../ADECHIT_STATUS.md).

## Pagina principală și controalele comune

Pe PC, bara cu luna/grupa și explicația ei este ascunsă; tabelul începe la același
Y cu panoul LUNA / ANUL. Pe mobil, primul rând conține numai cele două comboboxuri;
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
