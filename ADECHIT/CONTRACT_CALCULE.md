# SLICE-ADE0-02 — contractul calculelor ADECHIT

Referință obligatorie: [mdl_Situatie](ACCESS_SOURCES/Modules/mdl_Situatie.bas.txt),
interogările și tipurile Access asociate. Data analizei: 07.10.2026.
**Acesta este un contract extras static, nu o implementare și nu o probă de paritate.**
Nu schimbăm formulele. Necunoscutele se verifică pe rezultate Access înainte de acceptare.

## 1. Procedurile și apelanții

| Procedură | Apelanți găsiți | Tratament |
|---|---|---|
| CalculSituatieDebitori_Buget2021 | Rapoarte.Debitori_Click, Luni.bINC_Click, Prezenta.InchideLuna | Nucleul situației bugetare folosite de fluxul identificat |
| PreiaDateSalvate | Rapoarte.Debitori_Click când Prezenta.Inchisa=True | Citește snapshot; nu recalculează istoricul |
| ConcatRelated | Update_detalii | Construiește Detalii din documente, separator `;` |
| CalculSituatieDebitori_Buget | Nu s-a găsit apel extern în export | Variantă păstrată ca referință, nu amestecată cu 2021 |
| PreiaDetalii | Numai varianta anterioară | Include și BonuriF, spre deosebire de qExplicatie din 2021 |

Semnătura 2021 primește `grp`, `sav`, `tIDL`. Corpul **nu folosește grp sau tIDL**.
Luna efectivă este `Forms!Prezenta!IDL`; grupa este `Forms!Prezenta!IDG`, selectată numai
când `sav=False` și `Forms!Prezenta!Rapoarte!TGP=False`. La `sav=True` grupa este `*`.
API-ul web va primi contextul explicit, validat pe sesiune/unitate, fără dependențe de UI
în serviciul de calcul. Nu va interpreta automat argumentul tIDL ca luna de calcul.

## 2. Ordinea exactă a fazelor

| Ordine | Operație | Rezultat |
|---|---|---|
| 0 | DELETE din Situatie_buget, Solduri_Lunare, Explicatie_Lunara | Golește rezultatele de lucru; în VBA precedă BeginTrans |
| 1 | BeginTrans | Începutul tranzacției de calcul din VBA |
| 2 | [qPrezenta](ACCESS_SOURCES/queries/qPrezenta.sql) | Rândurile lunii/grupei, sume curente |
| 3 | [qSolduri](ACCESS_SOURCES/queries/qSolduri.sql) | Soldul anterior, pe IDP, pentru toți plătitorii |
| 4 | [Update_Solduri](ACCESS_SOURCES/queries/Update_Solduri.sql) | Copiază soldul/SID/SIC în Situatie_buget |
| 5 | [Update_Situatie](ACCESS_SOURCES/queries/Update_Situatie.sql) | Compensare, Anticipat, SFD/SFC |
| 6 | [qExplicatie](ACCESS_SOURCES/queries/qExplicatie.sql) | Rânduri de text pentru documentele lunii |
| 7 | [Update_detalii](ACCESS_SOURCES/queries/Update_detalii.sql) | ConcatRelated per IDZ, separator `;` |
| 8 | [Update_Compensare](ACCESS_SOURCES/queries/Update_Compensare.sql) | Corecția finală explicită a compensării |
| 9 | [Salvare_Lunara](ACCESS_SOURCES/queries/Salvare_Lunara.sql), numai sav=True | INSERT în SS_Buget |
| 10 | CommitTrans / rezultat True | Pe eroare: Rollback / rezultat False |

În web rezultatele intermediare sunt izolate per cerere/conexiune; nu golim un tabel
comun tuturor utilizatorilor. Păstrăm fazele și efectul conversiilor la fiecare scriere.
Salvare_Lunara este append fără filtru de deduplicare în SQL; API-ul va avea protecție
la executarea dublă a închiderii, fără dublarea snapshoturilor.

## 3. qPrezenta — populația și sumele lunii

- Populația pornește de la Prezenta, legată de Platitori, LunaD și **Grupe prin Prezenta.IDG**.
  Nu se filtrează Plecat=False; Plecat este copiat în rezultat. O persoană fără rând
  Prezenta în luna cerută nu apare automat în situație, chiar dacă are sold anterior.
- `xPlati = SELECT * FROM Plati WHERE Anulata=False`, legat prin IDZ.
- `Plata = SUM(IIf(TIP="2" AND Anulata=False, xPlati.Plata, 0))`.
- `Plati = SUM(IIf(TIP<>"2", xPlati.Plata, 0))`. Nu presupunem TIP=1 pentru toate
  datele istorice; NULL nu este automat echivalent cu „diferit de 2”.
- Retur: întâi SUM(Suma) pe `(IDP, IDZ, IDL, Anulat)` cu Anulat=False, apoi legare numai
  prin IDZ și `CDbl(Nz(Suma,0))`. Acest retur nu este însumat încă o dată peste plăți.
- ValoareContract și ValoareTotala se citesc din Prezenta; funcția nu recalculează
  taxele și nu aplică reduceri. Numele/grupa/educatorul vin din rândurile legate acum.
- Agregarea și gruparea exacte rămân cele din SQL; eventuale IDZ inconsistente sau
  joinuri care multiplică rândurile se raportează la migrare, nu se ascund prin DISTINCT.
- Filtrul folosește LIKE pe grupă și IDL; în web reprezentăm explicit „toate grupele”
  și luna aleasă, fără a primi expresii SQL de la browser.

## 4. qSolduri și Update_Solduri

Pentru fiecare IDP, înaintea lunii efective L:

```text
I = Platitori.SI
P = suma Prezenta.ValoareContract cu IDL < L
C = suma Plati.Plata cu IDL < L și Anulata=False
R = suma Retur.Suma cu IDL < L și Anulat=False
B = Nz(P,0) + Nz(I,0) − Nz(C,0) + Nz(R,0)
SID = B dacă B > 0, altfel 0
SIC = abs(B) dacă B < 0, altfel 0
```

UNION ALL și agregarea pe IDP preced RIGHT JOIN la Platitori. qSolduri aplică Nz
componentelor, astfel încât și persoanele fără mișcare primesc rezultatul zero.
Nu se folosesc ValoareTotala, SS_Buget sau COMP pentru acest sold istoric.

Update_Solduri atribuie `SoldInitial=Nz(Solduri_Lunare.Sold,0)`, SID și SIC din tabelul
intermediar către câmpurile țintei. **Solduri_Lunare are Double, iar ținta are Long**
pentru SoldInitial/SID/SIC. Conversia de aici precedă calculele următoare.

## 5. Update_Situatie și corecția finală

Notație pe rândul Situatie_buget: `V=ValoareContract`, `A=Plata+Plati`.
Expresiile de mai jos descriu ramurile pentru valori numerice nenule; nu autorizează
înlocuirea propagării NULL din IIf/Add cu zero unde sursa nu are Nz.

```text
C0 = dacă SID > 0: 0
     altfel dacă A > 0:
         dacă V − A < 0: 0
         altfel: V − A
     altfel:
         dacă SIC > V: V
         altfel: SIC

Compensare = dacă C0 > 0:
                 dacă SIC > 0 și SFD > 0: SIC
                 altfel: C0
             altfel: C0

Anticipat = dacă A − V − SID > 0: A − SID − V, altfel 0

F = Nz(SoldInitial,0) + Nz(V,0)
    − (Nz(Plata,0) + Nz(Plati,0) − Nz(Retur,0))
SFD = dacă F > 0: F, altfel 0
SFC = dacă F < 0: abs(F), altfel 0
```

Compensare citește SFD în **același UPDATE care îi atribuie noul SFD**. Nu presupunem
ordinea evaluării ACE și nu traducem într-un UPDATE MariaDB mizând pe aceeași ordine.
Urmează încă o operație, după scrierea și conversia SFD:

```text
Update_Compensare:
Compensare = dacă SFD > 0 și Compensare > 0: SIC
             altfel: Compensare
```

Nu plafonăm suplimentar Compensare la SIC, nu eliminăm această etapă și nu schimbăm
Anticipat într-o formulă alternativă de sold. O simplificare matematică trebuie să
treacă și cazurile de NULL, conversie și rotunjire, nu doar numere întregi pozitive.

## 6. Detalii și snapshot

qExplicatie produce, cu UNION ALL:

- `Ch:` + Numar, din Plati/Chitante, filtrând **Plati.Anulata=False**, nu separat
  Chitante.Anulata; filtrează Plati.IDL la luna aleasă.
- Primele două caractere din FelDoc + `.` + NrDoc, din Plati/AlteDoc, tot cu filtrul
  de anulare al plății și luna plății.
- `Re: ` + `Nz(NrDoc,'Fără nr.')`, din Retur cu Anulat=False și IDL selectat.

ConcatRelated ignoră NULL, păstrează șirurile vide și separă cu `;`. Apelul nu transmite
ORDER BY; qExplicatie ordonează doar pe IDZ. Ordinea documentelor în interiorul aceluiași
IDZ nu este garantată de sursă. Nu pretindem o ordine alfabetică/numerică implicită.
Rândurile fără explicații nu sunt atinse de INNER JOIN din Update_detalii.

Salvare_Lunara copiază 26 de câmpuri în SS_Buget, inclusiv valorile istorice Nume/CNP,
Grupa/Educator, Plecat și Detalii. Nu copiază SoldInitial; acesta nu există în schema
SS_Buget exportată. `Restanta` este copiată, deși lanțul 2021 nu îi atribuie explicit
o valoare și tabela de lucru are default 0.

PreiaDateSalvate golește rezultatul de lucru și copiază snapshoturile cu IDL exact,
plus filtrul IDG dacă TGP=False. Nu recalculează după numele sau taxele actuale.

## 7. Încasări după închidere — confirmat de utilizator

Permise în condițiile temporale ale Access, cu prezența blocată. Salvările Plati_*
adună suma în câmpul corespunzător al SS_Buget: Plata / Plati / Retur.
Apoi aplică C0 de mai sus, Anticipat și:

```text
F_snapshot = (SID − SIC) + ValoareContract − (Plata + Plati − Nz(Retur,0))
SFD = dacă F_snapshot > 0: F_snapshot, altfel 0
SFC = dacă F_snapshot < 0: abs(F_snapshot), altfel 0
dacă Compensare > 0 și SIC > 0 și SFD > 0: Compensare = SIC
```

Diferență de păstrat: aici formula pornește din SID−SIC, nu SoldInitial, și are Nz
numai pentru Retur. Detalii se extinde cu `;Ch:`, `;VI:` sau `;Re:` + număr,
deci textul nu este identic cu regenerarea qExplicatie pentru alte documente.
Nu există în aceste blocuri atribuire nouă pentru ValoareTotala sau Restanta.
La anulare nu am găsit actualizarea SS_Buget; cazul M02 din mapare rămâne deschis.

## 8. Ciclul lunar și variantele vechi

În Prezenta.InchideLuna, IDL curent este marcat închis, apoi calculul 2021 salvează
luna din formular; DeschideLuna creează luna următoare și preia copiii Plecat=False,
cu setul Activ din ValoriTaxe și ZilePrezenta/ZileAbsenta=0. `tIDL=IDL-1` este doar
o valoare transmisă nefolosită în funcția 2021. Condiția `tIDL<>0` există totuși în apelant.

Redeschiderea din arbore permite numai `MAX(LunaD.IDL)-1` și șterge Prezenta și LunaD
pentru IDL+1, plus snapshotul lunii redeschise. Relațiile cascade pot extinde efectele
ștergerii. Redeschiderea din Luni.bINC diferă (ultima lună, șterge snapshot IDL-1).
Până la validarea traseului folosit, acestea nu se unifică într-o comandă web distructivă.

Varianta CalculSituatieDebitori_Buget citește SQL din tabelul SQL, ID=1; în ramura sav
nu filtrează plățile anulate, iar retururile nu sunt filtrate după Anulat în calculele
sale istorice. Nu este echivalentă cu 2021. Conținutul SQL/ID=1 nu este exportat ca date.

## 9. Conversii care fac parte din paritate

| Loc | Tip / efect vizibil în export |
|---|---|
| Plati.Plata | Long; evenimentul Plati_chitante.Plata_AfterUpdate folosește și CInt |
| Prezenta.ValoareContract | Double |
| Prezenta.ValoareTotala, ValoareMancare, SI | Long |
| Solduri_Lunare, sume și solduri | Double |
| Situatie_buget.SoldInitial/SID/SIC/ValoareTotala/Plati/Retur/SFD/SFC | Long |
| Situatie_buget.ValoareContract/Plata/Restanta/Compensare/Anticipat | Double |
| SS_Buget.SID/SIC/ValoareTotala/Plati/Retur/SFD/SFC | Long |
| Retur.Suma și Chitante.Valoare | Double |

Referințe: [Situatie_buget](ACCESS_SOURCES/tables/Situatie_buget.md),
[Solduri_Lunare](ACCESS_SOURCES/tables/Solduri_Lunare.md),
[SS_Buget](ACCESS_SOURCES/tables/SS_Buget.md), [Plati](ACCESS_SOURCES/tables/Plati.md).
Nu presupunem că afișarea fără zecimale dovedește absența zecimalelor în date.
Nu alegem DECIMAL/ROUND/trunchiere ca înlocuitor al conversiilor ACE fără probe.
Conversia CInt din UI și conversia Long la stocare sunt două operații distincte.

## 10. Setul de referință necesar pentru acceptare

Pe o copie de date furnizată de utilizator se colectează intrările și rezultatele
intermediare după fiecare query, plus SS_Buget și rapoartele. Codex nu accesează serverul.

| Caz | Ce trebuie comparat |
|---|---|
| Fără istoric și fără mișcare | Zero vs NULL, existența rândului cu/fără Prezenta |
| Sold inițial pozitiv/negativ/zero | SoldInitial, SID, SIC și includerea tuturor persoanelor |
| Plată parțială, integrală, supraplată | Plata/Plati, Anticipat, Compensare, SFD/SFC |
| Credit anterior + datorie curentă + plată | Toate ramurile C0 și corecția finală |
| Restituiri curente/anterioare și anulate | Semnele, agregarea și propagarea soldului |
| TIP=2, alte valori, NULL | Separarea încasărilor, raport vs afișarea SitLunara |
| Două plăți și două retururi pe IDZ | Fără multiplicarea accidentală a sumelor prin join |
| Zecimale .49/.50/.51, pozitive/negative | Conversia fiecărui câmp Long, ordinea fazelor |
| Documente cu flaguri de anulare discordante | Filtrul real al fiecărei interogări, nu unul unificat inventat |
| Mai multe grupe/luni, persoană transferată/plecată | Selecția și numele/grupa din snapshot |
| Luna închisă, încasare nouă | Snapshot înainte/după, sold în luna următoare |
| Luna închisă, anulare | Constatarea M02; decizie separată înainte de implementare |
| Explicații goale/NULL, mai multe documente | Separatori, text și ordinea nedeterminată |
| Repetarea închiderii / două cereri simultane | Un singur rezultat persistent și niciun cache împărțit |

Acceptare: zero diferențe numerice neexplicate și nicio modificare de regulă ascunsă.
Exemplele calculate numai în Python nu înlocuiesc rezultatele de referință Access.
