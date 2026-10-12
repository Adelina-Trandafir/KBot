# VERCON — fluxuri de lucru și stări ale interfeței

Actualizat: 12.10.2026, **revizia 2**. Decizii de interfață primite de la utilizator în [Corectii_mockup-uri_01](MOCKUPURI/Corectii_mockup-uri_01/) și în mesajul ulterior cu cinci atașamente: tabelul NIR este unica referință vizuală, iar o listă de alimente poate avea mai multe BC-uri. Ultima revizie prevalează asupra propunerilor contradictorii anterioare. Documentare și mockupuri; comportamentele de mai jos nu sunt declarate implementate în aplicație.

## 1. Stările comune ale documentelor

| Stare | Formă/grilă | Arbore documente salvat | Comenzi |
|---|---|---|---|
| Consultare | Documentul selectat, valori doar pentru citire | În dreapta; frunza selectată este evidențiată | Adaugă, Modifică, Previzualizează; opțiuni la frunză |
| Adăugare | Antet și linii active, document nou | Ascuns; spațiul eliberat este folosit de editor | Renunță, Previzualizează, Salvează |
| Modificare | Copie de lucru a documentului selectat | Ascuns; spațiul eliberat este folosit de editor | Renunță, Previzualizează, Salvează |
| Salvare în curs | Draft păstrat, comanda de salvare nu se repetă | Rămâne ascuns | Starea salvării |
| Salvare eșuată/conflict/rezultat necunoscut | Draft păstrat; mecanismul comun de diagnostic/reconciliere | Rămâne ascuns până la rezolvare | Corectează/verifică salvarea conform protocolului existent |

La salvare confirmată se revine în consultare, se reafișează/reîmprospătează arborele și se selectează documentul salvat. La Renunță se abandonează draftul și se reafișează arborele, cu documentul anterior selectat când există. Renunțarea la draft nu anulează un document salvat.

Fiecare frunză a arborelui are la hover un buton **⋯** de opțiuni, accesibil și la focus/tastatură. Modificarea și anularea documentului salvat urmează drepturile, starea perioadei și motivul obligatoriu pentru anulare. Termenul „anulare” din notele despre revenirea arborelui este tratat ca ieșire din editare, nu ca ștergere a documentului salvat.

### 1.1 Contract vizual unic pentru grile

Referințe aprobate: [NIR consultare](MOCKUPURI/2026-10-12-corectate/02-nir-mijloace-fixe.png) și [NIR editor](MOCKUPURI/2026-10-12-corectate/02b-nir-adaugare.png). BC, porții/preț/valoare/distribuit, feluri de mâncare, BC-uri asociate și BP/BT/BS trebuie să folosească **același aspect de tabel**, nu doar aceeași paletă de culori.

- Celule contigue, borduri subțiri, antet și rânduri compacte, aceeași tipografie și densitate ca NIR; fără padding mare ori câmpuri prezentate ca niște carduri în celule.
- Editează în celulă cu aceleași controale compacte NIR; valorile calculate sunt text aliniat la dreapta. Coloane # și ⋮ unde există linii repetabile, cu același aspect NIR.
- **Adaugă rând în footer**, mic, aliniat la stânga; totalurile sunt celule aliniate cu coloanele numerice, imediat după linii. Nu se pune Adaugă produs în titlul secțiunii și nu se folosește o bandă mare separată pentru total.
- În consultare, celulele sunt pentru citire, iar Adaugă rând este vizibil/dezactivat, ca în NIR. În editor este activ pentru produsele documentului, felurile de mâncare și BC-urile asociate. Rezumatul fiecărei mese are un singur rând; nu se inventează mai multe rezumate pentru a avea buton de adăugare.
- Footerul grilei este distinct de bara cu Adaugă document/Modifică/Previzualizează/Salvează.

## 2. Recepție FC/NIR

Antetul are trei coloane:

1. Furnizor, categorie/cont și comisie.
2. Toate informațiile facturii: număr, dată, valoare.
3. Cele două informații NIR: număr și dată.

**Valoarea facturii este un câmp activ, completabil manual** la adăugare/modificare. Dacă nu este completat se folosește suma produselor. Totalul calculat al liniilor rămâne distinct și vizibil. Regula aprobată N01/N02 (0/NULL sau diferență strict mai mare de 1 leu → suma liniilor) rămâne aplicabilă la salvare; adnotarea despre câmpul activ nu introduce o altă toleranță.

Există **o singură grilă „Produse recepționate”**. Produsul se alege prin combobox cu căutare la tastare. La tipul MF, aceeași grilă se extinde cu nr. inventar, data intrării, durata normală, durata folosită și valoarea lunară. Coloanele sunt mai înguste și pot avea scroll orizontal în grilă. Nu există o secțiune MF separată sau o a doua grilă pentru aceleași linii. Documentul păstrează identitatea și cardinalitatea liniilor/activelor; exemplul din PNG folosește recepții individuale doar pentru lizibilitate.

Arborele din dreapta în consultare: **furnizor → dată → număr factură + total**. Nu este tabel de NIR-uri. Frunzele au ⋯ la hover. Butonul Adaugă de document este în bara de comenzi, distinct de Adaugă rând din grilă; comenzile de modificare a liniilor sunt active numai în editor.

## 3. Bon de consum

Grila produselor folosește aceeași familie DataGridView și aceleași editoare/comboboxuri ca la NIR. Arborele **Bonuri salvate** este în dreapta, înlocuind tabelul din propunerea inițială; o grupare potrivită este primitor → dată → număr BC + total. Această grupare BC este adaptare vizuală, nu o nouă cheie de date.

Adaugă/Modifică ascunde arborele. După salvare confirmată sau Renunță acesta reapare. Panourile de selecție produs și disponibil se păstrează unde sunt utile; disponibilul la data documentului exclude documentul editat și include separat draftul, conform contractului existent. Consultarea unui BC salvat nu îi scade liniile a doua oară. Totalul se numește **Valoare bon**, nu total de plată.

BC este o operație separată disponibilă și în gestiunea Alimente. Acesta poate fi creat/salvat înainte de a fi ales pentru lista de alimente; lista nu îl generează.

## 4. Lista de alimente și asocierea BC

Ecranul păstrează trei mese: Mic dejun, Dejun, Cină/Gustare. Rezumatele porții/preț/valoare/distribuit sunt **grile DataGridView**, la fel ca grilele de feluri de mâncare. Nu se adaugă în acest ecran tabelul „Alimente necesare”, disponibil după consum sau verificare stoc; aceste elemente ale primei propuneri sunt eliminate.

Arborele **Liste salvate** este în dreapta în consultare, formatat ca NIR/BC/BP: chevroane, iconuri albastre, linii de ierarhie, selecție albastru pal și ⋮ la frunză. Grupare: an → lună → dată → număr listă + valoare. Se ascunde la adăugare/modificare și reapare la salvare confirmată/Renunță.

**O listă are mai multe BC-uri asociate.** Secțiunea „Bonuri de consum asociate” este un DGV în stilul NIR, nu un selector unic sau un card. Coloane: #, Număr BC, Data, Primitor, Valoare bon (lei), ⋮. Fiecare bon apare o singură dată, cu totalul său. Footerul arată Adaugă rând și TOTAL BC; valoarea listei și diferența sunt valori de sinteză în același footer. Exemplul vizual: 780,0000 + 660,0000 = 1.440,0000 lei, valoare listă 1.440,0000 lei, diferență 0,0000. Egalitatea este doar exemplul ales, nu condiție obligatorie de salvare.

Fluxul corect de asociere:

1. Operatorul creează separat BC în aceeași gestiune Alimente sau folosește unul deja salvat.
2. În editorul listei, **Adaugă rând** din footerul BC-urilor deschide popupul Alege B.C., cu arbore de bonuri existente. Se afișează numai bonuri eligibile din gestiunea Alimente curentă care **nu sunt asociate altei liste**. Asocierile listei curente rămân în DGV; bonurile deja prezente nu pot fi adăugate din nou.
3. Alegerea adaugă un rând în draft, fără să înlocuiască celelalte BC-uri. Operatorul poate repeta alegerea pentru alte bonuri. Renunță în popup nu schimbă draftul. Nu se creează BC și nu se produc mișcări de stoc în popup.
4. Scoaterea unui rând prin ⋮ elimină numai asocierea din draft, fără anularea BC-ului. Se recalculează totalul BC-urilor și diferența; Renunță la editor restabilește întregul set salvat.
5. **Salvează lista** validează din nou contextul, eligibilitatea și revizia fiecărui BC, lipsa duplicatelor și a asocierii la altă listă. Salvează atomic antetul, mesele și întregul set de legături (adăugări, păstrări, eliminări). Nu creează/resalvează liniile BC și nu repetă consumul. Un singur BC devenit neeligibil blochează salvarea întregului set și păstrează draftul.
6. Salvarea confirmată revine în consultare și reafișează arborele listelor.

Cardinalitatea țintă solicitată este **listă 1 → N legături BC**. Exportul Access care arată IdLista unic rămâne probă istorică, nu limitare a interfeței web. DDL/generatorul existent trebuie aliniat în implementare: RowId pentru legătură, IdLista neunic, unicitate pe perechea contextuală listă–BC și protecție împotriva asocierii unui BC la două liste. [SCHEMA_VC](SCHEMA_VC.md) explică diferența. Nu se repară/șterg tacit date istorice; incompatibilitățile de import primesc diagnostic. Salvarea fără BC, dacă este permisă de contractul funcțional existent, nu creează unul automat.

## 5. Predare, transfer și casare OI/MF

Aceste pagini **nu au panourile din stânga Perioada și Deținători**. Nu au nici panourile propuse în dreapta Efectul transferului și Istoric produs. Contextul global rămâne vizibil. În consultare, în dreapta este arborele cu toate documentele familiei curente din gestiune: BP, BT sau BS. În adăugare/modificare acesta se ascunde, iar editorul ocupă spațiul.

| Familie | Sursă | Destinație | Produse eligibile |
|---|---|---|---|
| Predare | Conturile în care au intrat produsele | Primitor și cont primitor, după câmpurile sursă | Stocul de magazie al contului de intrare ales |
| Transfer | Predător | Primitor | Numai produse deținute de predătorul ales |
| Casare | Predător | **Fără secțiune Primitor** | Numai produse deținute de predătorul ales |

Grila și comboboxurile sunt comune cu NIR/BC. Butonul Adaugă de document există și aici. La schimbarea sursei se recalculează opțiunile eligibile; liniile incompatibile nu sunt reinterpretate tacit. Cantitatea se revalidează la salvare. Transferul păstrează efectul net zero; predarea/casarea păstrează efectele de stoc/registru definite în contract, chiar dacă panourile ilustrative au fost eliminate.

Nu se introduc transferuri între gestiuni/subunități: D12 rămâne în afara acestei clarificări. Arborele multianual an/lună cerut pentru **SFP/Rapoarte** rămâne valabil; eliminarea lui din pagina documentelor OI nu îl elimină din rapoarte.

## 6. Legătura cu planul și mockupurile

Acoperire: VC02-02/03/04/05, VC03-02 (cardinalitate), VC06-02/03/05, VC07-01/02/03. [Mockupuri corectate, revizia 2](MOCKUPURI/2026-10-12-corectate-v2/INDEX.md). Stocuri, NIR și Rapoarte sunt păstrate identic; BC, Alimente și OI au grilele corectate.
