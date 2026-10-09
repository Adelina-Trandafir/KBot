# ADECHIT — reguli adoptate

Adoptate explicit de utilizator la 08.10.2026, SLICE-ADE0-04.
Se aplică lucrului pentru ADECHIT, inclusiv integrărilor sale în PYTHON și componentelor
web comune modificate pentru acest proiect. Numerotarea păstrează lista selectată din
conversație. Regulile specifice VB.NET/WinForms nu sunt încorporate.

Se citesc împreună cu [ADECHIT_STATUS](../ADECHIT_STATUS.md), înaintea lucrului.
Rămân obligatorii interdicția citirii/modificării oricărui Claude.md, reutilizarea tuturor
sistemelor comune și lucrul local: utilizatorul publică prin AvacontPush și testează serverul.
Aceste reguli nu autorizează modificarea surselor Access sau schimbarea calculelor de business.

## Datele reale și procesele Access — instrucțiune explicită, 08.10.2026

- Datele reale ADECHIT sunt în
  `C:\Users\Adelina Trandafir\source\repos\KBOT\ADECHIT\ACCESS_SOURCES\baza2020_PP.mdb`.
  Citirea locală doar-citire a reușit prin Microsoft.ACE.OLEDB.16.0. Se citesc punctual
  schema, numărători și datele necesare feliei, fără afișarea masivă a înregistrărilor.
- **Dacă rămâne un proces MSACCESS, NU încercăm să îl oprim**, nici prin terminare
  forțată, închiderea ferestrei, automatizare COM sau alte metode. Regula se aplică
  inclusiv unui proces pornit pentru această lucrare; oprirea o face utilizatorul.
- Îl anunțăm clar: **„A rămas un proces MSACCESS deschis. Te rog să îl oprești tu;
  eu nu voi încerca să îl opresc.”** Dacă PID-ul este cunoscut, îl includem în mesaj.
  Nu afirmăm că există un proces rămas numai pe baza unei erori a driverului.
- Operațiile care depind de eliberarea bazei așteaptă intervenția utilizatorului;
  nu ocolim regula prin altă comandă de închidere.

## 1. Fără diacritice în cod

Identificatorii, comentariile și câmpurile API sunt ASCII. Excepția este textul vizibil
utilizatorului, care păstrează diacriticele. Întrebarea de verificare este: utilizatorul
vede efectiv acest șir? Dacă nu, este ASCII. Regula are prioritate față de stilul codului
existent: codul vechi neconform se corectează, nu se copiază ca model.
Numele câmpurilor trebuie să concorde între Python și JavaScript. Corectarea unui contract
existent se face coordonat cu apelanții și datele, fără redenumiri care rup compatibilitatea.
Documentația în română și sursele de referință nemodificabile nu sunt cod nou de normalizat.

## 2. Limba codului și a mesajelor

Codul, identificatorii, comentariile și numele testelor sunt în engleză.
Textele vizibile utilizatorului sunt în română, cu diacritice literale, inclusiv mesajele
de eroare API destinate afișării. Mesajele tehnice interne urmează limba codului.

## 4. Sursele importate sunt referințe

Exportul ADECHIT/ACCESS_SOURCES rămâne nemodificat. Implementarea web este separată;
nu rescriem referința pentru a o face să corespundă noii implementări.

## 5. Mediul Python și testarea locală

Se folosește mediul virtual Python al proiectului. Testele dependente de baza live
nu rulează local; probele web locale folosesc localhost:5050. Lipsa mediului sau a unei
dependențe se raportează; nu se pretinde că un test omis a trecut.

### Preferința explicită de testare — 09.10.2026

Utilizatorul testează modificările și raportează rezultatele. Nu pornim teste
automate sau verificări vizuale din inițiativă pentru acest flux. Excepția
autorizată explicit în thread a fost verificarea pornirii serverului și a
deschiderii aplicației, exclusiv nonvizual (ADE1-06). O autorizare ulterioară
se aplică numai scopului cerut; nu extindem proba la UI sau alte operații.
Starea SCRIS LOCAL nu devine TESTAT LOCAL prin actualizarea documentației.

## 6. Notele de versiune

Când sunt solicitate note de versiune, se citește întâi docs/release-notes/README.md
și se modifică numai NOUTATI.md pentru această operație, în formatul cerut acolo.
Această regulă nu înlocuiește worklogul și statusul fiecărei intervenții.

## 9. Înregistrarea excepțiilor

Excepțiile sunt înregistrate prin sistemele comune existente Python/JS.
Nu se introduce un logger ADE paralel și nu se transferă mecanismele desktop în web.

## 10. Propagarea erorilor operațiilor riscante

Erorile de I/O, HTTP, baze de date, parsare, procese și integrări externe sunt înregistrate
și propagate, păstrând cauza/stiva. Nu sunt ascunse și nu sunt înlocuite cu rezultate
care arată ca un succes. Se evită dublarea înregistrării aceleiași excepții.

## 11. Tratarea erorilor la limita interfeței

La limita UI, eroarea este tratată controlat: înregistrare, mesaj și stare corectă
pentru utilizator. Nu se prezintă operația drept reușită. Tratarea controlată nu înseamnă
ignorare tăcută; modificările nesalvate și posibilitatea reîncercării trebuie păstrate coerent.

## 12. Fără tratare redundantă a excepțiilor

Codul trivial și helperii acoperiți deja de un apelant nu necesită blocuri proprii doar
formal. Evităm înregistrarea repetată a aceleiași erori; responsabilitatea de tratare
rămâne explicită la limita potrivită.

## 17. Tema comună

Fără culori hardcodate în componente. Se folosesc tema și variabilele CSS comune
existente; schimbarea temei trebuie să se reflecte și în componentele ADE.

## 22. Mesajele utilizatorului

Mesajele folosesc mecanismul comun de afișare și jurnalizare. Jurnalul mesajelor
și cel al excepțiilor au responsabilități distincte: unul păstrează ce i s-a comunicat
utilizatorului, celălalt descrie eroarea tehnică. Dacă integrarea web are o lipsă,
se extinde mecanismul comun, fără copie independentă pentru ADE.

## 28. Procedura ajutorului

Ajutorul respectă procedura sa de documentare și verificare. Căile și numerotarea
desktop nu se transferă automat în ADECHIT; se folosește convenția proiectului ADE.
Înaintea modificării ajutorului existent se citește documentația aplicabilă acelui sistem.

## 29. Documentarea schimbărilor vizibile

Orice schimbare vizibilă actualizează documentația de utilizare, cu referință la felie.
Actualizarea se face în aceeași intervenție; dacă rămâne restantă, subiectele afectate
sunt consemnate explicit în worklog și în Open threads din status.

## 30. Schema reală a bazei

Se citește exportul local din MariaDB_Schema înaintea presupunerilor despre coloane,
chei sau relații. Scripturile din sql pot fi depășite și nu înlocuiesc schema reală.
Dacă exportul lipsește sau este vechi, se cer clarificări în locul presupunerilor.
Nu se accesează direct serverul pentru a-l actualiza; utilizatorul furnizează exportul.

## 31. Statusul și citirea selectivă

ADECHIT_STATUS.md este sursa unică a stării lucrărilor. Se citește întâi indexul,
apoi numai secțiunile și sursele necesare intervenției. Indexul rămâne scurt;
detaliile sunt în fișierele de stare. Statusul, worklogul și numărul următor liber
se actualizează împreună, fără a confunda documentarea cu implementarea sau publicarea.
