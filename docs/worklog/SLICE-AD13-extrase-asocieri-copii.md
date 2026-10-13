# ADECHIT · asociere extrase cu copii și previzualizare recurentă

Antetul are butonul „☰ Meniu” lângă numele aplicației. Iconița A are dimensiunea de
30 px și antetul folosește padding-ul portalului (4 px vertical, 28 px orizontal pe desktop).

Din Meniu → Asociere extrase cu copii se deschide o machetă care reutilizează panourile
și DataGrid din Plătitori. Extrasele sunt comune bazei unității; grupele, copiii,
plătitorii și lunile aparțin exclusiv subunității selectate. Pentru altă subunitate,
operatorul închide macheta și schimbă subunitatea în antet.

## Asociere manuală

Operatorul alege încasarea, grupa, copilul, plătitorul activ și luna înregistrării.
Prezența introdusă nu este o condiție pentru asociere (ajustarea AD13-02).
Explicația este generată folosind configurația existentă cu înlocuirea `[LA]`;
dacă aceasta nu produce un text, se propune „Contravaloare hrană pentru {luna} {anul}”.
Explicația poate fi editată și nu se înlocuiește după ce operatorul a modificat-o.

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

Plătitorul anterior devine închis (`Activ=False`). Contactele și identitatea portalului
se păstrează din plătitorul ales; CUI-ul extrasului nu se transformă automat în CNP.
FelDoc nu are câmp, coloană sau control în macheta nouă. Detaliile extrasului
rămân disponibile pentru identificarea încasării; ReferintaDest nu se mai afișează.

## Utilizare în lunile următoare

Operatorul alege luna și apasă „Previzualizare automată”. Sistemul propune asocieri
după **ambele** valori Cod fiscal și Cont ale plătitorilor activi deja definiți,
ignorând spațiile / diferențele de majuscule și prefixul fiscal RO. Nu compară
identificatori prin conversie numerică.

Un rând se bifează inițial doar la o potrivire unică la nivelul întregii unități,
cu copil în subunitatea curentă, grupă vizibilă, date obligatorii complete
și sumă reprezentabilă exact. Părinții cu mai mulți copii, combinațiile repetate în
mai multe subunități și rândurile fără potrivire necesită alegere explicită.
Numele din subunități neautorizate nu sunt expuse în previzualizare.

Operatorul poate modifica copilul, numele plătitorului, codul fiscal, contul,
data, numărul documentului, suma și explicația. Se salvează doar rândurile bifate,
după confirmarea lotului. Corecțiile afectează ADECHIT, fără modificarea sursei FX.

**Mai multe încasări pentru același copil în aceeași lună sunt permise.** Fiecare
IDFXE generează propria plată și propriul document. În același lot, reînnoirile
succesive urmăresc plătitorul creat de rândul precedent pentru acel copil.
Nu există restricție unică pe copil și lună.

Lotul este atomic: orice rând invalid anulează toate scrierile lotului. Cheia de
idempotentă rămâne aceeași la reîncercarea unui payload identic. Versiunile datelor
previzualizate și amprenta sursei sunt controlate înainte de scriere.

## Instalare și limite

- Rulați **manual** `sql/AD_11_extrase_asocieri.sql` în fiecare bază a unității care
  folosește ADECHIT și FX_Extrase, după migrările AD existente.
- Scriptul readaugă FelDoc, eliminat anterior de AD_07. Nu rulați AD_07 după AD_11.
- Nu există modificări în AVACONT_COMUN și aplicația nu creează schema.
- IDFXE este unic în registrul de asocieri la nivelul unității, astfel că o încasare
  nu poate fi repartizată din nou în altă subunitate. Anularea documentului păstrează
  rezervarea și proveniența. Repartizarea unei încasări între mai mulți copii nu este
  inclusă în această machetă.
- Se propun încasările cu credit pozitiv și debit zero. Debitări / restituiri nu se
  generează prin acest flux.
- Câmpul existent `AD_Plati.Plata` este INT (Access Long). Sumele cu fracțiuni nu se
  rotunjesc implicit. Operatorul este informat și asocierea unei sume care nu poate
  fi păstrată exact este refuzată. Modificarea structurii și a calculelor pentru bani
  cu zecimale nu face parte din această modificare.
- Numele plătitorului are limita existentă de 50 de caractere. Nu se trunchiază
  automat; numele prea lung poate fi corectat explicit în previzualizare.
- Se aplică regulile existente privind lunile eligibile și actualizarea situației
  salvate pentru o lună închisă.
- Propunerile se pregătesc la cererea operatorului; nu există salvări programate sau
  generare fără previzualizare și selecție.
- Pagina de previzualizare are până la 150 de încasări; după salvare se pot încărca
  cele rămase. Asocierea manuală permite încărcarea paginilor suplimentare.

Fișiere: `adechit.html`, `adechit-layout.css`, stylesheet-ul nou
`adechit-bank-associations.css`, `adechit.css`, `app.js`, modulele noi
`bank-associations.js` și `bank-preview.js`, rutele noi `bank_associations.py` și
`bank_preview.py`, `routes/adechit/__init__.py`, `schema.json`, SQL-ul AD_11.

Stare: implementat în surse. Nu s-au rulat teste, verificări executabile sau SQL,
conform instrucțiunilor proiectului. Funcționarea în browser / MariaDB nu este validată.
