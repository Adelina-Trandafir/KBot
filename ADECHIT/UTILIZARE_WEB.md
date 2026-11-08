# ADECHIT — interfața și regulile actuale

Actualizat: 19.10.2026, SLICE-ADE9-08/09. Acest document consolidează rezultatul threadului ADE1-05/06 și ADE6-05–17. Regulile de mai jos înlocuiesc descrierile intermediare ale interfeței din workloguri; acestea rămân dovezi istorice. Starea implementării este în [status](../ADECHIT_STATUS.md).

## Pagina principală și controalele comune

<!-- slice: WB02-01 -->
Pe calculator, **Administratorul** unității curente găsește **Înrolează Certificat Digital** în meniul siglei **A**, deasupra **Setări tooltip**. Pagina verifică certificatul tokenului/cardului; numai apăsarea **Înrolează certificatul acesta** îl înrolează pe contul propriu și dezactivează certificatul anterior. **Dezactivează** oprește certificatul activ; **Înapoi în aplicație** revine la ADECHIT. Opțiunea lipsește pe telefon/tabletă. Sesiunea expirată cere revenire în aplicație și reautentificare.

<!-- slice: ADE9-08 -->
Eroarea de sub antet se șterge și se ascunde la următoarea apăsare/clic, tastare, schimbare de valoare sau trimitere de formular. Dacă acțiunea eșuează, apare noua eroare. Hoverul nu o ascunde.

<!-- slice: AD11-01 -->
<!-- slice: AD11-02 -->
<!-- slice: AD11-04 -->
Pe mobil, selectorul copilului are lățimea elementelor dashboardului și opțiuni cu font mărit; cardurile încap în ecran, graficele ample se derulează în card, iar portalul permite scroll vertical. „Imprimă” este ascuns; „Descarcă PDF” rămâne disponibil. Subtotalurile/totalul final al fișei au fundaluri gri distincte în print/PDF. [Portalul separat al părinților](PORTAL_PARINTI.md) oferă autentificare CNP/cod permanent/cod email, numai copii fără Plecat, selector pentru mai mulți copii, situații/grafice și fișă cont print/PDF pe toată perioada sau Interval. În macheta părinților, „Trimite Codul de Acces” apare numai cu email completat și salvează înaintea trimiterii; același email la alt CNP este refuzat cu identificarea părintelui.

<!-- slice: ADE6-22 -->
La deschidere, luna/grupa sunt nealese și tabelul copiilor este gol. Numai cel mai recent an se deschide și își încarcă lunile automat; ceilalți le încarcă la deschidere. Luna aleasă încarcă grupele, fără Toate; grupa încarcă situația propriilor copii și istoricul necesar soldurilor. Reîncărcarea și preluarea/adăugarea copiilor cer lună și grupă alese; închiderea/redeschiderea cere luna.

<!-- slice: ADE6-26 -->
În LUNA / ANUL, alegerea unei luni din alt an colapsează anul anterior (la prima alegere, cel deschis automat) și păstrează extins noul an. Simpla extindere pentru consultare nu colapsează anul selectat.

Grupele din arbore și combobox, copiii, plătitorii și educatorii sunt ordonați alfabetic după nume/denumire, cu regulile limbii române. Taxele se ordonează după Explicație, iar documentele după Explicație, cu rândul nou la sfârșit. Comboboxul lunilor folosește denumirea lunii. Arborele LUNA / ANUL păstrează ordinea cronologică descrescătoare pentru ani și luni. Ordonarea inițială a grilelor se poate schimba prin apăsarea antetului unei coloane.

Pe PC, bara cu luna/grupa și explicația ei este ascunsă; tabelul începe la același Y cu panoul LUNA / ANUL. Pe mobil, primul rând conține comboboxurile Anul, Luna și Grupa; mesajul de încărcare și spațiul rezervat lui sunt eliminate.

DataGrid-ul comun se păstrează inclusiv pe mobil. Peste tot în ADECHIT, numai Nume și Grupa (denumirea) au filtre; CNP, Educatori, date, valori, I și acțiunile nu au. Antetele/footerele DGV preiau culorile DGV-ului principal; antetele modale, culoarea antetului paginii principale. Formularele au fundal alb și butoane cu design comun, adaptat temei.

În grilele editabile, un clic editează pe PC și selectează pe mobil, unde editarea cere dublu clic sau Enter. Enter confirmă și trece la următoarea celulă editabilă, inclusiv pe rândul următor; Tab navighează la fel, Shift inversând direcția. Celulele needitabile sunt sărite. Validarea eșuată păstrează editorul. Escape anulează numai editarea celulei, inclusiv în combobox/calendar, fără închiderea ferestrei.

Datele calendaristice folosesc calendarul custom comun, ca la Plăți. Excepția este perioada taxelor: lună/an, fără calendar. Checkboxurile sunt editabile numai în coloanele declarate explicit astfel. Rândurile DGV ADE sunt cu 20% mai înalte pe mobil. Calculul lățimilor ține cont de borduri, scrollbarul vertical și rotunjiri; scrollul orizontal necesar unor grile late rămâne posibil pe PC.

## Plătitori pe PC și editorii

<!-- slice: ADE6-23 -->
Mesajul introductiv „Selectați grupa, copilul și plătitorul. Folosiți Adaugă sau Modifică.” este eliminat. Fereastra nu afișează nici mesajele informative de încărcare/salvare; zona de mesaje apare numai pentru erori.

<!-- slice: ADE6-21 -->
La fiecare deschidere se încarcă numai grupele, fără selecție; alegerea grupei descarcă copiii, iar alegerea copilului, persoanele asociate. Schimbarea grupei golește imediat copiii/plătitorii; răspunsurile pentru selecții vechi sunt ignorate. Anii și istoricul educatorilor se încarcă numai la deschiderea editorului grupei.

Cele trei liste dependente Grupe → Copii → Plătitori servesc numai selecției/afișării; Adaugă și Modifică deschid editorul elementului. Fostele coloane PL au antetul **I**, fără filtru:

| Listă | Semnificația bifei I |
|---|---|
| Grupe | Grupă închisă |
| Copii | Plecat |
| Plătitori / Platitori_sub | Închis: inversul lui Activ, numai la afișare |

Activ rămâne cu sensul normal în date și în editorul plătitorului. Închiderea grupei folosește InchisaDinAn: anul existent se păstrează, altfel se folosește anul curent; formularul nu cere separat anul.

Editorul grupei conține denumirea, bifa de închidere, ANI și tabelul perioadelor educatorilor. ANI și tabelul au înălțimi, antete, culori și rânduri aliniate. Coloanele umplu lățimea disponibilă; «+ Adaugă» se află în footerul tabelului. Lățimea maximă a editorului este 1120px, limitată la 96vw.

Editorul copilului conține nume, CNP, combobox grupă, data intrării, Plecat și data ieșirii. Editorul plătitorului conține nume, adresă, CNP, cod fiscal, cont, bancă, telefon, email și Activ. Ambele au lățime maximă 460px și distanță de 10px între rânduri, jumătate din dimensiunile anterioare.

Ferestrele sunt modale și blochează fundalul. Clicul în afară și Escape nu le închid. Salvarea închide editorul numai după succes; eroarea/conflictul păstrează datele nesalvate. Renunță abandonează explicit editările, conform machetelor.

CNP se verifică în JS și API după funcțiile VBA furnizate: 13 cifre, lună ≤12, zi ≤31, județ ≤52 sau 80/81 (nu sunt județe: persoane născute în străinătate, cu CNP românesc obținut ulterior) și cifra de control cu ponderile 279146358279, modulo 11 (restul 10 devine 1). Codurile sunt 0/2/3/4/5 pentru erori și -1 pentru valid. CNP gol este permis pentru date istorice/persoane juridice; verificarea nu introduce suplimentar validarea completă a unei date calendaristice.

Plătitorul nou se salvează activ (Activ bifat și blocat în editor); ceilalți plătitori ai **aceluiași copil** devin inactivi în aceeași tranzacție. Aceeași regulă se aplică activării unuia existent. Dezactivarea explicită permite zero activi; aceste salvări nu permit mai mulți activi și nu afectează alți copii. Importul nu curăță global dublurile istorice.

<!-- slice: ADE6-24 -->
Tabelul educatorilor are coloanele Email și CNP, ambele opționale, editabile ca text. Email acceptă maximum 255 de caractere și se verifică formatul; CNP acceptă 13 cifre și validarea existentă, inclusiv cifra de control. Golirea unui câmp îl salvează NULL. Datele sunt completate doar în web: Access nu are aceste coloane și migratorul nu le caută, nu le importă și nu le reconstruiește. La migrare rămân NULL. DDL-ul [AD_09_educatori_contact.sql](../sql/AD_09_educatori_contact.sql) se rulează numai pe AVACONT_SURSA; KBotAdmin sincronizează schema bazelor unităților.

Salvarea grupei și a perioadelor educatorilor este atomică. Istoricul educatorilor este sursa principală, cu fallback legacy numai când istoricul lipsește complet. Mutarea individuală/Plecat scrie jurnalul copilului și păstrează lunile închise; schimbarea grupei afectează prezența deschisă. Copiii plecați cu debit/credit rămân în grupa specială în luna următoare, cei cu sold zero ies. Transferul lot, compensarea și înlocuirea endpointului legacy /transfer: implementate.

## Plătitori pe mobil

<!-- slice: ADE6-25 -->
La o eroare de încărcare a formularului grupei se repetă „Modifică”. Mesajul de încărcare nu mai cere o cheie de salvare. Un deadlock sau timeout de blocare MariaDB este raportat ca „Baza de date este ocupată”, iar o salvare eșuată se reîncearcă din același formular, cu cheia operației păstrată. Operațiile nu se repetă automat. Corecția locală înlocuiește INSERT IGNORE pentru AD_Lock cu blocare exclusivă de la prima instrucțiune; fără DDL nou sau schimbarea autentificării.

La lățimi ≤980px se vede un singur DGV, pe nivelul curent. Fereastra păstrează 10px margine exterioară pe toate laturile și padding interior de 8px. Tabelul ocupă înălțimea disponibilă între antet/footer; scrollul vertical este numai în tabel, fără scroll exterior sau orizontal. Mesajul introductiv este eliminat.

Grupele și copiii au pe rând un buton ➡️ într-o coloană fără nume, filtru sau sortare, care deschide nivelul următor. Apăsarea lungă a fost înlocuită. Platitori_sub nu are această coloană. Butonul ↩️ din dreapta antetului revine la nivelul anterior; la Grupe închide fereastra listei. Footerul paginii are numai ➕ și ✏️ pentru nivelul curent.

| Listă | Lățimi |
|---|---|
| Grupe | Denumire 85% minus 40px; I 15%; ➡️ 40px; Educatori ascuns |
| Copii | Nume 50% minus 40px; CNP 35%; I 15%; ➡️ 40px |
| Plătitori | Nume 50%; CNP 35%; I 15% |

Lățimea butonului se scade numai din denumire/nume. Filtrarea denumirii/numelelor rămâne disponibilă și pe mobil.

## Numărul documentului în Alte plăți — AD20-01

<!-- slice: AD20-01 -->
În **Prezență → Alte plăți**, **Număr** este prima casetă editabilă a rândului nou. Introduceți numărul, apoi data, valoarea și explicația. Salvarea fără număr sau cu textul «Fără număr» este refuzată. Numărul trebuie să fie liber în subunitatea curentă în **anul calendaristic al datei documentului, de la 1 ianuarie până la 31 decembrie**, inclusiv față de documentele altor copii sau cele anulate. La 1 ianuarie al anului următor, același număr poate fi utilizat din nou. Refuzul apare în dialogul comun de eroare peste ecran; datele introduse rămân disponibile pentru corectare și reîncercare.

## Anularea unui document

<!-- slice: AD11-01 -->
În Chitanțe și Alte documente, ❌ «Anulare document» apare în dreapta documentului salvat numai cu dreptul «Anulare documente» (AD_ANULARE, acordat pe cont din administrare; vechiul `AllowCancelDocuments` nu mai există) și în luna deschisă sau ultima lună închisă. Lipsește la celelalte luni închise, restituiri și documente deja anulate (marcate tăiat).

Clicul cere confirmarea «Ești sigur/ă că vrei să anulezi …?» și «Motivul anulării». «Anulează» (dreapta) este inactiv fără motiv; «Renunță» (stânga), Escape sau clicul în afară închid fără anulare. Anularea recalculează situația copilului și păstrează documentul în listă, marcat anulat.

## Chitanțe pe PC

<!-- slice: AD16-01 -->
În chitanța PDF folosită la Listare și Descarcă PDF, câmpul **Adresa** al plătitorului ocupă două rânduri, în ambele exemplare. Textul folosește întreaga lățime disponibilă și se micșorează automat numai când este necesar pentru a încăpea integral. Se folosește adresa salvată, cu limita existentă de 255 de caractere; nu se taie textul la afișare.

<!-- slice: AD15-01 -->
La alegerea datei unui document nou, Valoarea se propune din soldul final al copilului pentru luna selectată: SFC pozitiv la Restituiri, SFD pozitiv la Chitanțe și Alte plăți. Suma rămâne editabilă. Aceste solduri înlocuiesc propunerea anterioară din SIC/SID (soldul inițial).

<!-- slice: ADE8-05 -->
Pe PC, în Chitanțe, 📥 de lângă 💾 deschide meniul ultimei chitanțe salvate a copilului curent; este inactiv până la prima salvare. Chitanțele existente au și propriul 📥. Meniul Windows are pictograme în stânga, navigare cu săgeți, Escape/clic în afară pentru închidere: 🖨️ Listare, ✉️ Trimitere pe mail, 📄 Descarcă PDF. Mailul rămâne inactiv și apare numai dacă plătitorul chitanței are EMail completat.

Listare deschide două exemplare și dialogul browserului; butonul Listare permite repetarea. Descarcă PDF generează aceeași chitanță. Datele/suma/seria/numărul provin din chitanța/plata salvate, persoana asociată și datele comune ale unității; chitanțele anulate au marcajul ANULATĂ. Listarea/PDF-ul nu emit numere și nu salvează rândul nou. Meniul este ascuns pe mobil.

Publicarea include fontul DejaVuSans.ttf și licența sa, șablonul receipt.html, modulele API/JS și CSS. PDF-ul necesită instalarea în mediul Python al serverului a `requirements-adechit.txt`, apoi restartul backendului. Fără DDL suplimentar. Nu s-au executat probe funcționale/vizuale; verificarea revine utilizatorului.

## Taxe și perioade

ADE6-20: la adăugarea unui rând în DGV, Escape în celula activată automat anulează întregul rând nou. După confirmarea primei celule, Escape anulează numai editorul curent. Aplicat la Taxe și perioadele educatorilor.

Taxele necesită valoare >0 și explicație nevidă la salvare. Pentru un rând nou, începutul este obligatoriu și după ultimul început; aceeași comparație se aplică la modificarea începutului unei taxe existente. Excepția migrării păstrează începutul NULL existent, nemodificat. La eroare se afișează un dialog cu «Continuă editarea» / «Anulează înregistrarea», inclusiv la ieșirea din rând din taste și la salvare. Anularea unui rând existent abandonează modificările lui; anularea unei taxe noi o elimină și reface taxa anterior activă. Schimbare TESTAT, RULAT, PUBLICAT.

ADE6-19: formularul are lățime maximă 744px (aproximativ 34% mai mic decât 1120px), cât coloanele DGV de 710px plus paddinguri și borduri; limita este 96vw. Schimbare TESTAT, RULAT, PUBLICAT.

Instrucțiunile introductive sunt eliminate în toate modurile; erorile rămân afișabile. Grila conține TaxaZilnica, Expl, DeLa, PanaLa și Activ (checkbox). DeLa se editează ca lună/an, afișat ll.aaaa și stocat YYYY-MM. PanaLa este numai pentru afișare și se completează automat, fără casetă la adăugare.

La adăugare, taxa nouă este activă implicit. Începutul propus este luna curentă sau luna de după ultima perioadă cunoscută. Noua perioadă trebuie să înceapă după toate începuturile cunoscute. Taxa anterior activă devine inactivă și primește ca sfârșit luna anterioară noului început; modificările se salvează atomic.

<!-- slice: AD14-03 -->
În **Taxe** și **Închidere lună**, ultima bifă **Activ** pe o taxă existentă le debifează pe celelalte. Poți reactiva orice taxă veche și schimba repetat alegerea înaintea salvării, indiferent de ordinea începuturilor. Simpla activare/dezactivare păstrează perioadele și nu este refuzată pentru suprapunerea cu taxa anterior activă. Perioadele noi păstrează completarea automată a sfârșiturilor. Anularea rândului nou reconstituie alegerea anterioară încă valabilă; sumele/explicațiile păstrează validările obișnuite.

**Taxele migrate pot avea DeLa/PanaLa NULL.** Lipsa datelor nu blochează citirea, editarea, utilizarea sau importul lor și nu se inventează perioade istorice. Numai taxele noi create prin catalog necesită DeLa. IDV istoric rămâne păstrat.

## Taxa lunii deschise — AD14-02

<!-- slice: AD14-02 -->
Pe calculator, după alegerea lunii, textul simplu «Taxa zilnică» din bara albastră este înlocuit de un combobox comun. Câmpul arată suma curentă; lista arată suma și explicația fiecărei taxe pozitive din subunitate. Ordinea este numerică după valoare, apoi alfabetic în română după explicație și cronologic după început, pentru valori egale.

Într-o lună deschisă, cu dreptul **Catalog**, alege o altă taxă și confirmă **Aplică taxa**. Se schimbă taxa întregii luni, pentru toate grupele subunității curente, inclusiv cele neafișate. Se păstrează zilele de prezență și se recalculează valorile contractuale/totale și situația. Operația nu împarte luna pe zile sau intervale de taxe. Taxa aleasă se folosește și pentru copiii preluați ulterior în aceeași lună.

Lunile închise sunt blocate inclusiv cu dreptul de editare a prezenței închise; fără Catalog, taxa se poate numai consulta. Înaintea schimbării, salvează/abandonează documentul început și salvează celula de prezență în editare. Salvarea blochează alte editări; succesul reîncarcă situația, păstrând sensul sortării și copilul selectat. Eroarea se afișează, selectorul revine la taxa cunoscută și permite reîncercarea aceleiași alegeri.

Alegerea păstrează valorile/perioadele taxelor din catalog, bifa **Activ** și lunile anterioare. Creează întâi taxa nouă prin **Meniu → Taxe**. Taxa activă din catalog rămâne propunerea pentru o lună nouă. Controlul din antet este ascuns pe telefon.

## Închiderea lunii cu schimbarea taxei — AD14-01

<!-- slice: AD14-01 -->

Lacătul lunii deschise, exceptând august, arată «Închidere lună»: fereastră modală, fundal estompat ca la mesaje, în locul confirmării simple. Arată taxa activă pentru luna următoare și tabelul taxelor, cu cea activă prima și fără sortare implicită. «Adaugă taxă» creează/selectează rândul activ, început în luna care se deschide sau în luna de după ultimul început; taxa anterior activă devine inactivă și primește sfârșitul. Se refolosesc regulile/validările Taxe și salvarea `/api/adechit/catalog-save`. «Închide luna» salvează taxele, apoi închide; «Renunță» / Escape abandonează modificările și lasă luna deschisă. Fără lista de taxe încărcată, închiderea este blocată. August păstrează fereastra anuală, fără selecția taxei.

## Închidere anuală — ADE7-04

<!-- slice: ADE7-04 -->

Închiderea lui august deschis arată fereastra anuală pe desktop; pe mobil (≤980px), fereastra lipsește și închiderea este dezactivată. Celelalte luni păstrează fluxul obișnuit.

În stânga este arborele custom al grupelor; în dreapta, DGV-ul custom al copiilor grupei alese. Prima coloană bifează selecția: Ctrl adaugă/elimină copii, Shift bifează intervalul în ordinea afișată. Trage selecția peste grupa destinație sau alege-o în comboboxul custom și apasă „Mută copiii selectați”. Poți muta unul/mai mulți copii sau toată grupa; „Grupa din august” păstrează originea.

„Plecat” este o bifă separată de selecție. Copiii astfel marcați ies la 31 august; cei cu sold sunt preluați în grupa specială pentru copiii plecați în septembrie. Sub tabelul copiilor este DGV-ul custom al educatorilor noului an. Numele grupei poate fi schimbat, iar educatorii pot fi adăugați, editați sau eliminați din plan.

<!-- slice: ADE6-24 -->
Email și CNP sunt opționale și în tabelul educatorilor noului an. Datele existente sunt preluate și păstrate la înlocuirea perioadelor; pot fi editate sau golite. Dacă se modifică numai contactele, perioadele existente nu sunt înlocuite.

„Adaugă grupă”, sub arbore, creează o grupă în planul noului an. Salvarea este refuzată dacă numele nu este unic, lipsește un educator sau lipsește un copil care rămâne. O grupă nouă fără copii poate fi eliminată din plan.

Sub tabele apare (când subunitatea are serie de chitanțe) caseta **„Reia numerotarea chitanțelor în noul an școlar”**, nebifată implicit. Bifată, seria rămâne, iar următoarea chitanță primește numărul din „Prima chitanță din septembrie” (1 propus). Nebifată, numerotarea continuă. Numărul unei chitanțe este unic pe serie și an școlar (septembrie–august), deci același număr poate reapărea în anul următor. Cere AD_13 aplicat.

„Renunță” abandonează planul. „Închide august și deschide septembrie” salvează atomic situația veche din august, aplică planul și creează septembrie. Mutările/plecările se jurnalizează, perioadele educatorilor se păstrează. Modificările altui utilizator impun reîncărcarea ferestrei. Sunt necesare drepturile închidere, catalog și transfer.

Implementat. Aspectul, interacțiunea drag-and-drop în browser și funcționarea pe MariaDB: TESTAT, RULAT, PUBLICAT.

## O încasare bancară pentru mai mulți copii

<!-- slice: AD15-01 -->
În **Asociere extrase cu copii → Asociere manuală**, alege extrasul, luna înregistrării, copilul și plătitorul activ. Completează **Suma pentru copil (lei)** și **Explicație document**, apoi **Adaugă copil la repartizare**. Repetă inclusiv pentru alte grupe ale subunității curente. Lista se ordonează alfabetic după numele copilului; **Șterge** permite refacerea sumei. Câmpul propune restul; totalul repartizat și restul apar sub listă.

La **Asociază și înregistrează în Alte plăți**, verifică fiecare copil/sumă în confirmare. Sumele trebuie să fie pozitive, în lei întregi, cu total egal exact cu încasarea. Aceeași tranzacție creează o plată și un document per copil. Pentru un singur copil poți salva direct întreaga sumă, fără listă. Previzualizarea automată rămâne pentru un singur copil; repartizarea cere modul manual.

O încasare deja salvată nu se poate repartiza din nou, nici în altă subunitate; anularea unui document păstrează proveniența și rezervarea extrasului. La schimbarea extrasului, a lunii, a modului, la reîncărcare sau închidere, repartizarea pregătită cere confirmare înainte de abandonare. Sunt necesare drepturile Încasare și Catalog. Repartizarea între copii necesită aplicarea manuală a [AD_15_extrase_repartizare.sql](../sql/AD_15_extrase_repartizare.sql), după AD_11, în baza unității.

## Ștergerea plăților preluate din extrase pentru repreluare — AD20-15

Pentru corecția solicitată la 05.11.2026 este pregătit [scriptul SQL manual](../sql/AD_20_15_reset_plati_extrase_luna_curenta.sql), limitat explicit la **noiembrie 2026**, după luna înregistrării plății, pentru toate subunitățile bazei selectate. Închide ferestrele ADECHIT de asociere/plăți, selectează baza unității și rulează **întregul fișier** într-o sesiune nouă de client MariaDB/HeidiSQL cu suport `DELIMITER`. La reușită scriptul confirmă tranzacția și afișează numărul plăților, documentelor și repartizărilor eliminate. Redeschide apoi ferestrele și reia asocierea.

Sunt șterse numai plățile identificate prin evidența preluării din extrase și documentele lor din AlteDoc. Se elimină și rezervările **extras–plată**, necesare pentru reapariția încasărilor în lista neasociată. Legăturile **copil–plătitor**, istoricul și datele bancare ale plătitorilor rămân păstrate, la fel ca prezența, plățile manuale, chitanțele, extrasele originale și istoricul încărcărilor. Scriptul se oprește fără ștergeri dacă identifică o lună închisă, situații salvate afectate sau legături neconcordante. Nu a fost executat de agent.

## Butoanele asocierii extraselor — AD20-01

<!-- slice: AD20-13 -->
În tabelul încasărilor din **Asociere extrase cu copii**, prima coloană este **Data extras** și afișează atributul **Data_extras** din antetul XML al extrasului importat. Pentru `Data_extras="04-Nov-2026"`, data afișată este **04.11.2026**. Aceeași coloană **Data extras** apare în **Ultimele încărcări**, imediat după **Încărcat**. Datele se sortează cronologic; Încărcat păstrează data și ora încărcării. Dacă o încărcare nu are legătură cu un extras importat, Data extras rămâne goală.

<!-- slice: AD20-14 -->
Arborele **LUNA / ANUL** din asociere folosește tot **Data_extras din antetul XML** pentru grupare. Luna aleasă filtrează încasările după aceeași dată, în modul manual și în previzualizarea automată. Astfel, antetul **04-Nov-2026** plasează extrasul în **Noiembrie 2026**, indiferent de data bancară sau de data documentului operațiunii. Anii și lunile își păstrează ordinea cronologică descrescătoare.

<!-- slice: AD20-01 -->
În fereastra **Asociere extrase cu copii**, **Închide** este în stânga jos. **Previzualizare automată** este în dreapta jos, lângă **Asociază și înregistrează în Alte plăți**. Modul automat păstrează în aceeași zonă comanda de revenire la asociere manuală și generarea documentelor bifate.

## Căutarea copilului în asocierea extraselor — AD20-03

<!-- slice: AD20-03 -->
Deasupra listelor **Grupe / Copii** este **Caută copil în toate grupele vizibile**. Introduceți minimum **2 caractere** din nume sau CNP. Lista arată numele, grupa și ultimele cifre din CNP, în ordine alfabetică românească după nume și grupă. Alegeți rezultatul: grupa și copilul se selectează și sunt aduși automat în vedere, iar plătitorii copilului se încarcă pentru asociere.

Căutarea include copiii din toate grupele **vizibile** ale subunității curente, indiferent de luna extrasului sau de existența prezenței în luna înregistrării. **Grupele ascunse nu apar în rezultate**, nici când «Arată ascunse» este bifat. Căutarea nu activează această bifă. Extrasul ales, luna și repartizările deja pregătite se păstrează la alegerea copilului. Pentru mai mult de 100 de potriviri, completați numele pentru a restrânge rezultatele.

### Eticheta și înlocuirea textului căutării — AD20-04

<!-- slice: AD20-04 -->
Eticheta **Caută copil în toate grupele vizibile** este în stânga comboboxului, inclusiv pe ecrane înguste. La intrarea în casetă cu mouse-ul, atingerea ecranului sau tastatura se selectează tot textul, astfel încât noua căutare poate fi scrisă direct peste cea existentă. Un clic ulterior, cât caseta are deja focus, permite poziționarea cursorului pentru corectarea textului.

### Panoul de înregistrare și butoanele — AD20-05

<!-- slice: AD20-05 -->
Pe calculator, **Luna înregistrării**, **Explicație document**, **Suma pentru copil** și repartizările ocupă o coloană în dreapta, care începe la nivelul părții de sus a tabelului de extrase și continuă până deasupra footerului. Tabelul de extrase este restrâns cu lățimea acelei coloane. Panoul are derulare proprie când conținutul nu încape. În modul automat, luna înregistrării rămâne în coloana din dreapta, lângă tabelul de revizuire.

Butoanele din footer (**Închide**, comutarea modului, **Asociază și înregistrează în Alte plăți** / **Generează documentele bifate**) au înălțimea minimă de **56 px**, dublu față de cei 28 px anteriori. Pe ecrane înguste, panourile se așază succesiv; butoanele păstrează înălțimea mărită.

### CNP, SID și suma propusă în asociere — AD20-06

<!-- slice: AD20-06 -->
<!-- slice: AD20-12 -->
Lista **Copii** afișează **CNP-ul complet** și soldurile **Lunii înregistrării**. **Suma pentru copil (lei)** propune inițial **întreaga încasare din extras**, apoi **diferența rămasă după sumele copiilor deja adăugați la repartizare**. Se actualizează la selectarea copilului, adăugare și ștergere. Când totalul este repartizat integral, restul propus este **0**. Suma rămâne editabilă.

Verificarea depășirii sumei se face la **ieșirea din casetă (lost focus)**. O sumă mai mare decât încasarea sau restul disponibil afișează eroare. La adăugarea copilului și la **Asociază și înregistrează** se reverifică limita; pentru salvare, suma copilului unic sau totalul repartizat trebuie să fie **exact egal cu suma extrasului**. Orice diferență afișează eroare și oprește salvarea întregului lot. Nu se afișează validări în timpul tastării.

Schimbarea lunii reîncarcă soldurile afișate. Pentru o lună închisă se folosesc soldurile salvate; pentru o lună deschisă, calculul existent. Copiii fără prezență în luna aleasă își pot afișa soldurile din istoric, fără ca simpla consultare să creeze prezență sau plată. În lista **Plătitori**, coloana **Banca** este eliminată.

### SIC în lista Copii — AD20-11

<!-- slice: AD20-11 -->
Lista **Copii** afișează și **SIC**, lângă **SID**, în lei, pentru **Luna înregistrării** aleasă. Ambele solduri se citesc din situația salvată a lunii închise sau din calculul existent, după caz, și se actualizează împreună la schimbarea lunii. Coloanele se sortează numeric. Lățimile interne sunt adaptate pentru CNP și cele două solduri, păstrând proporția Copii/Plătitori cerută în AD20-07. Propunerea sumei urmează restul din extras, conform AD20-12.

### Redistribuirea lățimii listelor — AD20-07

<!-- slice: AD20-07 -->
În asocierea extraselor, lățimea listei **Copii** este redusă cu **25%** față de varianta AD20-06, iar întregul spațiu eliberat este transferat listei **Plătitori**. Redistribuirea se aplică ambelor configurații pentru calculator; pe ecrane înguste se păstrează afișarea succesivă a listelor.

### Numere distincte pentru documentele copiilor — AD20-08

<!-- slice: AD20-08 -->
<!-- slice: AD20-10 -->
În asocierea manuală, caseta **Număr document** de sub **Luna înregistrării** este **editabilă**. Propune automat următorul număr pentru un copil nou: cu unu mai mare decât cel mai mare număr integral din **AlteDoc**, în subunitatea curentă și anul calendaristic afișat. Puteți înlocui propunerea cu alt număr întreg, pozitiv și liber în acel an. Se ține cont și de documentele anulate. Dacă anul nu are numere, se pornește de la **1**. Numerele nenumerice nu intră în calculul seriei.

<!-- slice: AD20-09 -->
Ultimul număr din AlteDoc se caută exclusiv în **anul calendaristic în curs**, stabilit după data curentă din România: de la **1 ianuarie până la 31 decembrie**. La 05.11.2026, se folosește anul **2026**. Aceeași regulă se aplică propunerii și reverificării la salvare, indiferent de luna aleasă sau data extrasului. La trecerea în anul următor, o propunere rămasă din anul anterior este refăcută înaintea salvării. Data extrasului afișată și data documentului sursă se păstrează.

Fiecare copil primește **numărul introdus în casetă** când este adăugat în repartizare. Lista și confirmarea finală arată copilul, numărul și suma. După adăugare, caseta propune numărul următor celui introdus, evitând numerele deja atribuite în repartizare. Numerele copiilor adăugați se păstrează la sortare sau ștergere; pentru corectarea numărului unui copil, ștergeți-l din repartizare și adăugați-l din nou cu numărul corect. Pentru salvarea directă a unui singur copil se folosește numărul din casetă. Simpla pregătire a listei nu consumă numere.

Serverul reverifică numerele la salvarea întregii repartizări. Un număr deja folosit în anul calendaristic în curs sau repetat între copii oprește întregul lot și apare un mesaj pentru corectare. Dacă a început un an nou, anul și propunerea sunt actualizate pentru revizuire și o nouă confirmare. Nu se schimbă numerotarea celorlalte fluxuri.

## Subunități — ADE10-04


<!-- slice: ADE10-04 -->

O unitate (DC) poate avea mai multe **subunități**, fiecare cu propriile grupe, copii, luni, taxe, plăți, serie și număr de chitanțe. Selectorul **Subunitate**, lângă unitate în antet, arată evidența curentă. Subunitatea unică se alege automat; dintre mai multe alegeți una la prima deschidere. Alegerea aparține filei: două file pot folosi subunități diferite. Schimbarea reîncarcă pagina; salvați/abandonați întâi fereastra deschisă sau documentul început. PDF-ul/listarea chitanței numesc subunitatea. Crearea se face prin import MDB cu ADE.Migrator (câmpul Subunitate).

## Predare și verificări efective

DDL-ul suplimentar este [AD_04_taxe_perioade.sql](../sql/AD_04_taxe_perioade.sql): adaugă DeLa/PanaLa VARCHAR(7) NULL în AD_ValoriTaxe. Se aplică o singură dată, după AD_03, în baza unității. Executat pe MariaDB. Utilizatorul publică prin AvacontPush; preview-ul local trebuie repornit pentru ultimele schimbări backend/schema. Nu presupunem că restartul sau DDL-ul au avut loc.

ADE6-06: 16 teste API și probe browser locale, inclusiv mobil, pentru starea de atunci. ADE6-07–17: testat, rulat și publicat (confirmat de operator, 23.10.2026). ADE1-06: doar pornirea/deschiderea aplicației pe localhost:5050 a fost verificată nonvizual, după corectarea blueprintului și redirecționării /portal. Marcajul de preview nu ocolește autentificarea producției; launcherul rămâne strict local. Această probă nu validează aspectul sau schimbările backend ulterioare. Nu există validare pe server, paritate completă cu Access sau publicare din chat.

Selectorul Subunitate apare numai daca aveti acces la mai mult de o subunitate. Accesul la o subunitate poate fi restrans la anumiti utilizatori (administratorul baza de date, tabelul AD_SubunitAccess).

## Conflicte de acces la portal (SLICE-AD11-03)

În **Plătitori**, „Portal blocat pentru părinții cu conflicte CNP/email” listează înregistrările de corectat din subunitatea curentă; conflictul se verifică pe întregul DC. Apăsarea intrării selectează grupa/copilul/părintele și deschide editorul. Conflictele vechi permit migrarea, dar blochează accesul părinților până la corectare; emailurile conflictuale noi sunt refuzate cu identificarea celuilalt părinte.

## Copil nou cu părinte, linkuri și dublu clic (SLICE-AD11-05)
<!-- slice: AD11-05 -->

- **«+» din Prezență** deschide un singur formular «Adaugă copil»: datele copilului și, dedesubt, secțiunea «Părinte (plătitor)». Există un singur «Salvează»; fără numele părintelui nu se salvează nimic, iar dacă părintele este refuzat nici copilul nu rămâne salvat.
- **«Adaugă copil» din administrarea plătitorilor** deschide același formular cu părinte.
- **După salvarea unui copil**, copacul lunilor rămâne deschis, deci luna aleasă se vede.
- **Portalul K-BOT:** clic pe sigla K-BOT (după autentificare) deschide meniul «Aplicații», la fel ca «A» din ADECHIT; ADECHIT se deschide de acolo.
- **Rapoartele** deschise pe ecran urmăresc tema aleasă (întunecată sau luminoasă); la listare și în PDF rămân pe alb.
- **Dublu clic** pe un rând din Prezență deschide administrarea plătitorilor cu grupa și copilul selectate (pe celulele zilelor dublu clic editează ziua). În administrarea plătitorilor, dublu clic pe o grupă, un copil sau un plătitor deschide fereastra de modificare.
- **Mesajele de eroare** au același aspect peste tot (fundal roșu deschis, text roșu închis).
- **Email deja folosit:** mesajul numește părintele, copilul și grupa; fiecare este un link care închide formularul curent și deschide înregistrarea respectivă (subunitatea se schimbă singură dacă este alta). Verificarea se face doar pe copiii activi, în subunitatea curentă; același părinte la mai mulți copii poate avea același email.
- **CNP / email întâlnite și în altă parte:** după ce părăsiți câmpul CNP sau Email din formularul plătitorului, un mesaj listează ceilalți părinți din DC cu același CNP sau email (cu subunitatea, dacă DC-ul are mai multe, și «(plecat)» pentru copiii plecați), cu aceleași linkuri. Este doar informare; salvarea este refuzată numai de regula de mai sus.

## Rapoarte (SLICE-ADE9-01)

<!-- slice: ADE9-09 -->
Titlul, etichetele bifelor și datelor și valorile din calendarele panoului sunt cu 2pt mai mici. Textul butoanelor de raport și dimensiunea pictogramei calendarului rămân cele existente.

<!-- slice: ADE9-07 -->
Comanda deschide imediat fereastra raportului cu **Se pregătește raportul…** și indicator de încărcare. Până la terminarea cererii nu poate porni alt raport/export în aceeași pagină, nici prin clicuri repetate sau închiderea ferestrei. Indicatorul dispare la afișarea primei pagini; la descărcare, fereastra se închide când fișierul pregătit este trimis browserului. Erorile apar în aceeași fereastră. Regula include chitanțele, dispozițiile de plată, exporturile PDF/Excel și fișa portalului părinților.

<!-- slice: ADE9-06 -->
Butoanele din panoul Rapoarte au același stil ca acțiunile VERCON: fontul interfeței, bordură simplă și contur cu umbră la trecerea mouse-ului sau la selectarea din tastatură. Butoanele indisponibile sunt estompate. Același feedback se aplică butoanelor de acțiune din ADECHIT.

<!-- slice: ADE9-05 -->
Sistemul comun extras din VERCON oferă previzualizare PDF cu **Tipărește**, **Alege imprimanta**, **Descarcă PDF**, zoom, potrivire la lățime, 100%, navigare între pagini și **Închide**; **Export Excel** deschide selecția coloanelor. PDF/Excel folosesc aceleași date generate, păstrate 10 minute pentru utilizatorul/unitatea/subunitatea curente. După expirare, închideți și regenerați; pentru date actualizate generați un raport nou, fiindcă exportul nu recalculează. Lățimile Excel configurate și valorile numerice ADECHIT se păstrează. Chitanțele folosesc același control, cu imprimare din bara raportului; fișa PDF a părinților, același motor de generare. Schimbarea contextului sau expirarea sesiunii închide previzualizările.


<!-- slice: ADE9-04 -->
Butonul **Situație de plată**, sub Situație lunară, folosește aceeași bifă **Toate grupele?**. Raportul A4 portret are coloanele Nr. crt., Numele și prenumele, restanță (SID), bani în cont (SIC), taxa lunară integrală și de plată, cu formula **SID + ZileLuna × TaxaZilnica − SIC**. Zilele configurate și taxa sunt ale lunii selectate; prezența și încasările din luna curentă nu reduc taxa din acest raport. Pentru lunile vechi fără ZileLuna se folosește numărul de zile lucrătoare, ca în prezență. Sumele negative se păstrează și reprezintă creditul rămas. Toate grupele înseamnă automat fiecare grupă pe pagină nouă, indiferent de **Separă grupe?**; textul configurat se repetă sub fiecare tabel.

Din **meniu → Text situație de plată**, cu dreptul Catalog, se editează textul pentru subunitatea curentă: liste, aldine/cursive/subliniere, aliniere, trei fonturi, mărimi 8–24 pt și șapte culori. Există și mod sursă HTML; lipirea în editorul vizual adaugă text simplu. Variabilele [UNITATE], [CONT], [COD_FISCAL], [GRUPA], [LUNA] și [AN] sunt înlocuite cu datele raportului. [CONT] este primul cont al unității; un cont ales explicit poate fi scris direct în text. HTML-ul este limitat la formatarea acceptată; scripturile, imaginile și linkurile sunt eliminate. Formatarea se păstrează în listare/PDF; Excel include nota ca text simplu. În PDF se folosesc fonturile standard sans/serif/mono și DejaVu pentru diacritice. Textul lung sau grupele numeroase pot continua pe mai multe pagini.

Panoul «Rapoarte» din dreapta ecranului Prezență deschide previzualizarea PDF pentru: Situație lunară (grupa aleasă, «Toate grupele?», «Separă grupe?»), Registru casă și Raport bancă (între «Dată început» și «Dată sfârșit», completate cu luna aleasă), Fișă cont și Fișă debitor (copilul selectat sau «Toți copiii din grupă»), Documente anulate și Situație financiară (luna aleasă). Pentru o restituire, butonul 📥 de pe rând oferă Dispoziția de plată. Rapoartele doar citesc: nu emit numere și nu schimbă situația. Pentru o lună închisă se folosește situația salvată la închidere. Detalii și abateri față de Access: `docs/worklog/SLICE-ADE9-01-rapoarte.md`.

<!-- slice: AD17-01 -->
Pentru **Situație lunară** (debitori), bifa **Ascunde rândurile cu zero**, bifată implicit, elimină doar copiii cu zero în toate coloanele numerice ale raportului, inclusiv zilele de prezență. Funcționează pentru o grupă, toate grupele și grupe separate, atât în ADECHIT web cât și în K-BOT → ADECHIT → Rapoarte. Filtrarea se aplică identic în previzualizare, PDF și Excel, pentru luni deschise și închise. Un rând cu sold final zero rămâne dacă are alte valori numerice diferite de zero. Grupele fără rânduri rămase nu se listează; dacă nu rămâne niciun rând, apare un mesaj. Totalurile valorice se păstrează. Schimbarea bifei se aplică la următoarea generare a raportului.

<!-- slice: AD18-01 -->
**Diferențe între luni**, disponibil în panoul Rapoarte și în K-BOT → ADECHIT → Rapoarte, compară luna aleasă (luna curentă a comparației) cu luna calendaristică imediat anterioară, inclusiv decembrie–ianuarie. Alegeți grupa sau **Toate grupele?**; opțional, **Separă grupe?** începe fiecare grupă pe pagină nouă. Sunt afișați numai copiii pentru care **SID curent ≠ SFD anterior** sau **SIC curent ≠ SFC anterior**, precum și copiii cu rând într-o singură lună, chiar dacă toate sumele sunt zero. Celelalte coloane nu decid includerea în raport. Bifa **Ascunde rândurile cu zero** nu se aplică aici.

Fiecare copil are o pereche numerotată: rândul complet din luna curentă, apoi rândul complet din luna anterioară. Coloanele **Luna**, **Grupa** și **Diferență / lipsă** arată perioada, grupa fiecărui rând și motivul afișării. Pentru luna fără rând apare **LIPSĂ**, cu numele și CNP-ul pentru identificare și fără valori numerice inventate. Copiii sunt ordonați alfabetic în limba română. O mutare între grupe păstrează perechea aceluiași copil; filtrul de grupă include perechea dacă aparține grupei în oricare dintre luni. La separarea grupelor se folosește grupa din luna curentă, iar pentru copilul lipsă în luna curentă, grupa anterioară. Pentru o grupă care nu mai apare în lista lunii curente, folosiți **Toate grupele?**.

Lunile închise folosesc situațiile salvate, iar cele deschise folosesc calculul situației lunare. Raportul nu schimbă datele. Valorile de sold lipsă sunt comparate ca zero, fără rotunjire de afișare. Dacă nu există diferențe sau rânduri lipsă, apare un mesaj; dacă luna calendaristică anterioară nu există în evidență, este indicată explicit, fără înlocuirea ei cu o lună mai veche. PDF-ul și Excelul folosesc aceeași selecție. Antetul numără perechile cu solduri diferite și rândurile lipsă în fiecare lună; nu se însumează sumele celor două luni.

<!-- slice: AD19-02 -->
După completarea unui istoric omis la migrare, **Fișă cont** păstrează rândul **Sold Inițial** și afișează separat **Sold reportat din anii anteriori (început 2021)**. Soldul reportat este suplimentar și se aplică de la începutul anului indicat. În lunile calculate intră o singură dată în soldul inițial, împreună cu SI existent și mișcările anterioare. Situațiile salvate ale lunilor închise rămân cele existente. Repetarea transferului nu dublează reportul. Regenerați rapoartele deja deschise după transfer.

## Prezenta in ultima luna inchisa

<!-- slice: ME01 -->
Prezenta unei luni inchise nu se poate modifica, cu o singura exceptie: un cont care are dreptul «Editare luna inchisa» (rolul AD_EDIT_INCHISA, impreuna cu «Prezenta») poate corecta zilele de prezenta din **ultima** luna inchisa. Celelalte luni inchise raman blocate pentru toti. Situatia salvata a copilului se rescrie pe loc (valoarea contractului si soldul), iar soldul initial al lunii deschise se recalculeaza singur din luna inchisa. Dreptul il da sau il retrage proprietarul sistemului, pe cont, din programul de administrare.

<!-- slice: AD13-01 -->
## Încărcarea extraselor bancare

**Meniu → Încărcare extrase bancare** aduce extrasele în unitate înaintea asocierii cu copiii (necesită drepturile **Încasare** și **Catalog**). Se pot alege fișiere PDF sau un folder întreg, ori se poate citi căsuța de e-mail a unității (**Setări e-mail…**: server IMAP, adresă, parolă de aplicație, dosar, expeditor opțional, textul din numele atașamentului, data de început). Parola se păstrează criptat pe server și se poate schimba oricând; dacă se schimbă serverul sau adresa, se introduce din nou. **Verifică e-mailul acum** citește doar mesajele noi și nu modifică nimic în căsuță. Un extras nu se încarcă de două ori (același fișier, același extras sub alt nume, operațiuni deja existente); tabelul **Ultimele încărcări** arată starea fiecărui fișier. Butonul **Asociază plăți cu extrase…** deschide asocierea existentă.
