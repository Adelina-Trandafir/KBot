# SLICE-ADE0-02 — maparea Access → web

Data: 07.10.2026. Analiză statică a exportului, fără rulare Access sau MariaDB.
Deciziile proiectului: [ADECHIT_STATUS](../ADECHIT_STATUS.md).
Calculele: [contractul de paritate](CONTRACT_CALCULE.md).
Acoperirea obiectelor: [inventarul complet](INVENTAR_OBIECTE.md).

„Apel găsit” înseamnă că există cod sau legătură de formular în export, nu că traseul
a fost executat în această analiză. „Fără apel găsit” nu dovedește că un obiect este
inutil: meniurile/configurările externe și deschiderea manuală nu sunt complet vizibile.
Nicio constatare de aici nu autorizează ștergerea datelor originale.

## 1. Intrarea și navigarea

- [Autoexec](ACCESS_SOURCES/Macros/Autoexec.txt) deschide formularul ADECHIT când
  funcția `autoexec()` întoarce True. Conținutul funcției și al ribbonului încărcat
  din afara traseului inspectat nu este considerat verificat integral.
- [ADECHIT.INTRARE_Click](ACCESS_SOURCES/Forms/ADECHIT.txt) selectează fișierul unității,
  reface legăturile de tabele, pornește mecanisme de backup și o corecție RunOnce.
  Web folosește alegerea unității și conexiunile existente; nu reproduce linkarea MDB,
  backupul FTP din client sau corecția istorică automată „Copii plecati 2021”.
- [basRibbonCallbacks](ACCESS_SOURCES/Modules/basRibbonCallbacks.bas.txt) conține
  deschideri către Platitori2016, Prezenta, Config_Taxe, Config_Unit și Config_Mail.
  Ultimele două nu devin automat pagini web: datele unității se refolosesc, mailul
  de business rămâne de stabilit; mailul de autentificare comun rămâne reutilizat.
- [Prezenta](ACCESS_SOURCES/Forms/Prezenta.txt) găzduiește Luni, Grupe_PR,
  Prezenta_sub_buget, Plati_chitante, Rapoarte și subTreeView. Schimbarea filei
  folosește `IncarcaPlati` din [mdl_2020](ACCESS_SOURCES/Modules/mdl_2020.bas.txt):
  0 = chitanțe, 1 = alte documente, 2 = restituiri.
- `IncarcaPrezenta` din [mdl_Popup](ACCESS_SOURCES/Modules/mdl_Popup.bas.txt) alege
  Prezenta_sub_buget sau Prezenta_sub prin CFGs/XX/Platitor. Web reține fluxul relevant
  fără reducerile excluse, nu copiază automat toate ramurile configurabile.

## 2. Matricea operațiilor incluse

| Operație / sursă exactă | Citire / calcul | Scriere Access | Echivalent web propus |
|---|---|---|---|
| Platitori2016.Sav_Click; Grupe, sub_Platitori, Platitori_sub_L, Platitori_sub_L_2, Delegati_L | Grupe, copii, persoane asociate/delegați; editările sunt pregătite în `_L` | Grupe, Platitori, Platitori_sub, Delegati | Grile comune editabile; API de salvare fără tabele `_L` |
| Config_Taxe.Activ_Click / Form_AfterInsert | ValoriTaxe; dezactivează celelalte seturi | ValoriTaxe | Seturi de taxe ADE, păstrarea taxei aplicabile lunii |
| Prezenta.mcTree_Click / IncarcaGrupe | LunaD, Grupe; SS_Buget la luni închise | Numai starea ecranului/cache | Arbore an/lună, grupă, persoană; context trimis explicit API-ului |
| Prezenta_sub_buget.bPrel_Click / ComboPrez.Sav_Click | Copii nepreluați, Plecat=False, Work_Days, SitLunara | Prezenta sau cache, în funcție de acțiune | Preluare în luna deschisă; păstrarea identității IDP/IDL/IDG |
| Prezenta_sub_buget.ZilePrezenta_AfterUpdate / Sav_Click | TaxaZilnica din setul ales; calculul detaliat mai jos | LunaD și Prezenta | Editarea prezenței în celulă, cu validare/salvare pe server |
| ComboGrupa.Sav_Click, ramura normală | Copilul și prezența selectată | Platitori.IDG, Prezenta.IDG, MutaCopil | Transfer cu istoric și aceeași unitate; operație atomică |
| ComboGrupa.Sav_Click, OA=TRG | Copiii bifați din catalog | Numai Platitori.IDG | Transfer de lot distinct; nu pretindem că are același efect ca transferul individual |
| Plati_chitante.Form_BeforeInsert / Sav_Click | Persoana activă IDS; CFGs/CH; explicație prin fMail2021/qVariabile | Plati TIP=2, Chitante, următorul număr CFGs; SS_Buget dacă luna închisă | Încasare + chitanță + număr, atomic; tabel comun Unitati_Chitante |
| Plati_Alte.Sav_Click | tmpPlatiAlte, număr/fel/sumă | Plati TIP=1, AlteDoc; SS_Buget dacă luna închisă | Alte încasări, cu rând editabil și document asociat |
| Plati_Retur.Sav_Click | tmpPlatiRetur, număr/explicație/sumă | Retur; SS_Buget dacă luna închisă | Restituire și dispoziție de plată |
| Del_Click în cele trei formulare Plati_* | Fereastra temporală, motiv, CodAnulare opțional | Flaguri Anulata/Anulat; Motivul în Plati; vezi diferența Retur | Anulare, nu ștergere; contractul lunii închise cere clarificarea neconcordanței din §6 |
| Rapoarte.Debitori_Click | Deschisă: CalculSituatieDebitori_Buget2021; închisă: PreiaDateSalvate | Tabele de calcul locale; fără recalcul la citirea situației închise | Raport din calculul exact sau din snapshot, după starea lunii |
| Prezenta.mcTree_NodeCheck / InchideLuna / DeschideLuna | Verificări grupe, calcul 2021, copii activi, taxe, zile | LunaD, SS_Buget, Prezenta pentru luna următoare | Serviciu tranzacțional pentru ciclul lunar; fără tabele de lucru globale |
| Rapoarte.FC_Click / mdl_2020.FisaCont | AddFisaCont + sold cumulativ pe IDP/Data | FisaCont, rezultat derivat | Fișă calculată la cerere |
| Rapoarte.Registru_Click / bRapBanca_Click / DA_Click / FD_Click | RegCasa, RaportBanca, DocumenteAnulate, familia FisaDebitor_* | Fără scriere business în aceste deschideri | Rapoarte ADE, după rezolvarea dependențelor legacy |

Formularele din tabel sunt în [Forms](ACCESS_SOURCES/Forms), iar interogările în
[queries](ACCESS_SOURCES/queries). Numele procedurilor identifică exact traseul citit.

## 3. Reguli de lucru confirmate în cod

### Prezență și taxe

`Prezenta_sub_buget.ZilePrezenta_AfterUpdate` face:

```text
ValoareContract = ZilePrezenta × TaxaZilnica(IDV ales în ecran)
ValoareTotala = ValoareTotala_anterioară
               − ZilePrezenta.OldValue × TaxaZilnica + ValoareContract
```

La afișarea unei luni deschise, `IncarcaPrezenta` reconstruiește:
`ValoareTotala = ValoareContract + SID + Retur − Platita − SIC`, folosind SitLunara.
Aceasta este o valoare afișată în cache; nu o confundăm automat cu valoarea persistentă
Prezenta.ValoareTotala citită de qPrezenta. Salvarea copiază cache-ul în Prezenta.

Salvarea caută rândul prin `(IDP, IDL, IDG)`, nu doar `(IDP, IDL)`. Nu impunem o
unicitate mai strictă înainte de verificarea datelor de transfer. Perioadele detaliate
din Prezenta_sub sunt accesibile prin DatePicker, dar nu sunt necesare motorului 2021;
necesitatea UI-ului de calendar detaliat rămâne distinctă de editarea numărului de zile.

`Work_Days` exclude sâmbătă/duminică prin șiruri `Sat`/`Sun`, fără calendar de sărbători.
Comportamentul pe instalarea Access depinde de localizare; nu introducem tacit un
calendar de sărbători sau alt număr de zile la portare.

### Lună închisă — decizie a utilizatorului, 07.10.2026

**Păstrăm încasările într-o lună închisă, cu actualizarea situației salvate.
Prezența rămâne blocată.** Aceasta corectează formularea prea generală din planul inițial.

Cele trei formulare Plati_chitante / Plati_Alte / Plati_Retur refuză inserarea/anularea
când `IDL_selectat + 1 < DMax("IDL", "Prezenta")`. Referința este **Prezenta**, nu
LunaD și nici data calculatorului. Mesajul spune „luna prezentă sau anterioară”.
Maparea temporală a IDL trebuie păstrată/verificată la migrare.

La salvare, fiecare modifică snapshotul SS_Buget identificat prin IDZ:
chitanță → Plata, alt document → Plati, restituire → Retur; recalculează compensarea,
anticipatul și soldurile și adaugă explicația documentului. Formulele sunt în
[contract](CONTRACT_CALCULE.md). Nu este permisă editarea generală a snapshotului în grilă.

### Numerotare și explicație chitanță

Fluxul inclus `Plati_chitante` citește CFGs/Numar la inserare și scrie `Numar+1` la
salvare. **Pe acest flux, configurația este următorul număr de utilizat.** Citirea
veche nu filtrează `f`, scrierea filtrează `f='CH'`; importul cerut de utilizator
selectează explicit CH și raportează duplicate/lipsuri.

`Chitante_bun` și `Plati` au alt mecanism de numerotare. Nu îl amestecăm cu cel curent.
Seria și numărul se iau din date; nu introducem GR40/783 sau fallbackul „Serie” ca seed.
La migrare se compară configurația cu numerele existente, fără „reparare” automată.

Explicația se generează prin `fMail2021`, care citește qVariabile și substituie
17 marcaje, inclusiv `[LA] = fluna(Luna) + '/' + Anul`. `[LA]` nu este câmpul numeric
LunaD.LA de forma MMYYYY. Textul de restanță poate fi „Achitat restanta”. Substituția
nu înseamnă că trebuie activat sistemul de trimitere email al aplicației vechi.

## 4. Modelul persistent propus

Mapare pentru ADE3, **nu DDL aprobat sau executat**:

| Sursă | Destinație propusă | Identitate / observație |
|---|---|---|
| Grupe | ADE_Grupe | IDG; denumire, educator |
| Platitori | ADE_Platitori | IDP; aici sunt copiii; IDG, IDV, SI, Plecat și datele personale |
| Platitori_sub | ADE_Platitori_sub | IDS → IDP; persoanele plătitoare; Activ |
| Delegati | ADE_Delegati | IDD → IDS; necesitatea câmpurilor exclusiv de facturare se separă |
| ValoriTaxe | ADE_ValoriTaxe | IDV; păstrăm istoricul necesar, fără recalcul de reduceri excluse |
| LunaD | ADE_LunaD | IDL; lună/an, set de taxe, Inchisa |
| Prezenta | ADE_Prezenta | IDZ → IDP/IDL/IDG/IDV; valori istorice nealterate la import |
| Prezenta_sub | ADE_Prezenta_sub, condiționat | IDX → IDZ; perioade detaliate, dacă păstrăm calendarul/istoricul aferent |
| Plati | ADE_Plati | IDPL → IDP/IDZ/IDS/IDL; sumele și anulările alimentează soldurile |
| Chitante | ADE_Chitante | IDC → IDPL; serie, număr, explicație, valoare și anulare |
| AlteDoc | ADE_AlteDoc | IDA → IDPL; număr/fel/data documentului |
| Retur | ADE_Retur | IDR → IDP/IDZ/IDS/IDL; sumă, document, anulare |
| SS_Buget | ADE_SS_Buget | Snapshot lunar: ID, IDP/IDL/IDZ/IDG, sume și descrieri istorice |
| MutaCopil | ADE_MutaCopil | IDM → IDP/IDL/IDZ; grupa veche/nouă și data |
| CFGs, numai CH | AVACONT_COMUN.Unitati_Chitante | Asociere DC; serie, următorul număr, șablon explicație |
| Unitati | Sistemul comun existent | Nu creăm un duplicat ADE_Unitati; nu suprascriem datele comune la import |

Cheile Access nu se renumerotează independent. Alegerea păstrării lor sau a unei mapări
la chei noi trebuie să mențină toate relațiile și ordinea IDL. Relațiile declarate sunt
în [relationships.md](ACCESS_SOURCES/relationships.md). De exemplu, relația LunaD→Plati
este exportată fără integritate referențială; legătura pe IDS a plății apare în cod,
fără a presupune că toate FK-urile există în Access. Verificăm orfanii înainte de DDL/import.

## 5. Rapoarte și funcții suplimentare

- Incluse ca flux identificat: cele trei SituatieDebitori_buget*, Chitanta,
  Dispozitie plata, RegistruCasa, RaportBanca, DocumenteAnulate și FisaCont/FisaCont_s.
- Familia FisaDebitor este accesibilă din Rapoarte, dar include bonuri și reduceri în
  interogări vechi. Adaptarea conținutului trebuie separată de motorul de sold 2021.
- FisaCont folosește debitul din ValoareContract, creditul din Plati.Plata,
  returul ca debit, SI la 01.01.2001 și transferurile cu valoare zero. Soldul este cumulativ
  pe persoană, în ordinea IDP/Data; ordinea în aceeași zi nu este determinată complet.
- COMP/COMP_C/COMP_D reprezintă compensare între persoane, distinctă de coloana
  Compensare din mdl_Situatie. Formularul scrie COMP, dar lanțul 2021 **nu citește COMP**.
  Nu există apel de deschidere găsit în traseele inspectate. Nu îl adăugăm implicit în motor.
- SIIR este accesibil din Platitori2016; importă Excel prin IMPXLS și scrie Grupe,
  Platitori, Platitori_sub, Delegati. Portarea acestui import operațional rămâne de stabilit,
  separat de migrarea inițială ADE5.
- Mail/TrimitereCalup, exporturile Excel/PDF și variantele vechi rămân inventariate;
  existența lor în export nu echivalează cu includerea integrală în prima versiune web.

## 6. Neconcordanțe de rezolvat înaintea implementării dependente

| Cod | Dovezi din sursă | Consecință / pas necesar |
|---|---|---|
| M01 | Plati_Alte.Sav_Click scrie TIP=1; AddFisaCont/qVariabile folosesc 0 pentru alte documente și 1 pentru bon | Motorul 2021 separă 2 / diferit de 2; rapoartele/explicațiile trebuie clarificate, fără remapare tacită a istoricului |
| M02 | Del_Click din cele trei Plati_* schimbă anularea și reîncarcă prezența, fără UPDATE SS_Buget; luna închisă citește snapshotul | Nu există dovadă statică a recalculării snapshotului după anulare. De stabilit comportamentul dorit pentru snapshot, fără modificarea formulelor |
| M03 | Plati_Retur.Del_Click cere Motiv, dar nu îl atribuie lui Retur.Motivul | Nu pretindem că istoricul existent conține motivele cerute; persistarea în web se decide explicit |
| M04 | Data_BeforeUpdate folosește lună diferită AND an diferit | Validarea din cod nu este echivalentă cu mesajul „documentul trebuie să fie din luna selectată”; nu o schimbăm fără decizie |
| M05 | Prezenta.InchideLuna trimite IDL-1, dar funcția 2021 ignoră tIDL și citește formularul; Luni.bINC are altă redeschidere | Contractul calculului folosește IDL din contextul real; fluxul de redeschidere cere probă de referință |
| M06 | Platitori2016.Sav_Click actualizează grupa în toate rândurile Prezenta ale copilului; ComboGrupa normal schimbă numai IDZ selectat | Nu unificăm cele două comenzi de transfer/editare fără alegere explicită |
| M07 | qVariabile include bonuri, ramuri vechi TIP și potențial mai multe rânduri pentru același IDZ; fMail2021 ia primul rând și separă valori prin virgulă | `[LA]` este clarificat; alte șabloane reale sunt necesare înainte de portarea completă a explicațiilor |
| M08 | DocumenteAnulate folosește alias xPlati în FROM și calificări [plati] în expresii | De probat în Access; nu declarăm query-ul executabil identic pe SQL server |
| M09 | Formularele vechi referă Chitante/Plati_fact/FTP, fără obiect cu acel nume în export; rpt_SitFin referă Situatii salvate absent | Ramuri incomplet exportate/vechi, nu surse validate pentru noul flux |
| M10 | Unitati, SS_Buget și alte tabele sunt linked către MDB extern | Exportul de structură nu livrează rândurile; migrarea trebuie să primească datele reale ale backendului |

Niciuna dintre aceste constatări nu blochează scheletul comun sau editorul de grilă.
Blochează însă declararea parității finale a funcției dependente până la clarificare/probă.

## 7. Ce se poate face în continuare

ADE1 poate începe pe date fictive, folosind sistemele comune și localhost:5050.
ADE2 poate implementa editarea fără server real. Pentru ADE3/ADE5/ADE7 sunt necesare:
datele Access reale relevante (inclusiv CFGs), identificarea bazei/unității țintă și
rezultatele Access pentru cazurile din contract. Utilizatorul face push, rulează SQL
și probează serverul; nu s-au efectuat asemenea operații în ADE0-02.
