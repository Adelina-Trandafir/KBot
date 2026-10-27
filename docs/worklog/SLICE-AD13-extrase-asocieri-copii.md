# ADECHIT · asociere extrase cu copii și previzualizare recurentă

Descrierea asocierii este urmată de ajustările din 13.10.2026: [macheta și asocierea fără prezență (AD13-02)](#slice-ad13-02), apoi [istoricul plătitorilor și comutarea modului (AD13-03)](#slice-ad13-03). Confirmarea operatorului din 23.10 aparține notei AD13-03; limitele inițiale din celelalte secțiuni sunt păstrate. Încărcarea PDF/IMAP din 26.10 este o intervenție distinctă: [AD13-01](SLICE-AD13-01-incarcare-extrase.md). Identificatorii fișierelor istorice 02/03 nu schimbă alocarea numerelor din registrul încărcării.

Antetul are butonul „☰ Meniu” lângă numele aplicației. Iconița A are dimensiunea de 30 px și antetul folosește padding-ul portalului (4 px vertical, 28 px orizontal pe desktop).

Din Meniu → Asociere extrase cu copii se deschide o machetă care reutilizează panourile și DataGrid din Plătitori. Extrasele sunt comune bazei unității; grupele, copiii, plătitorii și lunile aparțin exclusiv subunității selectate. Pentru altă subunitate, operatorul închide macheta și schimbă subunitatea în antet.

## Asociere manuală

Operatorul alege încasarea, grupa, copilul, plătitorul activ și luna înregistrării. Prezența introdusă nu este o condiție pentru asociere (ajustarea AD13-02). Explicația este generată folosind configurația existentă cu înlocuirea `[LA]`; dacă aceasta nu produce un text, se propune „Contravaloare hrană pentru {luna} {anul}”. Explicația poate fi editată și nu se înlocuiește după ce operatorul a modificat-o.

Salvarea în aceeași tranzacție:

| Sursă | Destinație |
| --- | --- |
| `platitor_nume` | `AD_Platitori_sub.Nume`, într-un rând nou activ |
| `platitor_cui` | `AD_Platitori_sub.CUI`, păstrat ca text |
| `platitor_iban` | `AD_Platitori_sub.Cont` |
| `DataDoc` | `AD_AlteDoc.DataDoc` și `AD_Plati.Data` |
| `NrDoc` | `AD_AlteDoc.NrDoc` |
| `Referinta` | `AD_AlteDoc.FelDoc`, exclusiv în baza de date |
| explicație generată / editată | `AD_AlteDoc.Explicatie` |
| `suma_credit` | `AD_Plati.Plata` (`TIP=1`) |
| `IDFXE` | cheia unică din `AD_EXTRASE_ASOCIERI` |

Plătitorul anterior devine închis (`Activ=False`). Contactele și identitatea portalului se păstrează din plătitorul ales; CUI-ul extrasului nu se transformă automat în CNP. FelDoc nu are câmp, coloană sau control în macheta nouă. Detaliile extrasului rămân disponibile pentru identificarea încasării; ReferintaDest nu se mai afișează.

## Utilizare în lunile următoare

Operatorul alege luna și apasă „Previzualizare automată”. Sistemul propune asocieri după **ambele** valori Cod fiscal și Cont ale plătitorilor activi deja definiți, ignorând spațiile / diferențele de majuscule și prefixul fiscal RO. Nu compară identificatori prin conversie numerică.

Un rând se bifează inițial doar la o potrivire unică la nivelul întregii unități, cu copil în subunitatea curentă, grupă vizibilă, date obligatorii complete și sumă reprezentabilă exact. Părinții cu mai mulți copii, combinațiile repetate în mai multe subunități și rândurile fără potrivire necesită alegere explicită. Numele din subunități neautorizate nu sunt expuse în previzualizare.

Operatorul poate modifica copilul, numele plătitorului, codul fiscal, contul, data, numărul documentului, suma și explicația. Se salvează doar rândurile bifate, după confirmarea lotului. Corecțiile afectează ADECHIT, fără modificarea sursei FX.

**Mai multe încasări pentru același copil în aceeași lună sunt permise.** Fiecare IDFXE generează propria plată și propriul document. În același lot, reînnoirile succesive urmăresc plătitorul creat de rândul precedent pentru acel copil. Nu există restricție unică pe copil și lună.

Lotul este atomic: orice rând invalid anulează toate scrierile lotului. Cheia de idempotentă rămâne aceeași la reîncercarea unui payload identic. Versiunile datelor previzualizate și amprenta sursei sunt controlate înainte de scriere.

## Instalare și limite

- Rulați **manual** `sql/AD_11_extrase_asocieri.sql` în fiecare bază a unității care folosește ADECHIT și FX_Extrase, după migrările AD existente.
- Scriptul readaugă FelDoc, eliminat anterior de AD_07. Nu rulați AD_07 după AD_11.
- Nu există modificări în AVACONT_COMUN și aplicația nu creează schema.
- IDFXE este unic în registrul de asocieri la nivelul unității, astfel că o încasare nu poate fi repartizată din nou în altă subunitate. Anularea documentului păstrează rezervarea și proveniența. Repartizarea unei încasări între mai mulți copii nu este inclusă în această machetă.
- Se propun încasările cu credit pozitiv și debit zero. Debitări / restituiri nu se generează prin acest flux.
- Câmpul existent `AD_Plati.Plata` este INT (Access Long). Sumele cu fracțiuni nu se rotunjesc implicit. Operatorul este informat și asocierea unei sume care nu poate fi păstrată exact este refuzată. Modificarea structurii și a calculelor pentru bani cu zecimale nu face parte din această modificare.
- Numele plătitorului are limita existentă de 50 de caractere. Nu se trunchiază automat; numele prea lung poate fi corectat explicit în previzualizare.
- Se aplică regulile existente privind lunile eligibile și actualizarea situației salvate pentru o lună închisă.
- Propunerile se pregătesc la cererea operatorului; nu există salvări programate sau generare fără previzualizare și selecție.
- Pagina de previzualizare are până la 150 de încasări; după salvare se pot încărca cele rămase. Asocierea manuală permite încărcarea paginilor suplimentare.

Fișiere: `adechit.html`, `adechit-layout.css`, stylesheet-ul nou `adechit-bank-associations.css`, `adechit.css`, `app.js`, modulele noi `bank-associations.js` și `bank-preview.js`, rutele noi `bank_associations.py` și `bank_preview.py`, `routes/adechit/__init__.py`, `schema.json`, SQL-ul AD_11.

Stare: implementat în surse. Nu s-au rulat teste, verificări executabile sau SQL, conform instrucțiunilor proiectului. Funcționarea în browser / MariaDB nu este validată.

## ADECHIT · ajustările machetei după adnotările operatorului <a id="slice-ad13-02"></a>

13.10.2026. Ajustări aplicate peste AD13, conform imaginii și corecțiilor ulterioare.

### Machetă

- Fereastra de asociere ocupă întregul viewport, fără margini, limite de 1680/900 px sau colțuri rotunjite, și se suprapune modal peste pagina principală. Înălțimea folosește 100dvh cu fallback 100vh. Antetul are 46 px, ca aplicația principală; subsolul are minimum 38 px, butoane de minimum 28 px și padding vertical de 4 px. La rezoluții mici, zona de lucru derulează separat, păstrând antetul și acțiunile din subsol vizibile. Subsolul poate crește când butoanele sau mesajul necesită mai multe rânduri. Padding-ul interior este redus pentru mai mult spațiu util.
- Zona explicativă de sus și rezumatul permanent de jos au fost eliminate.
- Butoanele Asociere manuală / Previzualizare automată sunt în subsol, lângă acțiuni.
- Arborele Lună/An folosește controlul TreeView din pagina principală și stă în stânga extraselor, cu lățimea de 258 px pe desktop. Datele lui sunt lunile extraselor din întreaga unitate; implicit se selectează ultima lună disponibilă.
- Luna extraselor filtrează sursa; Luna înregistrării este un câmp separat pentru documentul ADE. Alegerea unui extras nu mai schimbă luna înregistrării.
- Căutarea este doar în FX_Extrase.Explicatii, prin substring (*text*), după minimum două caractere, cu actualizare după 300 ms de pauză în tastare. Ștergerea / scurtarea textului sub două caractere elimină filtrul. Conform precizării ulterioare, prefixul bancar înaintea primei cratime este exclus din căutare atunci când corespunde câmpului Referinta. Căutarea ignoră majusculele/minusculele: `gr` găsește atât `Gr40`, cât și `Grădinița 40`, fără a cere numele copilului sau scrierea consecventă a denumirii grădiniței. Explicația originală rămâne intactă pentru afișare, amprentă și proveniență. Regula se aplică și previzualizării.
- ID-ul extrasului și ReferintaDest nu se afișează în tabel. Codul fiscal și Încasarea rămân, la 90 / 80 px. Detalii extras umple spațiul rămas. Autosizing-ul inițial este dezactivat, astfel încât lățimile explicite să fie respectate.
- Grupele au un panou compact, copiii păstrează CNP-ul într-o coloană de 100 px, iar Plătitorii primesc spațiul rămas dintre aceste liste și formular.
- Luna înregistrării și Explicație document sunt în dreapta Plătitorilor, una sub alta, într-un formular de 260 px (230 px pe desktop mai îngust). Explicația are textarea compactă, editabilă.
- Bifa Arată ascunse este sub Grupe și este debifată la deschiderea ferestrei.
- Erorile și confirmările scurte apar în subsol; nu există text permanent care repetă selecția. FelDoc rămâne exclusiv în bază.

### Asociere fără prezență introdusă

Cererea identifică direct copilul și luna, împreună cu versiunile copilului, plătitorului și lunii. Nici interfața manuală, nici propunerile automate nu cer existența prezenței sau completarea numărului de zile.

Dacă rândul lunar lipsește, se creează în aceeași tranzacție un rând de legătură în AD_Prezenta cu zero zile și valori contractuale zero, folosind grupa copilului și taxa lunii. O prezență existentă nu este modificată. Într-o lună închisă eligibilă, se creează și situația salvată pentru rândul nou, înaintea actualizării cu plata. Astfel, plata rămâne vizibilă prin fluxurile existente de situații și rapoarte, fără a introduce documente fără legătură financiară sau a fabrica zile de prezență.

Se păstrează regulile existente privind lunile eligibile. Mai multe extrase pentru același copil în aceeași lună generează în continuare plăți și documente distincte, inclusiv când rândul lunar este creat de primul element al lotului.

Previzualizarea automată folosește copilul drept cheie a opțiunii, nu ID-ul prezenței. Căutarea în timpul tastării păstrează editările și bifele rândurilor revăzute, inclusiv la revenirea unui rând după ștergerea filtrului. Se generează numai rândurile bifate din rezultatul vizibil. Amprenta sursei și versiunile țintei revăzute se păstrează; o căutare nu acceptă implicit date schimbate între timp. Reîncărcarea explicită sau schimbarea lunii cer confirmare înainte de abandonarea editărilor.

### Fișiere și stare

Modificate: adechit.html, adechit-bank-associations.css, bank-associations.js, bank-preview.js, bank_associations.py, bank_preview.py, routes/adechit/__init__.py și nota AD13 inițială. Noi: bank-periods.js și bank_targets.py.

Nu sunt necesare modificări suplimentare de schemă față de SQL-ul AD_11 pregătit anterior. Nu s-a executat SQL și nu s-au rulat teste sau alte verificări executabile, conform AGENTS.md. Stare: modificări în surse; browserul și MariaDB nu au fost probate.

## ADECHIT · coloanele istoricului și comutarea modului de asociere <a id="slice-ad13-03"></a>

13.10.2026. Ajustări solicitate pentru fereastra Asociere extrase cu copii.

- Plătitor: bază și minimum 242 px, conform imaginii. Ocupă automat spațiul rămas, fără să poată fi restrâns sub această lățime.
- Coloana CUI își păstrează datele și primește titlul CNP Plătitor și lățimea 135 px, față de 90 px (+50%).
- Cont: 168 px, față de 210 px (−20%).
- Coloana nouă Banca are 152 px, conform aceleiași imagini. Închis are 60 px. Istoricul are o lățime de bază de 757 px; numai Plătitor preia spațiul liber. Valoarea de afișare BancaCont se calculează din cont, folosind aceeași regulă ca Parteneri: IBAN fără spații, în majuscule, caracterele 5–8 căutate în AVACONT_COMUN.BIC (Cod → Banca). Conturile fără cod cunoscut au banca goală. Valoarea Banca păstrată în istoricul existent nu este rescrisă de această citire.
- Endpoint-ul bank-extracts/payers citește copilul și plătitorii numai din subunitatea autorizată, apoi consultă BIC prin conexiunea comună K-BOT. Conexiunea și cursorul se închid după citire; nu se execută modificări SQL.
- Butonul Asociere manuală este ascuns în modul manual; butonul Previzualizare automată este ascuns în modul automat. Rămâne vizibil butonul de trecere la celălalt mod. Starea se reaplică la fiecare deschidere a ferestrei.

Modificate: bank-associations.js, adechit.html, bank_associations.py și routes/adechit/__init__.py. Nu sunt necesare modificări de schemă. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
